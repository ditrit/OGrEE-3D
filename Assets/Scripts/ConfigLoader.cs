﻿using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigLoader
{
    private struct SConfig
    {
        public string verbose;
        public string fullscreen;
        public Dictionary<string, string> textures;
        public string db_url;
        public string db_login;
        public string db_token;
    }

    private SConfig config;
    private bool verbose = false;

    ///<summary>
    /// Load a config file, look for CLI overrides and starts with --file if given.
    ///</summary>
    public void LoadConfig()
    {
        config = LoadConfigFile();
        OverrideConfig();

        ApplyConfig(config);

        string startFile = GetArg("--file");
        if (!string.IsNullOrEmpty(startFile))
            GameManager.gm.consoleController.RunCommandString($".cmds:{startFile}");
    }

    ///<summary>
    /// Get an argument by its name (ex -file).
    /// From https://forum.unity.com/threads/pass-custom-parameters-to-standalone-on-launch.429144/
    ///</summary>
    ///<param name="_name">Name of the wanted argument</param>
    private string GetArg(string _name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == _name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    ///<summmary>
    /// Override config if arguments are found.
    ///</summmary>
    private void OverrideConfig()
    {
        string[] args = new string[] { "--verbose", "--fullscreen", "--serverUrl", "--serverLogin", "--serverToken" };
        for (int i = 0; i < args.Length; i++)
        {
            string str = GetArg(args[i]);
            if (!string.IsNullOrEmpty(str))
            {
                switch (i)
                {
                    case 0:
                        config.verbose = str;
                        break;
                    case 1:
                        config.fullscreen = str;
                        break;
                    case 2:
                        config.db_url = str;
                        break;
                    case 3:
                        config.db_login = str;
                        break;
                    case 4:
                        config.db_token = str;
                        break;
                }

            }
        }
    }

    ///<summary>
    /// Try to load a custom config file. Otherwise, load default config file from Resources folder.
    ///</summary>
    private SConfig LoadConfigFile()
    {
        try
        {
            StreamReader jsonCongif = File.OpenText("OGREE 3D_Data/config.json");
            GameManager.gm.AppendLogLine("Load custom config file", "green");
            return JsonConvert.DeserializeObject<SConfig>(jsonCongif.ReadToEnd());
        }
        catch
        {
            TextAsset ResourcesCongif = Resources.Load<TextAsset>("config");
            GameManager.gm.AppendLogLine("Load default config file", "green");
            return JsonConvert.DeserializeObject<SConfig>(ResourcesCongif.ToString());
        }
    }

    ///<summary>
    /// For each SConfig value, call corresponding method.
    ///</summary>
    ///<param name="_config">The SConfig to apply</param>
    private void ApplyConfig(SConfig _config)
    {
        verbose = (_config.verbose == "true");

        FullScreenMode((_config.fullscreen == "true"));
    }

    ///<summary> 
    /// Set fullscreen mode.
    ///</summary> 
    ///<param name="_value">The value to set</param>
    private void FullScreenMode(bool _value)
    {
        if (verbose)
            GameManager.gm.AppendLogLine($"Fullscreen: {_value}");
        Screen.fullScreen = _value;
    }

    ///<summary>
    /// Send a get request to the given url. If no error, initialize ApiManager.
    ///</summary>
    ///<returns>The value of ApiManager.isInit</returns>
    public async Task<bool> ConnectToApi()
    {
#if API_DEBUG
        config.db_url = "https://ogree.chibois.net";
        config.db_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOjY0MTcxNzEyMzI2MzY2MDAzM30.TfF8sYnWvIS3nr5lncXShDnkRAVirALJxKtFI9P9Y20";
#endif
        await ApiManager.instance.Initialize(config.db_url, config.db_token);
        return ApiManager.instance.isInit;
    }

    ///<summary>
    /// Foreach texture declaration in config.textures, load it from url of file.
    /// Also load default "perf22" and "perf29" is needed.
    ///</summary>
    public IEnumerator LoadTextures()
    {
        foreach (KeyValuePair<string, string> kvp in config.textures)
        {
            UnityWebRequest www;
            if (kvp.Value.Contains("http"))
                www = UnityWebRequestTexture.GetTexture(kvp.Value);
            else
                www = UnityWebRequestTexture.GetTexture("file://" + kvp.Value);
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ProtocolError || www.result == UnityWebRequest.Result.ConnectionError)
                GameManager.gm.AppendLogLine($"{kvp.Key} not found at {kvp.Value}", "red");
            else
                GameManager.gm.textures.Add(kvp.Key, DownloadHandlerTexture.GetContent(www));
        }
        if (!GameManager.gm.textures.ContainsKey("perf22"))
        {
            GameManager.gm.AppendLogLine("Load default texture for perf22", "yellow");
            GameManager.gm.textures.Add("perf22", Resources.Load<Texture>("Textures/TilePerf22"));
        }
        if (!GameManager.gm.textures.ContainsKey("perf29"))
        {
            GameManager.gm.AppendLogLine("Load default texture for perf29", "yellow");
            GameManager.gm.textures.Add("perf29", Resources.Load<Texture>("Textures/TilePerf29"));
        }
    }
}
