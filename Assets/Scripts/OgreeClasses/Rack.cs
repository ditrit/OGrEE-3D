using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Rack : OObject
{
    private Vector3 originalLocalPos;
    private Vector2 originalPosXY;
    public Transform uRoot;
    public GameObject gridForULocation;

    private void OnEnable()
    {
        originalLocalPos = transform.localPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rack>() && transform.localPosition != originalLocalPos)
        {
            GameManager.gm.AppendLogLine($"Cannot move {name}, it will overlap {other.name}", false, eLogtype.warning);
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
                    if (_value == "true")
                    {
                        UHelpersManager.um.ToggleU(transform, true);
                        GameManager.gm.AppendLogLine($"U helpers ON for {name}.", false, eLogtype.info);
                    }
                    else if (_value == "false")
                    {
                        UHelpersManager.um.ToggleU(transform, false);
                        GameManager.gm.AppendLogLine($"U helpers OFF for {name}.", false, eLogtype.info);
                    }
                    break;
                case "temperature":
                    SetTemperature(_value);
                    updateAttr = true;
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

