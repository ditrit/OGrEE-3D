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

    private void Start()
    {
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.RemoveListener<OnDeselectItemEvent>(OnDeselectItem);
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

    ///
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        ActiveInputField(true);
        UpdateInputField(GameManager.gm.currentItems[0].GetComponent<OgreeObject>().currentLod.ToString());
    }

    ///
    private void OnDeselectItem(OnDeselectItemEvent _e)
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            UpdateInputField("0");
            ActiveInputField(false);
        }
    }

    ///<summary>
    /// Update input field with given value
    ///</summary>
    ///<param name="_value">The value to set the input field</param>
    public void UpdateInputField(string _value)
    {
        if (ApiManager.instance.isInit)
            inputField.text = _value;
        else
            inputField.text = "-";

    }

    ///<summary>
    /// Set the inputField interactable or not
    ///</summary>
    ///<param name="_value">Is the inputField interactible ?</param>
    private void ActiveInputField(bool _value)
    {
        if (ApiManager.instance.isInit)
            inputField.interactable = _value;
        else
            inputField.interactable = false;

    }
}
