using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class Prompt : MonoBehaviour
{
    public EPromptStatus state = 0;

    [SerializeField] private TMP_Text mainText;
    [SerializeField] private Button buttonA;
    [SerializeField] private Button buttonB;

    ///<summary>
    /// Setup texts of the Prompt. If no text given for buttonB, it will be hidden.
    ///</summary>
    ///<param name="_mainText">Message to display</param>
    ///<param name="_buttonAText">Custom text for "accept" button</param>
    ///<param name="_buttonBText">Custom text for "refuse" button. The button will be hidden if empty</param>
    ///<param name="_strVariable">A string variable to be used in Localization Table</param>
    public void Setup(string _mainText, string _buttonAText, string _buttonBText, string _strVariable = null)
    {
        if (!string.IsNullOrEmpty(_strVariable))
            mainText.GetComponent<LocalizeStringEvent>().StringReference.Add("str", new StringVariable { Value = _strVariable });
        mainText.GetComponent<LocalizeStringEvent>().StringReference.TableEntryReference = _mainText;
        buttonA.GetComponentInChildren<LocalizeStringEvent>().StringReference.TableEntryReference = _buttonAText;
        if (!string.IsNullOrEmpty(_buttonBText))
            buttonB.GetComponentInChildren<LocalizeStringEvent>().StringReference.TableEntryReference = _buttonBText;
        else
        {
            buttonB.gameObject.SetActive(false);
            buttonA.transform.localPosition = new(0, buttonA.transform.localPosition.y, buttonA.transform.localPosition.z);
        }
    }

    ///<summary>
    /// Called by buttonA: change state to "accept"
    ///</summary>
    public void Accept()
    {
        state = EPromptStatus.accept;
    }

    ///<summary>
    /// Called by buttonB: change state to "refuse"
    ///</summary>
    public void Refuse()
    {
        state = EPromptStatus.refuse;
    }
}
