using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Networking;
using Tomlyn;
using Tomlyn.Model;

public class ConfigLoader
{
    private struct SConfig
    {
        public bool verbose;
        public bool fullscreen;
        public string cachePath;
        public int cacheLimitMo;
        public int cliPort;
        public Dictionary<string, string> textures;
        public Dictionary<string, string> colors;
        public float alphaOnInteract;
        public int temperatureMinC;
        public int temperatureMaxC;
        public int temperatureMinF;
        public int temperatureMaxF;
        public List<List<int>> customTemperatureGradient;
        public bool useCustomGradient;
    }

    private SConfig config;
    private bool verbose = false;
    private readonly string cacheDirName = ".ogreeCache";

    ///<summary>
    /// Load a config file & look for command line overrides.
    ///</summary>
    public void LoadConfig()
    {
        // Load default config file
        TextAsset ResourcesConfig = Resources.Load<TextAsset>("config");
        config = JsonConvert.DeserializeObject<SConfig>(ResourcesConfig.ToString());

        // Load toml config from given path
        string configPath = GetArg("-c");
        if (string.IsNullOrEmpty(configPath))
            configPath = GetArg("--config-file");

        LoadConfigFile(configPath);

        // Override with args
        OverrideConfig();

        // Do things with completed config
        ApplyConfig();
    }

