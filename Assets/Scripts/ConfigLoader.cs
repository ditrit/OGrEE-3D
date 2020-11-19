﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ConfigLoader
{
    private struct SConfig
    {
        public string verbose;
        public string fullscreen;
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

        OverrideConfig("--verbose", config.verbose);
        OverrideConfig("--fullscreen", config.fullscreen);
        OverrideConfig("--serverUrl", config.db_url);
        OverrideConfig("--serverLogin", config.db_login);
        OverrideConfig("--serverToken", config.db_token);


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
    /// Get an argument and override config value.
    ///</summmary>
    ///<param name="_arg">The argument to search</param>
    ///<param name="_value">The config value to override</param>
    private void OverrideConfig(string _arg, string _value)
    {
        string str = GetArg(_arg);
        if (!string.IsNullOrEmpty(str))
            _value = str;
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
            return JsonUtility.FromJson<SConfig>(jsonCongif.ReadToEnd());
        }
        catch
        {
            TextAsset ResourcesCongif = Resources.Load<TextAsset>("config");
            GameManager.gm.AppendLogLine("Load default config file", "green");
            return JsonUtility.FromJson<SConfig>(ResourcesCongif.ToString());
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

        // MonoBehaviour.StartCoroutine(ConnectToApi(_config.db_url));
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

    ///
    public IEnumerator ConnectToApi()
    {
        UnityWebRequest www = UnityWebRequest.Get(config.db_url);

        yield return www.SendWebRequest();
        if (www.isHttpError || www.isNetworkError)
        {
            GameManager.gm.AppendLogLine($"Error while connecting to API: {www.error}", "red");
            yield break;
        }
        string response = www.downloadHandler.text;
        GameManager.gm.AppendLogLine(response);

        ApiManager.instance.Initialize(config.db_url, config.db_login, config.db_token);
    }
}