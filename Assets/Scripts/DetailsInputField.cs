using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailsInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI lockICon;
    [SerializeField] private Button lockLodButton;

    private void Start()
    {
        EventManager.instance.OnSelectItem.Add(OnSelectItem);
    }

    private void OnDestroy()
    {
        EventManager.instance.OnSelectItem.Remove(OnSelectItem);
    }

    public async void OnValueChanged(string _value)
    {
        if (_value.Contains("-"))
            _value = "0";
        int.TryParse(_value, out int level);
        foreach (GameObject go in GameManager.instance.GetSelected())
            await go.GetComponent<OgreeObject>().LoadChildren(level);
    }
    
    /// <summary>
    /// Lock the LOD of the selected objects
    /// </summary>
    public void LockLOD()
    {
        if (!GameManager.instance.selectMode)
            return;

        bool value = !GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>().isLodLocked;
        foreach (GameObject go in GameManager.instance.GetSelected())
            go.GetComponent<OgreeObject>().isLodLocked = value;
        if (value)
        {
            lockICon.text = "🔒";
            lockICon.color = Color.red;
            ActiveInputField(false);
        }
        else
        {
            lockICon.text = "🔓";
            lockICon.color = Color.green;
            ActiveInputField(true);
        }
    }

    /// <summary>
    /// Set up the LOD fields depending on the selection
    /// </summary>
    /// <param name="_e"></param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (!GameManager.instance.selectMode)
        {
            ActiveInputField(false);
            UpdateInputField("0");
            lockLodButton.gameObject.SetActive(false);
        }
        else
        {
            ActiveInputField(true);
            OgreeObject firstSelected = GameManager.instance.GetSelected()[0].GetComponent<OgreeObject>();
            UpdateInputField(firstSelected.currentLod.ToString());
            lockLodButton.gameObject.SetActive(firstSelected is Item item && item.referent == item);
            lockICon.text = firstSelected.isLodLocked ? "🔒" : "🔓";
            lockICon.color = firstSelected.isLodLocked ? Color.red : Color.green;
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
