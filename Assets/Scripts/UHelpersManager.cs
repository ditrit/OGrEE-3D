using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
        ToggleU(GameManager.instance.GetSelected()[0].transform, false);
    }

    ///<summary>
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    private void OnEditModeOut(EditModeOutEvent _e)
    {
        wasEdited = false;
        ToggleU(GameManager.instance.GetSelected()[0].transform, true);
    }

    ///<summary>
    /// When called, toggle U helpers and highlight U helpers when needed.
    ///</summary>
    ///<param name="_e">Event raised when selecting something</param>
    private void OnSelect(OnSelectItemEvent _e)
    {
        if (GameManager.instance.selectMode && !GameManager.instance.SelectIs<OgreeObject>("tempBar"))
        {
            ToggleU(GameManager.instance.GetSelected()[0].transform, true);
            HighlightULocation();
        }
    }

    ///<summary>
    /// Highlight the ULocation at the same height than the selected device.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    private void HighlightULocation()
    {
        OObject oObject = GameManager.instance.GetSelected()[0].GetComponent<OObject>();
        Rack rack = Utils.GetRackReferent(oObject);
        if (!rack)
            return;

        if (oObject.category == "rack")
        {
            GameObject uRoot = rack.uRoot.gameObject;
            uRoot.SetActive(true);  
            for (int i = 0; i < uRoot.transform.GetChild(0).childCount; i++)
                ChangeUColor(uRoot, i,true);
            wasEdited = false;
        }
        else if (oObject.category == "device")
        {
            if (wasEdited)
                return;

            float difference;
            Transform t = GameManager.instance.GetSelected()[0].transform.GetChild(0);
            float center = t.position.y;

            BoxCollider boxCollider = t.GetComponent<BoxCollider>();
            bool isEnabled = boxCollider.enabled;
            boxCollider.enabled = true;
            difference = boxCollider.bounds.extents.y;
            boxCollider.enabled = isEnabled;

            t = GameManager.instance.GetSelected()[0].transform;
            float delta = t.localPosition.y - t.GetComponent<OgreeObject>().originalLocalPosition.y;
            float lowerBound = center - difference - delta;
            float upperBound = center + difference - delta;

            GameObject uRoot = rack.GetComponent<Rack>().uRoot.gameObject;
            uRoot.SetActive(true);
            for (int i = 0; i < uRoot.transform.GetChild(0).childCount; i++)
            {
                Transform u = uRoot.transform.GetChild(0).GetChild(i);
                ChangeUColor(uRoot, i, lowerBound < u.position.y && u.position.y < upperBound);
            }
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
    public void ToggleU(Transform _transform, bool _active)
    {
        Rack rack = Utils.GetRackReferent(_transform.GetComponent<OObject>());
        if (!rack)
            return;

        Transform uRoot = rack.uRoot;
        if (_active)
        {
            if (!uRoot)
                GenerateUHelpers(_transform.GetComponent<Rack>());
            else
                uRoot.gameObject.SetActive(true);
        }
        else if (uRoot)
            uRoot.gameObject.SetActive(false);
    }

    ///<summary>
    /// Toggle U helpers of <paramref name="_transform"/> if it is a rack or of its parent rack otherwise
    ///</summary>
    ///<param name="_transform">The transform of a rack or a device</param>
    public void ToggleU(Transform _transform)
    {
        Rack rack = Utils.GetRackReferent(_transform.GetComponent<OObject>());
        if (!rack)
            return;

        Transform uRoot = rack.uRoot;
        if (!uRoot)
        {
            GenerateUHelpers(_transform.GetComponent<Rack>());
            GameManager.instance.AppendLogLine($"U helpers ON for {_transform.name}.", ELogTarget.logger, ELogtype.info);
        }
        else if (!uRoot.gameObject.activeSelf)
        {
            uRoot.gameObject.SetActive(true);
            GameManager.instance.AppendLogLine($"U helpers ON for {_transform.name}.", ELogTarget.logger, ELogtype.info);
        }
        else
        {
            uRoot.gameObject.SetActive(false);
            GameManager.instance.AppendLogLine($"U helpers OFF for {_transform.name}.", ELogTarget.logger, ELogtype.info);
        }
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
        Transform URearLeft = Instantiate(new GameObject(cornerRearLeft), _rack.uRoot).transform;
        URearLeft.localPosition = new Vector3(-boxSize.x / 2, 0, -boxSize.z / 2);
        Transform URearRight = Instantiate(new GameObject(cornerRearRight), _rack.uRoot).transform;
        URearRight.localPosition = new Vector3(boxSize.x / 2, 0, -boxSize.z / 2);
        Transform UFrontLeft = Instantiate(new GameObject(cornerFrontLeft),_rack.uRoot).transform;
        UFrontLeft.localPosition = new Vector3(-boxSize.x / 2, 0, boxSize.z / 2);
        Transform UFrontRight = Instantiate(new GameObject(cornerFrontRight), _rack.uRoot).transform;
        UFrontRight.localPosition = new Vector3(boxSize.x / 2, 0, boxSize.z / 2);

        float scale = GameManager.instance.uSize;
        if (_rack.attributes["heightUnit"] == "OU")
            scale = GameManager.instance.ouSize;

        List<float> Uslotpositions = _rack.transform.Cast<Transform>().Where(t => t.GetComponent<Slot>() && t.GetComponent<Slot>().isU).Select(t=>t.localPosition.y).Distinct().OrderBy(t => t).ToList();
        if (Uslotpositions.Count > 0)
        {
            for (int i = 0; i < Uslotpositions.Count; i++)
                BuildU(Uslotpositions[i], i + 1, scale, URearLeft, URearRight, UFrontLeft, UFrontRight);
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

            float offset = - (Unumber-1) * scale / 2;
            for (int i = 0; i < Unumber; i++)
                BuildU((offset + i * scale), i + 1, scale, URearLeft, URearRight, UFrontLeft, UFrontRight);
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
            ToggleU(GameManager.instance.GetSelected()[0].transform, true);
        }
    }

    /// <summary>
    /// Build one floor of the four U-helpers columns
    /// </summary>
    /// <param name="_positionY">(local) vertical position of the floor</param>
    /// <param name="_number">number of the floor</param>
    /// <param name="_scale">height of the floor (u size or ou size)</param>
    /// <param name="_URearLeft">parent of the rear-left column</param>
    /// <param name="_URearRight">parent of the rear-right column</param>
    /// <param name="_UFrontLeft">parent of the front-left column</param>
    /// <param name="_UFrontRight">parent of the front-right column</param>
    private void BuildU(float _positionY, int _number, float _scale, Transform _URearLeft, Transform _URearRight, Transform _UFrontLeft, Transform _UFrontRight)
    {
        Transform rearLeft = Instantiate(GameManager.instance.uLocationModel, _URearLeft).transform;
        rearLeft.localPosition = _positionY * Vector3.up;
        rearLeft.name = $"{cornerRearLeft}_u{_number}";
        rearLeft.GetComponentInChildren<TextMeshPro>().text = _number.ToString();
        rearLeft.localScale = Vector3.one * _scale;
        rearLeft.GetComponent<Renderer>().material.color = Color.red;

        Transform rearRight = Instantiate(GameManager.instance.uLocationModel, _URearRight).transform;
        rearRight.localPosition = _positionY * Vector3.up;
        rearRight.name = $"{cornerRearRight}_u{_number}";
        rearRight.GetComponentInChildren<TextMeshPro>().text = _number.ToString();
        rearRight.localScale = Vector3.one * _scale;
        rearRight.GetComponent<Renderer>().material.color = Color.yellow;

        Transform frontLeft = Instantiate(GameManager.instance.uLocationModel,_UFrontLeft).transform;
        frontLeft.localPosition = _positionY * Vector3.up;
        frontLeft.name = $"{cornerFrontLeft}_u{_number}";
        frontLeft.GetComponentInChildren<TextMeshPro>().text = _number.ToString();
        frontLeft.localScale = Vector3.one * _scale;
        frontLeft.GetComponent<Renderer>().material.color = Color.blue;

        Transform frontRight = Instantiate(GameManager.instance.uLocationModel, _UFrontRight).transform;
        frontRight.localPosition = _positionY * Vector3.up;
        frontRight.name = $"{cornerFrontRight}_u{_number}";
        frontRight.GetComponentInChildren<TextMeshPro>().text = _number.ToString();
        frontRight.localScale = Vector3.one * _scale;
        frontRight.GetComponent<Renderer>().material.color = Color.green;
    }
}
