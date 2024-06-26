using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UHelpersManager : MonoBehaviour
{
    static public UHelpersManager instance;

    private readonly string cornerRearLeft = "rearLeft";
    private readonly string cornerRearRight = "rearRight";
    private readonly string cornerFrontLeft = "frontLeft";
    private readonly string cornerFrontRight = "frontRight";
    private bool wasEdited = false;
    private List<Item> lastSelectedReferent = new();

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        EventManager.instance.EditModeIn.Add(OnEditModeIn);
        EventManager.instance.EditModeOut.Add(OnEditModeOut);
        EventManager.instance.OnSelectItem.Add(OnSelect);
    }

    private void OnDestroy()
    {
        EventManager.instance.EditModeIn.Remove(OnEditModeIn);
        EventManager.instance.EditModeOut.Remove(OnEditModeOut);
        EventManager.instance.OnSelectItem.Remove(OnSelect);
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
        if (GameManager.instance.selectMode && GameManager.instance.SelectIs<Item>())
        {
            foreach (GameObject obj in GameManager.instance.GetSelected())
            {
                if (GameManager.instance.configHandler.config.autoUHelpers && !GameManager.instance.GetPrevious().Contains(obj))
                    ToggleU(obj, true);
                if (obj.GetComponent<Item>().referent is Rack selectedRef && selectedRef.areUHelpersToggled)
                    HighlightULocation(obj.GetComponent<Item>());
            }
        }
        foreach (Item item in lastSelectedReferent)
            if (!GameManager.instance.GetSelectedReferents().Contains(item) && item is Rack rack && rack.areUHelpersToggled)
            {
                for (int i = 0; i < rack.uRoot.transform.GetChild(0).childCount; i++)
                    ChangeUColor(rack.uRoot, i, true);
                ToggleU(item.referent.gameObject, false);
            }
        lastSelectedReferent = GameManager.instance.GetSelectedReferents();
    }

    ///<summary>
    /// Highlight the ULocation at the same height than the given object.
    ///</summary>
    ///<param name="_item">The object to highlight its U location</param>
    private void HighlightULocation(Item _item)
    {
        Rack rackRef = Utils.GetRackReferent(_item);
        if (!rackRef || !rackRef.areUHelpersToggled)
            return;

        if (_item is Rack rack)
        {
            for (int i = 0; i < rack.uRoot.GetChild(0).childCount; i++)
                ChangeUColor(rack.uRoot, i, true);
            wasEdited = false;
        }
        else
        {
            if (wasEdited)
                return;

            if (_item.gameObject == GameManager.instance.GetSelected()[0])
                for (int i = 0; i < rackRef.uRoot.GetChild(0).childCount; i++)
                    ChangeUColor(rackRef.uRoot, i, false);

            Transform childTransform = _item.transform.GetChild(0);
            float center = childTransform.position.y;

            BoxCollider boxCollider = childTransform.GetComponent<BoxCollider>();
            bool isEnabled = boxCollider.enabled;
            boxCollider.enabled = true;
            float difference = boxCollider.bounds.extents.y;
            boxCollider.enabled = isEnabled;

            float delta = _item.transform.localPosition.y - _item.originalLocalPosition.y;
            float lowerBound = center - difference - delta;
            float upperBound = center + difference - delta;

            for (int i = 0; i < rackRef.uRoot.GetChild(0).childCount; i++)
            {
                Transform u = rackRef.uRoot.GetChild(0).GetChild(i);
                if (lowerBound < u.position.y && u.position.y < upperBound)
                    ChangeUColor(rackRef.uRoot, i, true);
            }
        }
    }

    /// <summary>
    /// Change the color of U helpers depending on their corner
    /// </summary>
    /// <param name="_uRoot">Root transform of U helpers</param>
    /// <param name="_index">U helper index</param>
    /// <param name="_activated">If the U helper is colored or not</param>
    private void ChangeUColor(Transform _uRoot, int _index, bool _activated)
    {
        GameObject obj = _uRoot.GetChild(0).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.red : Color.black;
        obj = _uRoot.GetChild(1).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.yellow : Color.black;
        obj = _uRoot.GetChild(2).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.blue : Color.black;
        obj = _uRoot.GetChild(3).GetChild(_index).gameObject;
        obj.GetComponent<Renderer>().material.color = _activated ? Color.green : Color.black;
    }

    ///<summary>
    /// Toggle U helpers of <paramref name="_target"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_target">A rack or a device</param>
    ///<param name="_active">Should the U helpers be visible ?</param>
    public void ToggleU(GameObject _target, bool _active)
    {
        Rack rack = Utils.GetRackReferent(_target.GetComponent<Item>());
        if (!rack)
            return;

        if (_active && !rack.uRoot)
            GenerateUHelpers(rack);
        rack.areUHelpersToggled = _active;
        rack.uRoot.gameObject.SetActive(rack.areUHelpersToggled);
        if (rack.areUHelpersToggled)
            HighlightULocation(_target.GetComponent<Item>());
    }

    ///<summary>
    /// Toggle U helpers of each GameObject in <paramref name="_targets"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_targets">A list of racks or devices</param>
    ///<param name="_active">Should the U helpers be visible ?</param>
    public void ToggleU(List<GameObject> _targets, bool _active)
    {
        foreach (GameObject obj in _targets)
            ToggleU(obj, _active);
    }

    ///<summary>
    /// Toggle U helpers of <paramref name="_target"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_target">A rack or a device</param>
    public void ToggleU(GameObject _target)
    {
        Rack rack = Utils.GetRackReferent(_target.GetComponent<Item>());
        if (!rack)
            return;

        ToggleU(_target, rack.areUHelpersToggled ^= true);
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "U helpers Toggled", new List<string>() { rack.areUHelpersToggled ? "ON" : "OFF", _target.name }), ELogTarget.logger, ELogtype.info);
    }

    ///<summary>
    /// Toggle U helpers of <paramref name="_selected"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_targets">A list of racks or devices</param>
    public void ToggleU(List<GameObject> _targets)
    {
        foreach (GameObject obj in _targets)
            ToggleU(obj);
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

        Vector3 boxSize = _rack.transform.GetChild(0).localScale;
        _rack.uRoot.localPosition = new Vector3(boxSize.x, 0, boxSize.z) / 2;
        _rack.uRoot.localEulerAngles = Vector3.zero;

        Transform URearLeft = new GameObject(cornerRearLeft).transform;
        URearLeft.parent = _rack.uRoot;
        URearLeft.localPosition = new(-boxSize.x / 2, 0, -boxSize.z / 2);
        URearLeft.localEulerAngles = Vector3.zero;
        Transform URearRight = new GameObject(cornerRearRight).transform;
        URearRight.parent = _rack.uRoot;
        URearRight.localPosition = new(boxSize.x / 2, 0, -boxSize.z / 2);
        URearRight.localEulerAngles = Vector3.zero;
        Transform UFrontLeft = new GameObject(cornerFrontLeft).transform;
        UFrontLeft.parent = _rack.uRoot;
        UFrontLeft.localPosition = new(-boxSize.x / 2, 0, boxSize.z / 2);
        UFrontLeft.localEulerAngles = Vector3.zero;
        Transform UFrontRight = new GameObject(cornerFrontRight).transform;
        UFrontRight.parent = _rack.uRoot;
        UFrontRight.localPosition = new(boxSize.x / 2, 0, boxSize.z / 2);
        UFrontRight.localEulerAngles = Vector3.zero;

        float scale = UnitValue.U;
        if ((string)_rack.attributes["heightUnit"] == LengthUnit.OU)
            scale = UnitValue.OU;

        //List<float> Uslotpositions = _rack.transform.Cast<Transform>().Where(t => t.GetComponent<Slot>() && t.GetComponent<Slot>().isU).Select(t => t.localPosition.y + 0.5f * (scale - t.GetChild(0).localScale.y)).Distinct().OrderBy(t => t).ToList();
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Transform child in _rack.transform)
            if (child.GetComponent<Slot>() is Slot slot && slot.isU)
            {
                minY = Mathf.Min(child.localPosition.y + 0.5f * scale, minY);
                maxY = Mathf.Max(child.localPosition.y + child.GetChild(0).localScale.y - 0.5f * scale, maxY);
            }

        if (minY < float.PositiveInfinity)
        {
            BuildU(minY, maxY, scale, URearLeft, cornerRearLeft, Color.red);
            BuildU(minY, maxY, scale, URearRight, cornerRearRight, Color.yellow);
            BuildU(minY, maxY, scale, UFrontLeft, cornerFrontLeft, Color.blue);
            BuildU(minY, maxY, scale, UFrontRight, cornerFrontRight, Color.green);
        }
        else if (_rack.attributes.ContainsKey("sizeWDHu") || _rack.attributes.ContainsKey("sizeWDHou") || (string)_rack.attributes["heightUnit"] == LengthUnit.U || (string)_rack.attributes["heightUnit"] == LengthUnit.OU)
        {
            float Unumber;
            if (_rack.attributes.ContainsKey("sizeWDHu"))
                Unumber = JsonConvert.DeserializeObject<int[]>((string)_rack.attributes["sizeWDHu"])[2]; // ??!
            else if (_rack.attributes.ContainsKey("sizeWDHou"))
                Unumber = JsonConvert.DeserializeObject<int[]>((string)_rack.attributes["sizeWDHou"])[2];
            else
                Unumber = (float)_rack.attributes["height"];
            bool exactHeight = Mathf.Approximately((int)Unumber, Unumber);
            float offset = scale / 2;
            BuildU(offset, offset + Unumber * scale, scale, URearLeft, cornerRearLeft, Color.red, exactHeight);
            BuildU(offset, offset + Unumber * scale, scale, URearRight, cornerRearRight, Color.yellow, exactHeight);
            BuildU(offset, offset + Unumber * scale, scale, UFrontLeft, cornerFrontLeft, Color.blue, exactHeight);
            BuildU(offset, offset + Unumber * scale, scale, UFrontRight, cornerFrontRight, Color.green, exactHeight);
        }
    }

    ///<summary>
    /// Call by GUI on reset transforms
    ///</summary>
    public void ResetUHelpers()
    {
        if (GameManager.instance.editMode || GameManager.instance.positionMode)
            return;
        if (!Utils.IsObjectMoved(GameManager.instance.GetFocused()[^1].GetComponent<OgreeObject>()))
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
    /// <param name="_exactHeight">true if the height of the U column is exactly a multiple of U height</param>
    private void BuildU(float _firstPositionY, float _lastPositionY, float _scale, Transform _UColumn, string _cornerName, Color _color, bool _exactHeight = false)
    {
        int floorNumber = 0;
        for (float positionY = _firstPositionY; positionY <= _lastPositionY || (Mathf.Abs(positionY - _lastPositionY) <= 1e-5 && !_exactHeight); positionY += _scale)
        {
            floorNumber++;
            Transform helper = Instantiate(GameManager.instance.uLocationModel, _UColumn).transform;
            helper.localPosition = positionY * Vector3.up;
            helper.name = $"{_cornerName}_u{floorNumber}";
            helper.GetChild(0).GetComponent<TextMeshPro>().text = floorNumber.ToString();
            helper.GetChild(1).GetComponent<TextMeshPro>().text = floorNumber.ToString();
            helper.localScale = Vector3.one * _scale;
            helper.GetComponent<Renderer>().material.color = _color;
        }
    }
}
