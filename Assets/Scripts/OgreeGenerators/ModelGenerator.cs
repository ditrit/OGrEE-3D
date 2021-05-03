using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ModelGenerator : MonoBehaviour
{
    public static ModelGenerator instance;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    ///<summary>
    /// Instantiate a rackTemplate (from GameManager) and apply the given data to it.
    ///</summary>
    ///<param name="_rk">THe rack data to apply</param>
    ///<param name="_parent">The parent of the created rack. Leave null if _bd contains the parendId</param>
    ///<param name="_copyAttr">If false, do not copy all attributes</param>
    ///<returns>The created Rack</returns>
    public Rack InstantiateModel(SApiObject _rk, Transform _parent = null, bool _copyAttr = true)
    {
        Transform parent = Utils.FindParent(_parent, _rk.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_rk.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newRack;
        if (GameManager.gm.rackTemplates.ContainsKey(_rk.attributes["template"]))
            newRack = Instantiate(GameManager.gm.rackTemplates[_rk.attributes["template"]]);
        else
        {
            GameManager.gm.AppendLogLine($"Unknown template \"{_rk.attributes["template"]}\"", "yellow");
            return null;
        }
        Renderer[] renderers = newRack.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = true;

        newRack.name = _rk.name;
        newRack.transform.parent = parent;

        Vector2 pos = JsonUtility.FromJson<Vector2>(_rk.attributes["posXY"]);
        Vector3 origin = parent.GetChild(0).localScale / 0.2f;
        // Vector3 boxOrigin = newRack.transform.GetChild(0).localScale / 2;
        Vector3 boxOrigin = newRack.transform.GetChild(0).GetComponent<BoxCollider>().size / 2;
        newRack.transform.position = parent.GetChild(0).position;

        Vector2 orient = Vector2.one;
        if (parent.GetComponent<Room>().attributes.ContainsKey("orientation"))
        {
            if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\+[ENSW]{1}\\+[ENSW]{1}$"))
            {
                // Lower Left corner of the room
                orient = new Vector2(1, 1);
            }
            else if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\-[ENSW]{1}\\+[ENSW]{1}$"))
            {
                // Lower Right corner of the room
                orient = new Vector2(-1, 1);
                newRack.transform.localPosition -= new Vector3(GameManager.gm.tileSize, 0, 0);
            }
            else if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\-[ENSW]{1}\\-[ENSW]{1}$"))
            {
                // Upper Right corner of the room
                orient = new Vector2(-1, -1);
                newRack.transform.localPosition -= new Vector3(GameManager.gm.tileSize, 0, GameManager.gm.tileSize);
            }
            else if (Regex.IsMatch(parent.GetComponent<Room>().attributes["orientation"], "\\+[ENSW]{1}\\-[ENSW]{1}$"))
            {
                // Upper Left corner of the room
                orient = new Vector2(1, -1);
                newRack.transform.localPosition -= new Vector3(0, 0, GameManager.gm.tileSize);
            }
        }
        newRack.transform.localPosition += new Vector3(origin.x * -orient.x, 0, origin.z * -orient.y);
        newRack.transform.localPosition += new Vector3(pos.x * orient.x, 0, pos.y * orient.y) * GameManager.gm.tileSize;

        Rack rack = newRack.GetComponent<Rack>();
        rack.UpdateFromSApiObject(_rk, _copyAttr);
        rack.attributes["template"] = _rk.attributes["template"];
        rack.attributes["posXY"] = _rk.attributes["posXY"];
        rack.attributes["posXYUnit"] = _rk.attributes["posXYUnit"];
        rack.attributes["orientation"] = _rk.attributes["orientation"];

        Vector3 fixPos = Vector3.zero;
        switch (rack.attributes["orientation"])
        {
            case "front":
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                if (orient.y == 1)
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                else
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z + GameManager.gm.tileSize);
                break;
            case "rear":
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                if (orient.y == 1)
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z + GameManager.gm.tileSize);
                else
                    fixPos = new Vector3(boxOrigin.x, boxOrigin.y, boxOrigin.z);
                break;
            case "left":
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                if (orient.x == 1)
                    fixPos = new Vector3(-boxOrigin.z + GameManager.gm.tileSize, boxOrigin.y, boxOrigin.x);
                else
                    fixPos = new Vector3(boxOrigin.z, boxOrigin.y, boxOrigin.x);
                break;
            case "right":
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                if (orient.x == 1)
                    fixPos = new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x + GameManager.gm.tileSize);
                else
                    fixPos = new Vector3(-boxOrigin.z + GameManager.gm.tileSize, boxOrigin.y, -boxOrigin.x + GameManager.gm.tileSize);
                break;
        }
        newRack.transform.localPosition += fixPos;

        newRack.GetComponent<DisplayObjectData>().PlaceTexts("frontrear", true);
        newRack.GetComponent<DisplayObjectData>().SetLabel("#name");

        rack.UpdateColor();
        // GameManager.gm.SetRackMaterial(newRack.transform);

        string hn = rack.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
        {
            Object[] components = rack.transform.GetComponentsInChildren<Object>();
            foreach (Object comp in components)
            {
                if (comp.gameObject != rack.gameObject)
                {
                    string compHn = comp.UpdateHierarchyName();
                    GameManager.gm.allItems.Add(compHn, comp.gameObject);
                }
            }
        }

        return rack;
    }
}
