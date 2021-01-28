using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceScreenSettings : MonoBehaviour
{
    private int startingWidth;
    private int startingHeight;
    private FullScreenMode startingScreenMode;

    private void OnEnable()
    {
        startingWidth = Screen.currentResolution.width;
        startingHeight = Screen.currentResolution.height;
        startingScreenMode = Screen.fullScreenMode;

        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
    }

    private void OnDestroy()
    {
        Screen.SetResolution(startingWidth, startingHeight, startingScreenMode);
    }
}
