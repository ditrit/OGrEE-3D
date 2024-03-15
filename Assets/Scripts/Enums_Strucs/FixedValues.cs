using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum ELogtype
{
    info,
    infoCli,
    infoApi,
    success,
    successCli,
    successApi,
    warning,
    warningCli,
    warningApi,
    error,
    errorCli,
    errorApi
}

public enum ELogTarget
{
    cli,
    logger,
    both,
    none
}

public enum EPromptStatus
{
    wait,
    accept,
    refuse
}
public enum ELabelMode
{
    Default,
    FloatingOnTop,
    Hidden,
    Forced
}

public class Category
{
    public const string Domain = "domain";
    public const string Site = "site";
    public const string Building = "building";
    public const string Room = "room";
    public const string Rack = "rack";
    public const string Device = "device";
    public const string Group = "group";
    public const string Corridor = "corridor";
    public const string Sensor = "sensor";
    public const string Generic = "generic";
}

public class CommandType
{
    public const string Login = "login";
    public const string Logout = "logout";
    public const string LoadTemplate = "load template";
    public const string Select = "select";
    public const string Delete = "delete";
    public const string Focus = "focus";
    public const string Create = "create";
    public const string Modify = "modify";
    public const string Interact = "interact";
    public const string UI = "ui";
    public const string Camera = "camera";
    public const string ModifyTag = "modify-tag";
    public const string DeleteTag = "delete-tag";
    public const string CreateLayer = "create-layer";
    public const string ModifyLayer = "modify-layer";
    public const string DeleteLayer = "delete-layer";
}

public class CommandParameter
{
    public const string TilesName = "tilesName";
    public const string TilesColor = "tilesColor";
    public const string LocalCS = "localCS";
    public const string Label = "label";
    public const string LabelFont = "labelFont";
    public const string LabelBackground = "labelBackground";
    public const string Alpha = "alpha";
    public const string Slots = "slots";
    public const string Content = "content";
    public const string U = "U";
}

public class Command
{
    public const string Delay = "delay";
    public const string Infos = "infos";
    public const string Debug = "debug";
    public const string Highlight = "highlight";
    public const string ClearCache = "clearcache";
    public const string Move = "move";
    public const string Translate = "translate";
    public const string Wait = "wait";
}

public class Orientation
{
    public const string Front = "front";
    public const string Rear = "rear";
    public const string Left = "left";
    public const string Right = "right";
    public const string FrontFlipped = "frontflipped";
    public const string RearFlipped = "rearflipped";
}

public class LabelPos
{
    public const string Front = "front";
    public const string Rear = "rear";
    public const string Left = "left";
    public const string Right = "right";
    public const string Top = "top";
    public const string Bottom = "bottom";
    public const string FrontRear = "frontrear";
}

public class SensorPos
{
    public const string Front = "front";
    public const string Rear = "rear";
    public const string Left = "left";
    public const string Right = "right";
    public const string Lower = "lower";
    public const string Upper = "upper";
    public const string Center = "center";
}

public class AxisOrientation
{
    public const string Default = "+x+y";
    public const string XMinus = "-x+y";
    public const string YMinus = "+x-y";
    public const string BothMinus = "-x-y";
}

public class LengthUnit
{
    public const string Tile = "t";
    public const string U = "U";
    public const string Millimeter = "mm";
    public const string Centimeter = "cm";
    public const string Meter = "m";
    public const string Feet = "f";
    public const string OU = "OU";
}

public class UnitValue
{
    public const float U = 0.04445f;
    public const float OU = 0.048f;
    public const float Tile = 0.6f;
    public const float Foot = 0.3048f;
}

public class LaunchArgs
{
    public const string ConfigPathShort = "-c";
    public const string ConfigPathLong = "--config-file";
    public const string VerboseShort = "-v";
    public const string VerboseLong = "--verbose";
    public const string FullScreenShort = "-fs";
    public const string FullScreenLong = "--fullscreen";
    public const string CliPortShort = "-p";
    public const string CliPortLong = "--cliPort";
    public static readonly ReadOnlyCollection<string> Args = new(new List<string>()
    {
        ConfigPathShort, ConfigPathLong,
        VerboseShort, VerboseLong,
        FullScreenShort, FullScreenLong,
        CliPortShort, CliPortLong
    });
}

