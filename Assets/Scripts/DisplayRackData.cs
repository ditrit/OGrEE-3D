using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Object))]
public class DisplayRackData : MonoBehaviour
{
    [SerializeField] private  TextMeshPro textName = null;
    [SerializeField] private TextMeshPro textDesc = null;
    
    private Object obj;

    private void Awake()
    {
        obj = GetComponent<Object>();
        // FillTexts();
    }

    public void FillTexts()
    {
        textName.text = obj.name;
        textDesc.text = obj.description;
    }
}
