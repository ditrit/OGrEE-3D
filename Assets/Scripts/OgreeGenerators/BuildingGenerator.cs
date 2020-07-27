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
            Debug.LogWarning("Building must be child of a datacenter");
            return;
        }
        // GameObject newBD = new GameObject(_data.name);
        GameObject newBD = Instantiate(GameManager.gm.tileModel);
        newBD.name = _data.name;
        newBD.transform.parent = _data.parent;

        // originalSize
        Vector3 originalSize = newBD.transform.GetChild(0).localScale;
        newBD.transform.GetChild(0).localScale = new Vector3(originalSize.x * _data.size.x, originalSize.y, originalSize.z * _data.size.z);
        
        Vector3 origin = newBD.transform.GetChild(0).localScale / 0.2f;
        newBD.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        newBD.transform.localPosition += new Vector3(_data.pos.x, 0, _data.pos.z) * GameManager.gm.tileSize;
       
        Building bd = newBD.AddComponent<Building>();
        // fill bd infos...

        newBD.AddComponent<HierarchyName>();
        if (_changeHierarchy)
            GameManager.gm.currentItem = newBD;
    }

    public void CreateRoom(SRoomInfos _data)
    {
        GameObject tile = Instantiate(GameManager.gm.tileModel);
        tile.name = _data.name;
        tile.transform.parent = GameObject.Find(_data.parentName).transform;

        Vector3 originalSize = tile.transform.GetChild(0).localScale;
        tile.transform.GetChild(0).localScale = new Vector3(originalSize.x * _data.size.x, originalSize.y, originalSize.z * _data.size.y);
        
        Vector3 origin = tile.transform.GetChild(0).localScale / 0.2f;
        tile.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        tile.transform.localPosition += new Vector3(_data.pos.x, 0, _data.pos.y) * GameManager.gm.tileSize;

        Room room = tile.AddComponent<Room>();
        room.size = _data.size;
        room.sizeUnit = EUnit.tile;
        room.technical = new SMargin(_data.margin.x, 0, 0, _data.margin.y);
        room.reserved = new SMargin();

        switch (_data.orient)
        {
            case "front":
                room.orientation = EOrientation.N;
                tile.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "rear":
                room.orientation = EOrientation.S;
                tile.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "left":
                room.orientation = EOrientation.W;
                tile.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case "right":
                room.orientation = EOrientation.E;
                tile.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
        }

        Filters.instance.AddIfUnknowned(Filters.instance.itRooms, tile);
        Filters.instance.AddIfUnknowned(Filters.instance.itRoomsList, tile.name);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownItRooms, Filters.instance.itRoomsList);

        tile.AddComponent<HierarchyName>();
    }
}
