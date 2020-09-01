using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Filters : MonoBehaviour
{
    public static Filters instance;

    [Header("Rooms")]
    public List<string> roomsList;
    // public List<GameObject> rooms;
    public TMP_Dropdown dropdownRooms = null;

    [Header("Rack rows")]
    public List<string> rackRowsList;
    // public List<GameObject> racks;
    public TMP_Dropdown dropdownRackRows = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);

        DefaultList(roomsList, "All");
        UpdateDropdownFromList(dropdownRooms, roomsList);
        DefaultList(rackRowsList, "All");
        UpdateDropdownFromList(dropdownRackRows, rackRowsList);
    }

    ///<summary>
    /// Add an item to a list if it isn't already in it.
    ///</summary>
    ///<param name="_list">The list to check and to complete</param>
    ///<param name="_item">The item to add</param>
    public void AddIfUnknowned<T>(List<T> _list, T _item)
    {
        if (!_list.Contains(_item))
            _list.Add(_item);
    }

    ///<summary>
    /// Clear a list and add a value in it.
    ///</summary>
    ///<param name="_list">The list to clear</param>
    ///<param name="_item">The item to add</param>
    public void DefaultList<T>(List<T> _list, T _item)
    {
        _list.Clear();
        _list.Add(_item);
    }

    ///<summary>
    /// Update a dropdown values from a list.
    ///</summary>
    ///<param name="_dropdown">The dropdown to update</param>
    ///<param name="_lst">The list to put as values of _dropdown</param>
    public void UpdateDropdownFromList(TMP_Dropdown _dropdown, List<string> _lst)
    {
        _dropdown.ClearOptions();
        foreach (string str in _lst)
            _dropdown.options.Add(new TMP_Dropdown.OptionData(str));
        _dropdown.RefreshShownValue();

    }

    ///<summary>
    /// Called by a GUI dropdown.
    /// Display the selected room.
    ///</summary>
    public void FilterRooms()
    {
        string txt = dropdownRooms.GetComponentInChildren<TMP_Text>().text;
        // if (txt == "All")
        // {
        //     foreach (GameObject room in rooms)
        //         room.SetActive(true);
        // }
        // else
        // {
        //     foreach (GameObject room in rooms)
        //         room.SetActive(room.name == txt);
        // }
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<Room>())
            {
                if (txt == "All" || obj.GetComponent<Room>().name == txt)
                    obj.SetActive(true);
                else
                    obj.SetActive(false);
            }

        }
    }

    ///<summary>
    /// Called by a GUI dropdown.
    /// Display the selected rack row.
    ///</summary>
    public void FilterRackRows()
    {
        string txt = dropdownRackRows.GetComponentInChildren<TMP_Text>().text;
        // if (txt == "All")
        // {
        //     foreach (GameObject rack in racks)
        //         rack.SetActive(true);
        // }
        // else
        // {
        //     foreach (GameObject rack in racks)
        //         rack.SetActive(rack.name[0].ToString() == txt);
        // }
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<Rack>())
            {
                if (txt == "All" || obj.GetComponent<Rack>().name[0].ToString() == txt)
                    obj.SetActive(true);
                else
                    obj.SetActive(false);
            }

        }
    }

    public void FilterRackTenant()
    {
        string txt = dropdownRackRows.GetComponentInChildren<TMP_Text>().text;//
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<Rack>())
            {
                if (txt == "All" || obj.GetComponent<Rack>().tenant.name == txt)
                    obj.SetActive(true);
                else
                    obj.SetActive(false);
            }

        }
    }

}
