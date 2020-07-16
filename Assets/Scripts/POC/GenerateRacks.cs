using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRacks : MonoBehaviour
{
    [Header("Auto Generate racks")]
    public bool autoGenRacks = true;
    public int xNb;
    public int zNb;
    public int nbPerRack;

    [Header("Room data")]
    public Vector2 margin = new Vector2(3, 3); // tile
    // public Transform root = null;

    private void Start()
    {
        if (autoGenRacks)
            AutoRacks();

    }

    private void AutoRacks()
    {
        float tileU = GameManager.gm.tileSize;

        float xUnit = GameManager.gm.rackModel.transform.GetChild(0).transform.localScale.x;
        float zUnit = GameManager.gm.rackModel.transform.GetChild(0).transform.localScale.z;
        Debug.Log($"[GenerateRacks] xUnit:{xUnit} / zUnit:{zUnit}");

        for (int z = 0; z < zNb; z++)
        {
            for (int x = 0; x < xNb; x++)
            {
                Vector3 pos = new Vector3(tileU + x * (tileU + xUnit), 0, tileU + z * (tileU + zUnit));
                GameObject tmpRack = Instantiate(GameManager.gm.rackModel, pos, Quaternion.identity, transform);
                for (int i = 0; i < nbPerRack; i++)
                    Instantiate(GameManager.gm.serverModel, tmpRack.transform);
            }
        }
    }

    public void CreateRack(SRackInfos _data)
    {
        GameObject newRack = Instantiate(GameManager.gm.rackModel);
        newRack.name = _data.name;

        Transform parent = GameObject.Find(_data.parentName).transform;
        newRack.transform.parent = parent;
        
        newRack.transform.GetChild(0).localScale = new Vector3(_data.size.x / 100, _data.height * 0.0445f, _data.size.y / 100);
        
        Vector3 origin = parent.GetChild(0).localScale / -0.2f;
        newRack.transform.localPosition = new Vector3(origin.x, 0, origin.z);
        newRack.transform.localPosition += newRack.transform.GetChild(0).localScale / 2;
        newRack.transform.localPosition += new Vector3(_data.pos.x - 1 + margin.x, 0, _data.pos.y - 1 + margin.y) * GameManager.gm.tileSize;

        Object obj = newRack.GetComponent<Object>();
        obj.description = _data.comment;
        obj.posXY = _data.pos;
        obj.posXYUnit = EUnit.tile;
        obj.size = new Vector2(_data.size.x, _data.size.y);
        obj.sizeUnit = EUnit.cm;
        obj.height = _data.height;
        obj.heightUnit = EUnit.U;
        switch (_data.orient)
        {
            case "front":
                obj.orient = EObjOrient.Frontward;
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "rear":
                obj.orient = EObjOrient.Backward;
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
            case "left":
                obj.orient = EObjOrient.Left;
                newRack.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            case "right":
                obj.orient = EObjOrient.Right;
                newRack.transform.localEulerAngles = new Vector3(0, -90, 0);
                break;
        }

        // RackFilter rf = GetComponent<RackFilter>();
        // rf.AddIfUnknowned(rf.racks, newRack);
        // rf.AddIfUnknowned(rf.rackRows, _data.row);
        // rf.UpdateDropdownFromList(rf.dropdownRackRows, rf.rackRows);
        Filters.instance.AddIfUnknowned(Filters.instance.racks, newRack);
        Filters.instance.AddIfUnknowned(Filters.instance.rackRowsList, newRack.name[0].ToString());
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRackRows, Filters.instance.rackRowsList);

        newRack.GetComponent<DisplayRackData>().FillTexts();
    }

    public void CreateRack(string _rackJson)
    {
        SRackInfos data = JsonUtility.FromJson<SRackInfos>(_rackJson);
        CreateRack(data);
    }

}
