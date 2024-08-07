using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NonSquareBuildingGenerator
{
    private struct SCommonTemplate
    {
        public string slug;
        public List<float> sizeWDHm;
        public Vector2 isolationPoleCenter;
        public float isolationPoleRadius;
        public List<Vector2> vertices;
        public List<STile> tiles;
        public float tileAngle;
        public Vector2 offset;

        public SCommonTemplate(SBuildingTemplate _src)
        {
            slug = _src.slug;
            sizeWDHm = _src.sizeWDHm;
            offset = new(_src.vertices[0][0], _src.vertices[0][1]);
            vertices = new();
            foreach (List<float> vertex in _src.vertices)
                vertices.Add(new(vertex[0] - offset.x, vertex[1] - offset.y));
            (isolationPoleRadius, isolationPoleCenter) = PolyLabel.FindPoleOfIsolation(vertices, 0.1f);
            tiles = null;
            tileAngle = float.NaN;
        }

        public SCommonTemplate(SRoomTemplate _src)
        {
            slug = _src.slug;
            sizeWDHm = _src.sizeWDHm;
            offset = new(_src.vertices[0][0], _src.vertices[0][1]);
            vertices = new();
            foreach (List<float> vertex in _src.vertices)
                vertices.Add(new(vertex[0] - offset.x, vertex[1] - offset.y));

            List<Vector3> walls = vertices.Select(w => new Vector3(w.x, 0, w.y)).ToList();
            if (!MostlyClockWise(walls))
                vertices.Reverse();
            (isolationPoleRadius, isolationPoleCenter) = PolyLabel.FindPoleOfIsolation(vertices, 0.1f);
            tiles = _src.tiles;
            tileAngle = _src.tileAngle;
        }
    }

    /// <summary>
    /// Build the walls and floor of a non convex building
    /// </summary>
    /// <param name="_building">The transform of the building's floor</param>
    /// <param name="_template">The template of the non convex building</param>
    public void CreateShape(Transform _building, SBuildingTemplate _template)
    {
        Debug.Log($"Create shape of {_template.slug}");
        SCommonTemplate data = new(_template);

        BuildFloor(_building, data, Vector2.zero, false);
        BuildWalls(_building, data);

        TMPro.TextMeshPro nameText = _building.GetComponent<Building>().nameText;
        nameText.transform.localPosition = new(data.isolationPoleCenter.x, 0.003f, data.isolationPoleCenter.y);
        nameText.rectTransform.sizeDelta = Mathf.Sqrt(2) * data.isolationPoleRadius * Vector2.one;
    }

    /// <summary>
    /// Build the walls and floor of a non convex room then place it
    /// </summary>
    /// <param name="_room">The transform of the room's floor</param>
    /// <param name="_template">The template of the non convex room</param>
    public void CreateShape(Transform _room, SRoomTemplate _template)
    {
        Debug.Log($"Create shape of {_template.slug}");
        SCommonTemplate data = new(_template);

        BuildFloor(_room, data, data.offset, _template.floorUnit == LengthUnit.Tile);
        _room.GetChild(0).localPosition += new Vector3(0, 0.001f, 0);
        BuildWalls(_room, data);
        TMPro.TextMeshPro nameText = _room.GetComponent<Room>().nameText;
        nameText.transform.localPosition = new(data.isolationPoleCenter.x, 0.003f, data.isolationPoleCenter.y);
        nameText.rectTransform.sizeDelta = Mathf.Sqrt(2) * data.isolationPoleRadius * Vector2.one;
    }

    /// <summary>
    /// Build the walls of a non convex building or room
    /// </summary>
    /// <param name="_root">the transform of the building's / room's floor</param>
    /// <param name="_template">the template of the non convex room</param>
    private void BuildWalls(Transform _root, SCommonTemplate _template)
    {
        float height = _template.sizeWDHm[2];
        int vCount = _template.vertices.Count;
        Vector3[] verticesRoomWalls = new Vector3[4 * (vCount + 1)];
        int[] trianglesRoomWalls = new int[6 * (vCount + 1)];
        float[] xWalls = new float[vCount];
        float[] zWalls = new float[vCount];

        Transform walls = _root.GetComponent<Building>().walls.GetChild(0);
        Mesh meshWalls = new() { name = "meshWalls" };
        walls.GetComponent<MeshFilter>().mesh = meshWalls;
        walls.GetComponent<MeshCollider>().sharedMesh = meshWalls;

        for (int i = 0; i < vCount; i++)
        {
            xWalls[i] = _template.vertices[i][0];
            zWalls[i] = _template.vertices[i][1];
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
    /// <param name="_root">the transform of the room's floor</param>
    /// <param name="_template">the template of the non convex room</param>
    /// <param name="_tiles">if true, build the tiles from the template's tiles field</param>
    /// <param name="_offset">Position of the first vertice</param>
    private void BuildFloor(Transform _root, SCommonTemplate _template, Vector2 _offset, bool _tiles)
    {
        List<int> trianglesRoom = new();

        List<Vector3> walls = _template.vertices.Select(w => new Vector3(w.x, 0, w.y)).ToList();
        List<Vector3> shrinkingWalls = new(walls);
        Transform floor = _root.GetChild(0);
        Mesh meshFloor = new() { name = "meshFloor" };
        floor.GetComponent<MeshFilter>().mesh = meshFloor;
        int index = 0;
        Vector3 A, B, C;
        int failSafe = shrinkingWalls.Count;
        while (shrinkingWalls.Count >= 3)
        {
            A = shrinkingWalls.NextIndex(index, 0);
            B = shrinkingWalls.NextIndex(index, 1);
            C = shrinkingWalls.NextIndex(index, 2);

            //For debugging purposes
            //string s = "";
            //shrinkingWalls.ForEach(t => s += t.ToString() + ", ");
            if (ClockWise(A, B, C) && !TriangleIntersectWalls(A, B, C, walls))
            {
                trianglesRoom.Add(walls.IndexOf(A));
                trianglesRoom.Add(walls.IndexOf(B));
                trianglesRoom.Add(walls.IndexOf(C));

                shrinkingWalls.Remove(B);
                failSafe = shrinkingWalls.Count;
            }
            if (failSafe == 0)
            {
                GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Invalid vertex array in template", _template.slug), ELogTarget.both, ELogtype.error);
                break;
            }

            failSafe--;
            index++;
        }
        meshFloor.vertices = walls.ToArray();
        meshFloor.triangles = trianglesRoom.ToArray();
        meshFloor.RecalculateNormals();

        floor.GetComponent<MeshCollider>().sharedMesh = meshFloor;
        floor.GetComponent<MeshCollider>().convex = false;

        //For building only : roof
        Transform roof = _root.Find("Roof");
        if (roof)
        {
            roof.GetComponent<MeshFilter>().mesh = meshFloor;
            roof.GetComponent<MeshCollider>().sharedMesh = meshFloor;
            roof.GetComponent<MeshCollider>().convex = false;
        }

        if (_tiles)
        {
            OgreeObject site = null;
            if (_root.transform.parent && _root.transform.parent.parent)
                site = _root.transform.parent.parent.GetComponentInParent<OgreeObject>();
            for (int i = 0; i < _template.tiles.Count; i++)
            {
                string[] separated = _template.tiles[i].location.Split('/');
                float x = Utils.ParseDecFrac(separated[0]);
                float z = Utils.ParseDecFrac(separated[1]);

                separated = _template.tiles[i].label.Split('/');
                float xCoord = Utils.ParseDecFrac(separated[0]);
                float zCoord = Utils.ParseDecFrac(separated[1]);

                // Tiles           
                GameObject newTile = Object.Instantiate(GameManager.instance.tileModel, floor);
                newTile.name = $"Tile_{_template.tiles[i].label}";
                newTile.transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text = _template.tiles[i].label;
                newTile.transform.localPosition = new(x - _offset.x, 0.001f, z - _offset.y);

                Tile tile = newTile.GetComponent<Tile>();
                tile.color = _template.tiles[i].color;
                tile.texture = _template.tiles[i].texture;
                tile.coord = new(xCoord, zCoord);
                if (site && site.attributes.ContainsKey("usableColor"))
                    newTile.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{site.attributes["usableColor"]}");
                else
                    newTile.GetComponent<Renderer>().material.color = Utils.ParseHtmlColor(GameManager.instance.configHandler.GetColor("usableZone"));
                tile.defaultMat = new(newTile.GetComponent<Renderer>().material);
            }

        }
    }

    /// <summary>
    /// Check if a list vertices defining a polygon is listed <i>mostly</i> clockwise
    /// <br/> Mostly because, if the polygon is non-convex, it can be not just clockwise or non-clockwise
    /// <br/><see href="https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order"/>
    /// </summary>
    /// <param name="_points">the list vertices defining a polygon</param>
    /// <returns>True if the list is mostly clockwise</returns>
    private static bool MostlyClockWise(List<Vector3> _points)
    {
        float sum = 0;
        for (int i = 0; i < _points.Count; i++)
            sum += (_points.NextIndex(i).x - _points[i].x) * (_points.NextIndex(i).z + _points[i].z);
        return sum > 0;
    }

    /// <summary>
    /// Check if three points are listed in clockwise order 
    /// <br/><b>IGNORE VERTICAL COMPONENT</b>
    /// <br/><see href="https://bryceboe.com/2006/10/23/line-segment-intersection-algorithm/"/>
    /// </summary>
    /// <param name="_a">the first point</param>
    /// <param name="_b">the second point</param>
    /// <param name="_c">the third point</param>
    /// <returns>true if <paramref name="_a"/>, <paramref name="_b"/> and <paramref name="_c"/> are in a clockwise order</returns>
    private bool ClockWise(Vector3 _a, Vector3 _b, Vector3 _c)
    {
        return (_c.z - _a.z) * (_b.x - _a.x) < (_b.z - _a.z) * (_c.x - _a.x);
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
    private bool Intersect(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d)
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
    private bool TriangleIntersectWalls(Vector3 _corner1, Vector3 _corner2, Vector3 _corner3, List<Vector3> _walls)
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

