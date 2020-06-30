using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class DebugText : MonoBehaviour
{
    private TextMeshProUGUI txt;

    private void Start()
    {
        txt = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        int currentFPS = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        int count = GameObject.FindObjectsOfType<GameObject>().Length;

        txt.text = $"Object count: {count}\nFPS: {currentFPS}";
        
    }
}
