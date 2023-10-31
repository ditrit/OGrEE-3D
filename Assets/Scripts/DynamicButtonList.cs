using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class DynamicButtonList : MonoBehaviour
{
    [Header("Setup before use")]
    [SerializeField] private string ListName;
    public GameObject buttonPrefab;
    [Header("Setup in prefab")]
    [SerializeField] private TMP_Text buttonToggleText;
    private bool isExpanded;
    private string displayText;
    private string hideText;

    private void Start()
    {
        displayText = $"Display {ListName} list";
        hideText = $"Hide {ListName} list";
        buttonToggleText.text = displayText;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Get all buttons except the hide/display one.
    /// </summary>
    /// <returns>A list with generated buttons</returns>
    public List<Transform> GetButtons()
    {
        List<Transform> list = new();
        foreach (Transform btn in transform)
        {
            if (btn.GetSiblingIndex() != 0)
                list.Add(btn);
        }
        return list;
    }

    /// <summary>
    /// Rebuild buttons using <paramref name="_buildBtnsMethod"/>.
    /// </summary>
    /// <param name="_buildBtnsMethod"></param>
    public void RebuildMenu(Func<int> _buildBtnsMethod)
    {
        // Wipe previous buttons
        foreach (Transform btn in GetButtons())
            Destroy(btn.gameObject);

        // Build buttons using given method
        int count = _buildBtnsMethod();
        gameObject.SetActive(count > 0);

        // Toggle buttons according to isExpended
        foreach (Transform btn in GetButtons())
            btn.gameObject.SetActive(isExpanded);
        UpdateBackgroundSize(count + 1); // + 1 because of the hide/display button
    }

    /// <summary>
    /// Update the background size of the DynamicList according to <see cref="isExpanded"/>.
    /// </summary>
    /// <param name="_buttonsCount">The number of elements in the list</param>
    public void UpdateBackgroundSize(int _buttonsCount)
    {
        int count = isExpanded ? _buttonsCount : 1;

        float btnHeight = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
        float padding = GetComponent<VerticalLayoutGroup>().padding.top;
        float spacing = GetComponent<VerticalLayoutGroup>().spacing;

        float menuWidth = GetComponent<RectTransform>().sizeDelta.x;
        float menuHeight = padding * 2 + (btnHeight + spacing) * count;
        GetComponent<RectTransform>().sizeDelta = new Vector2(menuWidth, menuHeight);
    }

    /// <summary>
    /// Called by GUI button: Expend or reduce the DynamicList
    /// </summary>
    public void ToggleMenu()
    {
        isExpanded ^= true;
        buttonToggleText.text = isExpanded ? hideText : displayText;

        foreach (Transform btn in GetButtons())
            btn.gameObject.SetActive(isExpanded);

        UpdateBackgroundSize(transform.childCount);
    }
}
