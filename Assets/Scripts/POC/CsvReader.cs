using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsvReader : MonoBehaviour
{
    public string csvName = "ogree_racks(7062)";

    private TextAsset file;
    string[] fileRows;

    private GenerateRacks gr;
    private RackFilter rf;

    private void Awake()
    {
        gr = GetComponent<GenerateRacks>();
        rf = GetComponent<RackFilter>();

        file = Resources.Load<TextAsset>(csvName);
        fileRows = file.text.Split('\n');
    }

    private void Start()
    {
        GenerateSitesList();
        CreateRacksFromCsv(rf.sites[0]);
    }

    private void GenerateSitesList()
    {
        // Skiping 1st & last row
        for (int i = 1; i < fileRows.Length - 1; i++)
        {
            string[] row = fileRows[i].Split(';');
            rf.AddIfUnknowned(rf.sites, row[0].ToUpper());
            // rf.siteSizes should be fill here
        }
        rf.UpdateDropdownFromList(rf.dropdownSites, rf.sites);
    }

    public void CreateRacksFromCsv(string _site)
    {
        rf.racks.Clear();
        rf.DefaultList(rf.rackRows, "All");

        // Skiping 1st & last row
        for (int i = 1; i < fileRows.Length - 1; i++)
        {
            string[] row = fileRows[i].Split(';');
            if (row[0].ToUpper() == _site)
            {
                GenerateRacks.SRackInfos data = new GenerateRacks.SRackInfos();
                data.name = row[13];
                data.orient = row[6];
                data.pos = new Vector2(float.Parse(row[8]), float.Parse(row[7]));
                data.size = new Vector2(float.Parse(row[9]), float.Parse(row[10]));
                data.height = int.Parse(row[11]);
                data.comment = row[12];
                data.row = row[2];

                gr.CreateRack(data);

                rf.AddIfUnknowned(rf.rackRows, data.row);
            }
        }

        rf.UpdateDropdownFromList(rf.dropdownRackRows, rf.rackRows);
    }

}
