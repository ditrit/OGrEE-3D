﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReadFromJson
{
    #region Rack
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
        public SRackSlot[] components;
    }

    [System.Serializable]
    private struct SRackSlot
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
    #endregion

    #region Component
    [System.Serializable]
    private struct SComponent
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
        public SComponentChild components;
    }

    [System.Serializable]
    private struct SComponentChild
    {
        public string location;
        public string type;
        public string role;
        public string position;
        public int[] elemPos;
        public int[] elemSize;
        public string mandatory;
        public string labelPos;
    }
    #endregion


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
        foreach (SRackSlot comp in rackData.components)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = comp.location;
            go.transform.parent = rack.transform;
            go.transform.localScale = new Vector3(comp.elemSize[0], comp.elemSize[2], comp.elemSize[1]) / 1000;
            go.transform.localPosition = go.transform.parent.GetChild(0).localScale / -2;
            go.transform.localPosition += go.transform.localScale / 2;
            go.transform.localPosition += new Vector3(comp.elemPos[0], comp.elemPos[2], comp.elemPos[1]) / 1000;
            go.transform.localEulerAngles = Vector3.zero;

            GameObject textHolder = new GameObject();
            textHolder.name = "textHolder";

            TextMeshPro text = textHolder.AddComponent<TextMeshPro>();
            text.text = go.name;
            text.fontSize = 0.5f;
            text.alignment = TextAlignmentOptions.MidlineGeoAligned;

            switch (comp.labelPos)
            {
                case "front":
                    textHolder.transform.SetParent(go.transform);
                    textHolder.transform.localPosition = new Vector3(0, 0, go.transform.localScale.z / -2);
                    textHolder.transform.localPosition += new Vector3(0, 0, 0.06f);
                    textHolder.transform.localEulerAngles = Vector3.zero;
                    text.rectTransform.sizeDelta = new Vector2(go.transform.localScale.x, go.transform.localScale.y);
                    break;
                case "rear":
                    textHolder.transform.SetParent(go.transform);
                    textHolder.transform.localPosition = new Vector3(0, 0, go.transform.localScale.z / 2);
                    textHolder.transform.localPosition += new Vector3(0, 0, -0.06f);
                    textHolder.transform.localEulerAngles = new Vector3(0, 180, 0);
                    text.rectTransform.sizeDelta = new Vector2(go.transform.localScale.x, go.transform.localScale.y);
                    break;
                case "top":
                    textHolder.transform.localEulerAngles = new Vector3(90, 0, 0);
                    textHolder.transform.SetParent(go.transform);
                    textHolder.transform.localPosition = new Vector3(0, go.transform.localScale.y * 10, 0);
                    textHolder.transform.localPosition += new Vector3(0, 0.1f, 0);
                    text.rectTransform.sizeDelta = new Vector2(go.transform.localScale.x, go.transform.localScale.y);
                    break;
                case "left":
                    textHolder.transform.localEulerAngles = new Vector3(0, -90, 0);
                    textHolder.transform.SetParent(go.transform);
                    textHolder.transform.localPosition = new Vector3(go.transform.localScale.x * -10, 0, 0);
                    textHolder.transform.localPosition += new Vector3(-0.07f, 0, 0);
                    text.rectTransform.sizeDelta = new Vector2(go.transform.localScale.z, go.transform.localScale.y);
                    break;
                case "right":
                    textHolder.transform.localEulerAngles = new Vector3(0, 90, 0);
                    textHolder.transform.SetParent(go.transform);
                    textHolder.transform.localPosition = new Vector3(go.transform.localScale.x * 10, 0, 0);
                    textHolder.transform.localPosition += new Vector3(0.07f, 0, 0);
                    text.rectTransform.sizeDelta = new Vector2(go.transform.localScale.z, go.transform.localScale.y);
                    break;
                case "frontrear":
                    // place textHolder as front...
                    textHolder.transform.SetParent(go.transform);
                    textHolder.transform.localPosition = new Vector3(0, 0, go.transform.localScale.z / -2);
                    textHolder.transform.localPosition += new Vector3(0, 0, 0.06f);
                    textHolder.transform.localEulerAngles = Vector3.zero;
                    text.rectTransform.sizeDelta = new Vector2(go.transform.localScale.x, go.transform.localScale.y);
                    // ... and create a new one for rear
                    GameObject textHolderBis = new GameObject();
                    textHolderBis.name = "textHolderBis";
                    TextMeshPro textBis = textHolderBis.AddComponent<TextMeshPro>();
                    textBis.text = go.name;
                    textBis.fontSize = 0.5f;
                    textBis.alignment = TextAlignmentOptions.MidlineGeoAligned;
                    textHolderBis.transform.SetParent(go.transform);
                    textHolderBis.transform.localPosition = new Vector3(0, 0, go.transform.localScale.z / 2);
                    textHolderBis.transform.localPosition += new Vector3(0, 0, -0.06f);
                    textHolderBis.transform.localEulerAngles = new Vector3(0, 180, 0);
                    textBis.rectTransform.sizeDelta = new Vector2(go.transform.localScale.x, go.transform.localScale.y);
                    break;
            }
        }

#if !DEBUG
        Renderer[] renderers = rack.transform.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = false;
#endif

        GameManager.gm.rackPresets.Add(rack.name, rack.gameObject);
    }

}
