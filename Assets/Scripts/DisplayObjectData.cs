using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayObjectData : MonoBehaviour
{
    [SerializeField] private TextMeshPro labelFront = null;
    [SerializeField] private TextMeshPro labelRear = null;
    [SerializeField] private TextMeshPro labelTop = null;
    [SerializeField] private TextMeshPro labelBottom = null;
    [SerializeField] private TextMeshPro labelLeft = null;
    [SerializeField] private TextMeshPro labelRight = null;

    ///<summary>
    /// Assign labels references with children of a Device prefab
    ///</summary>
    public void Setup()
    {
        labelFront = transform.GetChild(1).GetComponent<TextMeshPro>();
        labelRear = transform.GetChild(2).GetComponent<TextMeshPro>();
        labelRight = transform.GetChild(3).GetComponent<TextMeshPro>();
        labelLeft = transform.GetChild(4).GetComponent<TextMeshPro>();
        labelTop = transform.GetChild(5).GetComponent<TextMeshPro>();
        labelBottom = transform.GetChild(6).GetComponent<TextMeshPro>();
    }

    ///<summary>
    /// Setup all texts regarding object's scale.
    /// Also Enable or disable texts according to _labelPos.
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
        labelBottom.transform.localPosition = new Vector3(0, boxSize.y + 0.002f, 0) / -2;

        labelFront.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRear.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRight.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.y);
        labelLeft.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.y);
        labelTop.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.z);
        labelBottom.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.z);

        switch (_labelPos)
        {
            case "frontrear":
                labelFront.gameObject.SetActive(true);
                labelRear.gameObject.SetActive(true);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(false);
                break;
            case "front":
                labelFront.gameObject.SetActive(true);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(false);
                break;
            case "rear":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(true);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(false);
                break;
            case "right":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(true);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(false);
                break;
            case "left":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(true);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(false);
                break;
            case "top":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(true);
                labelBottom.gameObject.SetActive(false);
                break;
            case "bottom":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(true);
                break;
            case "none":
                labelFront.gameObject.SetActive(false);
                labelRear.gameObject.SetActive(false);
                labelRight.gameObject.SetActive(false);
                labelLeft.gameObject.SetActive(false);
                labelTop.gameObject.SetActive(false);
                labelBottom.gameObject.SetActive(false);
                break;
        }
    }

    ///<summary>
    /// Set displayed texts with given string.
    ///</summary>
    ///<param name="_str">The string to display</param>
    public void UpdateLabels(string _str)
    {
        labelFront.text = _str;
        labelRear.text = _str;
        labelRight.text = _str;
        labelLeft.text = _str;
        labelTop.text = _str;
        labelBottom.text = _str;
    }
}
