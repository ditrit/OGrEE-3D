using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rack))]
public class DisplayRackData : MonoBehaviour
{
    // textNameRear is the root of all texts
    [SerializeField] private TextMeshPro textNameRear = null;
    [SerializeField] private TextMeshPro textNameFront = null;
    [SerializeField] private TextMeshPro textNameTop = null;
    [SerializeField] private TextMeshPro textDesc = null;

    ///<summary>
    /// Move displayed texts in the top/front of the rack.
    /// Also set width of name and description regarding rack width.
    ///</summary>
    public void PlaceTexts()
    {
        Vector3 boxSize = transform.GetChild(0).localScale;
        textNameRear.transform.localPosition = new Vector3(0, boxSize.y, -boxSize.z) / 2;
        textNameRear.transform.localPosition += new Vector3(0, -textNameRear.rectTransform.sizeDelta.y / 2, -0.001f);
        textNameFront.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f);

        textNameRear.rectTransform.sizeDelta = new Vector2(boxSize.x - 0.1f, textNameRear.rectTransform.sizeDelta.y);
        textNameFront.rectTransform.sizeDelta = new Vector2(boxSize.x - 0.1f, textNameRear.rectTransform.sizeDelta.y);
        textNameTop.rectTransform.sizeDelta = new Vector2(boxSize.x - 0.1f, textNameRear.rectTransform.sizeDelta.y);
        textDesc.rectTransform.sizeDelta = new Vector2(boxSize.x - 0.1f, textDesc.rectTransform.sizeDelta.y);
    }

    ///<summary>
    /// Set displayed texts regarding Rack data.
    ///</summary>
    public void FillTexts()
    {
        textNameRear.text = GetComponent<Rack>().name + " (R)";
        textNameFront.text = GetComponent<Rack>().name + " (F)";
        textNameTop.text = GetComponent<Rack>().name;
        textDesc.text = "";
        foreach (string desc in GetComponent<Rack>().description)
            textDesc.text += $"{desc}\n";
    }
}
