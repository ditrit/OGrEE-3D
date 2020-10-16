using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZoomManager : MonoBehaviour
{
    public static ZoomManager instance;

    [Header("Data")]
    public int zoomLevel = 0;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI uiText = null;
    [SerializeField] private Slider slider = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        SetZoom(slider.value);
    }

    public void SetZoom(float _value)
    {
        zoomLevel = Mathf.Clamp(zoomLevel, 0, 3);

        slider.value = _value;
        uiText.text = $"Zoom level = {_value}";
        zoomLevel = (int)_value;
    }
}
