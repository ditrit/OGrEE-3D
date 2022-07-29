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
        public string cachePath;
        public float cacheLimitMo;
        public Dictionary<string, string> textures;
        public Dictionary<string, string> colors;
        public string api_url;
        public string api_token;
    }

    private SConfig config;
    private bool verbose = false;
    private string cacheDirName = ".ogreeCache";

    ///<summary>
    /// Load a config file, look for CLI overrides and starts with --file if given.
    ///</summary>
    public void LoadConfig()
    {
        
        config = LoadConfigFile(out string fileType);
        OverrideConfig();
        ApplyConfig();
        GameManager.gm.AppendLogLine($"Load {fileType} config file", false, eLogtype.success);

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
        string[] args = new string[] { "--verbose", "--fullscreen", "--apiUrl", "--apiToken" };
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
                        config.api_url = str;
                        break;
                    case 3:
                        config.api_token = str;
                        break;
                }

            }
        }
    }

    ///<summary>
    /// Try to load a custom config file. Otherwise, load default config file from Resources folder.
    ///</summary>
    private SConfig LoadConfigFile(out string _fileType)
    {
        try
        {
            StreamReader jsonCongif = File.OpenText("OGrEE-3D_Data/config.json");
            _fileType = "custom";
            return JsonConvert.DeserializeObject<SConfig>(jsonCongif.ReadToEnd());
        }
        catch
        {
            TextAsset ResourcesCongif = Resources.Load<TextAsset>("config");
            _fileType = "default";
            return JsonConvert.DeserializeObject<SConfig>(ResourcesCongif.ToString());
        }
    }

    ///<summary>
    /// For each SConfig value, call corresponding method.
    ///</summary>
    private void ApplyConfig()
    {
        verbose = (config.verbose == "true");

        CreateCacheDir();
        FullScreenMode((config.fullscreen == "true"));
    }

    ///<summary> 
    /// Set fullscreen mode.
    ///</summary> 
    ///<param name="_value">The value to set</param>
    private void FullScreenMode(bool _value)
    {
        if (verbose)
            GameManager.gm.AppendLogLine($"Fullscreen: {_value}", false);
        Screen.fullScreen = _value;
    }

    ///<summary>
    /// Create a cache folder in the given directory.
    ///</summary>
    private void CreateCacheDir()
    {
        if (!string.IsNullOrEmpty(config.cachePath) && !config.cachePath.EndsWith("/"))
            config.cachePath += "/";
        string fullPath = config.cachePath + cacheDirName;
        try
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                GameManager.gm.AppendLogLine($"Cache folder created at {fullPath}", false, eLogtype.success);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    ///<summary>
    /// Get the path of the cache directory from config.
    ///</summary>
    ///<returns>The path of the cache directory</returns>
    public string GetCacheDir()
    {
        return config.cachePath + cacheDirName;
    }

    ///<summary>
    /// Get the limit size in Mo of the cache from config.
    ///</summary>
    ///<returns>The limit size (Mo) of the cache</returns>
    public float GetCacheLimit()
    {
        return config.cacheLimitMo;
    }

    ///<summary>
    /// Save API url and token in config.
    ///</summary>
    ///<param name="_url">URL of the API to connect</param>
    ///<param name="_token">Corresponding authorisation token</param>
    public void RegisterApi(string _url, string _token)
    {
        config.api_url = _url;
        config.api_token = _token;
    }

    ///<summary>
    /// Send a get request to the given url. If no error, initialize ApiManager.
    ///</summary>
    ///<returns>The value of ApiManager.isInit</returns>
    public async Task<bool> ConnectToApi()
    {
#if API_DEBUG
        config.api_url = "https://c.api.ogree.ditrit.io";
        // config.api_url = "http://172.24.22.55:3001";
        config.api_token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOjY4MTM2NTc0MTE1NTc3ODU2MX0.eNWzvP3TwakyHMMPS8HJYW_Jd2GZwbVp-_DHwbB0DaA"; // master key
#endif
        await ApiManager.instance.Initialize(config.api_url, config.api_token);
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
                GameManager.gm.AppendLogLine($"{kvp.Key} not found at {kvp.Value}", false, eLogtype.error);
            else
                GameManager.gm.textures.Add(kvp.Key, DownloadHandlerTexture.GetContent(www));
        }
        if (!GameManager.gm.textures.ContainsKey("perf22"))
        {
            GameManager.gm.AppendLogLine("Load default texture for perf22", false, eLogtype.warning);
            GameManager.gm.textures.Add("perf22", Resources.Load<Texture>("Textures/TilePerf22"));
        }
        if (!GameManager.gm.textures.ContainsKey("perf29"))
        {
            GameManager.gm.AppendLogLine("Load default texture for perf29", false, eLogtype.warning);
            GameManager.gm.textures.Add("perf29", Resources.Load<Texture>("Textures/TilePerf29"));
        }
    }

    ///<summary>
    /// Get registered API url.
    ///</summary>
    public string GetApiUrl()
    {
        return config.api_url;
    }

    ///<summary>
    /// Get a color in hexadecimal format from loaded colors.
    ///</summary>
    ///<param name="_askedColor">The color key to get</param>
    ///<returns>The color value</returns>
    public string GetColor(string _askedColor)
    {
        foreach (KeyValuePair<string, string> kvp in config.colors)
        {
            if (kvp.Key == _askedColor)
                return kvp.Value;
        }
        return null;
    }
}
