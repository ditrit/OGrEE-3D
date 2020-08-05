using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadFromJson
{
    [System.Serializable]
    private struct SRackFromJson
    {
        public string name;
        public string slug;
        public string vendor;
        public string model;
        public string type;
        public string role;
        public string orientation;
        public string side;
        public string fulllength;
        public int[] sizeWDHmm;
        public SComponent[] components;
    }

    [System.Serializable]
    private struct SComponent
    {
        public string location;
        public string family;
        public string role;
        public string installed;
        public int[] elemPos;
        public int[] elemSize;
        public string mandatory;
        public string labelPos;
    }

    public void CreateRackTemplate(string _json)
    {
        SRackFromJson rackData = JsonUtility.FromJson<SRackFromJson>(_json);

        SRackInfos infos = new SRackInfos();
        infos.name = rackData.slug;
        infos.parent = GameManager.gm.templatePlaceholder;
        infos.orient = "front";
        infos.size = new Vector2(rackData.sizeWDHmm[0] / 10, rackData.sizeWDHmm[1] / 10);
        infos.height = (int)(rackData.sizeWDHmm[2] / GameManager.gm.uSize / 1000);
        ObjectGenerator.instance.CreateRack(infos, false);

        Rack rack = GameObject.Find(rackData.slug).GetComponent<Rack>();
        rack.vendor = rackData.vendor;
        rack.model = rackData.model;
        foreach (SComponent comp in rackData.components)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = comp.location;
            go.transform.parent = rack.transform;
            go.transform.localScale = new Vector3(comp.elemSize[0], comp.elemSize[2], comp.elemSize[1]) / 1000;
            go.transform.localPosition = go.transform.parent.GetChild(0).localScale / -2;
            go.transform.localPosition += go.transform.localScale / 2;
            go.transform.localPosition += new Vector3(comp.elemPos[0], comp.elemPos[2], comp.elemPos[1]) / 1000;
        }

        Renderer[] renderers = rack.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;

        GameManager.gm.rackPresets.Add(rack.name, rack.gameObject);
    }

}
