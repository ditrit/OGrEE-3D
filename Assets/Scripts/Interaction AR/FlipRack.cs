using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipRack : MonoBehaviour
{

    public float tiltAngle = 180;

    public void Rotate()
    {
        OgreeObject rack = GameManager.gm.currentItems[0].GetComponent<OgreeObject>();
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack to rotate it", "yellow");
        }

        else
        {
            if (rack.category == "rack")
            {
                Vector3 rotationToAdd = new Vector3(rack.transform.eulerAngles.x, rack.transform.eulerAngles.y + tiltAngle, rack.transform.eulerAngles.z);
                rack.transform.eulerAngles = rotationToAdd;
            }

            else
            {
                GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
            }
        }
    }
}