using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    ///
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

    ///
    private static void BuildFloor(Transform _root, ReadFromJson.SRoomFromJson _template, bool _tiles)
    {
        List<List<int>> verticesClone = new List<List<int>>(_template.vertices);
        List<Vector3> verticesRoom = new List<Vector3>();
        List<int> trianglesRoom = new List<int>();

        Transform floor = _root.GetComponent<Room>().usableZone;
        Mesh meshFloor = new Mesh { name = "meshFloor" };
        floor.GetComponent<MeshFilter>().mesh = meshFloor;

        while (verticesClone.Count > 3)
        {
            for (int i = 0; i < verticesClone.Count; i++)
            {
                int a = i;
                int l = verticesClone.Count;
                for (int j = 1; j < l - i - 2; j++)
                {
                    int b = i + j;
                    int c = i + j + 1;

                    bool convex = true;

                    Vector3 vertexA = new Vector3(verticesClone[a][0], 0, verticesClone[a][1]);
                    Vector3 vertexB = new Vector3(verticesClone[b][0], 0, verticesClone[b][1]);
                    Vector3 vertexC = new Vector3(verticesClone[c][0], 0, verticesClone[c][1]);

                    double angle = AngleOffAroundAxis(vertexA - vertexB, vertexC - vertexB, Vector3.up);

                    if (angle < 0)
                    {
                        convex = false;
                        break;
                    }
                    for (int k = 0; k < l; k++)
                    {
                        if (k != a && k != b && k != c)
                        {
                            for (int h = 0; h < l; h++)
                            {
                                if (h != a && h != b && h != c && h != k)
                                {
                                    Vector2 vA = new Vector2(verticesClone[a][0], verticesClone[a][1]);
                                    Vector2 vB = new Vector2(verticesClone[b][0], verticesClone[b][1]);
                                    Vector2 vC = new Vector2(verticesClone[c][0], verticesClone[c][1]);
                                    Vector2 vK = new Vector2(verticesClone[k][0], verticesClone[k][1]);
                                    Vector2 vH = new Vector2(verticesClone[h][0], verticesClone[h][1]);
                                    if (LineSegmentsIntersection(vA, vB, vK, vH) || LineSegmentsIntersection(vA, vC, vK, vH) || LineSegmentsIntersection(vertexC, vB, vK, vH))
                                    {
                                        convex = false;
                                        break;
                                    }
                                }
                            }
                            if (!convex)
                                break;
                        }
                    }
                    if (convex)
                    {
                        int ind = verticesRoom.Count;
                        verticesRoom.Add(new Vector3(verticesClone[a][0] / 100f, 0, verticesClone[a][1] / 100f));
                        verticesRoom.Add(new Vector3(verticesClone[b][0] / 100f, 0, verticesClone[b][1] / 100f));
                        verticesRoom.Add(new Vector3(verticesClone[c][0] / 100f, 0, verticesClone[c][1] / 100f));

                        trianglesRoom.Add(ind);
                        trianglesRoom.Add(ind + 2);
                        trianglesRoom.Add(ind + 1);

                        verticesClone.RemoveAt(b);

                        break;
                    }
                }
                if (l != verticesClone.Count)
                    break;
            }
        }
        int length = verticesRoom.Count;
        verticesRoom.Add(new Vector3(verticesClone[0][0] / 100f, 0, verticesClone[0][1] / 100f));
        verticesRoom.Add(new Vector3(verticesClone[1][0] / 100f, 0, verticesClone[1][1] / 100f));
        verticesRoom.Add(new Vector3(verticesClone[2][0] / 100f, 0, verticesClone[2][1] / 100f));

        trianglesRoom.Add(length);
        trianglesRoom.Add(length + 2);
        trianglesRoom.Add(length + 1);

        meshFloor.vertices = verticesRoom.ToArray();
        meshFloor.triangles = trianglesRoom.ToArray();
        meshFloor.RecalculateNormals();

        floor.GetComponent<MeshCollider>().sharedMesh = meshFloor;
        floor.GetComponent<MeshCollider>().convex = false;

        if (_tiles)
        {
            int n = _template.tiles.Length;
            for (int i = 0; i < n; i++)
            {
                string element = _template.tiles[i].location;
                string[] separated = new string[2];
                separated = element.Split('/');

                int x = int.Parse(separated[0]);
                int z = int.Parse(separated[1]);

                // Tiles           
                GameObject tile = Object.Instantiate(GameManager.gm.tileModel, floor);
                tile.name = $"Tile_{_template.tiles[i].location}";
                tile.transform.localPosition = 0.6f * (new Vector3(x, 0, z));
                tile.transform.localPosition += new Vector3(GameManager.gm.tileSize, 0.001f, GameManager.gm.tileSize) / 2;
            }
        }
    }

    ///
    private static double AngleOffAroundAxis(Vector3 a, Vector3 b, Vector3 axis, bool clockwise = true)
    {
        Vector3 right;
        if (clockwise)
        {
            right = Vector3.Cross(b, axis);
            b = Vector3.Cross(axis, right);
        }
        else
        {
            right = Vector3.Cross(axis, b);
            b = Vector3.Cross(right, axis);
        }
        return Mathf.Atan2(Vector3.Dot(a, right), Vector3.Dot(a, b)) * 180 / Mathf.PI;
    }

    ///
    private static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
        if (d == 0.0f)
            return false;

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;
        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
            return false;

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);
        return true;
    }
}
