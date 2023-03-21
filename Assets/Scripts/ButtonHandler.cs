using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ButtonHandler
{
    public delegate bool Condition();
    public Condition interactCondition;
    public Button button;
    public Condition toggledCondition;

    public ButtonHandler(Button _button)
    {
        button = _button;
        EventManager.instance.AddListener<OnSelectItemEvent>(CheckSelect);
        EventManager.instance.AddListener<OnFocusEvent>(CheckFocus);
        EventManager.instance.AddListener<OnUnFocusEvent>(CheckUnfocus);
        EventManager.instance.AddListener<EditModeInEvent>(CheckEditIn);
        EventManager.instance.AddListener<EditModeOutEvent>(CheckEditOut);
        EventManager.instance.AddListener<ImportFinishedEvent>(CheckImportFinished);
    }

    public void Check()
    {
        button.interactable = interactCondition();
        ColorBlock cb = button.colors;
        if (toggledCondition is null)
            return;
        Debug.Log(toggledCondition());
        if (toggledCondition())
        {
            cb.normalColor = Color.green;
            cb.selectedColor = Color.green;
        }
        else
        {
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
        }
        button.colors = cb;
    }

    private void CheckSelect(OnSelectItemEvent _e)
    {
        Check();
    }

    private void CheckFocus(OnFocusEvent _e)
    {
        Check();
    }

    private void CheckUnfocus(OnUnFocusEvent _e)
    {
        Check();
    }

    private void CheckEditIn(EditModeInEvent _e)
    {
        Check();
    }

    private void CheckEditOut(EditModeOutEvent _e)
    {
        Check();
    }
    private void CheckImportFinished(ImportFinishedEvent _e)
    {
        Check();
    }
}