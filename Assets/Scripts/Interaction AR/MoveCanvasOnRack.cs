using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCanvasOnRack : MonoBehaviour
{
    public void MoveCanvas()
    {
        GameObject rack = GameManager.gm.currentItems[0];
        OgreeObject rackOgree = rack.GetComponent<OgreeObject>();
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack to rotate it", "yellow");
        }
        else
        {
            if (rackOgree.category == "rack")
            {
                Vector3 RackPosition = rack.transform.position;
                transform.position = transform.position + RackPosition;
            }
            else
            {
                GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
            }
        }
    }
}
