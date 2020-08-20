using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public string contact;

    ///<summary>
    /// Check for a _param attribute and assign _value to it.
    ///</summary>
    ///<param name="_param">The attribute to modify</param>
    ///<param name="_value">The value to assign</param>
    public void SetAttribute(string _param, string _value)
    {
        if (_param == "contact")
            contact = _value;
        else
            GameManager.gm.AppendLogLine($"[Customer] {name}: unknowed attribute to update.", "yellow");
    }
}
