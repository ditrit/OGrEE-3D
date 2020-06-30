using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Update()
    {
        // Shouldn't be here
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
