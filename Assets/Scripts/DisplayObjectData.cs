using System.Collections;
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
    [SerializeField] private List<TextMeshPro> usedLabels = new List<TextMeshPro>();
    public string attrToDisplay = "";
    private bool isBold = false;
    private bool isItalic = false;
    private string color = "ffffff";
    private string backgroundColor = "000000";
    private ELabelMode currentLabelMode;
    private Vector3 boxSize;
    [SerializeField] private CameraControl cc;

    private void Start()
    {
        EventManager.instance.AddListener<SwitchLabelEvent>(OnSwitchLabelEvent);
        cc = FindObjectOfType<CameraControl>();
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<SwitchLabelEvent>(OnSwitchLabelEvent);
    }

    private void Update()
    {

        if (hasFloatingLabel && currentLabelMode == ELabelMode.FloatingOnTop)
        {
            floatingLabel.transform.localPosition = new Vector3(0, boxSize.y + floatingLabel.textBounds.size.y + 0.1f, 0) / 2;
            floatingLabel.transform.LookAt(cc.transform);
            floatingLabel.transform.rotation = Quaternion.LookRotation(cc.transform.forward);
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
        OgreeObject oObj = GetComponent<OgreeObject>();
        if (oObj && oObj.attributes.ContainsKey("template")
            && !string.IsNullOrEmpty(oObj.attributes["template"]))
        {
            Vector2 size = JsonUtility.FromJson<Vector2>(oObj.attributes["size"]);
            if (oObj.attributes["sizeUnit"] == LengthUnit.MilliMeter)
                size /= 1000;
            else if (oObj.attributes["sizeUnit"] == LengthUnit.CentiMeter)
                size /= 100;

            float height = Utils.ParseDecFrac(oObj.attributes["height"]);
            if (oObj.attributes["heightUnit"] == LengthUnit.U)
                height *= GameManager.instance.uSize;
            else if (oObj.attributes["heightUnit"] == LengthUnit.MilliMeter)
                height /= 1000;
            else if (oObj.attributes["heightUnit"] == LengthUnit.CentiMeter)
                height /= 100;
            boxSize = new Vector3(size.x, height, size.y);
        }
        else
            boxSize = transform.GetChild(0).localScale;

        labelFront.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f) / 2;
        labelRear.transform.localPosition = new Vector3(0, 0, boxSize.z + 0.002f) / -2;
        labelRight.transform.localPosition = new Vector3(boxSize.x + 0.002f, 0, 0) / 2;
        labelLeft.transform.localPosition = new Vector3(boxSize.x + 0.002f, 0, 0) / -2;
        labelTop.transform.localPosition = new Vector3(0, boxSize.y + 0.002f, 0) / 2;
        labelBottom.transform.localPosition = new Vector3(0, boxSize.y + 0.002f, 0) / -2;

        labelFront.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRear.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.y);
        labelRight.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.y);
        labelLeft.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.y);
        if (boxSize.x >= boxSize.z)
        {
            labelTop.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.z);
            labelBottom.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.z);
            if (hasFloatingLabel)
                floatingLabel.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.z);
        }
        else
        {
            labelTop.transform.localEulerAngles = new Vector3(90, 0, -90);
            labelTop.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.x);
            labelBottom.transform.localEulerAngles = new Vector3(90, 0, -90);
            labelBottom.rectTransform.sizeDelta = new Vector2(boxSize.z, boxSize.x);
            if (hasFloatingLabel)
                floatingLabel.rectTransform.sizeDelta = new Vector2(boxSize.x, boxSize.x);
        }

        foreach (TextMeshPro tmp in usedLabels)
            tmp.gameObject.SetActive(false);
        usedLabels.Clear();
        switch (_labelPos)
        {
            case LabelPos.FrontRear:
                usedLabels.Add(labelFront);
                usedLabels.Add(labelRear);
                break;
            case LabelPos.Front:
                usedLabels.Add(labelFront);
                break;
            case LabelPos.Rear:
                usedLabels.Add(labelRear);
                break;
            case LabelPos.Right:
                usedLabels.Add(labelRight);
                break;
            case LabelPos.Left:
                usedLabels.Add(labelLeft);
                break;
            case LabelPos.Top:
                usedLabels.Add(labelTop);
                break;
            case LabelPos.Bottom:
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
        {
            if (_str[0] == '#')
            {
                string attr = _str.Substring(1);
                if (attr == "name")
                    WriteLabels(obj.name, true);
                else if (attr.Contains("description"))
                {
                    if (attr == "description")
                        WriteLabels(string.Join("\n", obj.description));
                    else if (int.TryParse(attr.Substring(11), out int i) && i > 0 && obj.description.Count >= i)
                        WriteLabels(obj.description[i - 1]);
                    else
                        GameManager.instance.AppendLogLine("Wrong description index", ELogTarget.both, ELogtype.warning);
                }
                else if (obj.attributes.ContainsKey(attr))
                    WriteLabels(obj.attributes[attr]);
                else
                {
                    GameManager.instance.AppendLogLine($"{name} doesn't contain {attr} attribute.", ELogTarget.both, ELogtype.warning);
                    return;
                }
            }
            else
                WriteLabels(_str);
        }
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
                OgreeObject obj = GetComponent<OgreeObject>();
                if (obj && obj.category == Category.Rack)
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
        if (currentLabelMode == ELabelMode.Default)
            foreach (TextMeshPro tmp in usedLabels)
                tmp.GetComponent<MeshRenderer>().enabled = _value;
        else if (currentLabelMode == ELabelMode.FloatingOnTop)
            floatingLabel.gameObject.SetActive(_value);
    }

    private void OnSwitchLabelEvent(SwitchLabelEvent _e)
    {
        // Ignore slots
        if (GetComponent<Slot>())
            return;

        if (!GameManager.instance.focusMode || GameManager.instance.GetFocused().Contains(gameObject)
            || GameManager.instance.GetFocused().Contains(transform.parent.gameObject))
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
        }
        else
            GameManager.instance.AppendLogLine("Unknown color", ELogTarget.both, ELogtype.warning);
    }
}