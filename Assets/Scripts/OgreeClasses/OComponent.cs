using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OComponent : Device
{
    public ECompCategory category;
    public string location; // logical name of the component

    private void Awake()
    {
        role = EDeviceRole.child;
    }
}
