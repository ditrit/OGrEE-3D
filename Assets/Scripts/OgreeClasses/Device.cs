using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : Object
{
    public EDeviceRole role;

    public Device()
    {
        family = EObjFamily.device;
    }
}
