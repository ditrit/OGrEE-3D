using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rack : OObject
{
    private Vector3 originalLocalPos;
    private Vector2 originalPosXY;
    private Transform uRoot;

    private void OnEnable()
    {
        originalLocalPos = transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rack>() && transform.localPosition != originalLocalPos)
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
        bool updateAttr = false;
        if (_param.StartsWith("description"))
        {
            SetDescription(_param.Substring(11), _value);
            updateAttr = true;
        }
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
                    if (_value.EndsWith("@recursive"))
                    {
                        string[] data = _value.Split('@');
                        SetAllDomains(data[0]);
                    }
                    else
                    {
                        SetDomain(_value);
                        UpdateColorByTenant();
                    }
                    updateAttr = true;
                    break;
                case "color":
                    SetColor(_value);
                    updateAttr = true;
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
                case "temperature":
                    SetTemperature(_value);
                    break;
                default:
                    if (attributes.ContainsKey(_param))
                        attributes[_param] = _value;
                    else
                        attributes.Add(_param, _value);
                    updateAttr = true;
                    break;
            }
        }
        if (updateAttr && ApiManager.instance.isInit)
            PutData();
        GetComponent<DisplayObjectData>().UpdateLabels();
    }

    ///<summary>
    /// Called by MoveObject: Move the rack in its room's orientation.
    ///</summary>
    ///<param name="_v">The translation vector</param>
    public void DragRack(Vector2 _v)
    {
        Utils.SwitchAllCollidersInRacks(true);

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
            case "+E+N":
                transform.localPosition += new Vector3(_v.x, 0, _v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.x, _v.y);
                break;
            case "+N+W":
                transform.localPosition += new Vector3(_v.y, 0, -_v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.y, -_v.x);
                break;
            case "+W+S":
                transform.localPosition += new Vector3(-_v.x, 0, -_v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.x, -_v.y);
                break;
            case "+S+E":
                transform.localPosition += new Vector3(-_v.y, 0, _v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.y, _v.x);
                break;
        }
        attributes["posXY"] = JsonUtility.ToJson(posXY);
        StartCoroutine(ReactiveCollider());
    }

    ///<summary>
    /// Move the rack in its room's orientation.
    ///</summary>
    ///<param name="_v">The translation vector</param>
    ///<param name="_isRelative">If true, _v is a relative vector</param>
    public void MoveRack(Vector2 _v, bool _isRelative)
    {
        Utils.SwitchAllCollidersInRacks(true);

        originalLocalPos = transform.localPosition;
        Vector2 posXY = JsonUtility.FromJson<Vector2>(attributes["posXY"]);
        originalPosXY = posXY;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = false;
        }

        if (_isRelative)
        {
            transform.localPosition += new Vector3(_v.x, 0, _v.y) * GameManager.gm.tileSize;
            attributes["posXY"] = JsonUtility.ToJson(originalPosXY + _v);
        }
        else
        {
            transform.localPosition -= new Vector3(posXY.x, 0, posXY.y) * GameManager.gm.tileSize;
            transform.localPosition += new Vector3(_v.x, 0, _v.y) * GameManager.gm.tileSize;
            attributes["posXY"] = JsonUtility.ToJson(_v);
        }

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
        else if (_value == "true" && !uRoot)
            GenerateUHelpers();
        else if (_value == "false" && uRoot)
            Destroy(uRoot.gameObject);
    }
    public void ToggleU()
    {
        if (uRoot)
            Destroy(uRoot.gameObject);
        else
            GenerateUHelpers();
    }

    ///<summary>
    /// Create uRoot, place it and call GenerateUColumn() for each corner
    ///</summary>
    private void GenerateUHelpers()
    {
        Vector3 rootPos;
        Transform box = transform.GetChild(0);
        if (box.childCount == 0)
            rootPos = new Vector3(0, box.localScale.y / -2, 0);
        else
            rootPos = new Vector3(0, box.GetComponent<BoxCollider>().size.y / -2, 0);

        uRoot = new GameObject("uRoot").transform;
        uRoot.parent = transform;
        uRoot.localPosition = rootPos;
        uRoot.localEulerAngles = Vector3.zero;
        GenerateUColumn("rearLeft");
        GenerateUColumn("rearRight");
        GenerateUColumn("frontLeft");
        GenerateUColumn("frontRight");
    }

    ///<summary>
    /// Instantiate one GameManager.uLocationModel per U in the given column
    ///</summary>
    ///<param name="_corner">Corner of the column</param>
    private void GenerateUColumn(string _corner)
    {
        Vector3 boxSize = transform.GetChild(0).localScale;

        // By defalut, attributes["heightUnit"] == "U"
        float scale = GameManager.gm.uSize;
        int max = (int)Utils.ParseDecFrac(attributes["height"]);
        if (attributes["heightUnit"] == "OU")
        {
            scale = GameManager.gm.ouSize;
            max = (int)Utils.ParseDecFrac(attributes["height"]);
        }
        else if (attributes["heightUnit"] == "cm")
        {
            scale = GameManager.gm.uSize;
            max = Mathf.FloorToInt(Utils.ParseDecFrac(attributes["height"]) / (GameManager.gm.uSize * 100));
        }

        if (!string.IsNullOrEmpty(attributes["template"]))
        {
            Transform firstSlot = null;
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Slot>())
                {
                    firstSlot = child;
                    break;
                }
            }
            if (firstSlot)
            {
                uRoot.localPosition = new Vector3(uRoot.localPosition.x, firstSlot.localPosition.y, uRoot.localPosition.z);
                uRoot.localPosition -= new Vector3(0, firstSlot.GetChild(0).localScale.y / 2, 0);
            }
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
