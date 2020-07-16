using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rack))]
public class DisplayRackData : MonoBehaviour
{
    [SerializeField] private  TextMeshPro textName = null;
    [SerializeField] private TextMeshPro textDesc = null;
    
    private Rack rack;

    private void Awake()
    {
        rack = GetComponent<Rack>();
    }

    public void FillTexts()
    {
        textName.text = rack.name;
        textDesc.text = rack.description;
    }
}
