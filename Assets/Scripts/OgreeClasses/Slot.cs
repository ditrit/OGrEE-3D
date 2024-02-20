using UnityEngine;

public class Slot : MonoBehaviour
{
    public bool used = false;
    public float[] orient;
    public bool isU;
    public string formFactor;
    public string labelPos;

    ///<summary>
    /// Disable slot renderer and disable its labels.
    ///</summary>
    ///<param name="_value">Is the slot taken ?</param>
    public void SlotTaken(bool _value)
    {
        used = _value;
        GetComponent<ObjectDisplayController>().Display(!used,!used);
    }
}
