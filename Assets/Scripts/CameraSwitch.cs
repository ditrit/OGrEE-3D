using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] private GameObject fpsCam = null;
    [SerializeField] private GameObject overviewCam = null;

    private void Start()
    {
        if (fpsCam)
            fpsCam?.SetActive(false);
    }

    public void SwitchCam()
    {
        fpsCam.SetActive(overviewCam.activeSelf);
        overviewCam.SetActive(!fpsCam.activeSelf);
    }
}
