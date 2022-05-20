using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRack : MonoBehaviour
{

    public float tiltAngle = 180;

    ///<summary>
    /// Rotate a rack from 180° if it is selected or if one of its children are selected.
    ///</summary>
    ///<param name="_height">World position y to place the grid</param>
    public void Rotate()
    {
        Transform t = GameManager.gm.currentItems[0].transform;

        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack or one of its child to rotate it", "yellow");
        }

        else
        {
            while(t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    Vector3 rotationToAdd = new Vector3(t.eulerAngles.x, t.eulerAngles.y + tiltAngle, t.eulerAngles.z);
                    t.transform.eulerAngles = rotationToAdd;
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
        }
    }
}
