using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public bool used = false;
    public string installed;
    public string orient;
    public string mandatory;
    public string labelPos;

    ///<summary>
    /// Disable slot renderer and disable its labels
    ///</summary>
    ///<param name="_value">Is the slot taken ?</param>
    public void SlotTaken(bool _value)
    {
        used = _value;
        transform.GetChild(0).GetComponent<Renderer>().enabled = !_value;
        TextMeshPro[] labels = GetComponentsInChildren<TextMeshPro>();
        foreach (TextMeshPro label in labels)
            label.gameObject.SetActive(!_value);
    }
}
