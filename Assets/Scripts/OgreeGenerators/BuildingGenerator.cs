using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator
{
    ///<summary>
    /// Instantiate a buildingModel (from GameManager) and apply the given data to it.
    ///</summary>
    ///<param name="_bd">The building data to apply</param>
    ///<param name="_parent">The parent of the created building</param>
    ///<returns>The created Building</returns>
    public Building CreateBuilding(SApiObject _bd, Transform _parent)
    {
        if (GameManager.instance.allItems.Contains(_bd.id))
        {
            GameManager.instance.AppendLogLine($"{_bd.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        SBuildingFromJson template = new();
        if (!string.IsNullOrEmpty(_bd.attributes["template"]))
        {
            if (GameManager.instance.buildingTemplates.ContainsKey(_bd.attributes["template"]))
                template = GameManager.instance.buildingTemplates[_bd.attributes["template"]];
            else
            {
                GameManager.instance.AppendLogLine($"Unknown template {_bd.attributes["template"]}. Abord drawing {_bd.name}", ELogTarget.both, ELogtype.error);
                return null;
            }
        }

        // Get data from _bd.attributes
        Vector2 posXY = Utils.ParseVector2(_bd.attributes["posXY"]);
        Vector2 size = Utils.ParseVector2(_bd.attributes["size"]);
        float height = Utils.ParseDecFrac(_bd.attributes["height"]);
        float rotation = Utils.ParseDecFrac(_bd.attributes["rotation"]);

        // Instantiate the good prefab and setup the Buiding component
        GameObject newBD;
        if (template.vertices != null)
            newBD = Object.Instantiate(GameManager.instance.nonConvexBuildingModel);
        else
            newBD = Object.Instantiate(GameManager.instance.buildingModel);
        newBD.name = _bd.name;
        newBD.transform.parent = _parent;

        Building building = newBD.GetComponent<Building>();
        building.UpdateFromSApiObject(_bd);

        // Apply rotation
        newBD.transform.localEulerAngles = new(0, rotation, 0);

        Transform roof = newBD.transform.Find("Roof");
        if (template.vertices != null)
        {
            building.isSquare = false;
            NonSquareBuildingGenerator.CreateShape(newBD.transform, template);
        }
        else
        {
            // Apply size & move the floor to have the container at the lower left corner of it
            Transform floor = newBD.transform.GetChild(0);
            Vector3 originalSize = floor.localScale;
            floor.localScale = new(originalSize.x * size.x, originalSize.y, originalSize.z * size.y);
            floor.localPosition = new Vector3(floor.localScale.x, 0, floor.localScale.z) / 0.2f;
            roof.localScale = floor.localScale;
            roof.localPosition = floor.localPosition;

            // Align walls & nameText to the floor & setup nameText
            building.walls.localPosition = new(floor.localPosition.x, building.walls.localPosition.y, floor.localPosition.z);

            building.nameText.transform.localPosition = new(floor.localPosition.x, building.nameText.transform.localPosition.y, floor.localPosition.z);
            building.nameText.rectTransform.sizeDelta = size;

            BuildWalls(building.walls, new(floor.localScale.x * 10, height, floor.localScale.z * 10), 0);
        }
        // Apply posXY
        if (_parent)
            newBD.transform.localPosition = new(posXY.x, 0, posXY.y);
        else
            newBD.transform.localPosition = Vector3.zero;

        // Setup nameText
        building.nameText.text = _bd.name;
        building.nameText.gameObject.SetActive(!newBD.GetComponentInChildren<Room>());

        building.UpdateColorByDomain();

        roof.localPosition = new(roof.localPosition.x, height, roof.localPosition.z);

        GameManager.instance.allItems.Add(building.id, newBD);
        return building;
    }

    ///<summary>
    /// Instantiate a roomModel (from GameManager) and apply given data to it.
    ///</summary>
    ///<param name="_ro">The room data to apply</param>    
    ///<param name="_parent">The parent of the created room</param>
    ///<returns>The created Room</returns>
    public Room CreateRoom(SApiObject _ro, Transform _parent)
    {
        if (GameManager.instance.allItems.Contains(_ro.id))
        {
            GameManager.instance.AppendLogLine($"{_ro.id} already exists.", ELogTarget.both, ELogtype.warning);
            return null;
        }

        SRoomFromJson template = new();
        if (!string.IsNullOrEmpty(_ro.attributes["template"]))
        {
            if (GameManager.instance.roomTemplates.ContainsKey(_ro.attributes["template"]))
                template = GameManager.instance.roomTemplates[_ro.attributes["template"]];
            else
            {
                GameManager.instance.AppendLogLine($"Unknown template {_ro.attributes["template"]}. Abort drawing {_ro.name}", ELogTarget.both, ELogtype.error);
                return null;
            }
        }

        // Get data from _ro.attributes
        Vector2 posXY = Utils.ParseVector2(_ro.attributes["posXY"]);
        Vector2 size = Utils.ParseVector2(_ro.attributes["size"]);
        float height = Utils.ParseDecFrac(_ro.attributes["height"]);
        float rotation = Utils.ParseDecFrac(_ro.attributes["rotation"]);

        // Instantiate the good prefab and setup the Room component
        GameObject newRoom;
        if (template.vertices != null)
            newRoom = Object.Instantiate(GameManager.instance.nonConvexRoomModel);
        else
            newRoom = Object.Instantiate(GameManager.instance.roomModel);
        newRoom.name = _ro.name;
        newRoom.transform.parent = _parent;

        Room room = newRoom.GetComponent<Room>();
        room.UpdateFromSApiObject(_ro);

        // Apply rotation
        newRoom.transform.localEulerAngles = new(0, rotation, 0);

        if (template.vertices != null)
        {
            room.isSquare = false;
            NonSquareBuildingGenerator.CreateShape(newRoom.transform, template);
        }
        else
        {
            // Apply size...
            Vector3 originalSize = room.usableZone.localScale;
            room.usableZone.localScale = new(originalSize.x * size.x, originalSize.y, originalSize.z * size.y);
            room.reservedZone.localScale = room.usableZone.localScale;
            room.technicalZone.localScale = room.usableZone.localScale;
            room.tilesGrid.localScale = room.usableZone.localScale;
            room.tilesGrid.GetComponent<Renderer>().material.mainTextureScale = size / UnitValue.Tile;
            room.tilesGrid.GetComponent<Renderer>().material.mainTextureOffset = new(size.x / UnitValue.Tile % 1, size.y / UnitValue.Tile % 1);

            // ...and move the floors layer, wall & text to have the container at the lower left corner of them
            room.usableZone.localPosition = new Vector3(room.usableZone.localScale.x, room.usableZone.localPosition.y, room.usableZone.localScale.z) / 0.2f;
            room.reservedZone.localPosition = new Vector3(room.usableZone.localScale.x, room.reservedZone.localPosition.y, room.usableZone.localScale.z) / 0.2f;
            room.technicalZone.localPosition = new Vector3(room.usableZone.localScale.x, room.technicalZone.localPosition.y, room.usableZone.localScale.z) / 0.2f;
            room.tilesGrid.localPosition = new Vector3(room.usableZone.localScale.x, room.tilesGrid.localPosition.y, room.usableZone.localScale.z) / 0.2f;

            room.walls.localPosition = new Vector3(room.usableZone.localScale.x, room.walls.localPosition.y, room.usableZone.localScale.z) / 0.2f;

            room.nameText.transform.localPosition = new Vector3(room.usableZone.localScale.x, room.nameText.transform.localPosition.y, room.usableZone.localScale.z) / 0.2f;
            room.nameText.rectTransform.sizeDelta = size;

            BuildWalls(room.walls, new Vector3(room.usableZone.localScale.x * 10, height, room.usableZone.localScale.z * 10), -0.001f);

            room.UpdateZonesColor();
        }
        // Apply posXY
        if (_parent)
            newRoom.transform.localPosition = new(posXY.x, 0, posXY.y);
        else
            newRoom.transform.localPosition = Vector3.zero;

        // Set UI room's name
        room.nameText.text = newRoom.name;

        room.UpdateColorByDomain();

        GameManager.instance.allItems.Add(room.id, newRoom);

        if (template.vertices == null)
        {
            if (_ro.attributes.ContainsKey("reserved") && _ro.attributes.ContainsKey("technical")
                && !string.IsNullOrEmpty(_ro.attributes["reserved"]) && !string.IsNullOrEmpty(_ro.attributes["technical"]))
            {
                SMargin reserved = new(Utils.ParseVector4(_ro.attributes["reserved"]));
                SMargin technical = new(Utils.ParseVector4(_ro.attributes["technical"]));
                room.SetAreas(reserved, technical);
            }
        }

        if (room.attributes.ContainsKey("separators"))
        {
            Dictionary<string, SSeparator> separators = JsonConvert.DeserializeObject<Dictionary<string, SSeparator>>(room.attributes["separators"]);
            foreach (KeyValuePair<string, SSeparator> sep in separators)
                room.BuildSeparator(new SSeparator(sep.Key, sep.Value));
        }

        if (room.attributes.ContainsKey("pillars"))
        {
            Dictionary<string, SPillar> pillars = JsonConvert.DeserializeObject<Dictionary<string, SPillar>>(room.attributes["pillars"]);
            foreach (KeyValuePair<string, SPillar> pillar in pillars)
                room.BuildPillar(new SPillar(pillar.Key, pillar.Value));
        }
        room.ComputeChildrenOrigin();
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

        wallFront.localScale = new(_dim.x, _dim.y, 0.01f);
        wallBack.localScale = new(_dim.x, _dim.y, 0.01f);
        wallRight.localScale = new(_dim.z, _dim.y, 0.01f);
        wallLeft.localScale = new(_dim.z, _dim.y, 0.01f);

        wallFront.localPosition = new(0, wallFront.localScale.y / 2, _dim.z / 2 + _offset);
        wallBack.localPosition = new(0, wallFront.localScale.y / 2, -(_dim.z / 2 + _offset));
        wallRight.localPosition = new(_dim.x / 2 + _offset, wallFront.localScale.y / 2, 0);
        wallLeft.localPosition = new(-(_dim.x / 2 + _offset), wallFront.localScale.y / 2, 0);
    }
}
