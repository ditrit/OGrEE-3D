using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Tomlyn;
using Tomlyn.Model;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ConfigHandler
{
    [SerializeField] private SConfig config;
    [SerializeField] private string savedConfigPath;

    ///<summary>
    /// Load a config file & look for command line overrides.
    ///</summary>
    public void LoadConfig()
    {
        // Load default config file
        config = DefaultValues.Config.Clone();

        // Load toml config from given path
        string configPath = GetArg(LaunchArgs.ConfigPathShort);
        if (string.IsNullOrEmpty(configPath))
            configPath = GetArg(LaunchArgs.ConfigPathLong);

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
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == _name && args.Length > i + 1)
                return args[i + 1];
        }
        return null;
    }

    ///<summmary>
    /// Override config if arguments are found.
    ///</summmary>
    private void OverrideConfig()
    {
        foreach (string arg in LaunchArgs.Args)
        {
            string str = GetArg(arg);
            if (string.IsNullOrEmpty(str))
                continue;

            switch (arg)
            {
                case LaunchArgs.VerboseShort:
                case LaunchArgs.VerboseLong:
                    config.verbose = bool.Parse(str);
                    break;

                case LaunchArgs.FullScreenShort:
                case LaunchArgs.FullScreenLong:
                    config.fullscreen = bool.Parse(str);
                    break;
                case LaunchArgs.CliPortShort:
                case LaunchArgs.CliPortLong:
                    config.cliPort = int.Parse(str);
                    break;
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
            StreamReader loadedConfig = File.OpenText(DefaultValues.DefaultConfigPath);
            savedConfigPath = DefaultValues.DefaultConfigPath;
#else
            if (string.IsNullOrEmpty(_path))
                _path = DefaultValues.DefaultConfigPath;
            StreamReader loadedConfig = File.OpenText(_path);
            savedConfigPath = _path;
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
        config.doubleClickDelay = Mathf.Clamp(Convert.ToSingle(table["doubleClickDelay"]), 0.01f, 1);
        config.autoUHelpers = (bool)table["autoUHelpers"];

        config.moveSpeed = Mathf.Clamp(Convert.ToInt32(table["moveSpeed"]), 1, 50);
        config.rotationSpeed = Mathf.Clamp(Convert.ToInt32(table["rotationSpeed"]), 1, 100);
        config.humanHeight = Mathf.Clamp(Convert.ToSingle(table["humanHeight"]), 1.5f, 1.8f);

        foreach (KeyValuePair<string, object> kvp in (TomlTable)table["textures"])
        {
            if (!string.IsNullOrEmpty((string)kvp.Value))
                config.textures[kvp.Key] = (string)kvp.Value;
        }
        // foreach (KeyValuePair<string, string> kvp in config.textures)
        //     Debug.Log($"{kvp.Key}: {kvp.Value}");

        foreach (KeyValuePair<string, object> kvp in (TomlTable)table["colors"])
        {
            if (!string.IsNullOrEmpty((string)kvp.Value) && ColorUtility.TryParseHtmlString((string)kvp.Value, out Color c))
                config.colors[kvp.Key] = (string)kvp.Value;
            else
                GameManager.instance.AppendLogLine($"\"{kvp.Key}\" value cannot be used as a color", ELogTarget.logger, ELogtype.error);
        }
        // foreach (KeyValuePair<string, string> kvp in config.colors)
        //     Debug.Log($"{kvp.Key}: {kvp.Value}");

        TomlTable temperatureTable = (TomlTable)table["temperature"];
        config.temperatureMinC = Convert.ToInt32(temperatureTable["minC"]);
        config.temperatureMaxC = Convert.ToInt32(temperatureTable["maxC"]);
        config.temperatureMinF = Convert.ToInt32(temperatureTable["minF"]);
        config.temperatureMaxF = Convert.ToInt32(temperatureTable["maxF"]);

        config.useCustomGradient = (bool)temperatureTable["useCustomGradient"];
        List<List<int>> tempGradient = new();
        foreach (object colorDef in (TomlArray)temperatureTable["customTemperatureGradient"])
        {
            List<int> tmp = new();
            foreach (object i in (TomlArray)colorDef)
                tmp.Add(Convert.ToInt32(i));
            if (tmp.Count == 4 && tempGradient.Count < 8)
                tempGradient.Add(tmp);
        }
        if (tempGradient.Count >= 2)
            config.customTemperatureGradient = tempGradient;
        // foreach (List<int> x in config.customTemperatureGradient)
        // {
        //     string str = "";
        //     foreach (int i in x)
        //         str += $"{i}/";
        //     Debug.Log(str);
        // }
    }

    ///<summary>
    /// For each SConfig value, call corresponding method.
    ///</summary>
    private void ApplyConfig()
    {
        GameManager.instance.server.SetupPorts(config.cliPort);
        CreateCacheDir();
        FullScreenMode(config.fullscreen);
        SetMaterialColor("selection", GameManager.instance.selectMat);
        SetMaterialColor("focus", GameManager.instance.focusMat);
        SetMaterialColor("edit", GameManager.instance.editMat);
        SetMaterialColor("highlight", GameManager.instance.highlightMat);
        SetMaterialColor("scatterPlot", GameManager.instance.scatterPlotMat);
    }

    /// <summary>
    /// In used config.toml file, change <see cref="_key"/> to its new <see cref="_value"/>.
    /// </summary>
    /// <param name="_key">The config parameter to update</param>
    /// <param name="_value">The new value to write</param>
    public void WritePreference(string _key, string _value)
    {
        if (!string.IsNullOrEmpty(savedConfigPath))
        {
            string[] arrLine = File.ReadAllLines(savedConfigPath);
            for (int i = 0; i < arrLine.Length; i++)
            {
                if (arrLine[i].StartsWith(_key))
                    arrLine[i] = $"{_key} = {_value}";
            }
            File.WriteAllLines(savedConfigPath, arrLine);
        }
    }

    ///<summary> 
    /// Set fullscreen mode.
    ///</summary> 
    ///<param name="_value">The value to set</param>
    private void FullScreenMode(bool _value)
    {
        if (config.verbose)
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
        string fullPath = config.cachePath + DefaultValues.CacheDirName;
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
        return config.cachePath + DefaultValues.CacheDirName;
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
    /// Get the minimum and the maximum of a temperature unit defined in <see cref="TemperatureUnits"/>
    ///</summary>
    ///<param name="_unit">The temperature unit for the extremum, must be in <see cref="TemperatureUnits"/></param>
    ///<returns>The minimum and the maximum for the temperature unit</returns>
    public (int min, int max) GetTemperatureLimit(string _unit)
    {
        if (_unit is null)
        {
            GameManager.instance.AppendLogLine("Null temperature unit", ELogTarget.logger, ELogtype.error);
            return (0, 0);
        }
        if (_unit == TemperatureUnits.Celsius)
            return (config.temperatureMinC, config.temperatureMaxC);
        if (_unit == TemperatureUnits.Fahrenheit)
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

    /// <summary>
    /// Get the value of <see cref="config.doubleClickDelay"/>
    /// </summary>
    /// <returns>The value of <see cref="config.doubleClickDelay"/></returns>
    public float GetDoubleClickDelay()
    {
        return config.doubleClickDelay;
    }

    /// <summary>
    /// Set the value of <see cref="config.doubleClickDelay"/>
    /// </summary>
    /// <param name="_value">The value to set</param>
    public void SetDoubleClickDelay(float _value)
    {
        config.doubleClickDelay = Mathf.Clamp(_value, 0.01f, 1);
    }

    /// <summary>
    /// Get the value of <see cref="config.moveSpeed"/>
    /// </summary>
    /// <returns>The value of <see cref="config.moveSpeed"/></returns>
    public float GetMoveSpeed()
    {
        return config.moveSpeed;
    }

    /// <summary>
    /// Set the value of <see cref="config.moveSpeed"/>
    /// </summary>
    /// <param name="_value">The value to set</param>
    public void SetMoveSpeed(float _value)
    {
        config.moveSpeed = Mathf.Clamp(_value, 1, 50);
    }

    /// <summary>
    /// Get the value of <see cref="config.rotationSpeed"/>
    /// </summary>
    /// <returns>The value of <see cref="config.rotationSpeed"/></returns>
    public float GetRotationSpeed()
    {
        return config.rotationSpeed;
    }

    /// <summary>
    /// Set the value of <see cref="config.rotationSpeed"/>
    /// </summary>
    /// <param name="_value">The value to set</param>
    public void SetRotationSpeed(float _value)
    {
        config.rotationSpeed = Mathf.Clamp(_value, 1, 100);
    }

    /// <summary>
    /// Get the value of <see cref="config.humanHeight"/>
    /// </summary>
    /// <returns>The value of <see cref="config.humanHeight"/></returns>
    public float GetHumanHeight()
    {
        return config.humanHeight;
    }

    /// <summary>
    /// Set the value of <see cref="config.humanHeight"/>
    /// </summary>
    /// <param name="_value">The value to set</param>
    public void SetHumanHeight(float _value)
    {
        config.humanHeight = Mathf.Clamp(_value, 1.5f, 1.8f);
    }

    /// <summary>
    /// Get the value of <see cref="config.autoUHelpers"/>
    /// </summary>
    /// <returns>The value of <see cref="config.autoUHelpers"/></returns>
    public bool GetAutoUHelpers()
    {
        return config.autoUHelpers;
    }

    /// <summary>
    /// Set the value of <see cref="config.autoUHelpers"/>
    /// </summary>
    /// <param name="_value">The value to set</param>
    public void SetAutoUHelpers(bool _value)
    {
        config.autoUHelpers = _value;
    }
}