[Serializable]
public struct SConfig
{
    // private members to define custom setters
    private float alphaOnInteract;
    private float doubleClickDelay;
    private float moveSpeed;
    private float rotationSpeed;
    private float humanHeight;

    public bool verbose;
    public bool fullscreen;
    public string cachePath;
    public int cacheLimitMo;
    public int cliPort;
    public float AlphaOnInteract
    {
        readonly get => alphaOnInteract;
        set => alphaOnInteract = Mathf.Clamp(value, 0, 100);
    }
    public bool autoUHelpers;
    public Dictionary<string, string> textures;
    public Dictionary<string, string> colors;
    public int temperatureMinC;
    public int temperatureMaxC;
    public int temperatureMinF;
    public int temperatureMaxF;
    public List<List<int>> customTemperatureGradient;
    public bool useCustomGradient;
    public float DoubleClickDelay
    {
        readonly get => doubleClickDelay;
        set => doubleClickDelay = Mathf.Clamp(value, 0.01f, 1);
    }
    public float MoveSpeed
    {
        readonly get => moveSpeed;
        set => moveSpeed = Mathf.Clamp(value, 1, 50);
    }
    public float RotationSpeed
    {
        readonly get => rotationSpeed;
        set => rotationSpeed = Mathf.Clamp(value, 1, 100);
    }
    public float HumanHeight
    {
        readonly get => humanHeight;
        set => humanHeight = Mathf.Clamp(value, 1.5f, 1.8f);
    }

    /// <summary>
    /// Deep copy
    /// </summary>
    /// <returns>A deep copy of this struct</returns>
    public SConfig Clone()
    {
        SConfig clone = this;
        clone.textures = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> keyValuePair in textures)
            clone.textures.Add(keyValuePair.Key, keyValuePair.Value);
        clone.colors = new Dictionary<string, string>();
        foreach (KeyValuePair<string, string> keyValuePair in colors)
            clone.colors.Add(keyValuePair.Key, keyValuePair.Value);
        clone.customTemperatureGradient = new List<List<int>>();
        foreach (List<int> color in customTemperatureGradient)
            clone.customTemperatureGradient.Add(color.GetRange(0, color.Count));
        return clone;
    }
}

public class DefaultValues
{
    public const string CacheDirName = ".ogreeCache";
#if UNITY_EDITOR
    public const string DefaultConfigPath = "Assets/Resources/config.toml";
#else
    public const string DefaultConfigPath = "../OGrEE-Core/config.toml";
#endif
    public static readonly SConfig Config = new()
    {
        verbose = false,
        fullscreen = false,
        cachePath = $"{Application.dataPath}/",
        cacheLimitMo = 100,
        cliPort = 5500,
        AlphaOnInteract = 50,
        autoUHelpers = true,
        textures = new()
        {
            { "perf22", "https://raw.githubusercontent.com/ditrit/OGREE-3D/master/Assets/Resources/Textures/TilePerf22.png" },
            { "perf29", "https://raw.githubusercontent.com/ditrit/OGREE-3D/master/Assets/Resources/Textures/TilePerf29.png" }
        },
        colors = new()
        {
            { "selection", "#21FF00" },
            { "edit", "#C900FF" },
            { "focus", "#FF9F00" },
            { "highlight", "#00D5FF" },
            { "scatterPlot", "#2C4CBE" },
            { "usableZone", "#DBEDF2" },
            { "reservedZone", "#F2F2F2" },
            { "technicalZone", "#EBF2DE" }
        },
        temperatureMinC = 0,
        temperatureMaxC = 100,
        temperatureMinF = 32,
        temperatureMaxF = 212,
        customTemperatureGradient = new()
        {
            new List<int>() { 0, 0, 255, 0 },
            new List<int>() { 255, 0, 0, 100 },
            new List<int>() { 255, 255, 0, 50 }
        },
        DoubleClickDelay = 0.25f,
        MoveSpeed = 15,
        RotationSpeed = 50,
        HumanHeight = 1.62f
    };
}

public class TemperatureUnits
{
    public const string Celsius = "°C";
    public const string Fahrenheit = "°F";
}