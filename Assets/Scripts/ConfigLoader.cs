using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigLoader// : MonoBehaviour
{
    private struct SConfig
    {
        public bool verbose;
        public bool fullscreen;
        public string db_url;
        public string db_login;
        public string db_tocken;
    }
    private bool verbose = false;

    public void LoadConfigFile()
    {
        try
        {
            StreamReader jsonCongif = File.OpenText("OGREE 3D_Data/config.json");
            GameManager.gm.AppendLogLine("Load custom config file", "green");
            LoadConfig(JsonUtility.FromJson<SConfig>(jsonCongif.ReadToEnd()));
        }
        catch
        {
            TextAsset ResourcesCongif = Resources.Load<TextAsset>("config");
            GameManager.gm.AppendLogLine("Load default config file", "green");
            LoadConfig(JsonUtility.FromJson<SConfig>(ResourcesCongif.ToString()));
        }
    }

    private void LoadConfig(SConfig _config)
    {
        verbose = _config.verbose;
        FullScreenMode(_config.fullscreen);
    }

    private void FullScreenMode(bool _value)
    {
        if (verbose)
            GameManager.gm.AppendLogLine($"Fullscreen: {_value}");
        Screen.fullScreen = _value;
    }
}
