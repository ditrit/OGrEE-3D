using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class UManager : MonoBehaviour
{
    static public UManager um;
    public float yPositionDelta = 0.0f;
    public float initialYPosition = 0.0f;
    private GameObject objFocused;
    public string cornerRearLeft = "rearLeft";
    public string cornerRearRight = "rearRight";
    public string cornerFrontLeft = "frontLeft";
    public string cornerFrontRight = "frontRight";
    public bool isFocused = false;

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
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);
    }

    void Update()
    {
        if (isFocused)
            yPositionDelta = objFocused.transform.position.y - initialYPosition;
    }

    private void OnEditModeIn(EditModeInEvent _e)
    {
        objFocused = _e.obj;
        initialYPosition = _e.obj.transform.position.y;
        isFocused = true;
    }

    private void OnEditModeOut(EditModeOutEvent _e)
    {
        isFocused = false;
    }

    ///<summary>
    /// Highlight the ULocation at the same height than the selected device.
    ///</summary>
    ///<param name="_obj">The object to save. If null, set default text</param>
    public void HighlightULocation()
    {
        if (GameManager.gm.currentItems[0].GetComponent<OgreeObject>().category != "rack")
        {
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

            float lowerBound = center - difference - yPositionDelta;
            float upperBound = center + difference - yPositionDelta;
            t = GameManager.gm.currentItems[0].transform;
            while(t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    GameObject uRoot = t.Find("uRoot").gameObject;
                    uRoot.SetActive(true);
                    for (int i = 0; i < uRoot.transform.childCount; i++)
                    {
                        if (lowerBound < uRoot.transform.GetChild(i).position.y && uRoot.transform.GetChild(i).position.y < upperBound)
                        {
                            GameObject obj = uRoot.transform.GetChild(i).gameObject;
                            string name = obj.name;
                            if( Regex.IsMatch(name, cornerRearLeft, RegexOptions.IgnoreCase) )
                                obj.GetComponent<Renderer>().material.color = Color.red;
                            if( Regex.IsMatch(name, cornerRearRight, RegexOptions.IgnoreCase) )
                                obj.GetComponent<Renderer>().material.color = Color.yellow;
                            if( Regex.IsMatch(name, cornerFrontLeft, RegexOptions.IgnoreCase) )
                                obj.GetComponent<Renderer>().material.color = Color.blue;   
                            if( Regex.IsMatch(name, cornerFrontRight, RegexOptions.IgnoreCase) )
                                obj.GetComponent<Renderer>().material.color = Color.green;                                                             
                        }
                        else
                        {
                            uRoot.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().material.color = new Color (0, 0, 0, 1);
                        }
                    }
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
        }
        else
        {
            GameObject uRoot = GameManager.gm.currentItems[0].transform.Find("uRoot").gameObject;

            for (int i = 0; i < uRoot.transform.childCount; i++)
            {
                GameObject obj = uRoot.transform.GetChild(i).gameObject;
                uRoot.SetActive(true);
                string name = obj.name;
                if( Regex.IsMatch(name, cornerRearLeft, RegexOptions.IgnoreCase) )
                    obj.GetComponent<Renderer>().material.color = Color.red;
                if( Regex.IsMatch(name, cornerRearRight, RegexOptions.IgnoreCase) )
                    obj.GetComponent<Renderer>().material.color = Color.yellow;
                if( Regex.IsMatch(name, cornerFrontLeft, RegexOptions.IgnoreCase) )
                    obj.GetComponent<Renderer>().material.color = Color.blue;   
                if( Regex.IsMatch(name, cornerFrontRight, RegexOptions.IgnoreCase) )
                    obj.GetComponent<Renderer>().material.color = Color.green;  
            }
        }
    }
}
