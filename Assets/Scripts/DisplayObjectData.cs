using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        usedLabels.Clear();
        item = GetComponent<Item>();
        Transform shape = transform.GetChild(0);

        if (item && item.attributes.ContainsKey("template") && !string.IsNullOrEmpty(item.attributes["template"]))
        {
            Vector2 size = Utils.ParseVector2(item.attributes["size"]);
            if (item.attributes["sizeUnit"] == LengthUnit.Millimeter)
                size /= 1000;
            else if (item.attributes["sizeUnit"] == LengthUnit.Centimeter)
                size /= 100;

            float height = Utils.ParseDecFrac(item.attributes["height"]);
            if (item.attributes["heightUnit"] == LengthUnit.U)
                height *= UnitValue.U;
            else if (item.attributes["heightUnit"] == LengthUnit.Millimeter)
                height /= 1000;
            else if (item.attributes["heightUnit"] == LengthUnit.Centimeter)
                height /= 100;
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
        foreach (TextMeshPro tmp in usedLabels)
        {
            tmp.gameObject.SetActive(true);
            tmp.margin = new Vector4(tmp.rectTransform.sizeDelta.x, 0, tmp.rectTransform.sizeDelta.x, 0) / 20;
        }
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
        OgreeObject obj = GetComponent<OgreeObject>();
        if (obj)
            WriteLabels(_str);

        Slot s = GetComponent<Slot>();
        if (s)
            WriteLabels(name);
        attrToDisplay = _str;
        Sensor sensor = GetComponent<Sensor>();
        if (sensor)
        {
            if (_str == "#temperature")
                WriteLabels($"{Utils.FloatToRefinedStr(sensor.temperature)} {sensor.temperatureUnit}");
            else
                GameManager.instance.AppendLogLine($"Sensor can only show temperature (for now)", ELogTarget.both, ELogtype.warning);
        }
    }

    ///<summary>
    /// Set displayed texts with given string.
    ///</summary>
    ///<param name="_str">The string to display</param>
    ///<param name="_face">If set to true, add referential to front and rear labels</param>
    private void WriteLabels(string _str, bool _face = false)
    {
        foreach (TextMeshPro tmp in usedLabels)
        {
            tmp.text = _str;
            if (_face)
            {
                if (TryGetComponent(out Rack _))
                {
                    if (tmp == labelFront)
                        tmp.text += " (F)";
                    if (tmp == labelRear)
                        tmp.text += " (R)";
                }
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
            floatingLabel.transform.GetChild(0).GetComponent<Renderer>().material.color = Utils.ParseHtmlColor("#" + backgroundColor);
        }
    }

    ///<summary>
    /// Display or hide labels.
    ///</summary>
    ///<param name="_value">The value to assign</param>
    public void SwitchLabel(ELabelMode _value)
    {
        switch (_value)
        {
            case ELabelMode.Default:
                GetComponent<LODGroup>().enabled = true;
                foreach (TextMeshPro tmp in usedLabels)
                    tmp.GetComponent<MeshRenderer>().enabled = true;
                if (hasFloatingLabel)
                    floatingLabel.gameObject.SetActive(false);
                break;
            case ELabelMode.FloatingOnTop:
                GetComponent<LODGroup>().enabled = true;
                if (hasFloatingLabel)
                {
                    foreach (TextMeshPro tmp in usedLabels)
                        tmp.GetComponent<MeshRenderer>().enabled = false;
                    floatingLabel.gameObject.SetActive(true);
                }
                break;
            case ELabelMode.Hidden:
                GetComponent<LODGroup>().enabled = true;
                foreach (TextMeshPro tmp in usedLabels)
                    tmp.GetComponent<MeshRenderer>().enabled = false;
                if (hasFloatingLabel)
                    floatingLabel.gameObject.SetActive(false);
                break;
            case ELabelMode.Forced:
                GetComponent<LODGroup>().enabled = false;
                foreach (TextMeshPro tmp in usedLabels)
                    tmp.GetComponent<MeshRenderer>().enabled = true;
                if (hasFloatingLabel)
                    floatingLabel.gameObject.SetActive(false);
                break;
            default: break;
        }
        currentLabelMode = _value;
    }

    public void ToggleLabel(bool _value)
    {
        if (currentLabelMode == ELabelMode.Default || currentLabelMode == ELabelMode.Forced || !hasFloatingLabel)
            foreach (TextMeshPro tmp in usedLabels)
                tmp.GetComponent<MeshRenderer>().enabled = _value;
        else if (currentLabelMode == ELabelMode.FloatingOnTop)
            floatingLabel.gameObject.SetActive(_value);
    }

    private void OnSwitchLabelEvent(SwitchLabelEvent _e)
    {
        // Ignore slots and opened groups
        if (GetComponent<Slot>() || (item is Group group && !group.isDisplayed))
            return;

        if ((item && item.referent == item && !GameManager.instance.GetSelected().Contains(gameObject)) ||
            GameManager.instance.GetSelected().Contains(transform.parent.gameObject) ||
            GameManager.instance.GetFocused().Contains(transform.parent.gameObject))
            SwitchLabel(_e.value);
    }

    ///<summary>
    /// Set Font attributes (bold, italic, color).
    ///</summary>
    ///<param name="_value">The attribute to set, with its value if needed</param>
    public void SetLabelFont(string _value)
    {
        string pattern = "^(bold|italic|color@[0-9a-fA-F]{6})$";
        if (Regex.IsMatch(_value, pattern))
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
        else
            GameManager.instance.AppendLogLine("Unknown labelFont attribute", ELogTarget.both, ELogtype.warning);
    }

    ///<summary>
    /// Set background color for a box label.
    ///</summary>
    ///<param name="_value">The color to set</param>
    public void SetLabelBackgroundColor(string _value)
    {
        string pattern = "[0-9a-fA-F]{6}$";
        if (Regex.IsMatch(_value, pattern))
        {
            backgroundColor = _value;
            UpdateLabels();
        }
        else
            GameManager.instance.AppendLogLine("Unknown color", ELogTarget.both, ELogtype.warning);
    }
}