﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AServerItem : MonoBehaviour
{
    public new string name;
    public int id;
    public int parentId;

    public void UpdateId(int _id)
    {
        id = _id;
    }
}
