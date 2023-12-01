using TMPro;
using UnityEngine;

public class Separator : MonoBehaviour
{

    [SerializeField] private TMP_Text textA;
    [SerializeField] private TMP_Text textB;

    /// <summary>
    /// Setup all texts
    /// </summary>
    public void Initialize()
    {
        SetupText(textA);
        SetupText(textB);
    }

    /// <summary>
    /// Display or not texts
    /// </summary>
    /// <param name="_value">Value to toggle texts</param>
    public void ToggleTexts(bool _value)
    {
        textA.gameObject.SetActive(_value);
        textB.gameObject.SetActive(_value);
    }

    /// <summary>
    /// Set the name and place given <paramref name="_text"/>.
    /// </summary>
    /// <param name="_text">The text to setup</param>
    private void SetupText(TMP_Text _text)
    {
        Vector3 scale = transform.GetChild(0).localScale / 2;
        _text.text = name;
        _text.transform.localPosition = new Vector3(scale.x, scale.y, _text.transform.localPosition.z);
        _text.gameObject.SetActive(false);
    }

}
