﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager gm;

    public GameObject currentItem = null;

    [Header("Custom units")]
    public float tileSize = 0.6f;
    public float uSize = 0.045f;
    public float ouSize = 0.048f;

    [Header("Models")]
    public GameObject tileModel;
    public GameObject roomModel;
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

    public GameObject FindAbsPath(string _path)
    {
        HierarchyName[] objs = FindObjectsOfType<HierarchyName>();
        // Debug.Log($"Looking for {_path} in {objs.Length} objects");
        for (int i = 0; i < objs.Length; i++)
        {
            // Debug.Log($"'{objs[i].fullname}' vs '{_path}'");
            if (objs[i].fullname == _path)
                return objs[i].gameObject;
        }

        return null;
    }
}