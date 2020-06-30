using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationTest : MonoBehaviour
{
    public bool isRoom = false;
    public EOrientation orientation;
    public EObjOrient roomOrientation;

    public bool reorient = false;

    private void Start()
    {
        OrientObj();
    }

    private void Update()
    {
        if (reorient)
        {
            OrientObj();
            reorient = false;
        }
    }

    private void OrientObj()
    {
        if (isRoom)
        {
            switch (orientation)
            {
                case EOrientation.N:
                    transform.localEulerAngles = new Vector3(0, -180, 0);
                    break;
                case EOrientation.S:
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                    break;
                case EOrientation.E:
                    transform.localEulerAngles = new Vector3(0, -90, 0);
                    break;
                case EOrientation.W:
                    transform.localEulerAngles = new Vector3(0, 90, 0);
                    break;
            }
        }
        else
        {
            switch (roomOrientation)
            {
                case EObjOrient.Frontward:
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                    break;
                case EObjOrient.Backward:
                    transform.localEulerAngles = new Vector3(0, 180, 0);
                    break;
                case EObjOrient.Left:
                    transform.localEulerAngles = new Vector3(0, -90, 0);
                    break;
                case EObjOrient.Right:
                    transform.localEulerAngles = new Vector3(0, 90, 0);
                    break;
            }
        }
    }
}
