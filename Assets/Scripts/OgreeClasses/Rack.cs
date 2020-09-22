using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rack : Object
{
    private Vector3 originalLocalPos;
    private Vector3 originalPosXY;
    private Transform uRoot;

    public Rack()
    {
        family = EObjFamily.rack;
    }

    // protected override void OnDestroy()
    // {
    //     base.OnDestroy();
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rack>() && GameManager.gm.currentItems.Contains(gameObject))
        {
            // Debug.Log($"{name}.OnTriggerEnter() with {other.name}");
            GameManager.gm.AppendLogLine($"Cannot move {name}, it will overlap {other.name}", "yellow");
            transform.localPosition = originalLocalPos;
            posXY = originalPosXY;
        }
    }

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public override void SetAttribute(string _param, string _value)
    {
        switch (_param)
        {
            case "description":
                description = _value;
                break;
            case "vendor":
                vendor = _value;
                break;
            case "type":
                type = _value;
                break;
            case "model":
                model = _value;
                break;
            case "serial":
                serial = _value;
                break;
            case "tenant":
                AssignTenant(_value);
                break;
            case "alpha":
                UpdateAlpha(_value);
                break;
            case "displayU":
                ToggleU(_value);
                break;
            default:
                GameManager.gm.AppendLogLine($"[Rack] {name}: unknowed attribute to update.", "yellow");
                break;
        }
    }

    ///<summary>
    /// Update rack's color according to its Tenant.
    ///</summary>
    public void UpdateColor()
    {
        if (tenant == null)
            return;

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        ColorUtility.TryParseHtmlString(tenant.color, out myColor);
        mat.color = new Color(myColor.r, myColor.g, myColor.b, mat.color.a);
    }

    ///<summary>
    /// Move the rack in its room's orientation.
    ///</summary>
    ///<param name="_v">The translation vector</param>
    public void MoveRack(Vector2 _v)
    {
        originalLocalPos = transform.localPosition;
        originalPosXY = posXY;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = false;
        }

        Room room = transform.parent.GetComponent<Room>();
        switch (room.orientation)
        {
            case EOrientation.N:
                transform.localPosition += new Vector3(_v.x, 0, _v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.x, _v.y);
                break;
            case EOrientation.W:
                transform.localPosition += new Vector3(_v.y, 0, -_v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.y, -_v.x);
                break;
            case EOrientation.S:
                transform.localPosition += new Vector3(-_v.x, 0, -_v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.x, -_v.y);
                break;
            case EOrientation.E:
                transform.localPosition += new Vector3(-_v.y, 0, _v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.y, _v.x);
                break;
        }
        StartCoroutine(ReactiveCollider());
    }

    ///<summary>
    /// Toggle U location cubes.
    ///</summary>
    ///<param name="_value">True or false value</param>
    private void ToggleU(string _value)
    {
        if (_value != "true" && _value != "false")
        {
            GameManager.gm.AppendLogLine("displayU value has to be true or false", "yellow");
            return;
        }
        else if (_value == "true")
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
        else
        {
            Destroy(uRoot.gameObject);
        }
    }

    ///<summary>
    /// Instantiate one GameManager.uLocationModel per U in the given column
    ///</summary>
    ///<param name="_corner">Corner of the column</param>
    private void GenerateUColumn(string _corner)
    {
        float scale = GameManager.gm.uSize;
        if (heightUnit == EUnit.OU)
            scale = GameManager.gm.ouSize;

        for (int i = 1; i <= height; i++)
        {
            Transform obj = Instantiate(GameManager.gm.uLocationModel).transform;
            obj.name = $"{_corner}_u{i}";
            obj.GetComponentInChildren<TextMeshPro>().text = i.ToString();
            obj.parent = uRoot;
            obj.localScale = Vector3.one * scale;
            switch (_corner)
            {
                case "rearLeft":
                    obj.localPosition = new Vector3(-transform.GetChild(0).localScale.x / 2,
                                                    i * scale - scale / 2,
                                                    -transform.GetChild(0).localScale.z / 2);
                    obj.localEulerAngles = new Vector3(0, 0, 0);
                    obj.GetComponent<Renderer>().material.color = Color.red;
                    break;
                case "rearRight":
                    obj.localPosition = new Vector3(transform.GetChild(0).localScale.x / 2,
                                                    i * scale - scale / 2,
                                                    -transform.GetChild(0).localScale.z / 2);
                    obj.localEulerAngles = new Vector3(0, 0, 0);
                    obj.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case "frontLeft":
                    obj.localPosition = new Vector3(-transform.GetChild(0).localScale.x / 2,
                                                    i * scale - scale / 2,
                                                    transform.GetChild(0).localScale.z / 2);
                    obj.localEulerAngles = new Vector3(0, 180, 0);
                    obj.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case "frontRight":
                    obj.localPosition = new Vector3(transform.GetChild(0).localScale.x / 2,
                                                    i * scale - scale / 2,
                                                    transform.GetChild(0).localScale.z / 2);
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
        // yield return new WaitForSeconds(1);
        yield return new WaitForEndOfFrame(); // end of current frame
        yield return new WaitForEndOfFrame(); // end of next frame
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = true;
        }
    }
}
