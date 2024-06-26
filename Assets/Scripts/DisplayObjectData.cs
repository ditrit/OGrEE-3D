﻿using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;

public class DisplayObjectData : MonoBehaviour
{
    [SerializeField] private TextMeshPro labelFront = null;
    [SerializeField] private TextMeshPro labelRear = null;
    [SerializeField] private TextMeshPro labelTop = null;
    [SerializeField] private TextMeshPro labelBottom = null;
    [SerializeField] private TextMeshPro labelLeft = null;
    [SerializeField] private TextMeshPro labelRight = null;
    [SerializeField] private TextMeshPro floatingLabel = null;
    public bool hasFloatingLabel = false;
    [SerializeField] private List<TextMeshPro> usedLabels = new();
    public string attrToDisplay = "";
    private bool isBold = false;
    private bool isItalic = false;
    private string color = "ffffff";
    private string backgroundColor = "000000";
    private ELabelMode currentLabelMode;
    private Vector3 boxSize;
    private Item item;
    private LODGroup lodGroup;

    private void Start()
    {
        EventManager.instance.SwitchLabel.Add(OnSwitchLabelEvent);
    }

    private void OnDestroy()
    {
        EventManager.instance.SwitchLabel.Remove(OnSwitchLabelEvent);
    }

    private void Update()
    {

        if (hasFloatingLabel && currentLabelMode == ELabelMode.FloatingOnTop)
        {
            floatingLabel.transform.LookAt(GameManager.instance.cameraControl.transform);
            floatingLabel.transform.rotation = Quaternion.LookRotation(GameManager.instance.cameraControl.transform.forward);
            floatingLabel.transform.GetChild(0).localScale = Vector2.ClampMagnitude(floatingLabel.textBounds.size, 20);
        }
    }

