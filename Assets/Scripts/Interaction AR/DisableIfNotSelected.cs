using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNotSelected : MonoBehaviour
{
    public GameObject returnButton;
    public void Update()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        
        if (GameManager.gm.currentItems.Count > 0 && returnButton.activeSelf == false)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
