using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    public EObjectType type;
    
    [Header("Common fields")]
    public string description;
    public string row;
    public Vector2 pos; // besoin de le noter ? Peut être directement appliqué
    public EUnit posUnit;
    public Vector2 size; // idem: appliqué au localscale ??
    public EUnit sizeUnit;
    public int height;
    public EUnit heightUnit;
    public EObjOrient orient;

    [Header("Room fields")]
    public float floorHeight = 100;
    public EUnit floorUnit;

    [Header("ItRoom fields")]
    public SMargin technical = new SMargin(5, 5, 0, 0);
    public SMargin reserved = new SMargin(3, 1, 2, 5);

    [Header("Component fields")]
    public string vendor;
    public string model;
    public string serial;
    public EComponentType componentType;

    [Header("Device fields")]
    public ERole role;

    [Header("Pdu fields")]
    public int nbCiruit;
    public int nbPhase;
    public int nbOutlet;
    public float voltage;
    public float amperage;



    private void Start()
    {
        // Debug.Log("Object.Start");
    }

    private void Update()
    {

    }
}
