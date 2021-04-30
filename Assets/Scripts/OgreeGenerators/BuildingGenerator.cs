using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public static BuildingGenerator instance;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    ///<summary>
    /// Instantiate a buildingModel (from GameManager) and apply the given data to it.
    ///</summary>
    ///<param name="_bd">The building data to apply</param>
    ///<param name="_parent">The parent of the created building. Leave null if _bd contains the parendId</param>
    ///<returns>The created Building</returns>
    public Building CreateBuilding(SApiObject _bd, Transform _parent = null)
    {
        Transform si = Utils.FindParent(_parent, _bd.parentId);
        if (!si || si.GetComponent<OgreeObject>().category != "site")
        {
            GameManager.gm.AppendLogLine($"Parent site not found", "red");
            return null;
        }
        string hierarchyName = $"{si.GetComponent<OgreeObject>().hierarchyName}.{_bd.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        // Position and size data from _bd.attributes
        Vector2 posXY = JsonUtility.FromJson<Vector2>(_bd.attributes["posXY"]);
        float posZ = float.Parse(_bd.attributes["posZ"]);
        Vector2 size = JsonUtility.FromJson<Vector2>(_bd.attributes["size"]);
        float height = float.Parse(_bd.attributes["height"]);

        GameObject newBD = Instantiate(GameManager.gm.buildingModel);
        newBD.name = _bd.name;
        newBD.transform.parent = si;
        newBD.transform.localEulerAngles = Vector3.zero;

        // originalSize
        Vector3 originalSize = newBD.transform.GetChild(0).localScale;
        newBD.transform.GetChild(0).localScale = new Vector3(originalSize.x * size.x, originalSize.y, originalSize.z * size.y);

        Vector3 origin = newBD.transform.GetChild(0).localScale / 0.2f;
        newBD.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        newBD.transform.localPosition += new Vector3(posXY.x, posXY.y, posZ);

        Building building = newBD.GetComponent<Building>();
        building.UpdateFromSApiObject(_bd);

        BuildWalls(building.walls, new Vector3(newBD.transform.GetChild(0).localScale.x * 10, height, newBD.transform.GetChild(0).localScale.z * 10), 0);

        string hn = building.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newBD);

        return building;
    }

    ///<summary>
    /// Instantiate a roomModel (from GameManager) and apply given data to it.
    ///</summary>
    ///<param name="_ro">The room data to apply</param>    
    ///<param name="_parent">The parent of the created room. Leave null if _bd contains the parendId</param>
    ///<returns>The created Room</returns>
    public Room CreateRoom(SApiObject _ro, Transform _parent = null)
    {
        Transform bd = Utils.FindParent(_parent, _ro.parentId);
        if (!bd || bd.GetComponent<OgreeObject>().category != "building")
        {
            GameManager.gm.AppendLogLine($"Parent building not found", "red");
            return null;
        }
        string hierarchyName = $"{bd.GetComponent<OgreeObject>().hierarchyName}.{_ro.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        // Position and size data from _ro.attributes
        Vector2 posXY = JsonUtility.FromJson<Vector2>(_ro.attributes["posXY"]);
        float posZ = float.Parse(_ro.attributes["posZ"]);
        Vector2 size = JsonUtility.FromJson<Vector2>(_ro.attributes["size"]);
        float height = float.Parse(_ro.attributes["height"]);

        GameObject newRoom = Instantiate(GameManager.gm.roomModel);
        newRoom.name = _ro.name;
        newRoom.transform.parent = bd;

        Room room = newRoom.GetComponent<Room>();
        room.UpdateFromSApiObject(_ro);

        Vector3 originalSize = room.usableZone.localScale;
        room.usableZone.localScale = new Vector3(originalSize.x * size.x, originalSize.y, originalSize.z * size.y);
        room.reservedZone.localScale = room.usableZone.localScale;
        room.technicalZone.localScale = room.usableZone.localScale;
        room.tilesEdges.localScale = room.usableZone.localScale;
        room.tilesEdges.GetComponent<Renderer>().material.mainTextureScale = size / 0.6f;
        room.tilesEdges.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(size.x / 0.6f % 1, size.y / 0.6f % 1);
        BuildWalls(room.walls, new Vector3(room.usableZone.localScale.x * 10, height, room.usableZone.localScale.z * 10), -0.001f);

        Vector3 bdOrigin = bd.GetChild(0).localScale / -0.2f;
        Vector3 roOrigin = room.usableZone.localScale / 0.2f;
        newRoom.transform.position = bd.position;
        newRoom.transform.localPosition += new Vector3(bdOrigin.x, 0, bdOrigin.z);
        newRoom.transform.localPosition += new Vector3(posXY.x, posXY.y, posZ);

        if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)E(\\+|\\-)N"))
        {
            newRoom.transform.eulerAngles = new Vector3(0, 0, 0);
            newRoom.transform.position += new Vector3(roOrigin.x, 0, roOrigin.z);
        }
        else if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)W(\\+|\\-)S"))
        {
            newRoom.transform.eulerAngles = new Vector3(0, 180, 0);
            newRoom.transform.position += new Vector3(-roOrigin.x, 0, -roOrigin.z);
        }
        else if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)N(\\+|\\-)W"))
        {
            newRoom.transform.eulerAngles = new Vector3(0, -90, 0);
            newRoom.transform.position += new Vector3(-roOrigin.z, 0, roOrigin.x);
        }
        else if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)S(\\+|\\-)E"))
        {
            newRoom.transform.eulerAngles = new Vector3(0, 90, 0);
            newRoom.transform.position += new Vector3(roOrigin.z, 0, -roOrigin.x);
        }

        // Set UI room's name
        room.nameText.text = newRoom.name;
        room.nameText.rectTransform.sizeDelta = size;

        // Add room to GUI room filter
        Filters.instance.AddIfUnknown(Filters.instance.roomsList, newRoom.name);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRooms, Filters.instance.roomsList);

        room.UpdateZonesColor();

        string hn = room.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, newRoom);

        if (_ro.attributes.ContainsKey("reserved") && _ro.attributes.ContainsKey("technical")
            && !string.IsNullOrEmpty(_ro.attributes["reserved"]) && !string.IsNullOrEmpty(_ro.attributes["technical"]))
        {
            SMargin reserved = JsonUtility.FromJson<SMargin>(_ro.attributes["reserved"]);
            SMargin technical = JsonUtility.FromJson<SMargin>(_ro.attributes["technical"]);
            room.SetAreas(reserved, technical);
        }

        if (!string.IsNullOrEmpty(_ro.attributes["template"]) && GameManager.gm.roomTemplates.ContainsKey(_ro.attributes["template"]))
        {
            ReadFromJson.SRoomFromJson template = GameManager.gm.roomTemplates[_ro.attributes["template"]];
            room.SetAreas(new SMargin(template.reservedArea), new SMargin(template.technicalArea));

            foreach (ReadFromJson.SSeparator sep in template.separators)
                CreateSeparatorFromJson(sep, newRoom.transform);
        }

        return room;
    }

    ///<summary>
    /// Instantiate a separatorModel (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_sp">Informations about the separator</param>
    ///<param name="_parent">The parent of the created separator. Leave null if _bd contains the parendId</param>
    public OgreeObject CreateSeparator(SApiObject _sp, Transform _parent = null)
    {
        Transform parent = Utils.FindParent(_parent, _sp.parentId);
        if (!parent || parent.GetComponent<OgreeObject>().category != "room")
        {
            GameManager.gm.AppendLogLine($"Parent room not found", "red");
            return null;
        }
        string hierarchyName = $"{parent.GetComponent<OgreeObject>().hierarchyName}.{_sp.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        // pos1XYm = startPos - pos2XYm = endPos
        Vector2 pos1 = JsonUtility.FromJson<Vector2>(_sp.attributes["startPos"]);
        Vector2 pos2 = JsonUtility.FromJson<Vector2>(_sp.attributes["endPos"]);

        float length = Vector2.Distance(pos1, pos2);
        float height = parent.GetComponent<Room>().walls.GetChild(0).localScale.y;
        float angle = Vector3.SignedAngle(Vector3.right, pos2 - pos1, Vector3.up);
        // Debug.Log($"[{_sp.name}]=> {angle}");

        GameObject separator = Instantiate(GameManager.gm.separatorModel);
        separator.name = _sp.name;
        separator.transform.parent = parent;

        // Set textured box
        separator.transform.GetChild(0).localScale = new Vector3(length, height, 0.001f);
        separator.transform.GetChild(0).localPosition = new Vector3(length, height, 0) / 2;
        Renderer rend = separator.transform.GetChild(0).GetComponent<Renderer>();
        rend.material.mainTextureScale = new Vector2(length, height) * 1.5f;

        // Place the separator in the right place
        Vector3 roomScale = parent.GetComponent<Room>().technicalZone.localScale * -5;
        separator.transform.localPosition = new Vector3(roomScale.x, 0, roomScale.z);
        separator.transform.localPosition += new Vector3(pos1.x, 0, pos1.y);
        separator.transform.localEulerAngles = new Vector3(0, -angle, 0);

        OgreeObject sp = separator.GetComponent<OgreeObject>();
        sp.UpdateFromSApiObject(_sp);

        string hn = sp.UpdateHierarchyName();
        GameManager.gm.allItems.Add(hn, separator);

        return sp;
    }

    ///<summary>
    /// Set walls children of _root. They have to be in Front/Back/Right/Left order.
    ///</summary>
    ///<param name="_root">The root of walls.</param>
    ///<param name="_dim">The dimensions of the building/room</param>
    ///<param name="_offset">The offset for avoiding walls overlaping</param>
    private void BuildWalls(Transform _root, Vector3 _dim, float _offset)
    {
        Transform wallFront = _root.GetChild(0);
        Transform wallBack = _root.GetChild(1);
        Transform wallRight = _root.GetChild(2);
        Transform wallLeft = _root.GetChild(3);

        wallFront.localScale = new Vector3(_dim.x, _dim.y, 0.01f);
        wallBack.localScale = new Vector3(_dim.x, _dim.y, 0.01f);
        wallRight.localScale = new Vector3(_dim.z, _dim.y, 0.01f);
        wallLeft.localScale = new Vector3(_dim.z, _dim.y, 0.01f);

        wallFront.localPosition = new Vector3(0, wallFront.localScale.y / 2, _dim.z / 2 + _offset);
        wallBack.localPosition = new Vector3(0, wallFront.localScale.y / 2, -(_dim.z / 2 + _offset));
        wallRight.localPosition = new Vector3(_dim.x / 2 + _offset, wallFront.localScale.y / 2, 0);
        wallLeft.localPosition = new Vector3(-(_dim.x / 2 + _offset), wallFront.localScale.y / 2, 0);
    }

    ///<summary>
    /// Convert ReadFromJson.SSeparator to SSeparatorInfos and call CreateSeparator().
    ///</summary>
    ///<param name="_sepData">Data from json</param>
    ///<param name="_root">The room of the separator</param>
    private void CreateSeparatorFromJson(ReadFromJson.SSeparator _sepData, Transform _root)
    {
        SApiObject sp = new SApiObject();
        sp.description = new List<string>();
        sp.attributes = new Dictionary<string, string>();

        sp.name = _sepData.name;
        Vector2 pos1 = new Vector2(_sepData.pos1XYm[0], _sepData.pos1XYm[1]);
        sp.attributes["startPos"] = JsonUtility.ToJson(pos1);
        Vector2 pos2 = new Vector2(_sepData.pos2XYm[0], _sepData.pos2XYm[1]);
        sp.attributes["endPos"] = JsonUtility.ToJson(pos2);

        BuildingGenerator.instance.CreateSeparator(sp, _root);
    }
}
