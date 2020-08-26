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
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    public void CreateBuilding(SBuildingInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Datacenter>() == null)
        {
            GameManager.gm.AppendLogLine("Building must be child of a datacenter", "yellow");
            return;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return;
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
        BuildWalls(bd.walls, new Vector3(newBD.transform.GetChild(0).localScale.x * 10, _data.size.y, newBD.transform.GetChild(0).localScale.z * 10));
        // fill bd infos...

        newBD.AddComponent<HierarchyName>();

        GameManager.gm.allItems.Add(hierarchyName, newBD);
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newBD);
    }

    ///<summary>
    /// Instantiate a roomModel (from GameManager) and apply _data to it.
    ///</summary>
    ///<param name="_data">Informations about the room</param>
    ///<param name="_changeHierarchy">Should the current item change to this one ?</param>
    public void CreateRoom(SRoomInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Building>() == null)
        {
            GameManager.gm.AppendLogLine("Room must be child of a Building", "yellow");
            return;
        }
        string hierarchyName = $"{_data.parent.GetComponent<HierarchyName>()?.fullname}.{_data.name}";
        if (GameManager.gm.allItems.Contains(hierarchyName))
        {
            GameManager.gm.AppendLogLine($"{hierarchyName} already exists.", "yellow");
            return;
        }

        GameObject newRoom = Instantiate(GameManager.gm.roomModel);
        newRoom.name = _data.name;
        newRoom.transform.parent = _data.parent;

        Room room = newRoom.GetComponent<Room>();

        Vector3 originalSize = room.usableZone.localScale;
        room.usableZone.localScale = new Vector3(originalSize.x * _data.size.x, originalSize.y, originalSize.z * _data.size.z);
        room.reservedZone.localScale = room.usableZone.localScale;
        room.technicalZone.localScale = room.usableZone.localScale;
        room.tilesEdges.localScale = room.usableZone.localScale;
        room.tilesEdges.GetComponent<Renderer>().material.mainTextureScale = new Vector2(_data.size.x, _data.size.z) / 0.6f;
        BuildWalls(room.walls, new Vector3(room.usableZone.localScale.x * 10, _data.size.y, room.usableZone.localScale.z * 10));

        Vector3 bdOrigin = _data.parent.GetChild(0).localScale / -0.2f;
        Vector3 roOrigin = room.usableZone.localScale / 0.2f;
        newRoom.transform.position = _data.parent.position;
        newRoom.transform.localPosition += new Vector3(bdOrigin.x, 0, bdOrigin.z);
        newRoom.transform.localPosition += _data.pos;

        room.size = new Vector2(_data.size.x, _data.size.z);
        room.sizeUnit = EUnit.tile;
        room.height = _data.size.y;
        room.heightUnit = EUnit.m;
        switch (_data.orient)
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

        room.nameText.text = newRoom.name;
        room.nameText.rectTransform.sizeDelta = room.size;

        Filters.instance.AddIfUnknowned(Filters.instance.rooms, newRoom);
        Filters.instance.AddIfUnknowned(Filters.instance.roomsList, newRoom.name);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRooms, Filters.instance.roomsList);

        newRoom.AddComponent<HierarchyName>();

        // Get tenant from related Datacenter
        room.tenant = newRoom.transform.parent.parent.GetComponent<Datacenter>().tenant;

        GameManager.gm.allItems.Add(hierarchyName, newRoom);
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newRoom);
    }

    ///<summary>
    /// Set walls children of _root. They have to be in Front/Back/Right/Left order.
    ///</summary>
    ///<param name="_root">The root of walls.</param>
    ///<param name="_dim">The dimensions of the building/room</param>
    private void BuildWalls(Transform _root, Vector3 _dim)
    {
        Transform wallFront = _root.GetChild(0);
        Transform wallBack = _root.GetChild(1);
        Transform wallRight = _root.GetChild(2);
        Transform wallLeft = _root.GetChild(3);

        wallFront.localScale = new Vector3(_dim.x, _dim.y, 0.01f);
        wallBack.localScale = new Vector3(_dim.x, _dim.y, 0.01f);
        wallRight.localScale = new Vector3(_dim.z, _dim.y, 0.01f);
        wallLeft.localScale = new Vector3(_dim.z, _dim.y, 0.01f);

        wallFront.localPosition = new Vector3(0, wallFront.localScale.y / 2, _dim.z / 2);
        wallBack.localPosition = new Vector3(0, wallFront.localScale.y / 2, -_dim.z / 2);
        wallRight.localPosition = new Vector3(_dim.x / 2, wallFront.localScale.y / 2, 0);
        wallLeft.localPosition = new Vector3(-_dim.x / 2, wallFront.localScale.y / 2, 0);
    }
}