    ///<summary>
    /// Get an argument by its name (ex -file).
    /// From https://forum.unity.com/threads/pass-custom-parameters-to-standalone-on-launch.429144/
    ///</summary>
    ///<param name="_name">Name of the wanted argument</param>
    ///<returns>The value of the asked argument</returns>
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
        string[] args = new string[] { "-v", "--verbose", "-fs", "--fullscreen" };
        for (int i = 0; i < args.Length; i++)
        {
            string str = GetArg(args[i]);
            if (!string.IsNullOrEmpty(str))
            {
                switch (i)
                {
                    case 0:
                        config.verbose = bool.Parse(str);
                        break;
                    case 1:
                        config.verbose = bool.Parse(str);
                        break;
                    case 2:
                        config.fullscreen = bool.Parse(str);
                        break;
                    case 3:
                        config.fullscreen = bool.Parse(str);
                        break;
                }
            }
        }
    }

    ///<summary>
    /// Try to load a custom config file. Otherwise, load default config file from Resources folder.
    ///</summary>
    ///<returns>The type of loaded config : "custom" or "default"</returns>
    private void LoadConfigFile(string _path = null)
    {
        try
        {
#if UNITY_EDITOR
            StreamReader loadedConfig = File.OpenText("Assets/Resources/config.toml");
#else
            if (string.IsNullOrEmpty(_path))
                _path = "./config.toml";
            StreamReader loadedConfig = File.OpenText(_path);
#endif
            TomlTable tomlConfig = Toml.ToModel(loadedConfig.ReadToEnd());
            ModifyConfig(tomlConfig);
            GameManager.instance.AppendLogLine($"Load custom config file ({_path})", ELogTarget.logger, ELogtype.success);
        }
        catch (Exception _e)
        {
            Debug.LogWarning(_e);
            GameManager.instance.AppendLogLine(_e.Message, ELogTarget.none, ELogtype.warning);
            GameManager.instance.AppendLogLine($"Load default config file", ELogTarget.logger, ELogtype.success);
        }
    }

    ///<summary>
    /// Apply given toml config to config
    ///</summary>
    ///<param name="_customConfig">The loaded config.toml file</param>
    private void ModifyConfig(TomlTable _customConfig)
    {
        Debug.Log("Parse config.toml");
        TomlTable table = (TomlTable)_customConfig["OGrEE-3D"];

        config.verbose = (bool)table["verbose"];
        config.fullscreen = (bool)table["fullscreen"];
        config.cachePath = (string)table["cachePath"];
        config.cacheLimitMo = Convert.ToInt32(table["cacheLimitMo"]);
        config.cliPort = Convert.ToInt32(table["cliPort"]);
        config.alphaOnInteract = Mathf.Clamp(Convert.ToInt32(table["alphaOnInteract"]), 0, 100);

        foreach (var kvp in (TomlTable)table["textures"])
        {
            if (!string.IsNullOrEmpty((string)kvp.Value))
                config.textures[kvp.Key] = (string)kvp.Value;
        }
        // foreach (var kvp in config.textures)
        //     Debug.Log($"{kvp.Key}: {kvp.Value}");

        foreach (var kvp in (TomlTable)table["colors"])
        {
            if (!string.IsNullOrEmpty((string)kvp.Value) && ColorUtility.TryParseHtmlString((string)kvp.Value, out Color c))
                config.colors[kvp.Key] = (string)kvp.Value;
            else
                GameManager.instance.AppendLogLine($"\"{kvp.Key}\" value cannot be used as a color", ELogTarget.logger, ELogtype.error);
        }
        // foreach (var kvp in config.colors)
        //     Debug.Log($"{kvp.Key}: {kvp.Value}");

        TomlTable temperatureTable = (TomlTable)table["temperature"];
        config.temperatureMinC = Convert.ToInt32(temperatureTable["minC"]);
        config.temperatureMaxC = Convert.ToInt32(temperatureTable["maxC"]);
        config.temperatureMinF = Convert.ToInt32(temperatureTable["minF"]);
        config.temperatureMaxF = Convert.ToInt32(temperatureTable["maxF"]);

        config.useCustomGradient = (bool)temperatureTable["useCustomGradient"];
        List<List<int>> tempGradient = new List<List<int>>();
        foreach (var colorDef in (TomlArray)temperatureTable["customTemperatureGradient"])
        {
            List<int> tmp = new List<int>();
            foreach (var i in (TomlArray)colorDef)
                tmp.Add(Convert.ToInt32(i));
            if (tmp.Count == 4 && tempGradient.Count < 8)
                tempGradient.Add(tmp);
        }
        if (tempGradient.Count >= 2)
            config.customTemperatureGradient = tempGradient;
        // foreach (var x in config.customTemperatureGradient)
        // {
        //     string str = "";
        //     foreach (var i in x)
        //         str += $"{i}/";
        //     Debug.Log(str);
        // }
    }

    ///<summary>
    /// For each SConfig value, call corresponding method.
    ///</summary>
    private void ApplyConfig()
    {
        verbose = config.verbose;

        GameManager.instance.server.SetupPorts(config.cliPort);
        CreateCacheDir();
        FullScreenMode(config.fullscreen);
        SetMaterialColor("selection", GameManager.instance.selectMat);
        SetMaterialColor("focus", GameManager.instance.focusMat);
        SetMaterialColor("edit", GameManager.instance.editMat);
        SetMaterialColor("highlight", GameManager.instance.highlightMat);
    }

    ///<summary> 
    /// Set fullscreen mode.
    ///</summary> 
    ///<param name="_value">The value to set</param>
    private void FullScreenMode(bool _value)
    {
        if (verbose)
            GameManager.instance.AppendLogLine($"Fullscreen: {_value}", ELogTarget.none);
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
                GameManager.instance.AppendLogLine($"Cache folder created at {fullPath}", ELogTarget.logger, ELogtype.success);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    ///<summary>
    /// Use a key on config.color to set a material (with alpha = 0.5).
    ///</summary>
    ///<param name="_key">The value to get</param>
    ///<param name="_mat">The material to edit</param>
    private void SetMaterialColor(string _key, Material _mat)
    {
        Color tmp;
        if (config.colors.ContainsKey(_key))
        {
            tmp = Utils.ParseHtmlColor(config.colors[_key]);
            tmp.a = config.alphaOnInteract / 100;
            _mat.color = tmp;
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
    public int GetCacheLimit()
    {
        return config.cacheLimitMo;
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
                GameManager.instance.AppendLogLine($"{kvp.Key} not found at {kvp.Value}", ELogTarget.logger, ELogtype.error);
            else
                GameManager.instance.textures.Add(kvp.Key, DownloadHandlerTexture.GetContent(www));
        }
        if (!GameManager.instance.textures.ContainsKey("perf22"))
        {
            GameManager.instance.AppendLogLine("Load default texture for perf22", ELogTarget.logger, ELogtype.warning);
            GameManager.instance.textures.Add("perf22", Resources.Load<Texture>("Textures/TilePerf22"));
        }
        if (!GameManager.instance.textures.ContainsKey("perf29"))
        {
            GameManager.instance.AppendLogLine("Load default texture for perf29", ELogTarget.logger, ELogtype.warning);
            GameManager.instance.textures.Add("perf29", Resources.Load<Texture>("Textures/TilePerf29"));
        }
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

    ///<summary>
    /// Get the minimum and the maximum of a temperature unit
    ///</summary>
    ///<param name="_unit">The temperature unit for the extremum, must be "c" or "f"</param>
    ///<returns>The minimum and the maximum for the temperature unit</returns>
    public (int min, int max) GetTemperatureLimit(string _unit)
    {
        if (_unit is null)
        {
            GameManager.instance.AppendLogLine("Null temperature unit", ELogTarget.logger, ELogtype.error);
            return (0, 0);
        }
        _unit = _unit.ToLower();
        if (_unit == "°c")
            return (config.temperatureMinC, config.temperatureMaxC);
        if (_unit == "°f")
            return (config.temperatureMinF, config.temperatureMaxF);
        GameManager.instance.AppendLogLine($"Unrecognised temperature unit : {_unit}", ELogTarget.logger, ELogtype.error);
        return (0, 0);
    }

    /// <summary>
    /// Get custom gradient colors to be used to represent temperatures
    /// </summary>
    /// <returns>the list of user-defined temperatures and their positions on a gradient</returns>
    public List<List<int>> GetCustomGradientColors()
    {
        return config.customTemperatureGradient;
    }

    /// <summary>
    /// Should the user-defined temperature color gradient be used ?
    /// </summary>
    /// <returns>True if yes, false if not </returns>
    public bool IsUsingCustomGradient()
    {
        return config.useCustomGradient;
    }
}
