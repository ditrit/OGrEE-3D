using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNotSelected : MonoBehaviour
{
    public GameObject returnButton;
    public GameObject Sliders;

    public void Update()

    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
            Sliders.SetActive(false);
        }

        
        if (GameManager.gm.currentItems.Count > 0 && returnButton.activeSelf == false)
        {
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
