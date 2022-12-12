using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

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
        public string apiUrl;
        public string apiToken;
        public int temperatureMinC;
        public int temperatureMaxC;
        public int temperatureMinF;
        public int temperatureMaxF;
        public int[][] customTemperatureGradient;
        public bool useCustomGradient;
    }

    private SConfig config;
    private bool verbose = false;
    private string cacheDirName = ".ogreeCache";

    ///<summary>
    /// Load a config file, look for CLI overrides and starts with --file if given.
    ///</summary>
    public void LoadConfig()
    {
        string fileType = LoadConfigFile();
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
                        config.verbose = bool.Parse(str);
                        break;
                    case 1:
                        config.fullscreen = bool.Parse(str);
                        break;
                    case 2:
                        config.apiUrl = str;
                        break;
                    case 3:
                        config.apiToken = str;
                        break;
                }

            }
        }
    }

    ///<summary>
    /// Try to load a custom config file. Otherwise, load default config file from Resources folder.
    ///</summary>
    private string LoadConfigFile()
    {
        TextAsset ResourcesConfig = Resources.Load<TextAsset>("config");
        config = JsonConvert.DeserializeObject<SConfig>(ResourcesConfig.ToString());
        try
        {
            StreamReader jsonConfig = File.OpenText("OGrEE-3D_Data/config.json");
            ModifyConfig(JsonConvert.DeserializeObject<SConfig>(jsonConfig.ReadToEnd()));
            return "custom";
        }
        catch (System.Exception _e)
        {
            GameManager.gm.AppendLogLine(_e.Message, false, eLogtype.warning);
            return "default";
        }
    }

    ///
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
        if (_custom.customTemperatureGradient.Length >= 2)
        {
            foreach (int[] tab in _custom.customTemperatureGradient)
            {
                if (tab.Length != 4)
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

        GameManager.gm.server.SetupPorts(config.cliPort);
        CreateCacheDir();
        FullScreenMode(config.fullscreen);
        SetMaterialColor("selection", GameManager.gm.selectMat);
        SetMaterialColor("focus", GameManager.gm.focusMat);
        SetMaterialColor("edit", GameManager.gm.editMat);
        SetMaterialColor("highlight", GameManager.gm.highlightMat);
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
    /// Save API url and token in config.
    ///</summary>
    ///<param name="_url">URL of the API to connect</param>
    ///<param name="_token">Corresponding authorisation token</param>
    public void RegisterApi(string _url, string _token)
    {
        config.apiUrl = _url;
        config.apiToken = _token;
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
        await ApiManager.instance.Initialize(config.apiUrl, config.apiToken);
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
        return config.apiUrl;
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
        _unit = _unit.ToLower();
        if (_unit == "°c")
            return (config.temperatureMinC, config.temperatureMaxC);
        if (_unit == "°f")
            return (config.temperatureMinF, config.temperatureMaxF);
        GameManager.gm.AppendLogLine($"Unrecognised temperature unit : {_unit}", false, eLogtype.error);
        return (0, 0);
    }

    /// <summary>
    /// Get custom gradient colors to be used to represent temperatures
    /// </summary>
    /// <returns>the list of user-defined temperatures and their positions on a gradient</returns>
    public List<int[]> GetCustomGradientColors()
    {
        return config.customTemperatureGradient.ToList();
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
