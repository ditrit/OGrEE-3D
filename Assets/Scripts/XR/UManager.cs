using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Microsoft.MixedReality.Toolkit.Input;
using System.Threading.Tasks;

public class UManager : MonoBehaviour
{
    static public UManager um;
    public float yPositionDelta = 0.0f;
    public float initialYPosition = 0.0f;
    public string cornerRearLeft = "rearLeft";
    public string cornerRearRight = "rearRight";
    public string cornerFrontLeft = "frontLeft";
    public string cornerFrontRight = "frontRight";
    public bool isFocused = false;
    public bool wasEdited = false;

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
    }

    ///<summary>
    /// Highlight the ULocation at the same height than the selected device.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    public async Task HighlightULocation()
    {
        if (GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category != "rack")
        {
            if (wasEdited)
            {
                return;
            }

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

            DeltaPositionManager delta = GameManager.gm.currentItems[0].GetComponent<DeltaPositionManager>();
            float lowerBound = center - difference - delta.yPositionDelta;
            float upperBound = center + difference - delta.yPositionDelta;
            t = GameManager.gm.currentItems[0].transform;
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
                        {
                            ChangeUColor(uRoot, i);
                        }
                        else
                        {
                            uRoot.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 0, 1);
                        }
                    }
                    return;
                }
                t = t.parent.transform;
            }
        }
        else
        {
            Transform t = GameManager.gm.currentItems[0].transform;
            while (!t.GetComponent<Rack>().uRoot)
            {
                await Task.Delay(50);
            }
            GameObject uRoot = GameManager.gm.currentItems[0].transform.Find("uRoot").gameObject;
            uRoot.SetActive(true);
            for (int i = 0; i < uRoot.transform.childCount; i++)
            {
                ChangeUColor(uRoot, i);
            }
            wasEdited = false;
        }
    }

    ///<summary>
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    public void ChangeUColor(GameObject _uRoot, int _i)
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
    /// Disable Uhelpers when entering in edit mode.
    ///</summary>
    ///<param name="_e">Event raised when entering edit mode</param>
    public void OnEditModeIn(EditModeInEvent _e)
    {
        wasEdited = true;
        ToggleU(false);
    }

    ///<summary>
    /// Toggle U location cubes.
    ///</summary>
    ///<param name="_bool">True or false value</param>
    public void ToggleU(bool _bool)
    {
        Transform t = GameManager.gm.currentItems[0].transform;
        while (t != null)
        {
            if (t.GetComponent<OgreeObject>().category == "rack")
            {
                GameObject uRoot = t.Find("uRoot").gameObject;
                if (_bool)
                    uRoot.SetActive(true);
                else if (!_bool)
                    uRoot.SetActive(false);
                return;
            }
            t = t.parent.transform;
        }
    }

}
