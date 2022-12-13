using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Two List extensions methods to be able to treat them as circular
/// </summary>
public static class CircularListExtension
{
    /// <summary>
    /// Returns the item <paramref name="_offset"/> elements after <paramref name="_elem"/> in a circular list
    /// </summary>
    /// <typeparam name="T">type of variables in the list</typeparam>
    /// <param name="_input">the list from which we call the funciton</param>
    /// <param name="_elem">the element of the list we start the count from</param>
    /// <param name="_offset">how many times we go to the next element before returning it</param>
    /// <returns>the item <paramref name="_offset"/> elements after <paramref name="_elem"/> in <paramref name="_input"/> or the default value of <typeparamref name="T"/> if <paramref name="_elem"/> is not in <paramref name="_input"/></returns>
    public static T NextElem<T>(this List<T> _input, T _elem, int _offset = 1)
    {
        int index = _input.IndexOf(_elem);
        if (index == -1)
            return default;
        return _input[(index + _offset) % _input.Count];
    }

    /// <summary>
    /// Returns the item at index <paramref name="_offset"/> + <paramref name="_index"/> in a circular list
    /// </summary>
    /// <typeparam name="T">type of variables in the list</typeparam>
    /// <param name="_input">the list from which we call the funciton</param>
    /// <param name="_index">the index of the list we start the count from</param>
    /// <param name="_offset">how many times we go to the next element before returning it</param>
    /// <returns>the item at index <paramref name="_index"/> in <paramref name="_input"/></returns>
    public static T NextIndex<T>(this List<T> _input, int _index, int _offset = 1)
    {
        return _input[(_index + _offset) % _input.Count];
    }

}

public static class NonSquareRoomGenerator
{
    public static void CreateShape(GameObject _room, ReadFromJson.SRoomFromJson _template)
    {
        Debug.Log($"Create shape of {_template.slug}");

        if (_template.floorUnit == "t")
            BuildFloor(_room.transform, _template, true);
        else
            BuildFloor(_room.transform, _template, false);
        BuildWalls(_room.transform, _template);

        Vector3 center = new Vector3(_template.center[0], _template.center[1], _template.center[2]);
        _room.GetComponent<Room>().nameText.transform.localPosition = center + new Vector3(0, 0.003f, 0);
    }

    /// <summary>
    /// Build the walls of a non convex room
    /// </summary>
    /// <param name="_root">the transform of the room's flooe</param>
    /// <param name="_template">the template of the non convex room</param>
    private static void BuildWalls(Transform _root, ReadFromJson.SRoomFromJson _template)
    {
        float height = _template.sizeWDHm[2];
        int vCount = _template.vertices.Count;
        Vector3[] verticesRoomWalls = new Vector3[4 * (vCount + 1)];
        int[] trianglesRoomWalls = new int[6 * (vCount + 1)];
        float[] xWalls = new float[vCount];
        float[] zWalls = new float[vCount];

        Transform walls = _root.GetComponent<Room>().walls;
        Mesh meshWalls = new Mesh { name = "meshWalls" };
        walls.GetComponent<MeshFilter>().mesh = meshWalls;

        for (int i = 0; i < vCount; i++)
        {
            xWalls[i] = _template.vertices[i][0] / 100f;
            zWalls[i] = _template.vertices[i][1] / 100f;
        }

        for (int i = 0; i < vCount - 1; i++)
        {
            verticesRoomWalls[4 * i] = new Vector3(xWalls[i], 0, zWalls[i]);
            verticesRoomWalls[4 * i + 1] = new Vector3(xWalls[i + 1], 0, zWalls[i + 1]);
            verticesRoomWalls[4 * i + 2] = new Vector3(xWalls[i], height, zWalls[i]);
            verticesRoomWalls[4 * i + 3] = new Vector3(xWalls[i + 1], height, zWalls[i + 1]);
        }

        verticesRoomWalls[4 * (vCount - 1)] = new Vector3(xWalls[vCount - 1], 0, zWalls[vCount - 1]);
        verticesRoomWalls[4 * (vCount - 1) + 1] = new Vector3(xWalls[0], 0, zWalls[0]);
        verticesRoomWalls[4 * (vCount - 1) + 2] = new Vector3(xWalls[vCount - 1], height, zWalls[vCount - 1]);
        verticesRoomWalls[4 * (vCount - 1) + 3] = new Vector3(xWalls[0], height, zWalls[0]);

        for (int i = 0; i < vCount + 1; i++)
        {
            trianglesRoomWalls[6 * i] = 4 * i;
            trianglesRoomWalls[6 * i + 1] = 4 * i + 2;
            trianglesRoomWalls[6 * i + 2] = 4 * i + 1;
            trianglesRoomWalls[6 * i + 3] = 4 * i + 1;
            trianglesRoomWalls[6 * i + 4] = 4 * i + 2;
            trianglesRoomWalls[6 * i + 5] = 4 * i + 3;
        }

        meshWalls.vertices = verticesRoomWalls;
        meshWalls.triangles = trianglesRoomWalls;
        meshWalls.RecalculateNormals();
    }

