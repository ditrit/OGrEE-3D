using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateFloor : MonoBehaviour
{
    public int length;
    public int width;
    public EOrientation orientation;

    private Transform tilesRoot = null;

    private void Start()
    {
        tilesRoot = transform.Find("TilesRoot");
        CreateFloor(new Vector2(length, width));

    }

    public void CreateFloor(Vector2 _size)
    {
        float u = GameManager.gm.tileModel.transform.GetChild(0).transform.localScale.x * 10;
        // Debug.Log($"[GenerateFloor] tile unit:{u}");

        for (int z = 0; z < _size.y; z++)
        {
            for (int x = 0; x < _size.x; x++)
            {
                Instantiate(GameManager.gm.tileModel, new Vector3(x * u, 0, z * u), Quaternion.identity, tilesRoot);
                if (x % 3 == 0 && z % 2 == 0)
                {
                    GameObject tmp = new GameObject("Light");
                    tmp.transform.localPosition = new Vector3(x * u, 3, z * u);
                    tmp.transform.parent = tilesRoot;
                    Light light = tmp.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.intensity = 0.3f;
                }
            }
        }

        switch (orientation)
        {
            case EOrientation.N:
                transform.localEulerAngles = new Vector3(0, 0, 0);
                // Camera.main.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case EOrientation.S:
                transform.localEulerAngles = new Vector3(0, 180, 0);
                // Camera.main.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case EOrientation.E:
                transform.localEulerAngles = new Vector3(0, 90, 0);
                // Camera.main.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case EOrientation.W:
                transform.localEulerAngles = new Vector3(0, -90, 0);
                // Camera.main.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
        }
    }

    public void DeleteFloor()
    {
        foreach (Transform child in tilesRoot)
            Destroy(child.gameObject);

    }

}
