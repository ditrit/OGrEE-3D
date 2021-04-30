using System.Collections;
using System.Collections.Generic;
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
    ///<returns>The created Rack</returns>
    public Rack InstantiateModel(SApiObject _rk, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _rk.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }

        string hierarchyName = $"{parent.GetComponent<HierarchyName>()?.fullname}.{_rk.name}";
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
        Destroy(newRack.GetComponent<HierarchyName>());

        newRack.name = _rk.name;
        newRack.transform.parent = parent;

        Vector2 pos = JsonUtility.FromJson<Vector2>(_rk.attributes["posXY"]);
        Vector3 origin = newRack.transform.parent.GetChild(0).localScale / -0.2f;
        // Vector3 boxOrigin = newRack.transform.GetChild(0).localScale / 2;
        Vector3 boxOrigin = newRack.transform.GetChild(0).GetComponent<BoxCollider>().size / 2;
        newRack.transform.position = newRack.transform.parent.GetChild(0).position;
        newRack.transform.localPosition += new Vector3(origin.x, 0, origin.z);
        newRack.transform.localPosition += new Vector3(pos.x, 0, pos.y) * GameManager.gm.tileSize;

        Rack rack = newRack.GetComponent<Rack>();
        rack.name = newRack.name;
        rack.id = _rk.id;
        rack.parentId = _rk.parentId;
        if (string.IsNullOrEmpty(rack.parentId))
            rack.parentId = parent.GetComponent<OgreeObject>().id;
        rack.category = "rack";
        rack.description = _rk.description;
        rack.domain = _rk.domain;
        if (string.IsNullOrEmpty(rack.domain))
            rack.domain = parent.GetComponent<OgreeObject>().domain;
        rack.attributes["template"] = _rk.attributes["template"];
        rack.attributes["posXY"] = _rk.attributes["posXY"];
        rack.attributes["posXYUnit"] = _rk.attributes["posXYUnit"];
        rack.attributes["orientation"] = _rk.attributes["orientation"];

        switch (rack.attributes["orientation"])
        {
            case "front":
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                newRack.transform.localPosition += boxOrigin;
                break;
            case "rear":
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                newRack.transform.localPosition += new Vector3(boxOrigin.x, boxOrigin.y, -boxOrigin.z);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
            case "left":
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                newRack.transform.localPosition += new Vector3(-boxOrigin.z, boxOrigin.y, boxOrigin.x);
                newRack.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0, 0);
                break;
            case "right":
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                newRack.transform.localPosition += new Vector3(boxOrigin.z, boxOrigin.y, -boxOrigin.x);
                newRack.transform.localPosition += new Vector3(0, 0, GameManager.gm.tileSize);
                break;
        }

        newRack.GetComponent<DisplayObjectData>().PlaceTexts("frontrear", true);
        newRack.GetComponent<DisplayObjectData>().SetLabel("name");

        rack.UpdateColor();
        // GameManager.gm.SetRackMaterial(newRack.transform);

        string hn = newRack.AddComponent<HierarchyName>().fullname;
        GameManager.gm.allItems.Add(hn, newRack);

        if (!string.IsNullOrEmpty(rack.attributes["template"]))
        {
            HierarchyName[] components = rack.transform.GetComponentsInChildren<HierarchyName>();
            foreach (HierarchyName comp in components)
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
