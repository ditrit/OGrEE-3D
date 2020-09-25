using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayObjectData : MonoBehaviour
{
    [SerializeField] private TextMeshPro labelFront = null;
    [SerializeField] private TextMeshPro labelRear = null;
    [SerializeField] private TextMeshPro labelTop = null;
    [SerializeField] private TextMeshPro labelLeft = null;
    [SerializeField] private TextMeshPro labelRight = null;

    ///<summary>
    /// Move displayed texts in the top/front of the rack.
    /// Also set width of name and description regarding rack width.
    ///</summary>
    ///<param name="_labelPos">Labels to display</param>
    public void PlaceTexts(string _labelPos)
    {
        Vector3 boxSize = transform.GetChild(0).localScale;
        labelFront.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f) / 2;
        labelRear.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f) / -2;
        labelRight.transform.localPosition = new Vector3(boxSize.x + 0.002f, 0, 0) / 2;
        labelLeft.transform.localPosition = new Vector3(boxSize.x + 0.002f, 0, 0) / -2;
        labelTop.transform.localPosition = new Vector3(0, boxSize.y + 0.002f, 0) / 2;

        labelFront.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRear.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRight.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.y);
        labelLeft.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.y);
        labelTop.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.z);

        switch (_labelPos)
        {
            case "frontrear":
                labelFront.gameObject.SetActive(true);
                labelRear.gameObject.SetActive(true);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                break;
            case "front":
                labelFront.gameObject.SetActive(true);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                break;
            case "rear":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(true);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                break;
            case "right":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(true);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                break;
            case "left":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(true);
                labelTop.gameObject.SetActive(false);
                break;
            case "top":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(true);
                break;
        }
    }

    ///<summary>
    /// Set displayed texts with given string.
    ///</summary>
    ///<param name="_str">The string to display</param>
    public void UpdateLabels(string _str)
    {
        labelFront.text = _str + " (F)";
        labelRear.text = _str + " (R)";
        labelRight.text = _str;
        labelLeft.text = _str;
        labelTop.text = _str;
    }
}
