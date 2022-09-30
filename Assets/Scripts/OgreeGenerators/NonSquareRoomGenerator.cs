using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonSquareRoomGenerator : MonoBehaviour
{
    public static NonSquareRoomGenerator instance;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    public void CreateShape(GameObject _room, ReadFromJson.SRoomFromJson _template)
    {
        Debug.Log($"Create shape of {_template.slug}");
        // _room.GetComponent<MeshFilter>().mesh = new Mesh(); // needed ?
        // _room.GetComponent<MeshRenderer>().material = GameManager.gm.defaultMat; // needed ?

        // Vector3 center = 0.6f * (new Vector3(_template.center[0], _template.center[1], _template.center[2]));
        // center += new Vector3(0, 0.001f, 0);
        // Vector3 center = new Vector3(GameManager.gm.uSize / 2, 0001f, GameManager.gm.uSize / 2);
        Vector3 center = new Vector3(0, 0001f, 0);

        if (_template.floorUnit == "t")
            BuildFloor(_room.transform, _template, center, true);
        else
            BuildFloor(_room.transform, _template, center, false);
        BuildWalls(_room.transform, _template, center);
    }

    ///
    private void BuildWalls(Transform _root, ReadFromJson.SRoomFromJson _template, Vector3 _center)
    {
        float height = _template.sizeWDHm[2];
        int vCount = _template.vertices.Count;
        Vector3[] verticesRoomWalls = new Vector3[4 * (vCount + 1)];
        int[] trianglesRoomWalls = new int[6 * (vCount + 1)];
        float[] xWalls = new float[vCount];
        float[] zWalls = new float[vCount];

        GameObject walls = new GameObject("Walls");
        Mesh meshWalls = new Mesh();
        walls.transform.parent = _root.transform;
        walls.transform.localPosition = new Vector3(0, height / 2, 0);
        walls.AddComponent<MeshFilter>().mesh = meshWalls;
        walls.AddComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

        for (int i = 0; i < vCount; i++)
        {
            xWalls[i] = (float)_template.vertices[i][0] / 100f;
            zWalls[i] = (float)_template.vertices[i][1] / 100f;
        }

        for (int i = 0; i < vCount - 1; i++)
        {
            verticesRoomWalls[4 * i] = new Vector3(xWalls[i], 0, zWalls[i]) - _center;
            verticesRoomWalls[4 * i + 1] = new Vector3(xWalls[i + 1], 0, zWalls[i + 1]) - _center;
            verticesRoomWalls[4 * i + 2] = new Vector3(xWalls[i], height, zWalls[i]) - _center;
            verticesRoomWalls[4 * i + 3] = new Vector3(xWalls[i + 1], height, zWalls[i + 1]) - _center;
        }

        verticesRoomWalls[4 * (vCount - 1)] = new Vector3(xWalls[vCount - 1], 0, zWalls[vCount - 1]) - _center;
        verticesRoomWalls[4 * (vCount - 1) + 1] = new Vector3(xWalls[0], 0, zWalls[0]) - _center;
        verticesRoomWalls[4 * (vCount - 1) + 2] = new Vector3(xWalls[vCount - 1], height, zWalls[vCount - 1]) - _center;
        verticesRoomWalls[4 * (vCount - 1) + 3] = new Vector3(xWalls[0], height, zWalls[0]) - _center;

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
    }

    ///
    private void BuildFloor(Transform _root, ReadFromJson.SRoomFromJson _template, Vector3 _center, bool _tiles)
    {
        List<List<int>> verticesClone = new List<List<int>>(_template.vertices);
        List<Vector3> verticesRoom = new List<Vector3>();
        List<int> trianglesRoom = new List<int>();

        GameObject floor = new GameObject("Floor");
        Mesh meshFloor = new Mesh();
        floor.transform.parent = _root;
        floor.transform.localPosition = Vector3.zero;
        floor.AddComponent<MeshFilter>().mesh = meshFloor;
        floor.AddComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

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
                        verticesRoom.Add(new Vector3(verticesClone[a][0] / 100f, 0, verticesClone[a][1] / 100f) - _center);
                        verticesRoom.Add(new Vector3(verticesClone[b][0] / 100f, 0, verticesClone[b][1] / 100f) - _center);
                        verticesRoom.Add(new Vector3(verticesClone[c][0] / 100f, 0, verticesClone[c][1] / 100f) - _center);

                        trianglesRoom.Add(ind);
                        trianglesRoom.Add(ind + 1);
                        trianglesRoom.Add(ind + 2);

                        verticesClone.RemoveAt(b);

                        break;
                    }
                }
                if (l != verticesClone.Count)
                    break;
            }
        }
        int length = verticesRoom.Count;
        verticesRoom.Add(new Vector3(verticesClone[0][0] / 100f, 0, verticesClone[0][1] / 100f) - _center);
        verticesRoom.Add(new Vector3(verticesClone[1][0] / 100f, 0, verticesClone[1][1] / 100f) - _center);
        verticesRoom.Add(new Vector3(verticesClone[2][0] / 100f, 0, verticesClone[2][1] / 100f) - _center);

        trianglesRoom.Add(length);
        trianglesRoom.Add(length + 2);
        trianglesRoom.Add(length + 1);

        meshFloor.vertices = verticesRoom.ToArray();
        meshFloor.triangles = trianglesRoom.ToArray();

        if (_tiles)
        {
            GameObject allTiles = new GameObject("Tiles");
            allTiles.transform.parent = _root.transform;
            allTiles.transform.localPosition = Vector3.zero;

            GameObject labels = new GameObject("Labels");
            labels.transform.parent = _root.transform;
            labels.transform.localPosition = Vector3.zero;

            int n = _template.tiles.Length;
            for (int i = 0; i < n; i++)
            {
                string element = _template.tiles[i].location;
                string[] separated = new string[2];
                separated = element.Split('/');

                int x = int.Parse(separated[0]);
                int z = int.Parse(separated[1]);

                // Tiles           
                GameObject tile = Instantiate(GameManager.gm.tileModel, allTiles.transform);
                tile.name = "Tile : " + _template.tiles[i].location;
                tile.transform.localPosition = 0.6f * (new Vector3(x, 0, z)) - _center;

                // Labels <= TO REFACTOR WITH TMP
                // GameObject label = new GameObject("Label");
                // label.transform.position = 0.6f * (new Vector3(x, 0, z));
                // TextMesh affichage = label.AddComponent<TextMesh>();
                // if (string.IsNullOrEmpty(_template.tiles[i].label))
                //     affichage.text = _template.tiles[i].location;
                // else
                //     affichage.text = _template.tiles[i].label;
                // affichage.characterSize = 0.2f;
                // affichage.fontSize = 8;
                // affichage.color = Color.black;
                // affichage.alignment = TextAlignment.Center;
                // affichage.anchor = TextAnchor.MiddleCenter;
                // affichage.transform.localPosition = 0.6f * (new Vector3(x, 0.5f, z));
                // affichage.transform.parent = label.transform;
                // label.transform.parent = labels.transform;
            }
        }
    }

    ///
    private double AngleOffAroundAxis(Vector3 a, Vector3 b, Vector3 axis, bool clockwise = true)
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
    private bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
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
