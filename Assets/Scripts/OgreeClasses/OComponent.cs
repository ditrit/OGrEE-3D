using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OComponent : Device
{
    public ECompCategory category;
    public string location; // logical name of the component

    public OComponent()
    {
        role = EDeviceRole.child;
    }
}
