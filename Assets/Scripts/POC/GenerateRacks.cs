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
    public Transform root = null;
    private float tileU = 0.6f;

    private void Start()
    {
        if (autoGenRacks)
            AutoRacks();

    }

    private void AutoRacks()
    {
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

    public void CreateRack(SRackInfos data)
    {
        GameObject newRack = Instantiate(GameManager.gm.rackModel, root);
        Object obj = newRack.GetComponent<Object>();

        newRack.name = data.name;
        newRack.transform.GetChild(0).localScale = new Vector3(data.size.x / 100, data.height * 0.0445f, data.size.y / 100);
        newRack.transform.localPosition = newRack.transform.GetChild(0).localScale / 2;
        newRack.transform.localPosition += new Vector3((data.pos.x - 1 + margin.x) * tileU, 0, (data.pos.y - 1 + margin.y) * tileU);

        obj.description = data.comment;
        obj.row = data.row;
        obj.pos = data.pos;
        obj.posUnit = EUnit.tile;
        obj.size = new Vector2(data.size.x, data.size.y);
        obj.sizeUnit = EUnit.cm;
        obj.height = data.height;
        obj.heightUnit = EUnit.U;
        switch (data.orient)
        {
            case "front":
                obj.orient = EObjOrient.Frontward;
                newRack.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            case "rear":
                obj.orient = EObjOrient.Backward;
                newRack.transform.localEulerAngles = new Vector3(0, 0, 0);
                break;
        }

        RackFilter rf = GetComponent<RackFilter>();
        rf.AddIfUnknowned(rf.racks, newRack);
    }

    public void CreateRack(string rackJson)
    {
        SRackInfos data = JsonUtility.FromJson<SRackInfos>(rackJson);
        CreateRack(data);
    }

}
