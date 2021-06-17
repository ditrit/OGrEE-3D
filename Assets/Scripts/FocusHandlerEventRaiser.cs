using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusHandlerEventRaiser : MonoBehaviour
{

    public void FocusHandlerUpdateArrayButtonPressed() {
        EventManager.Instance.Raise(new ImportFinishedEvent());
    }
    
}
