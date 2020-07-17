using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : Building
{
    // ITROOM : technical <> null + reserved <> null + WDunit = tile 
    // ROOM (AC + power): technical = 0 + reserved = 0 + WDunit = cm / inch / tile

    public EOrientation orientation;

    public SMargin technical;
    public SMargin reserved;
    public float floorHeight;
    public EUnit floorUnit;

}
