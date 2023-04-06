using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonHandler
{
    public delegate bool Condition();
    
    /// <summary>
    /// when this is true, the button is interactable
    /// </summary>
    public Condition interactCondition;
    
    public Button button;

    /// <summary>
    /// When this is true, the button is in an "activated" state
    /// </summary>
    public Condition toggledCondition;

    public Color toggledColor = Color.green;

    private ColorBlock defaultCB;

    /// <summary>
    /// if the button doesn't need to be toggled in an activated state,
    /// leave toggledCondition to null
    /// </summary>
    /// <param name="_button">the button to be handled </param>
    public ButtonHandler(Button _button)
    {
        button = _button;
        defaultCB = button.colors;
        EventManager.instance.AddListener<OnSelectItemEvent>(CheckSelect);
        EventManager.instance.AddListener<OnFocusEvent>(CheckFocus);
        EventManager.instance.AddListener<OnUnFocusEvent>(CheckUnfocus);
        EventManager.instance.AddListener<EditModeInEvent>(CheckEditIn);
        EventManager.instance.AddListener<EditModeOutEvent>(CheckEditOut);
        EventManager.instance.AddListener<ImportFinishedEvent>(CheckImportFinished);
    }

    /// <summary>
    /// Check for interaction and toggling condition, then activate/deactivate and toggle the button according to them
    /// </summary>
    public void Check()
    {
        button.interactable = interactCondition();
        if (toggledCondition is null)
            return;

        ColorBlock cb = button.colors;
        if (toggledCondition())
        {
            cb.normalColor = toggledColor;
            cb.selectedColor = toggledColor;
            cb.highlightedColor = toggledColor;
        }
        else
        {
            cb = defaultCB;
        }
        button.colors = cb;
    }

    /// <summary>
    /// Check executed when selecting
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void CheckSelect(OnSelectItemEvent _e)
    {
        Check();
    }

    /// <summary>
    /// Check executed when focusing
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void CheckFocus(OnFocusEvent _e)
    {
        Check();
    }

    /// <summary>
    /// Check executed when unfocusing
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void CheckUnfocus(OnUnFocusEvent _e)
    {
        Check();
    }

    /// <summary>
    /// Check executed when entering edit mode
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void CheckEditIn(EditModeInEvent _e)
    {
        Check();
    }

    /// <summary>
    /// Check executed when exiting edit mode
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void CheckEditOut(EditModeOutEvent _e)
    {
        Check();
    }

    /// <summary>
    /// Check executed when importing
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void CheckImportFinished(ImportFinishedEvent _e)
    {
        Check();
    }
}