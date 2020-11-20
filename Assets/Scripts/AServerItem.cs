using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AServerItem : MonoBehaviour
{
    public int id;

    public void UpdateId(int _id)
    {
        id = _id;
    }
}
