using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailsInputField : MonoBehaviour
{
    private TMP_InputField inputField;

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void OnValueChanged(string _value)
    {
        if (_value.Contains("-"))
            _value = "0";

        List<OgreeObject> objsToUpdate = new List<OgreeObject>();
        foreach (GameObject go in GameManager.gm.currentItems)
            objsToUpdate.Add(go.GetComponent<OgreeObject>());
        foreach (OgreeObject obj in objsToUpdate)
            obj?.LoadChildren(_value);
    }

    ///<summary>
    /// Update input field with given value
    ///</summary>
    ///<param name="_value">The value to set the input field</param>
    public void UpdateInputField(string _value)
    {
        inputField.text = _value;
    }

    ///<summary>
    /// Set the inputField interactable or not
    ///</summary>
    ///<param name="_value">Is the inputField interactible ?</param>
    public void ActiveInputField(bool _value)
    {
        inputField.interactable = _value;
    }
}
