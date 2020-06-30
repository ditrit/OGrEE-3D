using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RackFilter : MonoBehaviour
{
    [Header("Sites")]
    public List<string> sites;
    public Dictionary<string, Vector2> siteSizes = new Dictionary<string, Vector2>();
    public Dictionary<string, Vector2> siteMargins = new Dictionary<string, Vector2>();
    public TMP_Dropdown dropdownSites = null;

    [Header("Rack rows")]
    public List<GameObject> racks;
    public List<string> rackRows;
    public TMP_Dropdown dropdownRackRows = null;

    private void Start()
    {
        siteSizes.Add("ALPHA", new Vector2(30, 60));
        siteSizes.Add("PCY", new Vector2(20, 20));

        siteMargins.Add("ALPHA", new Vector2(3, 3));
        siteMargins.Add("PCY", new Vector2(1, 1));
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

    public void GenerateNewSite()
    {
        string txt = dropdownSites.GetComponentInChildren<TMP_Text>().text;

        GenerateFloor gf = GameObject.FindObjectOfType<GenerateFloor>();
        GenerateRacks gr = GameObject.FindObjectOfType<GenerateRacks>();

        gr.margin = siteMargins[txt];
        gf.DeleteFloor();
        gf.CreateFloor(siteSizes[txt]);

        for (int i = 0; i < racks.Count; i++)
            Destroy(racks[i]);

        GetComponent<CsvReader>().CreateRacksFromCsv(txt);
        UpdateDropdownFromList(dropdownRackRows, rackRows);
    }



}
