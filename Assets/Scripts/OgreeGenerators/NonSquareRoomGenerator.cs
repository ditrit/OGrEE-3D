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
        // _room.GetComponent<MeshFilter>().mesh = new Mesh();
        // _room.GetComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

        float height = _template.sizeWDHm[2];

        Vector3 center = 0.6f * (new Vector3(_template.center[0], _template.center[1], _template.center[2]));
        center += new Vector3(0, 0.001f, 0);

        List<List<int>> verticesWalls = _template.vertices;

        int m = verticesWalls.Count;

        int n = _template.tiles.Length;

        if (_template.floorUnit == "t")
        {
            Vector3[] verticesRoomFloor = new Vector3[4 * n];
            Vector3[] verticesRoomWalls = new Vector3[4 * (m + 1)];

            int[] trianglesRoomFloor = new int[6 * n];
            int[] trianglesRoomWalls = new int[6 * (m + 1)];

            int[] xlist = new int[n];
            int[] zlist = new int[n];

            GameObject labels = new GameObject("Labels");
            labels.transform.parent = _room.transform;
            GameObject allTiles = new GameObject("Tiles");
            allTiles.transform.parent = _room.transform;
            labels.transform.position = center;
            allTiles.transform.position = center;

            GameObject floor = new GameObject();
            Mesh meshFloor = new Mesh();
            floor.name = "Floor";
            floor.transform.position = center;
            floor.transform.parent = _room.transform;
            floor.transform.localPosition = Vector3.zero;
            floor.AddComponent<MeshFilter>().mesh = meshFloor;
            floor.AddComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

            GameObject walls = new GameObject();
            Mesh meshWalls = new Mesh();
            walls.name = "Walls";
            walls.transform.position = center;
            walls.transform.parent = _room.transform;
            walls.transform.localPosition = new Vector3(0, 0, 0);
            walls.AddComponent<MeshFilter>().mesh = meshWalls;
            walls.AddComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

            for (int i = 0; i < n; i++)
            {
                string element = _template.tiles[i].location;
                string[] separated = new string[2];
                separated = element.Split('/');

                int x = int.Parse(separated[0]);
                int z = int.Parse(separated[1]);

                xlist[i] = x;
                zlist[i] = z;

                // Tiles           

                GameObject tile = Instantiate(GameManager.gm.tileModel, allTiles.transform);
                tile.name = "Tile : " + _template.tiles[i].location;
                tile.transform.localPosition = 0.6f * (new Vector3(x, 0, z)) - center;
                // Labels

                GameObject label = new GameObject("Label");
                label.transform.position = 0.6f * (new Vector3(x, 0, z));
                TextMesh affichage = label.AddComponent<TextMesh>();
                if (string.IsNullOrEmpty(_template.tiles[i].label))
                {
                    affichage.text = _template.tiles[i].location;
                }
                else
                {
                    affichage.text = _template.tiles[i].label;
                }
                affichage.characterSize = 0.2f;
                affichage.fontSize = 8;
                affichage.color = Color.black;
                affichage.alignment = TextAlignment.Center;
                affichage.anchor = TextAnchor.MiddleCenter;
                affichage.transform.localPosition = 0.6f * (new Vector3(x, 0.5f, z));
                affichage.transform.parent = label.transform;
                label.transform.parent = labels.transform;

                // Room - Floor

                verticesRoomFloor[4 * i] = new Vector3(xlist[i] * 0.6f - 0.3f, 0, zlist[i] * 0.6f - 0.3f) - center;
                verticesRoomFloor[4 * i + 1] = new Vector3(xlist[i] * 0.6f - 0.3f, 0, (zlist[i] + 1) * 0.6f - 0.3f) - center;
                verticesRoomFloor[4 * i + 2] = new Vector3((xlist[i] + 1) * 0.6f - 0.3f, 0, zlist[i] * 0.6f - 0.3f) - center;
                verticesRoomFloor[4 * i + 3] = new Vector3((xlist[i] + 1) * 0.6f - 0.3f, 0, (zlist[i] + 1) * 0.6f - 0.3f) - center;

                trianglesRoomFloor[6 * i] = 4 * i;
                trianglesRoomFloor[6 * i + 1] = 4 * i + 1;
                trianglesRoomFloor[6 * i + 2] = 4 * i + 2;
                trianglesRoomFloor[6 * i + 3] = 4 * i + 1;
                trianglesRoomFloor[6 * i + 4] = 4 * i + 3;
                trianglesRoomFloor[6 * i + 5] = 4 * i + 2;
            }

            meshFloor.vertices = verticesRoomFloor;
            meshFloor.triangles = trianglesRoomFloor;

            // Room - Walls

            float[] xWalls = new float[m];
            float[] zWalls = new float[m];

            for (int i = 0; i < m; i++)
            {
                xWalls[i] = (float)verticesWalls[i][0] / 100f;
                zWalls[i] = (float)verticesWalls[i][1] / 100f;
            }
            Debug.Log("test");

            for (int i = 0; i < m - 1; i++)
            {
                verticesRoomWalls[4 * i] = new Vector3(xWalls[i] - 0.3f, 0, zWalls[i] - 0.3f) - center;
                verticesRoomWalls[4 * i + 1] = new Vector3(xWalls[i + 1] - 0.3f, 0, zWalls[i + 1] - 0.3f) - center;
                verticesRoomWalls[4 * i + 2] = new Vector3(xWalls[i] - 0.3f, height, zWalls[i] - 0.3f) - center;
                verticesRoomWalls[4 * i + 3] = new Vector3(xWalls[i + 1] - 0.3f, height, zWalls[i + 1] - 0.3f) - center;

            }
            Debug.Log("test");
            verticesRoomWalls[4 * (m - 1)] = new Vector3(xWalls[m - 1] - 0.3f, 0, zWalls[m - 1] - 0.3f) - center;
            verticesRoomWalls[4 * (m - 1) + 1] = new Vector3(xWalls[0] - 0.3f, 0, zWalls[0] - 0.3f) - center;
            verticesRoomWalls[4 * (m - 1) + 2] = new Vector3(xWalls[m - 1] - 0.3f, height, zWalls[m - 1] - 0.3f) - center;
            verticesRoomWalls[4 * (m - 1) + 3] = new Vector3(xWalls[0] - 0.3f, height, zWalls[0] - 0.3f) - center;
            Debug.Log("test");
            for (int i = 0; i < m + 1; i++)
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
        else
        {
            List<Vector3> verticesRoom2 = new List<Vector3>();
            List<int> trianglesRoom2 = new List<int>();

            List<Vector3> verticesWalls2 = new List<Vector3>();
            List<int> trianglesWalls2 = new List<int>();

            GameObject floor = new GameObject();
            Mesh meshFloor = new Mesh();
            floor.name = "Floor";
            floor.transform.position = center;
            floor.transform.parent = _room.transform;
            floor.transform.localPosition = new Vector3(0, -0.01f, 0);
            floor.AddComponent<MeshFilter>().mesh = meshFloor;
            floor.AddComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

            GameObject walls = new GameObject();
            Mesh meshWalls = new Mesh();
            walls.name = "Walls";
            walls.transform.position = center;
            walls.transform.parent = _room.transform;
            walls.transform.localPosition = new Vector3(0, 0, 0);
            walls.AddComponent<MeshFilter>().mesh = meshWalls;
            walls.AddComponent<MeshRenderer>().material = GameManager.gm.defaultMat;

            float[] xWalls = new float[m];
            float[] zWalls = new float[m];

            for (int i = 0; i < m; i++)
            {
                xWalls[i] = (float)verticesWalls[i][0] / 100f;
                zWalls[i] = (float)verticesWalls[i][1] / 100f;
            }
            for (int i = 0; i < m - 1; i++)
            {
                verticesWalls2.Add(new Vector3(xWalls[i] - 0.3f, 0, zWalls[i] - 0.3f) - center);
                verticesWalls2.Add(new Vector3(xWalls[i + 1] - 0.3f, 0, zWalls[i + 1] - 0.3f) - center);
                verticesWalls2.Add(new Vector3(xWalls[i] - 0.3f, height, zWalls[i] - 0.3f) - center);
                verticesWalls2.Add(new Vector3(xWalls[i + 1] - 0.3f, height, zWalls[i + 1] - 0.3f) - center);
            }
            verticesWalls2.Add(new Vector3(xWalls[m - 1] - 0.3f, 0, zWalls[m - 1] - 0.3f) - center);
            verticesWalls2.Add(new Vector3(xWalls[0] - 0.3f, 0, zWalls[0] - 0.3f) - center);
            verticesWalls2.Add(new Vector3(xWalls[m - 1] - 0.3f, height, zWalls[m - 1] - 0.3f) - center);
            verticesWalls2.Add(new Vector3(xWalls[0] - 0.3f, height, zWalls[0] - 0.3f) - center);
            for (int i = 0; i < m; i++)
            {
                trianglesWalls2.Add(4 * i);
                trianglesWalls2.Add(4 * i + 2);
                trianglesWalls2.Add(4 * i + 1);
                trianglesWalls2.Add(4 * i + 1);
                trianglesWalls2.Add(4 * i + 2);
                trianglesWalls2.Add(4 * i + 3);
            }

            meshWalls.vertices = verticesWalls2.ToArray();
            meshWalls.triangles = trianglesWalls2.ToArray();

            List<List<int>> verticesClone = new List<List<int>>(verticesWalls);

            while (verticesClone.Count > 3)
            {
                for (int i = 0; i < verticesClone.Count; i++)
                {
                    int l = verticesClone.Count;

                    int a = i;

                    for (int j = 1; j < l - i - 2; j++)
                    {
                        int b = i + j;
                        int c = i + j + 1;

                        bool convex = true;

                        Vector3 vertexA = new Vector3(verticesClone[a][0], 0, verticesClone[a][1]);
                        Vector3 vertexB = new Vector3(verticesClone[b][0], 0, verticesClone[b][1]);
                        Vector3 vertexC = new Vector3(verticesClone[c][0], 0, verticesClone[c][1]);

                        double angle = AngleOffAroundAxis(vertexA - vertexB, vertexC - vertexB, new Vector3(0, 1, 0));

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
                                {
                                    break;
                                }
                            }
                        }
                        if (convex)
                        {
                            int ind = verticesRoom2.Count;
                            verticesRoom2.Add(new Vector3(verticesClone[a][0] / 100f - 0.3f, 0, verticesClone[a][1] / 100f - 0.3f) - center);
                            verticesRoom2.Add(new Vector3(verticesClone[b][0] / 100f - 0.3f, 0, verticesClone[b][1] / 100f - 0.3f) - center);
                            verticesRoom2.Add(new Vector3(verticesClone[c][0] / 100f - 0.3f, 0, verticesClone[c][1] / 100f - 0.3f) - center);

                            trianglesRoom2.Add(ind);
                            trianglesRoom2.Add(ind + 1);
                            trianglesRoom2.Add(ind + 2);

                            verticesClone.RemoveAt(b);

                            break;
                        }
                    }
                    if (l != verticesClone.Count)
                    {
                        break;
                    }
                }
            }

            int taille = verticesRoom2.Count;
            verticesRoom2.Add(new Vector3(verticesClone[0][0] / 100f - 0.3f, 0, verticesClone[0][1] / 100f - 0.3f) - center);
            verticesRoom2.Add(new Vector3(verticesClone[1][0] / 100f - 0.3f, 0, verticesClone[1][1] / 100f - 0.3f) - center);
            verticesRoom2.Add(new Vector3(verticesClone[2][0] / 100f - 0.3f, 0, verticesClone[2][1] / 100f - 0.3f) - center);

            trianglesRoom2.Add(taille);
            trianglesRoom2.Add(taille + 2);
            trianglesRoom2.Add(taille + 1);

            meshFloor.vertices = verticesRoom2.ToArray();
            meshFloor.triangles = trianglesRoom2.ToArray();
        }
    }

    private void BuildWalls(Transform _root)
    {

    }

    private void buildFloor()
    {

    }

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
