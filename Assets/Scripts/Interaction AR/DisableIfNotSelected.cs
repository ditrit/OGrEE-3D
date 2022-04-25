using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNotSelected : MonoBehaviour
{
    public void Update()
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
        }

        
        if (GameManager.gm.currentItems.Count > 0)
        {
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }
}
