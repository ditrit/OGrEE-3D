using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rack : Object
{
    private Vector3 originalLocalPos;
    private Vector3 originalPosXY;
    private Transform uRoot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rack>() && GameManager.gm.currentItems.Contains(gameObject))
        {
            GameManager.gm.AppendLogLine($"Cannot move {name}, it will overlap {other.name}", "yellow");
            transform.localPosition = originalLocalPos;
            attributes["posXY"] = JsonUtility.ToJson(new Vector2(originalPosXY.x, originalPosXY.y));
        }
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public override void SetAttribute(string _param, string _value)
    {
        if (_param.StartsWith("description"))
            SetDescription(_param.Substring(11), _value);
        else
        {
            switch (_param)
            {
                case "label":
                    GetComponent<DisplayObjectData>().SetLabel(_value);
                    break;
                case "labelFont":
                    GetComponent<DisplayObjectData>().SetLabelFont(_value);
                    break;
                case "domain":
                    if (GameManager.gm.allItems.ContainsKey(_value))
                    {
                        domain = _value;
                        UpdateColor();
                    }
                    else
                        GameManager.gm.AppendLogLine($"Tenant \"{_value}\" doesn't exist. Please create it before assign it.", "yellow");
                    break;
                case "color":
                    SetColor(_value);
                    break;
                case "alpha":
                    UpdateAlpha(_value);
                    break;
                case "slots":
                    ToggleSlots(_value);
                    break;
                case "localCS":
                    ToggleCS(_value);
                    break;
                case "U":
                    ToggleU(_value);
                    break;
                default:
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    break;
            }
        }
        // PutData();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Update rack's color according to its Tenant.
    ///</summary>
    public void UpdateColor()
    {
        if (string.IsNullOrEmpty(domain))
            return;

        OgreeObject tenant = ((GameObject)GameManager.gm.allItems[domain]).GetComponent<OgreeObject>();

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        ColorUtility.TryParseHtmlString($"#{tenant.attributes["color"]}", out myColor);
        mat.color = new Color(myColor.r, myColor.g, myColor.b, mat.color.a);
    }

    ///<summary>
    /// Move the rack in its room's orientation.
    ///</summary>
    ///<param name="_v">The translation vector</param>
    public void MoveRack(Vector2 _v)
    {
        originalLocalPos = transform.localPosition;
        Vector2 posXY = JsonUtility.FromJson<Vector2>(attributes["posXY"]);
        originalPosXY = posXY;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = false;
        }

        Room room = transform.parent.GetComponent<Room>();
        switch (room.attributes["orientation"])
        {
            case "EN":
                transform.localPosition += new Vector3(_v.x, 0, _v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.x, _v.y);
                break;
            case "NW":
                transform.localPosition += new Vector3(_v.y, 0, -_v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.y, -_v.x);
                break;
            case "WS":
                transform.localPosition += new Vector3(-_v.x, 0, -_v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.x, -_v.y);
                break;
            case "SE":
                transform.localPosition += new Vector3(-_v.y, 0, _v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.y, _v.x);
                break;
        }
        attributes["posXY"] = JsonUtility.ToJson(posXY);
        StartCoroutine(ReactiveCollider());
    }

    ///<summary>
    /// Toggle U location cubes.
    ///</summary>
    ///<param name="_value">True or false value</param>
    public void ToggleU(string _value)
    {
        if (_value != "true" && _value != "false")
        {
            GameManager.gm.AppendLogLine("U value has to be true or false", "yellow");
            return;
        }
        else if (_value == "true")
        {
            if (!uRoot)
            {
                uRoot = new GameObject("uRoot").transform;
                uRoot.parent = transform;
                uRoot.localPosition = new Vector3(0, -transform.GetChild(0).localScale.y / 2, 0);
                uRoot.localEulerAngles = Vector3.zero;
                GenerateUColumn("rearLeft");
                GenerateUColumn("rearRight");
                GenerateUColumn("frontLeft");
                GenerateUColumn("frontRight");
            }
        }
        else
        {
            if (uRoot)
                Destroy(uRoot.gameObject);
        }
    }
    public void ToggleU()
    {
        if (uRoot)
            Destroy(uRoot.gameObject);
        else
        {
            uRoot = new GameObject("uRoot").transform;
            uRoot.parent = transform;
            uRoot.localPosition = new Vector3(0, -transform.GetChild(0).localScale.y / 2, 0);
            uRoot.localEulerAngles = Vector3.zero;
            GenerateUColumn("rearLeft");
            GenerateUColumn("rearRight");
            GenerateUColumn("frontLeft");
            GenerateUColumn("frontRight");
        }
    }

    ///<summary>
    /// Instantiate one GameManager.uLocationModel per U in the given column
    ///</summary>
    ///<param name="_corner">Corner of the column</param>
    private void GenerateUColumn(string _corner)
    {
        Vector3 boxSize = transform.GetChild(0).localScale;
        float scale = GameManager.gm.uSize;
        if (attributes["heightUnit"] == "OU")
            scale = GameManager.gm.ouSize;

        int max = (int)Utils.ParseDecFrac(attributes["height"]);
        if (GetComponentInChildren<Slot>())
        {
            max = 0;
            Slot[] allSlots = GetComponentsInChildren<Slot>();
            foreach (Slot s in allSlots)
            {
                if (s.orient == "horizontal")
                    max++;
            }

            Transform slot = GetComponentInChildren<Slot>().transform;
            uRoot.localPosition = new Vector3(uRoot.localPosition.x, slot.localPosition.y, uRoot.localPosition.z);
            uRoot.localPosition -= new Vector3(0, slot.GetChild(0).localScale.y / 2, 0);
        }

        for (int i = 1; i <= max; i++)
        {
            Transform obj = Instantiate(GameManager.gm.uLocationModel).transform;
            obj.name = $"{_corner}_u{i}";
            obj.GetComponentInChildren<TextMeshPro>().text = i.ToString();
            obj.parent = uRoot;
            obj.localScale = Vector3.one * scale;
            switch (_corner)
            {
                case "rearLeft":
                    obj.localPosition = new Vector3(-boxSize.x / 2, i * scale - scale / 2, -boxSize.z / 2);
                    obj.localEulerAngles = new Vector3(0, 0, 0);
                    obj.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case "rearRight":
                    obj.localPosition = new Vector3(boxSize.x / 2, i * scale - scale / 2, -boxSize.z / 2);
                    obj.localEulerAngles = new Vector3(0, 0, 0);
                    obj.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case "frontLeft":
                    obj.localPosition = new Vector3(-boxSize.x / 2, i * scale - scale / 2, boxSize.z / 2);
                    obj.localEulerAngles = new Vector3(0, 180, 0);
                    obj.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case "frontRight":
                    obj.localPosition = new Vector3(boxSize.x / 2, i * scale - scale / 2, boxSize.z / 2);
                    obj.localEulerAngles = new Vector3(0, 180, 0);
                    obj.GetComponent<Renderer>().material.color = Color.green;
                    break;
            }
        }
    }

    ///<summary>
    /// Coroutine: enable Rack's Collider after finish move (end of next frame)
    ///</summary>
    private IEnumerator ReactiveCollider()
    {
        yield return new WaitForEndOfFrame(); // end of current frame
        yield return new WaitForEndOfFrame(); // end of next frame
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = true;
        }
        Utils.SwitchAllCollidersInRacks(false);
    }
}