    /// <summary>
    /// Build the floor of a non convex room, optionnaly with its tiles
    /// </summary>
    /// <param name="_root">the transform of the room's flooe</param>
    /// <param name="_template">the template of the non convex room</param>
    /// <param name="_tiles">if true, build the tiles from the template's tiles field</param>
    private static void BuildFloor(Transform _root, ReadFromJson.SRoomFromJson _template, bool _tiles)
    {
        List<int> trianglesRoom = new List<int>();

        List<Vector3> walls = _template.vertices.Select(w => new Vector3(w[0] / 100f, 0, w[1] / 100f)).ToList();
        if (!MostlyClockWise(walls))
            walls.Reverse();
        List<Vector3> shrinkingWalls = new List<Vector3>(walls);
        Transform floor = _root.GetComponent<Room>().usableZone;
        Mesh meshFloor = new Mesh { name = "meshFloor" };
        floor.GetComponent<MeshFilter>().mesh = meshFloor;
        int index = 0;

        Vector3 A, B, C;
        while (shrinkingWalls.Count >= 3)
        {
            A = shrinkingWalls.NextIndex(index, 0);
            B = shrinkingWalls.NextIndex(index, 1);
            C = shrinkingWalls.NextIndex(index, 2);
            Debug.Log(ClockWise(A, B, C));
            if (ClockWise(A, B, C) && !TriangleIntersectWalls(A, B, C, walls))
            {
                trianglesRoom.Add(walls.IndexOf(A));
                trianglesRoom.Add(walls.IndexOf(B));
                trianglesRoom.Add(walls.IndexOf(C));

                shrinkingWalls.Remove(B);
            }
            index++;
        }
        meshFloor.vertices = walls.ToArray();
        meshFloor.triangles = trianglesRoom.ToArray();
        meshFloor.RecalculateNormals();

        floor.GetComponent<MeshCollider>().sharedMesh = meshFloor;
        floor.GetComponent<MeshCollider>().convex = false;

        if (_tiles)
        {
            for (int i = 0; i < _template.tiles.Length; i++)
            {
                string element = _template.tiles[i].location;
                string[] separated = element.Split('/');

                int x = int.Parse(separated[0]);
                int z = int.Parse(separated[1]);

                // Tiles           
                GameObject tile = Object.Instantiate(GameManager.gm.tileModel, floor);
                tile.name = $"Tile_{_template.tiles[i].location}";
                tile.transform.localPosition = 0.6f * (new Vector3(x, 0, z));
                tile.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0.001f, 2 * GameManager.gm.tileSize) / 2;
            }
        }
    }

    /// <summary>
    /// Check if a list vertices defining a polygon is listed <i>mostly</i> clockwise
    /// <br/> Mostly because, if the polygon is non-convex, it can no be just clockwise or non-clockwise
    /// <br/><see href="https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order"/>
    /// </summary>
    /// <param name="_points">the list vertices defining a polygon</param>
    /// <returns>True if the list is mostly clockwise</returns>
    private static bool MostlyClockWise(List<Vector3> _points)
    {
        float sum = 0;
        for (int i = 0; i < _points.Count; i++)
        {
            sum += (_points.NextIndex(i).x - _points[i].x) * (_points.NextIndex(i).x + _points[i].x);
        }
        return sum > 0;
    }

    /// <summary>
    /// Check if three points are listed in clockwise order 
    /// <br/><b>IGNORE VERTICAL COMPONENT</b>
    /// <br/><see href="https://bryceboe.com/2006/10/23/line-segment-intersection-algorithm/"/>
    /// </summary>
    /// <param name="A">the first point</param>
    /// <param name="B">the second point</param>
    /// <param name="C">the third point</param>
    /// <returns>true if <paramref name="A"/>, <paramref name="B"/> and <paramref name="C"/> are in a clockwise order</returns>
    private static bool ClockWise(Vector3 A, Vector3 B, Vector3 C)
    {
        return (C.z - A.z) * (B.x - A.x) < (B.z - A.z) * (C.x - A.x);
    }

    /// <summary>
    /// Check if two segments intersect 
    /// <br/><b>IGNORE VERTICAL COMPONENT</b>
    /// </summary>
    /// <param name="_a">first end of the first segment</param>
    /// <param name="_b">second end of the first segment</param>
    /// <param name="_c">first end of the seoncd segment</param>
    /// <param name="_d">second end of the second segment</param>
    /// <returns>true if the two segments intersect</returns>
    private static bool Intersect(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d)
    {
        return ClockWise(_a, _c, _d) != ClockWise(_b, _c, _d) && ClockWise(_a, _b, _c) != ClockWise(_a, _b, _d);
    }

    /// <summary>
    /// Check if a triangle intersect with at least one wall of a list
    /// </summary>
    /// <param name="_corner1">First corner of the triangle</param>
    /// <param name="_corner2">Second corner of the triangle</param>
    /// <param name="_corner3">Third corner of the triangle</param>
    /// <param name="_walls">List of walls</param>
    /// <returns>True if at least one wall intersect with a side of the triangle</returns>
    private static bool TriangleIntersectWalls(Vector3 _corner1, Vector3 _corner2, Vector3 _corner3, List<Vector3> _walls)
    {
        for (int i = 0; i < _walls.Count - 1; i++)
        {
            if (_walls[i] == _corner1 || _walls[i] == _corner2 || _walls[i] == _corner3 || _walls[i + 1] == _corner1 || _walls[i + 1] == _corner2 || _walls[i + 1] == _corner3)
                continue;
            if (Intersect(_corner1, _corner2, _walls[i], _walls[i + 1]) || Intersect(_corner2, _corner3, _walls[i], _walls[i + 1]) || Intersect(_corner3, _corner1, _walls[i], _walls[i + 1]))
                return true;
        }
        return false;
    }
}

