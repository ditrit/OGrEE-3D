using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rack))]
public class DisplayRackData : MonoBehaviour
{
    // textName is the root of all texts
    [SerializeField] private  TextMeshPro textName = null;
    [SerializeField] private TextMeshPro textDesc = null;
    
    ///<summary>
    /// Move displayed texts in the top/front of the rack. Also set width of name and description regarding rack width.
    ///</summary>
    public void PlaceTexts()
    {
        Vector3 boxSize = transform.GetChild(0).localScale;
        textName.transform.localPosition = new Vector3(0, boxSize.y, -boxSize.z) / 2;
        textName.transform.localPosition += new Vector3(0, -textName.rectTransform.sizeDelta.y / 2, -0.01f);

        textName.rectTransform.sizeDelta = new Vector2(boxSize.x - 0.1f, textName.rectTransform.sizeDelta.y);
        textDesc.rectTransform.sizeDelta = new Vector2(boxSize.x - 0.1f, textDesc.rectTransform.sizeDelta.y);
    }

    ///<summary>
    /// Set displayed texts regarding Rack data.
    ///</summary>
    public void FillTexts()
    {
        textName.text = GetComponent<Rack>().name;
        textDesc.text = GetComponent<Rack>().description;
    }
}
