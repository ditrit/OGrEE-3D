using UnityEngine;
using TMPro;
using System;

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
        if (GameManager.instance.SelectIs<OgreeObject>("tempBar"))
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
            for (int i = 0; i < uRoot.transform.childCount; i++)
                ChangeUColor(uRoot, i);
            wasEdited = false;
        }
        else if (oObject.category == "device")
        {
            if (wasEdited)
                return;

            float difference;
            Transform t = GameManager.instance.GetSelected()[0].transform.GetChild(0);
            float center = t.position.y;

            if (t.GetComponent<BoxCollider>().enabled)
                difference = t.GetComponent<BoxCollider>().bounds.extents.y;
            else
            {
                t.GetComponent<BoxCollider>().enabled = true;
                difference = t.GetComponent<BoxCollider>().bounds.extents.y;
                t.GetComponent<BoxCollider>().enabled = false;
            }

            t = GameManager.instance.GetSelected()[0].transform;
            float delta = t.localPosition.y - t.GetComponent<OgreeObject>().originalLocalPosition.y;
            float lowerBound = center - difference - delta;
            float upperBound = center + difference - delta;

            GameObject uRoot = rack.GetComponent<Rack>().uRoot.gameObject;
            uRoot.SetActive(true);
            for (int i = 0; i < uRoot.transform.childCount; i++)
            {
                if (lowerBound < uRoot.transform.GetChild(i).position.y && uRoot.transform.GetChild(i).position.y < upperBound)
                    ChangeUColor(uRoot, i);
                else
                    uRoot.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material.color = Color.black;
            }
            return;
        }
    }

    /// <summary>
    /// Change the color of U helpers depending on their corner
    /// </summary>
    /// <param name="_uRoot">Root transform of U helpers</param>
    /// <param name="_index">U helper index</param>
    private void ChangeUColor(GameObject _uRoot, int _index)
    {
        GameObject obj = _uRoot.transform.GetChild(_index).gameObject;
        if (obj.name.StartsWith(cornerRearLeft))
            obj.GetComponent<Renderer>().material.color = Color.red;
        if (obj.name.StartsWith(cornerRearRight))
            obj.GetComponent<Renderer>().material.color = Color.yellow;
        if (obj.name.StartsWith(cornerFrontLeft))
            obj.GetComponent<Renderer>().material.color = Color.blue;
        if (obj.name.StartsWith(cornerFrontRight))
            obj.GetComponent<Renderer>().material.color = Color.green;
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
        else if (!_active && uRoot)
            uRoot.gameObject.SetActive(false);
        return;
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
            GameManager.instance.AppendLogLine($"U helpers ON for {_transform.name}.", false, ELogtype.info);
        }
        else if (uRoot.gameObject.activeSelf == false)
        {
            uRoot.gameObject.SetActive(true);
            GameManager.instance.AppendLogLine($"U helpers ON for {_transform.name}.", false, ELogtype.info);
        }
        else
        {
            uRoot.gameObject.SetActive(false);
            GameManager.instance.AppendLogLine($"U helpers OFF for {_transform.name}.", false, ELogtype.info);
        }
        return;
    }

    ///<summary>
    /// Create uRoot, place it and call <see cref="GenerateUColumn"/> for each corner
    ///</summary>
    ///<param name="_rack">The rack where we create the U helpers</param>
    public void GenerateUHelpers(Rack _rack)
    {
        if (_rack.uRoot)
            return;
        Vector3 rootPos;
        Transform box = _rack.transform.GetChild(0);
        if (box.childCount == 0)
            rootPos = new Vector3(0, box.localScale.y / -2, 0);
        else
            rootPos = new Vector3(0, box.GetComponent<BoxCollider>().size.y / -2, 0);

        _rack.uRoot = new GameObject("uRoot").transform;
        _rack.uRoot.parent = _rack.transform;
        _rack.uRoot.localPosition = rootPos;
        _rack.uRoot.localEulerAngles = Vector3.zero;
        GenerateUColumn(_rack, cornerRearLeft);
        GenerateUColumn(_rack, cornerRearRight);
        GenerateUColumn(_rack, cornerFrontLeft);
        GenerateUColumn(_rack, cornerFrontRight);
    }

    ///<summary>
    /// Instantiate one <see cref="GameManager.uLocationModel"/> per U in the given column
    ///</summary>
    ///<param name="_corner">Corner of the column</param>
    ///<param name="_rack">The rack where we create the U helpers</param>
    private void GenerateUColumn(Rack _rack, string _corner)
    {
        Vector3 boxSize = _rack.transform.GetChild(0).localScale;

        // By default, attributes["heightUnit"] == "U"
        float scale = GameManager.instance.uSize;
        int max = (int)Utils.ParseDecFrac(_rack.attributes["height"]);
        if (_rack.attributes["heightUnit"] == "OU")
            scale = GameManager.instance.ouSize;
        else if (_rack.attributes["heightUnit"] == "cm")
            max = Mathf.FloorToInt(Utils.ParseDecFrac(_rack.attributes["height"]) / (GameManager.instance.uSize * 100));

        if (!string.IsNullOrEmpty(_rack.attributes["template"]))
        {
            Transform firstSlot = null;
            int minHeight = 0;
            foreach (Transform child in _rack.transform)
            {
                if (child.GetComponent<Slot>() && child.GetComponent<Slot>().orient == "horizontal")
                {
                    if (firstSlot)
                    {
                        // Should use position.y instead of its name
                        int height = GetUNumberFromName(child.name);
                        if (height < minHeight)
                            firstSlot = child;
                        else if (height > max)
                            max = height;
                    }
                    else
                    {
                        firstSlot = child;
                        minHeight = GetUNumberFromName(child.name);
                        max = minHeight;
                    }
                }
            }
            if (firstSlot)
            {
                _rack.uRoot.localPosition = new Vector3(_rack.uRoot.localPosition.x, firstSlot.localPosition.y, _rack.uRoot.localPosition.z);
                _rack.uRoot.localPosition -= new Vector3(0, firstSlot.GetChild(0).localScale.y / 2, 0);
            }
        }
        for (int i = 1; i <= max; i++)
        {
            Transform obj = Instantiate(GameManager.instance.uLocationModel).transform;
            obj.name = $"{_corner}_u{i}";
            obj.GetComponentInChildren<TextMeshPro>().text = i.ToString();
            obj.parent = _rack.uRoot;
            obj.localScale = Vector3.one * scale;
            if (_corner == cornerRearLeft)
            {
                obj.localPosition = new Vector3(-boxSize.x / 2, i * scale - scale / 2, -boxSize.z / 2);
                obj.localEulerAngles = new Vector3(0, 0, 0);
                obj.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (_corner == cornerRearRight)
            {
                obj.localPosition = new Vector3(boxSize.x / 2, i * scale - scale / 2, -boxSize.z / 2);
                obj.localEulerAngles = new Vector3(0, 0, 0);
                obj.GetComponent<Renderer>().material.color = Color.yellow;
            }
            else if (_corner == cornerFrontLeft)
            {
                obj.localPosition = new Vector3(-boxSize.x / 2, i * scale - scale / 2, boxSize.z / 2);
                obj.localEulerAngles = new Vector3(0, 180, 0);
                obj.GetComponent<Renderer>().material.color = Color.blue;
            }
            else if (_corner == cornerFrontRight)
            {
                obj.localPosition = new Vector3(boxSize.x / 2, i * scale - scale / 2, boxSize.z / 2);
                obj.localEulerAngles = new Vector3(0, 180, 0);
                obj.GetComponent<Renderer>().material.color = Color.green;
            }
            else
                Debug.LogError("Unkown Corner");
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
    /// Get the U number from the name of a U helper cube
    /// </summary>
    /// <param name="_name">the name of the U helper</param>
    /// <returns>The number of the U helper</returns>
    private int GetUNumberFromName(string _name)
    {
        string iteratedName = _name;
        while (iteratedName.Length > 0)
        {
            try
            {
                return (int)Utils.ParseDecFrac(iteratedName);
            }
            catch (FormatException)
            {
                iteratedName = iteratedName.Substring(1);
            }
        }
        return 0;
    }
}
