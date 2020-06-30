using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;

    [Header("Models")]
    public GameObject tileModel;
    public GameObject rackModel;
    public GameObject serverModel;

    private void Awake()
    {
        if (!gm)
            gm = this;
        else
            Destroy(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
