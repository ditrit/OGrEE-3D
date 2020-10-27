using System.Collections;
using System.Collections.Generic;
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
    /// Instantiate a buildingModel (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the building</param>
    ///<returns>The created Building</returns>
    public Building CreateBuilding(SBuildingInfos _data)
    {
        if (_data.parent.GetComponent<Datacenter>() == null)
        {
            GameManager.gm.AppendLogLine("Building must be child of a datacenter", "yellow");
            return null;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newBD = Instantiate(GameManager.gm.buildingModel);
        newBD.name = _data.name;
        newBD.transform.parent = _data.parent;
        newBD.transform.localEulerAngles = Vector3.zero;

        // originalSize
        Vector3 originalSize = newBD.transform.GetChild(0).localScale;
        newBD.transform.GetChild(0).localScale = new Vector3(originalSize.x * _data.size.x, originalSize.y, originalSize.z * _data.size.z);

        Vector3 origin = newBD.transform.GetChild(0).localScale / 0.2f;
        newBD.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        newBD.transform.localPosition += new Vector3(_data.pos.x, 0, _data.pos.z);

        Building bd = newBD.GetComponent<Building>();
        BuildWalls(bd.walls, new Vector3(newBD.transform.GetChild(0).localScale.x * 10, _data.size.y, newBD.transform.GetChild(0).localScale.z * 10), 0);
        bd.posXY = new Vector2(_data.pos.x, _data.pos.y);
        bd.posXYUnit = EUnit.m;
        bd.posZ = _data.pos.z;
        bd.posZUnit = EUnit.m;
        bd.size = new Vector2(_data.size.x, _data.size.z);
        bd.sizeUnit = EUnit.m;
        bd.height = _data.size.y;
        bd.heightUnit = EUnit.m;

        newBD.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(hierarchyName, newBD);

        return bd;
    }

    ///<summary>
    /// Instantiate a roomModel (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the room</param>
    ///<returns>The created Room</returns>
    public Room CreateRoom(SRoomInfos _data)
    {
        if (_data.parent.GetComponent<Building>() == null || _data.parent.GetComponent<Room>())
        {
            GameManager.gm.AppendLogLine("Room must be child of a Building", "yellow");
            return null;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return null;
        }

        GameObject newRoom = Instantiate(GameManager.gm.roomModel);
        newRoom.name = _data.name;
        newRoom.transform.parent = _data.parent;

        Vector3 size;
        string orient;
        if (string.IsNullOrEmpty(_data.template))
        {
            size = _data.size;
            orient = _data.orient;
        }
        else if (GameManager.gm.roomTemplates.ContainsKey(_data.template))
        {
            size = new Vector3(GameManager.gm.roomTemplates[_data.template].sizeWDHm[0],
                            GameManager.gm.roomTemplates[_data.template].sizeWDHm[2],
                            GameManager.gm.roomTemplates[_data.template].sizeWDHm[1]);
            orient = GameManager.gm.roomTemplates[_data.template].orientation;
        }
        else
        {
            GameManager.gm.AppendLogLine($"Unknown template \"{_data.template}\"", "yellow");
            return null;
        }

        Room room = newRoom.GetComponent<Room>();

        Vector3 originalSize = room.usableZone.localScale;
        room.usableZone.localScale = new Vector3(originalSize.x * size.x, originalSize.y, originalSize.z * size.z);
        room.reservedZone.localScale = room.usableZone.localScale;
        room.technicalZone.localScale = room.usableZone.localScale;
        room.tilesEdges.localScale = room.usableZone.localScale;
        room.tilesEdges.GetComponent<Renderer>().material.mainTextureScale = new Vector2(size.x, size.z) / 0.6f;
        room.tilesEdges.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(size.x / 0.6f % 1, size.z / 0.6f % 1);
        BuildWalls(room.walls, new Vector3(room.usableZone.localScale.x * 10, size.y, room.usableZone.localScale.z * 10), -0.001f);

        Vector3 bdOrigin = _data.parent.GetChild(0).localScale / -0.2f;
        Vector3 roOrigin = room.usableZone.localScale / 0.2f;
        newRoom.transform.position = _data.parent.position;
        newRoom.transform.localPosition += new Vector3(bdOrigin.x, 0, bdOrigin.z);
        newRoom.transform.localPosition += _data.pos;

        room.posXY = new Vector2(_data.pos.x, _data.pos.y);
        room.posXYUnit = EUnit.m;
        room.posZ = _data.pos.z;
        room.posZUnit = EUnit.m;
        room.size = new Vector2(size.x, size.z);
        room.sizeUnit = EUnit.m;
        room.height = size.y;
        room.heightUnit = EUnit.m;
        switch (orient)
        {
            case "EN":
                room.orientation = EOrientation.N;
                newRoom.transform.eulerAngles = new Vector3(0, 0, 0);
                newRoom.transform.position += new Vector3(roOrigin.x, 0, roOrigin.z);
                break;
            case "WS":
                room.orientation = EOrientation.S;
                newRoom.transform.eulerAngles = new Vector3(0, 180, 0);
                newRoom.transform.position += new Vector3(-roOrigin.x, 0, -roOrigin.z);
                break;
            case "NW":
                room.orientation = EOrientation.W;
                newRoom.transform.eulerAngles = new Vector3(0, -90, 0);
                newRoom.transform.position += new Vector3(-roOrigin.z, 0, roOrigin.x);
                break;
            case "SE":
                room.orientation = EOrientation.E;
                newRoom.transform.eulerAngles = new Vector3(0, 90, 0);
                newRoom.transform.position += new Vector3(roOrigin.z, 0, -roOrigin.x);
                break;
        }

        // Set UI room's name
        room.nameText.text = newRoom.name;
        room.nameText.rectTransform.sizeDelta = room.size;

        // Add room to GUI room filter
        Filters.instance.AddIfUnknown(Filters.instance.roomsList, newRoom.name);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRooms, Filters.instance.roomsList);

        // Get tenant from related Datacenter
        room.tenant = newRoom.transform.parent.parent.GetComponent<Datacenter>().tenant;
        room.UpdateZonesColor();

        if (!string.IsNullOrEmpty(_data.template))
        {
            room.SetZones(new SMargin(GameManager.gm.roomTemplates[_data.template].reservedArea),
                            new SMargin(GameManager.gm.roomTemplates[_data.template].technicalArea));
        }

        newRoom.AddComponent<HierarchyName>();
        GameManager.gm.allItems.Add(hierarchyName, newRoom);

        return room;
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
}
