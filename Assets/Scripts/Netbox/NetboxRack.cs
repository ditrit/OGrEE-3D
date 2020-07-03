using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NetboxRack
{
    [System.Serializable]
    public struct SData
    {
        public int id;
        public string url;
        public string name;
        public string slug;
    }

    [System.Serializable]
    public struct SStatus
    {
        public string value;
        public string label;
        public int id;
    }

    [System.Serializable]
    public struct SValue
    {
        public float value;
        public string lavel;
    }

    [System.Serializable]
    public struct SUnit
    {
        public string value;
        public string label;
        public int id;
    }

    public struct SCustom
    {

    }

    [System.Serializable]
    public struct SNbRack
    {
        public int id;
        public string name;
        public int facility_id;
        public string display_name;
        public SData site;
        public SData group;
        public string tenant;
        public SStatus status;
        public SData role;
        public string serial;
        public string asset_tag;
        public string type;
        public SValue width;
        public int u_height;
        public string desc_units;
        public float outer_width;
        public float outer_depth;
        public SUnit outer_unit;
        public string comments;
        public string[] tags;
        public SCustom custom_fields;
        public string created;
        public string last_updated;
        public int device_count;
        public int powerfeed_count;
    }

}
