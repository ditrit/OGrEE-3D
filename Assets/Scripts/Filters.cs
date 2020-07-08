using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Filters : MonoBehaviour
{
    public static Filters instance;

    [Header("ItRooms")]
    public List<string> itRoomsList;
    public List<GameObject> itRooms;
    public TMP_Dropdown dropdownItRooms = null;

    [Header("Rack rows")]
    public List<string> rackRowsList;
    public List<GameObject> racks;
    public TMP_Dropdown dropdownRackRows = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        
        DefaultList(itRoomsList, "All");
        UpdateDropdownFromList(dropdownItRooms, itRoomsList);
        DefaultList(rackRowsList, "All");
        UpdateDropdownFromList(dropdownRackRows, rackRowsList);
    }

    public void AddIfUnknowned<T>(List<T> _list, T _item)
    {
        if (!_list.Contains(_item))
            _list.Add(_item);
    }

    public void DefaultList<T>(List<T> _list, T _item)
    {
        _list.Clear();
        _list.Add(_item);
    }

    public void UpdateDropdownFromList(TMP_Dropdown _dropdown, List<string> _lst)
    {
        _dropdown.ClearOptions();
        foreach (string str in _lst)
            _dropdown.options.Add(new TMP_Dropdown.OptionData(str));
        _dropdown.RefreshShownValue();

    }

    public void FilterItRooms()
    {
        string txt = dropdownItRooms.GetComponentInChildren<TMP_Text>().text;
        if (txt == "All")
        {
            foreach (GameObject itRoom in itRooms)
                itRoom.SetActive(true);
        }
        else
        {
            foreach (GameObject itRoom in itRooms)
                itRoom.SetActive(itRoom.name == txt);
        }
    }

    public void FilterRackRows()
    {
        string txt = dropdownRackRows.GetComponentInChildren<TMP_Text>().text;
        if (txt == "All")
        {
            foreach (GameObject rack in racks)
                rack.SetActive(true);
        }
        else
        {
            foreach (GameObject rack in racks)
                rack.SetActive(rack.GetComponent<Object>().row == txt);
        }
    }

}
