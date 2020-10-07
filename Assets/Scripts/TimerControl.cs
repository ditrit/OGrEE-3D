using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerControl : MonoBehaviour
{
    [SerializeField] private ConsoleController consoleController = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private TextMeshProUGUI value = null;

    private void Start()
    {
        UpdateTimerValue(slider.value);
    }

    ///<summary>
    /// Attached to GUI Toggle. Change value of ConsoleController.timer.
    ///</summary>
    ///<param name="_value">Value given by the toggle</param>
    public void ToggleTimer(bool _value)
    {
        consoleController.timer = _value;
        slider.interactable = _value;
    }

    ///<summary>
    /// Attached to GUI Slider. Change value of ConsoleController.timerValue. Also update text field.
    ///</summary>
    ///<param name="_value">Value given by the slider</param>
    public void UpdateTimerValue(float _value)
    {
        consoleController.timerValue = _value;
        value.text = _value.ToString("0.##") + "s";
    }
}
