using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;

public class UHelpersManager : MonoBehaviour
{
    static public UHelpersManager instance;

    private readonly string cornerRearLeft = "rearLeft";
    private readonly string cornerRearRight = "rearRight";
    private readonly string cornerFrontLeft = "frontLeft";
    private readonly string cornerFrontRight = "frontRight";
    private bool wasEdited = false;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        EventManager.instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.instance.AddListener<EditModeOutEvent>(OnEditModeOut);
        EventManager.instance.AddListener<OnSelectItemEvent>(OnSelect);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.instance.RemoveListener<EditModeOutEvent>(OnEditModeOut);
        EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelect);
    }

    ///<summary>
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    private void OnEditModeIn(EditModeInEvent _e)
    {
        wasEdited = true;
        ToggleU(GameManager.instance.GetSelected()[0], false);
    }

    ///<summary>
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    private void OnEditModeOut(EditModeOutEvent _e)
    {
        wasEdited = false;
        ToggleU(GameManager.instance.GetSelected()[0], true);
    }

    ///<summary>
    /// When called, toggle U helpers and highlight U helpers when needed.
    ///</summary>
    ///<param name="_e">Event raised when selecting something</param>
    private void OnSelect(OnSelectItemEvent _e)
    {
        if (GameManager.instance.selectMode && GameManager.instance.SelectIs<OObject>() && !GameManager.instance.SelectIs<OgreeObject>("tempBar"))
        {
            ToggleU(GameManager.instance.GetSelected(), true);
            HighlightULocation(GameManager.instance.GetSelected());
        }
    }

    ///<summary>
    /// Highlight the ULocation at the same height than the selected device.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    private void HighlightULocation(List<GameObject> _selection)
    {
        if (_selection.Count == 0)
            return;
        switch (_selection[0].GetComponent<OObject>().category)
        {
            case Category.Rack:
                foreach (GameObject obj in _selection)
                {
                    GameObject u = obj.GetComponent<Rack>().uRoot.gameObject;
                    u.SetActive(true);
                    for (int i = 0; i < u.transform.GetChild(0).childCount; i++)
                        ChangeUColor(u, i, true);
                    wasEdited = false;
                }
                break;

            case Category.Device:
                if (wasEdited)
                    return;
                Rack rack = Utils.GetRackReferent(_selection[0].GetComponent<OObject>());
                if (!rack)
                    return;
                GameObject uRoot = rack.uRoot.gameObject;
                uRoot.SetActive(true);

                for (int i = 0; i < uRoot.transform.GetChild(0).childCount; i++)
                    ChangeUColor(uRoot, i, false);

                foreach (GameObject obj in _selection)
                {

                    float difference;
                    Transform t = obj.transform.GetChild(0);
                    float center = t.position.y;

                    BoxCollider boxCollider = t.GetComponent<BoxCollider>();
                    bool isEnabled = boxCollider.enabled;
                    boxCollider.enabled = true;
                    difference = boxCollider.bounds.extents.y;
                    boxCollider.enabled = isEnabled;

                    t = obj.transform;
                    float delta = t.localPosition.y - t.GetComponent<OgreeObject>().originalLocalPosition.y;
                    float lowerBound = center - difference - delta;
                    float upperBound = center + difference - delta;

                    for (int i = 0; i < uRoot.transform.GetChild(0).childCount; i++)
                    {
                        Transform u = uRoot.transform.GetChild(0).GetChild(i);
                        if (lowerBound < u.position.y && u.position.y < upperBound)
                            ChangeUColor(uRoot, i, true);
                    }
                }
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Change the color of U helpers depending on their corner
    /// </summary>
    /// <param name="_uRoot">Root transform of U helpers</param>
    /// <param name="_index">U helper index</param>
    /// <param name="_activated">If the U helper is colored or not</param>
    private void ChangeUColor(GameObject _uRoot, int _index, bool _activated)
    {
        GameObject obj = _uRoot.transform.GetChild(0).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.red : Color.black;
        obj = _uRoot.transform.GetChild(1).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.yellow : Color.black;
        obj = _uRoot.transform.GetChild(2).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.blue : Color.black;
        obj = _uRoot.transform.GetChild(3).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.green : Color.black;
    }

    ///<summary>
    /// Toggle U helpers of <paramref name="_transform"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_transform">The transform of a rack or a device</param>
    ///<param name="_active">Should the U helpers be visible ?</param>
    public void ToggleU(List<GameObject> _selection, bool _active)
    {
        foreach (GameObject obj in _selection)
        {
            Rack rack = Utils.GetRackReferent(obj.GetComponent<OObject>());
            if (!rack)
                break;

            Transform uRoot = rack.uRoot;
            if (_active)
            {
                if (!uRoot)
                    GenerateUHelpers(rack);
                else
                    uRoot.gameObject.SetActive(true);
            }
            else if (uRoot)
                uRoot.gameObject.SetActive(false);
        }
    }

    ///<summary>
    /// Toggle U helpers of <paramref name="_transform"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_transform">The transform of a rack or a device</param>
    public void ToggleU(List<GameObject> _selection)
    {
        foreach (GameObject obj in _selection)
        {
            Rack rack = Utils.GetRackReferent(obj.GetComponent<OObject>());
            if (!rack)
                break;

            Transform uRoot = rack.uRoot;
            if (!uRoot)
            {
                GenerateUHelpers(rack);
                GameManager.instance.AppendLogLine($"U helpers ON for {obj.name}.", ELogTarget.logger, ELogtype.info);
            }
            else if (!uRoot.gameObject.activeSelf)
            {
                uRoot.gameObject.SetActive(true);
                GameManager.instance.AppendLogLine($"U helpers ON for {obj.name}.", ELogTarget.logger, ELogtype.info);
            }
            else
            {
                uRoot.gameObject.SetActive(false);
                GameManager.instance.AppendLogLine($"U helpers OFF for {obj.name}.", ELogTarget.logger, ELogtype.info);
            }
        }
    }

    public void ToggleU(GameObject _selected)
    {
        ToggleU(new List<GameObject> { _selected });
    }
    public void ToggleU(GameObject _selected, bool _active)
    {
        ToggleU(new List<GameObject> { _selected }, _active);
    }

    ///<summary>
    /// Create uRoot, place it and create U helpers for each corner
    ///</summary>
    ///<param name="_rack">The rack where we create the U helpers</param>
    public void GenerateUHelpers(Rack _rack)
    {
        if (_rack.uRoot)
            return;

        _rack.uRoot = new GameObject("uRoot").transform;
        _rack.uRoot.parent = _rack.transform;
        _rack.uRoot.localPosition = Vector3.zero;
        _rack.uRoot.localEulerAngles = Vector3.zero;
        Vector3 boxSize = _rack.transform.GetChild(0).localScale;
        Transform URearLeft = new GameObject(cornerRearLeft).transform;
        URearLeft.parent = _rack.uRoot;
        URearLeft.localPosition = new Vector3(-boxSize.x / 2, 0, -boxSize.z / 2);
        URearLeft.localEulerAngles = Vector3.zero;
        Transform URearRight = new GameObject(cornerRearRight).transform;
        URearRight.parent = _rack.uRoot;
        URearRight.localPosition = new Vector3(boxSize.x / 2, 0, -boxSize.z / 2);
        URearRight.localEulerAngles = Vector3.zero;
        Transform UFrontLeft = new GameObject(cornerFrontLeft).transform;
        UFrontLeft.parent = _rack.uRoot;
        UFrontLeft.localPosition = new Vector3(-boxSize.x / 2, 0, boxSize.z / 2);
        UFrontLeft.localEulerAngles = Vector3.zero;
        Transform UFrontRight = new GameObject(cornerFrontRight).transform;
        UFrontRight.parent = _rack.uRoot;
        UFrontRight.localPosition = new Vector3(boxSize.x / 2, 0, boxSize.z / 2);
        UFrontRight.localEulerAngles = Vector3.zero;

        float scale = GameManager.instance.uSize;
        if (_rack.attributes["heightUnit"] == "OU")
            scale = GameManager.instance.ouSize;

        //List<float> Uslotpositions = _rack.transform.Cast<Transform>().Where(t => t.GetComponent<Slot>() && t.GetComponent<Slot>().isU).Select(t => t.localPosition.y + 0.5f * (scale - t.GetChild(0).localScale.y)).Distinct().OrderBy(t => t).ToList();
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Transform child in _rack.transform)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot && slot.isU)
            {
                minY = Mathf.Min(child.localPosition.y - 0.5f * (child.GetChild(0).localScale.y - scale), minY);
                maxY = Mathf.Max(child.localPosition.y + 0.5f * (child.GetChild(0).localScale.y - scale), maxY);
            }
        }

        if (minY < float.PositiveInfinity)
        {
            BuildU(minY, maxY, scale, URearLeft, cornerRearLeft, Color.red);
            BuildU(minY, maxY, scale, URearRight, cornerRearRight, Color.yellow);
            BuildU(minY, maxY, scale, UFrontLeft, cornerFrontLeft, Color.blue);
            BuildU(minY, maxY, scale, UFrontRight, cornerFrontRight, Color.green);
        }
        else if (_rack.attributes.ContainsKey("sizeWDHu") || _rack.attributes.ContainsKey("sizeWDHou") || _rack.attributes["heightUnit"] == "U" || _rack.attributes["heightUnit"] == "OU")
        {
            int Unumber;
            if (_rack.attributes.ContainsKey("sizeWDHu"))
                Unumber = JsonConvert.DeserializeObject<int[]>(_rack.attributes["sizeWDHu"])[2];
            else if (_rack.attributes.ContainsKey("sizeWDHou"))
                Unumber = JsonConvert.DeserializeObject<int[]>(_rack.attributes["sizeWDHou"])[2];
            else
                Unumber = int.Parse(_rack.attributes["height"]);

            float offset = -(Unumber - 1) * scale / 2;
            BuildU(offset, offset + (Unumber - 1) * scale, scale, URearLeft, cornerRearLeft, Color.red);
            BuildU(offset, offset + (Unumber - 1) * scale, scale, URearRight, cornerRearRight, Color.yellow);
            BuildU(offset, offset + (Unumber - 1) * scale, scale, UFrontLeft, cornerFrontLeft, Color.blue);
            BuildU(offset, offset + (Unumber - 1) * scale, scale, UFrontRight, cornerFrontRight, Color.green);
        }
    }

    ///<summary>
    /// Call by GUI on reset transforms
    ///</summary>
    public void ResetUHelpers()
    {
        if (GameManager.instance.editMode)
            return;
        if (!Utils.IsObjectMoved(GameManager.instance.GetFocused()[GameManager.instance.GetFocused().Count - 1].GetComponent<OgreeObject>()))
        {
            wasEdited = false;
            ToggleU(GameManager.instance.GetSelected()[0], true);
        }
    }

    /// <summary>
    /// Build one U-helpers column
    /// </summary>
    /// <param name="_firstPositionY">(local) vertical position of the first floor</param>
    /// <param name="_lastPositionY">(local) vertical position of the last floor</param>
    /// <param name="_scale">height of the floor (u size or ou size)</param>
    /// <param name="_UColumn">parent of the column</param>
    /// <param name="_cornerName">name of the column</param>
    /// <param name="_color">color of the column</param>
    private void BuildU(float _firstPositionY, float _lastPositionY, float _scale, Transform _UColumn, string _cornerName, Color _color)
    {
        int floorNumber = 0;
        for (float positionY = _firstPositionY; positionY <= _lastPositionY; positionY += _scale)
        {
            floorNumber++;
            Transform rearLeft = Instantiate(GameManager.instance.uLocationModel, _UColumn).transform;
            rearLeft.localPosition = positionY * Vector3.up;
            rearLeft.name = $"{_cornerName}_u{floorNumber}";
            rearLeft.GetChild(0).GetComponent<TextMeshPro>().text = floorNumber.ToString();
            rearLeft.GetChild(1).GetComponent<TextMeshPro>().text = floorNumber.ToString();
            rearLeft.localScale = Vector3.one * _scale;
            rearLeft.GetComponent<Renderer>().material.color = _color;
        }
    }
}
