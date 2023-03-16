using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Tomlyn;
using Tomlyn.Model;
using System;

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
    /// Load a config file, look for CLI overrides and starts with --file if given.
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

        string fileType = LoadConfigFile(configPath);

        // Override with args
        OverrideConfig();

        // Do things with completed config
        ApplyConfig();
        GameManager.instance.AppendLogLine($"Load {fileType} config file", false, ELogtype.success);

        // OUTDATED: Build-in CLI dependant
        string startFile = GetArg("--file");
        if (!string.IsNullOrEmpty(startFile))
            GameManager.instance.consoleController.RunCommandString($".cmds:{startFile}");
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
    private string LoadConfigFile(string _path = null)
    {
        try
        {
#if UNITY_EDITOR
            StreamReader loadedConfig = File.OpenText("Assets/Resources/config.toml");
#else
            StreamReader loadedConfig = File.OpenText(_path);
#endif
            TomlTable tomlConfig = Toml.ToModel(loadedConfig.ReadToEnd());
            ModifyConfig(tomlConfig);
            return "custom";
        }
        catch (System.Exception _e)
        {
            Debug.LogWarning(_e);
            GameManager.instance.AppendLogLine(_e.Message, false, ELogtype.warning);
            return "default";
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
        config.cacheLimitMo = Convert.ToInt32((long)(table["cacheLimitMo"]));
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
            if (!string.IsNullOrEmpty((string)kvp.Value))
                config.colors[kvp.Key] = (string)kvp.Value;
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
                tmp.Add(Convert.ToInt32((long)i));
            if (tmp.Count == 4)
                tempGradient.Add(tmp);
        }
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
    /// Apply given config.json to config
    ///</summary>
    ///<param name="_custom">The loaded config.json</param>
    private void ModifyConfig(SConfig _custom)
    {
        config.verbose = _custom.verbose;
        config.fullscreen = _custom.fullscreen;
        if (!string.IsNullOrEmpty(_custom.cachePath))
            config.cachePath = _custom.cachePath;
        if (_custom.cacheLimitMo > 0)
            config.cacheLimitMo = _custom.cacheLimitMo;
        if (_custom.cliPort > 0)
            config.cliPort = _custom.cliPort;
        foreach (KeyValuePair<string, string> kvp in _custom.textures)
        {
            if (kvp.Key == "perf22" || kvp.Key == "perf29")
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                    config.textures[kvp.Key] = kvp.Value;
            }
            else
                config.textures.Add(kvp.Key, kvp.Value);
        }
        List<string> colorsToCheck = new List<string>() { "selection", "edit", "focus", "highlight", "usableZone", "reservedZone", "technicalZone" };
        foreach (KeyValuePair<string, string> kvp in _custom.colors)
        {
            if (colorsToCheck.Contains(kvp.Key))
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                    config.colors[kvp.Key] = kvp.Value;
            }
            else
                config.colors.Add(kvp.Key, kvp.Value);
        }
        config.alphaOnInteract = Mathf.Clamp(_custom.alphaOnInteract, 0, 100);
        config.temperatureMinC = _custom.temperatureMinC;
        config.temperatureMaxC = _custom.temperatureMaxC;
        config.temperatureMinF = _custom.temperatureMinF;
        config.temperatureMaxF = _custom.temperatureMaxF;
        bool canUpdateTempGradient = true;
        if (_custom.customTemperatureGradient.Count >= 2 && _custom.customTemperatureGradient.Count <= 8)
        {
            foreach (List<int> tab in _custom.customTemperatureGradient)
            {
                if (tab.Count != 4)
                    canUpdateTempGradient = false;
            }
        }
        if (canUpdateTempGradient)
            config.customTemperatureGradient = _custom.customTemperatureGradient;
        config.useCustomGradient = _custom.useCustomGradient;
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
            GameManager.instance.AppendLogLine($"Fullscreen: {_value}", false);
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
                GameManager.instance.AppendLogLine($"Cache folder created at {fullPath}", false, ELogtype.success);
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
                GameManager.instance.AppendLogLine($"{kvp.Key} not found at {kvp.Value}", false, ELogtype.error);
            else
                GameManager.instance.textures.Add(kvp.Key, DownloadHandlerTexture.GetContent(www));
        }
        if (!GameManager.instance.textures.ContainsKey("perf22"))
        {
            GameManager.instance.AppendLogLine("Load default texture for perf22", false, ELogtype.warning);
            GameManager.instance.textures.Add("perf22", Resources.Load<Texture>("Textures/TilePerf22"));
        }
        if (!GameManager.instance.textures.ContainsKey("perf29"))
        {
            GameManager.instance.AppendLogLine("Load default texture for perf29", false, ELogtype.warning);
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
    /// Get a temperature extremum of a temperature unit
    ///</summary>
    ///<param name="_extremum">The extremum to get, must be "min" or "max"</param>
    ///<param name="_unit">The temperature unit for the extremum, must be "c" or "f"</param>
    ///<returns>The extremum for the temperature unit</returns>
    public (int min, int max) GetTemperatureLimit(string _unit)
    {
        if (_unit is null)
        {
            GameManager.instance.AppendLogLine("Null temperature unit", false, ELogtype.error);
            return (0, 0);
        }
        _unit = _unit.ToLower();
        if (_unit == "°c")
            return (config.temperatureMinC, config.temperatureMaxC);
        if (_unit == "°f")
            return (config.temperatureMinF, config.temperatureMaxF);
        GameManager.instance.AppendLogLine($"Unrecognised temperature unit : {_unit}", false, ELogtype.error);
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
