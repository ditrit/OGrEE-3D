using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;

    [Header("Custom units")]
    public float tileSize = 0.6f;
    public float uSize = 0.045f;
    public float ouSize = 0.048f;

    [Header("Models")]
    public GameObject tileModel;
    public GameObject rackModel;
    public GameObject serverModel;
    public GameObject deviceModel;

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