    ///<summary>
    /// Setup all texts regarding object's scale.
    /// Also Enable or disable texts according to _labelPos.
    ///</summary>
    ///<param name="_labelPos">Labels to display</param>
    public void PlaceTexts(string _labelPos)
    {
        foreach (TextMeshPro label in usedLabels)
            label.gameObject.SetActive(false);
        usedLabels.Clear();
        item = GetComponent<Item>();
        lodGroup = GetComponent<LODGroup>();
        Transform shape = transform.GetChild(0);

        if (item && item.attributes.HasKeyAndValue("template"))
        {
            Vector2 size = ((JArray)item.attributes["size"]).ToVector2();
            size *= item.attributes["sizeUnit"] switch
            {
                LengthUnit.Centimeter => 0.01f,
                LengthUnit.Millimeter => 0.001f,
                _ => 1
            };

            float height = (float)item.attributes["height"];
            height *= item.attributes["heightUnit"] switch
            {
                LengthUnit.U => UnitValue.U,
                LengthUnit.OU => UnitValue.OU,
                LengthUnit.Centimeter => 0.01f,
                LengthUnit.Millimeter => 0.001f,
                _ => 1
            };
            boxSize = new(size.x, height, size.y);
        }
        else
            boxSize = shape.localScale;


        if (hasFloatingLabel)
        {
            floatingLabel = Instantiate(GameManager.instance.floatingLabelModel, transform).GetComponent<TextMeshPro>();
            if (boxSize.x >= boxSize.z)
                floatingLabel.rectTransform.sizeDelta = new(boxSize.z, boxSize.z);
            else
                floatingLabel.rectTransform.sizeDelta = new(boxSize.x, boxSize.x);

            if (item is Group)
                floatingLabel.transform.localPosition = (boxSize.y / 2 + floatingLabel.rectTransform.sizeDelta.y / 4) * Vector3.up;
            else
                floatingLabel.transform.localPosition = new(boxSize.x / 2, boxSize.y + floatingLabel.rectTransform.sizeDelta.y / 4, boxSize.z / 2);
        }

        switch (_labelPos)
        {
            case LabelPos.FrontRear:
                if (!labelFront)
                    labelFront = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelFront.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.z + 0.002f) / 2 * Vector3.forward), transform.rotation * Quaternion.Euler(0, 180, 0));
                labelFront.rectTransform.sizeDelta = new(boxSize.x, boxSize.y);
                labelFront.name = "LabelFront";
                usedLabels.Add(labelFront);

                if (!labelRear)
                    labelRear = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelRear.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.z + 0.002f) / 2 * Vector3.back), transform.rotation * Quaternion.Euler(0, 0, 0));
                labelRear.rectTransform.sizeDelta = new(boxSize.x, boxSize.y);
                labelRear.name = "LabelRear";
                usedLabels.Add(labelRear);
                break;
            case LabelPos.Front:
                if (!labelFront)
                    labelFront = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelFront.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.z + 0.002f) / 2 * Vector3.forward), transform.rotation * Quaternion.Euler(0, 180, 0));
                labelFront.rectTransform.sizeDelta = new(boxSize.x, boxSize.y);
                labelFront.name = "LabelFront";
                usedLabels.Add(labelFront);
                break;
            case LabelPos.Rear:
                if (!labelRear)
                    labelRear = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelRear.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.z + 0.002f) / 2 * Vector3.back), transform.rotation * Quaternion.Euler(0, 0, 0));
                labelRear.rectTransform.sizeDelta = new(boxSize.x, boxSize.y);
                labelRear.name = "LabelRear";
                usedLabels.Add(labelRear);
                break;
            case LabelPos.Right:
                if (!labelRight)
                    labelRight = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelRight.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.x + 0.002f) / 2 * Vector3.right), transform.rotation * Quaternion.Euler(0, -90, 0));
                labelRight.rectTransform.sizeDelta = new(boxSize.z, boxSize.y);
                labelRight.name = "LabelRight";
                usedLabels.Add(labelRight);
                break;
            case LabelPos.Left:
                if (!labelLeft)
                    labelLeft = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelLeft.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.x + 0.002f) / 2 * Vector3.left), transform.rotation * Quaternion.Euler(0, 90, 0));
                labelLeft.rectTransform.sizeDelta = new(boxSize.z, boxSize.y);
                labelLeft.name = "LabelLeft";
                usedLabels.Add(labelLeft);
                break;
            case LabelPos.Top:
                if (!labelTop)
                    labelTop = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelTop.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.y + 0.002f) / 2 * Vector3.up), transform.rotation * Quaternion.Euler(90, 180, 0));
                if (boxSize.x >= boxSize.z)
                    labelTop.rectTransform.sizeDelta = new(boxSize.x, boxSize.z);
                else
                {
                    labelTop.transform.localEulerAngles = new(90, 0, -90);
                    labelTop.rectTransform.sizeDelta = new(boxSize.z, boxSize.x);
                }
                labelTop.name = "LabelTop";
                usedLabels.Add(labelTop);
                break;
            case LabelPos.Bottom:
                if (!labelBottom)
                    labelBottom = Instantiate(GameManager.instance.labelModel, transform).GetComponent<TextMeshPro>();
                labelBottom.rectTransform.SetPositionAndRotation(shape.position + transform.rotation * ((boxSize.y + 0.002f) / 2 * Vector3.down), transform.rotation * Quaternion.Euler(-90, 0, 0));
                if (boxSize.x >= boxSize.z)
                    labelBottom.rectTransform.sizeDelta = new(boxSize.x, boxSize.z);
                else
                {
                    labelBottom.transform.localEulerAngles = new(90, 0, -90);
                    labelBottom.rectTransform.sizeDelta = new(boxSize.z, boxSize.x);
                }
                labelBottom.name = "LabelBottom";
                usedLabels.Add(labelBottom);
                break;
        }
        List<Renderer> renderers = new();
        foreach (TextMeshPro tmp in usedLabels)
        {
            tmp.gameObject.SetActive(true);
            tmp.margin = new Vector4(tmp.rectTransform.sizeDelta.x, 0, tmp.rectTransform.sizeDelta.x, 0) / 20;
            renderers.Add(tmp.GetComponent<Renderer>());
        }
        LOD withLabels = new(0.1f, renderers.ToArray());
        LOD withoutLabels = new(0, new Renderer[0]);
        lodGroup.SetLODs(new LOD[] { withLabels, withoutLabels });
        lodGroup.RecalculateBounds();
    }

    ///<summary>
    /// Call SetLabel with previously used attribute
    ///</summary>
    public void UpdateLabels()
    {
        if (!string.IsNullOrEmpty(attrToDisplay))
            SetLabel(attrToDisplay);
    }

    ///<summary>
    /// Set corresponding labels with given field value. 
    ///</summary>
    ///<param name="_str">The attribute to set</param>
    public void SetLabel(string _str)
    {
        WriteLabels(_str);
        attrToDisplay = _str;
    }

    ///<summary>
    /// Set displayed texts with given string.
    ///</summary>
    ///<param name="_str">The string to display</param>
    private void WriteLabels(string _str)
    {
        foreach (TextMeshPro tmp in usedLabels)
        {
            tmp.text = _str;
            if (TryGetComponent(out Rack rack) && rack.name == _str)
            {
                if (tmp == labelFront)
                    tmp.text += " (F)";
                if (tmp == labelRear)
                    tmp.text += " (R)";
            }
            tmp.text = $"<color=#{color}>{tmp.text}</color>";

            if (isBold)
                tmp.text = $"<b>{tmp.text}</b>";
            if (isItalic)
                tmp.text = $"<i>{tmp.text}</i>";
        }
        if (hasFloatingLabel)
        {
            floatingLabel.text = $"<color=#{color}>{_str}</color>";
            if (isBold)
                floatingLabel.text = $"<b>{floatingLabel.text}</b>";
            if (isItalic)
                floatingLabel.text = $"<i>{floatingLabel.text}</i>";
            floatingLabel.transform.GetChild(0).GetComponent<Renderer>().material.color = Utils.ParseHtmlColor($"#{backgroundColor}");
        }
    }

    ///<summary>
    /// Activate or deactivate labels depending on given <paramref name="_labelType"/>
    ///</summary>
    ///<param name="_labelType">The ELabelMode to use</param>
    public void SwitchLabel(ELabelMode _labelType)
    {
        switch (_labelType)
        {
            case ELabelMode.Default:
                GetComponent<LODGroup>().enabled = true;
                foreach (TextMeshPro tmp in usedLabels)
                    tmp.gameObject.SetActive(true);
                if (hasFloatingLabel)
                    floatingLabel.gameObject.SetActive(false);
                break;
            case ELabelMode.FloatingOnTop:
                GetComponent<LODGroup>().enabled = true;
                if (hasFloatingLabel)
                {
                    foreach (TextMeshPro tmp in usedLabels)
                        tmp.gameObject.SetActive(false);
                    floatingLabel.gameObject.SetActive(true);
                }
                break;
            case ELabelMode.Hidden:
                GetComponent<LODGroup>().enabled = true;
                foreach (TextMeshPro tmp in usedLabels)
                    tmp.gameObject.SetActive(false);
                if (hasFloatingLabel)
                    floatingLabel.gameObject.SetActive(false);
                break;
            case ELabelMode.Forced:
                GetComponent<LODGroup>().enabled = false;
                foreach (TextMeshPro tmp in usedLabels)
                    tmp.gameObject.SetActive(true);
                if (hasFloatingLabel)
                    floatingLabel.gameObject.SetActive(false);
                break;
            default: break;
        }
        currentLabelMode = _labelType;
    }

    /// <summary>
    /// Toggle all labels MeshRenderer 
    /// </summary>
    /// <param name="_value"></param>
    public void ToggleLabel(bool _value)
    {
        foreach (TextMeshPro tmp in usedLabels)
            tmp.GetComponent<MeshRenderer>().enabled = _value;
        if (hasFloatingLabel)
        {
            floatingLabel.GetComponent<MeshRenderer>().enabled = _value;
            floatingLabel.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = _value;
        }
    }

    /// <summary>
    /// Called when receiving a SwitchLavelEvent, call SwitchLabel with given <see cref="ELabelMode"/>.
    /// </summary>
    /// <param name="_e"></param>
    private void OnSwitchLabelEvent(SwitchLabelEvent _e)
    {
        // Ignore slots
        if (GetComponent<Slot>())
            return;

        SwitchLabel(_e.value);
    }

    ///<summary>
    /// Set Font attributes (bold, italic, color).
    ///</summary>
    ///<param name="_value">The attribute to set, with its value if needed</param>
    public void SetLabelFont(string _value)
    {
        if (_value == "bold")
            isBold = !isBold;
        else if (_value == "italic")
            isItalic = !isItalic;
        else if (_value.Contains("color"))
        {
            string[] data = _value.Split('@');
            color = data[1];
        }
        UpdateLabels();
    }

    ///<summary>
    /// Set background color for a box label.
    ///</summary>
    ///<param name="_value">The color to set</param>
    public void SetLabelBackgroundColor(string _value)
    {
        backgroundColor = _value;
        UpdateLabels();
    }
}