﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public static class Utils
{
    ///<summary>
    /// Parse a string with format "[x,y]" into a Vector2.
    ///</summary>
    ///<param name="_input">String with format "[x,y]"</param>
    ///<returns>The parsed Vector2</returns>
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
    ///<returns>The parsed Vector3</returns>
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
    ///<returns>The parsed float</returns>
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
            foreach (DictionaryEntry de in GameManager.instance.allItems)
            {
                GameObject obj = (GameObject)de.Value;
                if (obj && obj.GetComponent<OgreeObject>().id == _id)
                    return obj;
            }
        }
        return null;
    }

    ///<summary>
    /// Get a list of objects from GameManager.allItems by their id.
    ///</summary>
    ///<param name="_idArray">The array of ids to search</param>
    ///<returns>the asked list of objects</returns>
    public static List<GameObject> GetObjectsById(string _idArray)
    {
        string[] ids = JsonConvert.DeserializeObject<string[]>(_idArray);

        List<GameObject> objects = new List<GameObject>();
        foreach (DictionaryEntry de in GameManager.instance.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            foreach (string objId in ids)
                if (obj.GetComponent<OgreeObject>().id == objId)
                    objects.Add(obj);
        }
        return objects;
    }

    ///<summary>
    /// Get an object from <see cref="GameManager.allItems"/> by it's hierarchyName.
    ///</summary>
    ///<param name="_id">The id to search</param>
    ///<returns>The asked object</returns>
    public static GameObject GetObjectByHierarchyName(string _hierarchyName)
    {
        if (!string.IsNullOrEmpty(_hierarchyName))
        {
            _hierarchyName = _hierarchyName.Replace('/', '.');
            foreach (DictionaryEntry de in GameManager.instance.allItems)
            {
                GameObject obj = (GameObject)de.Value;
                if (obj && obj.GetComponent<OgreeObject>().hierarchyName == _hierarchyName)
                    return obj;
            }
        }
        return null;
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
        return new Color(max - _color.r / 3, max - _color.g / 3, max - _color.b / 3, _color.a);
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
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_physicalList, _logicalList, obj, _leafIds);
        }
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
        {
            foreach (SApiObject obj in _src.children)
                ParseNestedObjects(_list, obj, _leafIds);
        }
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

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
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
    /// <param name="_oObject">The Oobject whose referent is returned</param>
    /// <returns>the Rack which is the referent of <paramref name="_oObject"/> or null</returns>
    public static Rack GetRackReferent(OObject _oObject)
    {
        try
        {
            return (Rack)_oObject.referent;
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
    /// Disable _target, destroy it and display given _msg to logger
    ///</summary>
    ///<param name="_target">The object to destroy</param>
    ///<param name="_msg">The message to display (ELogTarget.logger, ELogtype.success)</param>
    public static void CleanDestroy(this GameObject _target, string _msg)
    {
        _target.SetActive(false); //for UI
        Object.Destroy(_target);
        GameManager.instance.AppendLogLine(_msg, ELogTarget.logger, ELogtype.success);
    }

    ///<summary>
    /// Destroy an object and wait for its reference to be null
    ///</summary>
    ///<param name="_obj">The object to destroy</param>
    ///<param name="_frequency">The time to wait at each loop. 10 by default</param>
    public static async Task AwaitDestroy(this Object _obj, int _frequency = 10)
    {
        Object.Destroy(_obj);
        while (_obj)
            await Task.Delay(_frequency);
    }

    ///<summary>
    /// Convert a float to a string with "0.##" format
    ///</summary>
    ///<param name="_input">The float to convert</param>
    ///<returns>The converted float</returns>
    public static string FloatToRefinedStr(float _input)
    {
        return _input.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
