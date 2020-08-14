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

    public void CreateBuilding(SBuildingInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Datacenter>() == null)
        {
            GameManager.gm.AppendLogLine("Building must be child of a datacenter", "yellow");
            return;
        }
        GameObject newBD = Instantiate(GameManager.gm.tileModel);
        newBD.name = _data.name;
        newBD.transform.parent = _data.parent;
        newBD.transform.localEulerAngles = Vector3.zero;

        // originalSize
        Vector3 originalSize = newBD.transform.GetChild(0).localScale;
        newBD.transform.GetChild(0).localScale = new Vector3(originalSize.x * _data.size.x, originalSize.y, originalSize.z * _data.size.z);

        Vector3 origin = newBD.transform.GetChild(0).localScale / 0.2f;
        newBD.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        newBD.transform.localPosition += new Vector3(_data.pos.x, 0, _data.pos.z);

        Building bd = newBD.AddComponent<Building>();
        // fill bd infos...

        newBD.AddComponent<HierarchyName>();
        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newBD);
    }

    public void CreateRoom(SRoomInfos _data, bool _changeHierarchy)
    {
        if (_data.parent.GetComponent<Building>() == null)
        {
            GameManager.gm.AppendLogLine("Room must be child of a Building", "yellow");
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
        room.walls.localScale = new Vector3(room.usableZone.localScale.x * 10, 1, room.usableZone.localScale.z * 10);

        Vector3 bdOrigin = _data.parent.GetChild(0).localScale / -0.2f;
        Vector3 roOrigin = room.usableZone.localScale / 0.2f;
        newRoom.transform.localPosition = new Vector3(bdOrigin.x, 0, bdOrigin.z);
        newRoom.transform.localPosition += new Vector3(roOrigin.x, 0, roOrigin.z);
        newRoom.transform.localPosition += _data.pos;

        room.size = new Vector2(_data.size.x, _data.size.z);
        room.sizeUnit = EUnit.tile;
        room.floorHeight = _data.size.y;
        room.floorUnit = EUnit.cm;
        switch (_data.orient)
        {
            case "EN":
                room.orientation = EOrientation.N;
                newRoom.transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case "WS":
                room.orientation = EOrientation.S;
                newRoom.transform.eulerAngles = new Vector3(0, 180, 0);
                break;
            case "NW":
                room.orientation = EOrientation.W;
                newRoom.transform.eulerAngles = new Vector3(0, -90, 0);
                break;
            case "SE":
                room.orientation = EOrientation.E;
                newRoom.transform.eulerAngles = new Vector3(0, 90, 0);
                break;
        }

        Filters.instance.AddIfUnknowned(Filters.instance.itRooms, newRoom);
        Filters.instance.AddIfUnknowned(Filters.instance.itRoomsList, newRoom.name);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownItRooms, Filters.instance.itRoomsList);

        newRoom.AddComponent<HierarchyName>();

        int index = _data.parent.GetComponent<HierarchyName>().fullname.IndexOf(".");
        string rootName = _data.parent.GetComponent<HierarchyName>().fullname.Substring(0, index);
        room.tenant = GameManager.gm.tenants[rootName];

        if (_changeHierarchy)
            GameManager.gm.SetCurrentItem(newRoom);
    }
}
