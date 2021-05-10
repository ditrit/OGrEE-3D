using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TriLibCore;
using TriLibCore.General;
using UnityEngine;
using UnityEngine.Networking;

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
            OObject[] components = rack.transform.GetComponentsInChildren<OObject>();
            foreach (OObject comp in components)
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

    ///<summary>
    /// Replace the box GameObject by the given fbx model 
    ///</summary>
    ///<param name="_object">The Object to update</param>
    ///<param name="_modelPath">The path of the 3D model to load with TriLib</param>
    public void ReplaceBox(GameObject _object, string _modelPath)
    {
        Destroy(_object.transform.GetChild(0).gameObject);

        AssetLoaderOptions assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
        assetLoaderOptions.ScaleFactor = 0.1f;
        // BoxCollider added later
        assetLoaderOptions.GenerateColliders = false;
        assetLoaderOptions.ConvexColliders = false;

        UnityWebRequest webRequest = AssetDownloader.CreateWebRequest(_modelPath);
        AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError,
                                            _object, assetLoaderOptions, null, "fbx");

    }

    // This event is called when the model loading progress changes.
    // You can use this event to update a loading progress-bar, for instance.
    // The "progress" value comes as a normalized float (goes from 0 to 1).
    // Platforms like UWP and WebGL don't call this method at this moment, since they don't use threads.
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {

    }

    // This event is called when there is any critical error loading your model.
    // You can use this to show a message to the user.
    private void OnError(IContextualizedError contextualizedError)
    {
        Debug.Log("TriLib Error: " + contextualizedError);
    }

    // This event is called when all model GameObjects and Meshes have been loaded.
    // There may still Materials and Textures processing at this stage.
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Transform triLibWrapper = assetLoaderContext.RootGameObject.transform;
        Transform triLibObj = triLibWrapper.GetChild(0);

        triLibWrapper.name = $"Wraper_{triLibObj.name}";
        triLibWrapper.localPosition = Vector3.zero;

        triLibObj.parent = triLibWrapper.parent;
        triLibWrapper.parent = triLibObj;
        triLibObj.SetAsFirstSibling();
        triLibObj.localPosition = Vector3.zero;
        triLibWrapper.localPosition = Vector3.zero;

        cakeslice.Outline ol = triLibObj.gameObject.AddComponent<cakeslice.Outline>();
        ol.enabled = false;
        triLibObj.gameObject.AddComponent<BoxCollider>();

        triLibObj.tag = "Selectable";

    }

    // This event is called after OnLoad when all Materials and Textures have been loaded.
    // This event is also called after a critical loading error, so you can clean up any resource you want to.
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {

    }

}
