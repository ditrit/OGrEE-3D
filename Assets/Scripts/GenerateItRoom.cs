using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateItRoom : MonoBehaviour
{
    public void CreateItRoom(string _name, Vector2 _size, Vector2 _margin)
    {
        float u = GameManager.gm.tileModel.transform.GetChild(0).transform.localScale.x * 10;
        // Debug.Log($"[GenerateFloor] tile unit:{u}");

        GameObject tile = Instantiate(GameManager.gm.tileModel);
        tile.name = _name;

        Vector3 originalSize = tile.transform.GetChild(0).localScale;
        tile.transform.GetChild(0).localScale = new Vector3(originalSize.x * _size.x, originalSize.y, originalSize.z * _size.y);
        Vector3 tmpPos = tile.transform.GetChild(0).localScale / 0.2f;
        tile.transform.GetChild(0).localPosition = new Vector3(tmpPos.x, 0, tmpPos.z);

        Object obj = tile.AddComponent<Object>();
        obj.type = EObjectType.Itroom;
        obj.size = _size;
        obj.sizeUnit = EUnit.tile;
        obj.technical = new SMargin(_margin.x, 0, 0, _margin.y);
        obj.reserved = new SMargin();

        // Lights
        // GameObject lightsRoot = new GameObject("lightsRoot");
        // lightsRoot.transform.parent = tile.transform;
        
        // for (int z = 0; z < _size.y; z++)
        // {
        //     for (int x = 0; x < _size.x; x++)
        //     {
        //         if (x % 3 == 0 && z % 2 == 0)
        //         {
        //             GameObject lightObj = new GameObject("Light");
        //             lightObj.transform.localPosition = new Vector3(x * u + 1, 3, z * u);
        //             lightObj.transform.parent = lightsRoot.transform;
        //             Light light = lightObj.AddComponent<Light>();
        //             light.type = LightType.Point;
        //             light.intensity = 0.5f;
        //             light.shadows = LightShadows.Hard;
        //         }
        //     }
        // }

    }
}
