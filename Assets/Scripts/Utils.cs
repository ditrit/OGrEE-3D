﻿using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class Utils
{
    ///<summary>
    /// Parse a string into a float. Can be decimal, a fraction and/or negative.
    ///</summary>
    ///<param name="_input">The string which contains the float</param>
    ///<returns>The parsed float</returns>
    public static float ParseDecFrac(string _input)
    {
        _input = _input.Replace(" ", "");
        _input = _input.Replace(",", ".");
        if (_input.Contains("/"))
        {
            string[] div = _input.Split('/');
            float a = float.Parse(div[0], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            float b = float.Parse(div[1], NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            return a / b;
        }
        else
            return float.Parse(_input, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
    }

    ///<summary>
    /// Tries to return given <see cref="Transform"/>, otherwise look for given parent Id
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
            foreach (DictionaryEntry de in GameManager.instance.allItems)
            {
                GameObject go = (GameObject)de.Value;
                if (go && go.GetComponent<OgreeObject>().id == _parentId)
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
            return hit.collider.transform.parent.gameObject;
        else
            return null;
    }

    ///<summary>
    /// Get an object from <see cref="GameManager.allItems"/> by it's id.
    ///</summary>
    ///<param name="_id">The id to search</param>
    ///<returns>The asked object</returns>
    public static GameObject GetObjectById(string _id)
    {
        if (!string.IsNullOrEmpty(_id))
        {
            if (GameManager.instance.allItems.Contains(_id))
                return (GameObject)GameManager.instance.allItems[_id];
            else
                return null;
        }
        return null;
    }

    ///<summary>
    /// Get a list of objects from GameManager.allItems by their id.
    ///</summary>
    ///<param name="_idList">The array of ids to search</param>
    ///<returns>the asked list of objects</returns>
    public static List<GameObject> GetObjectsById(List<string> _idList)
    {
        List<GameObject> objects = new();
        foreach (string objId in _idList)
            if (GameManager.instance.allItems.Contains(objId))
                objects.Add((GameObject)GameManager.instance.allItems[objId]);
        return objects;
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_str">The hexadecimal value, without '#'</param>
    ///<returns>The wanted color</returns>
    public static Color ParseHtmlColor(string _str)
    {
        ColorUtility.TryParseHtmlString(_str, out Color newColor);
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
        return new(max - _color.r / 3, max - _color.g / 3, max - _color.b / 3, _color.a);
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_physicalList">The list of physical objects to complete</param>
    ///<param name="_logicalList">The list of logical objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    ///<param name="_leafIds">The list of leaf IDs to complete</param>
    public static void ParseNestedObjects(List<SApiObject> _physicalList, List<SApiObject> _logicalList, SApiObject _src, List<string> _leafIds)
    {
        if (_src.category == Category.Group)
            _logicalList.Add(_src);
        else
            _physicalList.Add(_src);
        if (_src.children != null)
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_physicalList, _logicalList, obj, _leafIds);
        else
            _leafIds.Add(_src.id);
    }

    ///<summary>
    /// Parse a nested SApiObject and add each item to a given list.
    ///</summary>
    ///<param name="_list">The list of objects to complete</param>
    ///<param name="_src">The head of nested SApiObjects</param>
    ///<param name="_leafIds">The list of leaf IDs to complete</param>
    public static void ParseNestedObjects(List<SApiObject> _list, SApiObject _src, List<string> _leafIds)
    {
        _list.Add(_src);
        if (_src.children != null)
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_list, obj, _leafIds);
        else
            _leafIds.Add(_src.id);
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

    /// <summary>
    /// Compute the signed volume of a pyramid from a Mesh
    /// </summary>
    /// <param name="_p1">First corner of the pyramid</param>
    /// <param name="_p2">Second corner of the pyramid</param>
    /// <param name="_p3">Third corner of the pyramid</param>
    /// <returns>signed volume of the pyramid</returns>
    public static float SignedVolumeOfPyramid(Vector3 _p1, Vector3 _p2, Vector3 _p3)
    {
        float v321 = _p3.x * _p2.y * _p1.z;
        float v231 = _p2.x * _p3.y * _p1.z;
        float v312 = _p3.x * _p1.y * _p2.z;
        float v132 = _p1.x * _p3.y * _p2.z;
        float v213 = _p2.x * _p1.y * _p3.z;
        float v123 = _p1.x * _p2.y * _p3.z;

        return (-v321 + v231 + v312 - v132 - v213 + v123) / 6;
    }

    /// <summary>
    /// Compute the volume of a mesh by adding the volume of each of its pyramids
    /// </summary>
    /// <param name="_meshFilter">The MeshFilter of the object whose volume is needed</param>
    /// <returns>The volume of the mesh</returns>
    public static float VolumeOfMesh(MeshFilter _meshFilter)
    {
        Mesh mesh = _meshFilter.sharedMesh;
        float volume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfPyramid(p1, p2, p3);
        }
        volume *= _meshFilter.transform.localScale.x * _meshFilter.transform.localScale.y * _meshFilter.transform.localScale.z;
        return Mathf.Abs(volume);
    }

    ///<summary>
    /// Map a value from a given range to another range and clamp it.
    ///</summary>
    ///<param name="_input">The value to map</param>
    ///<param name="_inMin">The minimal value of the input range</param>
    ///<param name="_inMax">The maximal value of the input range</param>
    ///<param name="_outMin">The minimal value of the output range</param>
    ///<param name="_outMax">The maximal value of the output range</param>
    ///<returns>The maped and clamped value</returns>
    public static float MapAndClamp(float _input, float _inMin, float _inMax, float _outMin, float _outMax)
    {
        return Mathf.Clamp((_input - _inMin) * (_outMax - _outMin) / (_inMax - _inMin) + _outMin, Mathf.Min(_outMin, _outMax), Mathf.Max(_outMin, _outMax));
    }

    /// <summary>
    /// Returns the referent of an OObject if it's a rack, returns null otherwise
    /// </summary>
    /// <param name="_item">The Oobject whose referent is returned</param>
    /// <returns>the Rack which is the referent of <paramref name="_item"/> or null</returns>
    public static Rack GetRackReferent(Item _item)
    {
        try
        {
            return (Rack)_item.referent;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    ///<summary>
    /// Wait end of frame to raise a ImportFinishedEvent in order to fill FocusHandler lists for the group content.
    ///</summary>
    public static IEnumerator ImportFinished()
    {
        yield return new WaitForEndOfFrame();
        EventManager.instance.Raise(new ImportFinishedEvent());
    }

    ///<summary>
    /// Loop through parents of given object and set their currentLod.
    ///</summary>
    ///<param name="_leaf">The object to start the loop</param>
    public static void RebuildLods(Transform _leaf)
    {
        Transform parent = _leaf.parent;
        while (parent)
        {
            OgreeObject leafObj = _leaf.GetComponent<OgreeObject>();
            OgreeObject parentObj = parent.GetComponent<OgreeObject>();

            if (leafObj.currentLod >= parentObj.currentLod)
                parentObj.currentLod = leafObj.currentLod + 1;

            _leaf = parent;
            parent = _leaf.parent;
        }
    }

    ///<summary>
    /// Disable _target, destroy it and display given message (using a Localization Table) to logger (ELogTarget.logger, ELogtype.success)
    ///</summary>
    ///<param name="_target">The object to destroy</param>
    ///<param name="_localizationTable">The Localization Table to use</param>
    ///<param name="_localizationKey">The Key to use in given Localization Table</param>
    ///<param name="_strVariable">A string variable to use il the Localization Table message</param>
    public static void CleanDestroy(this GameObject _target, string _localizationTable, string _localizationKey, string _strVariable)
    {
        _target.SetActive(false); //for UI
        Object.Destroy(_target);
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString(_localizationTable, _localizationKey, _strVariable), ELogTarget.logger, ELogtype.success);
    }

    /// <summary>
    /// Switch y and z value.
    /// </summary>
    /// <param name="_v">The vector to modify</param>
    /// <returns>A Z-Up oriented vector</returns>
    public static Vector3 ZAxisUp(this Vector3 _v)
    {
        return new(_v.x, _v.z, _v.y);
    }

    /// <summary>
    /// Set the alpha of a <paramref name="_color"/>
    /// </summary>
    /// <param name="_color">The color to modify</param>
    /// <param name="_alpha">The alpha to apply</param>
    /// <returns>This color with given <paramref name="_alpha"/></returns>
    public static Color WithAlpha(this Color _color, float _alpha)
    {
        _color.a = _alpha;
        return _color;
    }

    /// <summary>
    /// Check if a <paramref name="_key"/> exists in <paramref name="_dict"/> and if it is neither null or empty 
    /// </summary>
    /// <param name="_dict">The dictionnary to look in</param>
    /// <param name="_key">The key to look for</param>
    /// <returns>True if the key is in the dictionnary and it has a non empty value, otherweise false</returns>
    public static bool HasKeyAndValue(this Dictionary<string, string> _dict, string _key)
    {
        return _dict.ContainsKey(_key) && !string.IsNullOrEmpty(_dict[_key]);
    }

    /// <summary>
    /// Check if a <paramref name="_key"/> exists in <paramref name="_dict"/> and if it is neither null or empty 
    /// </summary>
    /// <param name="_dict">The dictionnary to look in</param>
    /// <param name="_key">The key to look for</param>
    /// <returns>True if the key is in the dictionnary and it has a non empty value, otherweise false</returns>
    public static bool HasKeyAndValue(this Dictionary<string, object> _dict, string _key)
    {
        return _dict.ContainsKey(_key) && _dict[_key] != null && _dict[_key].ToString() != "";
    }

    /// <summary>
    /// Create a Vector2 from this JArray
    /// </summary>
    /// <param name="_array">The array to transform</param>
    /// <returns>A Vector2 corresponding to the array</returns>
    public static Vector2 ToVector2(this JArray _array)
    {
        return new Vector2((float)_array[0], (float)_array[1]);
    }

    /// <summary>
    /// Create a Vector3 from this JArray
    /// </summary>
    /// <param name="_array">The array to transform</param>
    /// <returns>A Vector3 corresponding to the array</returns>
    public static Vector3 ToVector3(this JArray _array)
    {
        return new Vector3((float)_array[0], (float)_array[1], (float)_array[2]);
    }

    /// <summary>
    /// Create a Vector4 from this JArray
    /// </summary>
    /// <param name="_array">The array to transform</param>
    /// <returns>A Vector4 corresponding to the array</returns>
    public static Vector4 ToVector4(this JArray _array)
    {
        return new Vector4((float)_array[0], (float)_array[1], (float)_array[2], (float)_array[3]);
    }

    /// <summary>
    /// Round this float with asked decimals
    /// </summary>
    /// <param name="_value">The float to round</param>
    /// <param name="_decimal">The number of decimals to keep</param>
    /// <returns>Rounded float</returns>
    public static float Round(this float _value, int _decimal)
    {
        return (float)System.Math.Round(_value, _decimal);
    }

    /// <summary>
    /// Returns the item at index <paramref name="_offset"/> + <paramref name="_index"/> in a circular list
    /// </summary>
    /// <typeparam name="T">type of variables in the list</typeparam>
    /// <param name="_input">the list from which we call the funciton</param>
    /// <param name="_index">the index of the list we start the count from</param>
    /// <param name="_offset">how many times we go to the next element before returning it</param>
    /// <returns>the item at index <paramref name="_index"/> in <paramref name="_input"/></returns>
    public static T NextIndex<T>(this List<T> _input, int _index, int _offset = 1)
    {
        return _input[(_index + _offset) % _input.Count];
    }
}
