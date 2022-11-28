using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;

public class UHelpersManager : MonoBehaviour
{
    static public UHelpersManager um;

    private string cornerRearLeft = "rearLeft";
    private string cornerRearRight = "rearRight";
    private string cornerFrontLeft = "frontLeft";
    private string cornerFrontRight = "frontRight";
    [SerializeField] private bool wasEdited = false;

    private void Awake()
    {
        if (!um)
            um = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelect);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelect);
    }

    ///<summary>
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    private void OnEditModeIn(EditModeInEvent _e)
    {
        wasEdited = true;
        ToggleU(GameManager.gm.currentItems[0].transform, false);
    }

    ///<summary>
    /// When called, toggle U helpers and highlight U helpers when needed.
    ///</summary>
    ///<param name="_e">Event raised when selecting something</param>
    private void OnSelect(OnSelectItemEvent _e)
    {
        if (GameManager.gm.currentItems.Count > 0 && GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category != "tempBar")
        {
            ToggleU(GameManager.gm.currentItems[0].transform, true);
            HighlightULocation();
        }
    }

    ///<summary>
    /// Highlight the ULocation at the same height than the selected device.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    private async void HighlightULocation()
    {
        string category = GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category;
        if (category == "rack")
        {
            Transform t = GameManager.gm.currentItems[0].transform;
            while (!t.GetComponent<Rack>().uRoot)
                await Task.Delay(50);
            GameObject uRoot = t.GetComponent<Rack>().uRoot.gameObject;
            uRoot.SetActive(true);
            for (int i = 0; i < uRoot.transform.childCount; i++)
                ChangeUColor(uRoot, i);
            wasEdited = false;
        }
        else if (category == "device")
        {
            if (wasEdited)
                return;

            float difference;
            Transform t = GameManager.gm.currentItems[0].transform.GetChild(0);
            float center = t.position.y;

            if (t.GetComponent<BoxCollider>().enabled)
                difference = t.GetComponent<BoxCollider>().bounds.extents.y;
            else
            {
                t.GetComponent<BoxCollider>().enabled = true;
                difference = t.GetComponent<BoxCollider>().bounds.extents.y;
                t.GetComponent<BoxCollider>().enabled = false;
            }

            t = GameManager.gm.currentItems[0].transform;
            float delta = t.localPosition.y - t.GetComponent<OgreeObject>().originalLocalPosition.y;
            float lowerBound = center - difference - delta;
            float upperBound = center + difference - delta;
            while (t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    while (!t.GetComponent<Rack>().uRoot)
                    {
                        await Task.Delay(50);
                    }
                    GameObject uRoot = t.Find("uRoot").gameObject;
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
                t = t.parent.transform;
            }
        }
    }

    ///<summary>
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    private void ChangeUColor(GameObject _uRoot, int _i)
    {
        GameObject obj = _uRoot.transform.GetChild(_i).gameObject;
        string name = obj.name;
        if (Regex.IsMatch(name, cornerRearLeft, RegexOptions.IgnoreCase))
            obj.GetComponent<Renderer>().material.color = Color.red;
        if (Regex.IsMatch(name, cornerRearRight, RegexOptions.IgnoreCase))
            obj.GetComponent<Renderer>().material.color = Color.yellow;
        if (Regex.IsMatch(name, cornerFrontLeft, RegexOptions.IgnoreCase))
            obj.GetComponent<Renderer>().material.color = Color.blue;
        if (Regex.IsMatch(name, cornerFrontRight, RegexOptions.IgnoreCase))
            obj.GetComponent<Renderer>().material.color = Color.green;
    }

    ///<summary>
    /// Toggle U location cubes for rack or parent rack.
    ///</summary>
    ///<param name="_bool">True or false value</param>
    public void ToggleU(Transform _transform, bool _bool)
    {
        while (_transform != null)
        {
            if (_transform.GetComponent<Rack>())
            {
                Transform uRoot = _transform.GetComponent<Rack>().uRoot;
                if (_bool)
                {
                    if (!uRoot)
                        GenerateUHelpers(_transform.GetComponent<Rack>());
                    else
                        uRoot.gameObject.SetActive(true);
                }
                else if (!_bool && uRoot)
                    uRoot.gameObject.SetActive(false);
                return;
            }
            _transform = _transform.parent;
        }
    }

    ///<summary>
    /// Toggle U location cubes for rack or parent rack.
    ///</summary>
    public void ToggleU(Transform _transform)
    {
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine("Empty selection.", false, eLogtype.warning);
            return;
        }

        while (_transform != null)
        {
            if (_transform.GetComponent<Rack>())
            {
                Transform uRoot = _transform.GetComponent<Rack>().uRoot;
                if (!uRoot)
                {
                    GenerateUHelpers(_transform.GetComponent<Rack>());
                    GameManager.gm.AppendLogLine($"U helpers ON for {_transform.name}.", false, eLogtype.info);
                }
                else if (uRoot.gameObject.activeSelf == false)
                {
                    uRoot.gameObject.SetActive(true);
                    GameManager.gm.AppendLogLine($"U helpers ON for {_transform.name}.", false, eLogtype.info);
                }
                else
                {
                    uRoot.gameObject.SetActive(false);
                    GameManager.gm.AppendLogLine($"U helpers OFF for {_transform.name}.", false, eLogtype.info);
                }
                return;
            }
            _transform = _transform.parent;
        }
    }

    ///<summary>
    /// Create uRoot, place it and call GenerateUColumn() for each corner
    ///</summary>
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
    /// Instantiate one GameManager.uLocationModel per U in the given column
    ///</summary>
    ///<param name="_corner">Corner of the column</param>
    private void GenerateUColumn(Rack _rack, string _corner)
    {
        Vector3 boxSize = _rack.transform.GetChild(0).localScale;

        // By default, attributes["heightUnit"] == "U"
        float scale = GameManager.gm.uSize;
        int max = (int)Utils.ParseDecFrac(_rack.attributes["height"]);
        if (_rack.attributes["heightUnit"] == "OU")
            scale = GameManager.gm.ouSize;
        else if (_rack.attributes["heightUnit"] == "cm")
            max = Mathf.FloorToInt(Utils.ParseDecFrac(_rack.attributes["height"]) / (GameManager.gm.uSize * 100));

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
            Transform obj = Instantiate(GameManager.gm.uLocationModel).transform;
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
        if (GameManager.gm.editMode)
            return;
        if (!Utils.IsObjectMoved(GameManager.gm.focus[GameManager.gm.focus.Count - 1].GetComponent<OgreeObject>()))
        {
            wasEdited = false;
            ToggleU(GameManager.gm.currentItems[0].transform, true);
        }
    }

    private int GetUNumberFromName(string _name)
    {
        string iteratedName = _name;
        while (iteratedName.Length > 0)
        {
            try
            {
                return (int)Utils.ParseDecFrac(_name);
            }
            catch (System.FormatException)
            {
                iteratedName = iteratedName.Substring(1);
            }
        }
        return 0;
    }

}
