using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayObjectData : MonoBehaviour
{
    [SerializeField] private TextMeshPro labelFront = null;
    [SerializeField] private TextMeshPro labelRear = null;

    ///<summary>
    /// Move displayed texts in the top/front of the rack.
    /// Also set width of name and description regarding rack width.
    ///</summary>
    public void PlaceTexts()
    {
        Vector3 boxSize = transform.GetChild(0).localScale;
        labelFront.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f) / -2;
        labelRear.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f) / 2;

        labelFront.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRear.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
    }

    ///<summary>
    /// Set displayed texts regarding Object data.
    ///</summary>
    public void UpdateLabels()
    {
        labelFront.text = GetComponent<Object>().name + " (F)";
        labelRear.text = GetComponent<Object>().name + " (R)";
    }
}
