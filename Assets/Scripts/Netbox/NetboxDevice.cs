using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetboxDevice
{
    [System.Serializable]
    public struct SValue
    {
        public string value;
        public string label;
        public int id;
    }

    [System.Serializable]
    public struct SRack
    {
        public int id;
        public string url;
        public string name;
        public string display_name;
    }

    [System.Serializable]
    public struct SData
    {
        public int id;
        public string url;
        public string name;
        public string slug;
    }

    [System.Serializable]
    public struct SType
    {
        public int id;
        public string url;
        public SData manufacturer;
        public string model;
        public string slug;
        public string display_name;
    }

    public struct SCustom{}

    [System.Serializable]
    public struct SNbDevice
    {
        public int id;
        public string name;
        public string display_name;
        public SType device_type;
        public SData device_role;
        public string tenant;
        public string platform;
        public string serial;
        public string asset_tag;
        public SData site;
        public SRack rack;
        public string position;
        public SValue face;
        public string parent_device;
        public SValue status;
        public string primary_ip;
        public string primary_ip4;
        public string primary_ip6;
        public string cluster;
        public string virtual_chassis;
        public string vc_position;
        public string vc_priority;
        public string comments;
        public string local_contect_data;
        public string[] tags;
        public SCustom custom_fields;
        public SCustom config_context;
        public string created;
        public string last_updated;
    }

    [System.Serializable]
    public struct SNbDevices
    {
        public int count;
        public string next;
        public string previous;
        public SNbDevice[] results;
    }
}
