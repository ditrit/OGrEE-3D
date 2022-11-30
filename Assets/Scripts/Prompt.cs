using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    public void Setup(string _mainText, string _buttonAText, string _buttonBText)
    {
        mainText.text = _mainText;
        buttonA.GetComponentInChildren<TMP_Text>().text = _buttonAText;
        if (!string.IsNullOrEmpty(_buttonBText))
            buttonB.GetComponentInChildren<TMP_Text>().text = _buttonBText;
        else
        {
            buttonB.gameObject.SetActive(false);
            buttonA.transform.localPosition = new Vector3(0, buttonA.transform.localPosition.y, buttonA.transform.localPosition.z);
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
