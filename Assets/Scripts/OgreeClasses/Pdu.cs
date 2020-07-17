using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pdu : Object
{
    public int nbCircuit;
    public int nbPhase; // 1 or 3
    public int nboutlet; // 2 ... 40
    public int voltage; // 110 230 400
    public int amperage; // 16 32 64 
    public string installation; // vertical | horizontal
    public string position; //  U<#> | L(eft)<#> | R(ight)<#>

    private void Awake()
    {
        family = EObjFamily.pdu;
    }
}
