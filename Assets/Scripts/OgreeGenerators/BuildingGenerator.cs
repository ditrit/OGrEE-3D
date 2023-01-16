﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        string hierarchyName;
        if (_parent)
            hierarchyName = $"{_parent.GetComponent<OgreeObject>().hierarchyName}.{_bd.name}";
        else
            hierarchyName = _bd.name;
        if (GameManager.instance.allItems.Contains(hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{hierarchyName} already exists.", true, ELogtype.warning);
            return null;
        }

        // Position and size data from _bd.attributes
        Vector2 posXY = JsonUtility.FromJson<Vector2>(_bd.attributes["posXY"]);
        Vector2 size = JsonUtility.FromJson<Vector2>(_bd.attributes["size"]);
        float height = Utils.ParseDecFrac(_bd.attributes["height"]);

        GameObject newBD = Object.Instantiate(GameManager.instance.buildingModel);
        newBD.name = _bd.name;
        newBD.transform.parent = _parent;
        newBD.transform.localEulerAngles = Vector3.zero;

        // originalSize
        Vector3 originalSize = newBD.transform.GetChild(0).localScale;
        newBD.transform.GetChild(0).localScale = new Vector3(originalSize.x * size.x, originalSize.y, originalSize.z * size.y);

        Vector3 origin = newBD.transform.GetChild(0).localScale / 0.2f;
        newBD.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        newBD.transform.localPosition += new Vector3(posXY.x, 0, posXY.y);

        Building building = newBD.GetComponent<Building>();
        building.hierarchyName = hierarchyName;
        building.UpdateFromSApiObject(_bd);

        building.nameText.text = _bd.name;
        building.nameText.rectTransform.sizeDelta = size;
        building.nameText.gameObject.SetActive(!newBD.GetComponentInChildren<Room>());

        BuildWalls(building.walls, new Vector3(newBD.transform.GetChild(0).localScale.x * 10, height, newBD.transform.GetChild(0).localScale.z * 10), 0);

        GameManager.instance.allItems.Add(hierarchyName, newBD);
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
        string hierarchyName;
        if (_parent)
            hierarchyName = $"{_parent.GetComponent<OgreeObject>().hierarchyName}.{_ro.name}";
        else
            hierarchyName = _ro.name;
        if (GameManager.instance.allItems.Contains(hierarchyName))
        {
            GameManager.instance.AppendLogLine($"{hierarchyName} already exists.", true, ELogtype.warning);
            return null;
        }

        SRoomFromJson template = new SRoomFromJson();
        if (!string.IsNullOrEmpty(_ro.attributes["template"]))
        {
            if (GameManager.instance.roomTemplates.ContainsKey(_ro.attributes["template"]))
                template = GameManager.instance.roomTemplates[_ro.attributes["template"]];
            else
            {
                GameManager.instance.AppendLogLine($"Unknown template {_ro.attributes["template"]}. Abort drawing {_ro.name}", true, ELogtype.error);
                return null;
            }
        }

        // Position and size data from _ro.attributes
        Vector2 posXY = JsonUtility.FromJson<Vector2>(_ro.attributes["posXY"]);
        Vector2 size = JsonUtility.FromJson<Vector2>(_ro.attributes["size"]);
        float height = Utils.ParseDecFrac(_ro.attributes["height"]);

        GameObject newRoom;
        if (template.vertices != null)
            newRoom = Object.Instantiate(GameManager.instance.nonConvexRoomModel);
        else
            newRoom = Object.Instantiate(GameManager.instance.roomModel);
        newRoom.name = _ro.name;
        newRoom.transform.parent = _parent;

        Room room = newRoom.GetComponent<Room>();
        room.hierarchyName = hierarchyName;
        room.UpdateFromSApiObject(_ro);

        if (template.vertices != null)
        {
            NonSquareRoomGenerator.CreateShape(newRoom, template);
            newRoom.transform.localPosition += new Vector3(posXY.x, 0, posXY.y);
            if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)E(\\+|\\-)N"))
                newRoom.transform.eulerAngles = new Vector3(0, 0, 0);
            else if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)W(\\+|\\-)S"))
                newRoom.transform.eulerAngles = new Vector3(0, 180, 0);
            else if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)N(\\+|\\-)W"))
                newRoom.transform.eulerAngles = new Vector3(0, -90, 0);
            else if (Regex.IsMatch(room.attributes["orientation"], "(\\+|\\-)S(\\+|\\-)E"))
                newRoom.transform.eulerAngles = new Vector3(0, 90, 0);
        }
        else
        {
            Vector3 originalSize = room.usableZone.localScale;
            room.usableZone.localScale = new Vector3(originalSize.x * size.x, originalSize.y, originalSize.z * size.y);
            room.reservedZone.localScale = room.usableZone.localScale;
            room.technicalZone.localScale = room.usableZone.localScale;
            room.tilesEdges.localScale = room.usableZone.localScale;
            room.tilesEdges.GetComponent<Renderer>().material.mainTextureScale = size / 0.6f;
            room.tilesEdges.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(size.x / 0.6f % 1, size.y / 0.6f % 1);
            BuildWalls(room.walls, new Vector3(room.usableZone.localScale.x * 10, height, room.usableZone.localScale.z * 10), -0.001f);

            Vector3 bdOrigin = Vector3.zero;
            if (_parent)
                bdOrigin = _parent.GetChild(0).localScale / -0.2f;
            Vector3 roOrigin = room.usableZone.localScale / 0.2f;
            if (_parent)
                newRoom.transform.position = _parent.position;
            else
                newRoom.transform.position = Vector3.zero;
            newRoom.transform.localPosition += new Vector3(bdOrigin.x, 0, bdOrigin.z);
            newRoom.transform.localPosition += new Vector3(posXY.x, 0, posXY.y);

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

            room.UpdateZonesColor();
        }
        // Set UI room's name
        room.nameText.text = newRoom.name;
        room.nameText.rectTransform.sizeDelta = size;

        GameManager.instance.allItems.Add(hierarchyName, newRoom);

        if (template.vertices == null)
        {
            if (_ro.attributes.ContainsKey("reserved") && _ro.attributes.ContainsKey("technical")
                && !string.IsNullOrEmpty(_ro.attributes["reserved"]) && !string.IsNullOrEmpty(_ro.attributes["technical"]))
            {
                SMargin reserved = JsonUtility.FromJson<SMargin>(_ro.attributes["reserved"]);
                SMargin technical = JsonUtility.FromJson<SMargin>(_ro.attributes["technical"]);
                room.SetAreas(reserved, technical);
            }
        }

        // if (!string.IsNullOrEmpty(template.slug))
        // {
        //     if (template.vertices == null)
        //         room.SetAreas(new SMargin(template.reservedArea), new SMargin(template.technicalArea));

        //     if (template.separators != null && !room.attributes.ContainsKey("separators"))
        //     {
        //         foreach (SSeparator sep in template.separators)
        //             room.AddSeparator(sep);
        //     }

        //     if (template.tiles != null)
        //     {
        //         List<STile> tiles = new List<STile>();
        //         foreach (STile t in template.tiles)
        //             tiles.Add(t);
        //         room.attributes["tiles"] = JsonConvert.SerializeObject(tiles);
        //     }

        //     if (template.rows != null)
        //     {
        //         List<SRow> rows = new List<SRow>();
        //         foreach (SRow r in template.rows)
        //             rows.Add(r);
        //         room.attributes["rows"] = JsonConvert.SerializeObject(rows);
        //     }

        //     if (template.colors != null)
        //     {
        //         List<SColor> colors = new List<SColor>();
        //         foreach (SColor c in template.colors)
        //             colors.Add(c);
        //         room.attributes["customColors"] = JsonConvert.SerializeObject(colors);
        //     }
        // }

        if (room.attributes.ContainsKey("separators"))
        {
            List<SSeparator> separators = JsonConvert.DeserializeObject<List<SSeparator>>(room.attributes["separators"]);
            foreach (SSeparator sep in separators)
                room.BuildSeparator(sep);
        }

        if (room.attributes.ContainsKey("pillars"))
        {
            List<SPillar> pillars = JsonConvert.DeserializeObject<List<SPillar>>(room.attributes["pillars"]);
            foreach (SPillar pillar in pillars)
                room.BuildPillar(pillar);
        }

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
