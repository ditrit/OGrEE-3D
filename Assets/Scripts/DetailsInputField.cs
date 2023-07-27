using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class DetailsInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI lockICon;

    private void Start()
    {
        EventManager.instance.AddListener<OnSelectItemEvent>(OnSelectItem);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
    }

    public async void OnValueChanged(string _value)
    {
        if (_value.Contains("-"))
            _value = "0";
        int.TryParse(_value, out int level);
        foreach (GameObject go in GameManager.instance.GetSelected())
            await go.GetComponent<OgreeObject>().LoadChildren(level);
    }

    public void LockLOD()
    {
        if (!GameManager.instance.selectMode)
            return;

        bool value = !GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().LodLocked;
        foreach (GameObject go in GameManager.instance.GetSelected())
            go.GetComponent<OgreeObject>().LodLocked = value;
        if (value)
            lockICon.text = "🔒";
        else
            lockICon.text = "🔓";
    }

    ///
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (!GameManager.instance.selectMode)
        {
            ActiveInputField(false);
            UpdateInputField("0");
            lockICon.text = "🔓";
        }
        else
        {
            ActiveInputField(true);
            UpdateInputField(GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().currentLod.ToString());
            lockICon.text = GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().LodLocked ? "🔒" : "🔓";
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
