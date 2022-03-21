using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

public static class Utils
{
    ///<summary>
    /// Parse a string with format "[x,y]" into a Vector2.
    ///</summary>
    ///<param name="_input">String with format "[x,y]"</param>
    public static Vector2 ParseVector2(string _input)
    {
        Vector2 res = new Vector2();

        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        res.x = ParseDecFrac(parts[0]);
        res.y = ParseDecFrac(parts[1]);
        return res;
    }

    ///<summary>
    /// Parse a string with format "[x,y,z]" into a Vector3. The vector can be given in Y axis or Z axis up.
    ///</summary>
    ///<param name="_input">String with format "[x,y,z]"</param>
    ///<param name="_ZUp">Is the coordinates given are in Z axis up or Y axis up ? </param>
    public static Vector3 ParseVector3(string _input, bool _ZUp = true)
    {
        Vector3 res = new Vector3();

        _input = _input.Trim('[', ']');
        string[] parts = _input.Split(',');
        res.x = ParseDecFrac(parts[0]);
        if (_ZUp)
        {
            res.y = ParseDecFrac(parts[2]);
            res.z = ParseDecFrac(parts[1]);
        }
        else
        {
            res.y = ParseDecFrac(parts[1]);
            res.z = ParseDecFrac(parts[2]);
        }
        return res;
    }

    ///<summary>
    /// Parse a string into a float. Can be decimal, a fraction and/or negative.
    ///</summary>
    ///<param name="_input">The string which contains the float</param>
    public static float ParseDecFrac(string _input)
    {
        if (_input.Contains("/"))
        {
            string[] div = _input.Split('/');
            float a = float.Parse(div[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            float b = float.Parse(div[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            return a / b;
        }
        else
            return float.Parse(_input, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
    }

    ///<summary>
    /// Gets every Racks in GameManager.allItems and set their Collider.enabled.
    ///</summary>
    ///<param name="_value">The value to set</param>
    public static void SwitchAllCollidersInRacks(bool _value)
    {
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<Rack>())
                obj.GetComponent<Collider>().enabled = _value;
        }
    }

    ///<summary>
    /// Tries to return given Transform, otherwise look for given parent Id
    ///</summary>
    ///<param name="_parent">The Transform to check</param>
    ///<param name="_parentId">The ID to search</param>
    ///<returns>A valid Transform or null</returns>
    public static Transform FindParent(Transform _parent, string _parentId)
    {
        Transform parent = null;
        if (_parent)
            parent = _parent;
        else
        {
            foreach (DictionaryEntry de in GameManager.gm.allItems)
            {
                GameObject go = (GameObject)de.Value;
                if (go.GetComponent<OgreeObject>().id == _parentId)
                    parent = go.transform;
            }
        }
        return parent;
    }

    ///<summary>
    /// Cast a raycast from main camera and returns hit object
    ///</summary>
    ///<returns>Hit GameObject or null</returns>
    public static GameObject RaycastFromCameraToMouse()
    {
        RaycastHit hit;
        //Vector3 pointerPosition = new Vector3();
        //pointerPosition = Microsoft.MixedReality.Toolkit.Input.IMixedRealityPointer.Position;
        Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out hit);
        //IMixedRealityRaycastProvider.Raycast(out MixedRealityRaycastHit);
        if (hit.collider)
        {
            // Debug.Log(hit.collider.transform.parent.name);
            return hit.collider.transform.parent.gameObject;
        }
        else
            return null;
    }

    ///<summary>
    /// Get an object from GameManager.allItems by it's id.
    ///</summary>
    ///<param name="_id">The id to search</param>
    public static GameObject GetObjectById(string _id)
    {
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<OgreeObject>().id == _id)
                return obj;
        }
        return null;
    }

    public static void MoveObjectToCamera(GameObject _obj, Camera m_camera)
    {
        float speed = 10f * Time.deltaTime;
        float localAngleCameraRadian = Mathf.Deg2Rad * m_camera.transform.eulerAngles.y;
        Vector3 offset = new Vector3(Mathf.Sin(localAngleCameraRadian), .0f, Mathf.Cos(localAngleCameraRadian));
        GameManager.gm.AppendLogLine(offset.ToString(), "green");
        Vector3 newPostion = new Vector3(m_camera.transform.position.x, 0.0f, m_camera.transform.position.z);
        Vector3 newRotation = new Vector3(0.0f, m_camera.transform.eulerAngles.y + 90, 0.0f);

        _obj.transform.position = newPostion + offset;
        _obj.transform.localRotation = Quaternion.Euler(newRotation);
    }
}
