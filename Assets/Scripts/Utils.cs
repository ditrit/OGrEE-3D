﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

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
        _input = _input.Replace(",", ".");
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
        Physics.Raycast(Camera.main.transform.position, Camera.main.ScreenPointToRay(Input.mousePosition).direction, out RaycastHit hit);
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

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    ///<returns>The wanted color</returns>
    public static Color ParseColor(string _hex)
    {
        ColorUtility.TryParseHtmlString($"#{_hex}", out Color newColor);
        return newColor;
    }

    ///<summary>
    /// Get a lightest version of the inverted color.
    ///</summary>
    ///<param name="_color">The base color</param>
    ///<returns>The new color</returns>
    public static Color InvertColor(Color _color)
    {
        float max = _color.maxColorComponent;
        return new Color(max - _color.r / 3, max - _color.g / 3, max - _color.b / 3, _color.a);
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_physicalList">The list of physical objects to complete</param>
    ///<param name="_logicalList">The list of logical objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    public static void ParseNestedObjects(List<SApiObject> _physicalList, List<SApiObject> _logicalList, SApiObject _src)
    {
        if (_src.category == "group")
            _logicalList.Add(_src);
        else
            _physicalList.Add(_src);
        if (_src.children != null)
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_physicalList, _logicalList, obj);
        }
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_list">The list of objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    public static void ParseNestedObjects(List<SApiObject> _list, SApiObject _src)
    {
        _list.Add(_src);
        if (_src.children != null)
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_list, obj);
        }
    }

    ///<summary>
    /// Check is the given OgreeObject has been moved, rotated or rescaled.
    ///</summary>
    ///<param name="_obj">The object to check</param>
    ///<returns>True or false</returns>
    public static bool IsObjectMoved(OgreeObject _obj)
    {
        if (_obj.originalLocalPosition != _obj.transform.localPosition)
            return true;
        if (_obj.originalLocalRotation != _obj.transform.localRotation)
            return true;
        if (_obj.originalLocalScale != _obj.transform.localScale)
            return true;
        return false;
    }
}
