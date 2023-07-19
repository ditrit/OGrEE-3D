using System.Collections.Generic;
using System.Collections.ObjectModel;

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
}
public class CommandType
{
    public const string Login = "login";
    public const string LoadTemplate = "load template";
    public const string Select = "select";
    public const string Delete = "delete";
    public const string Focus = "focus";
    public const string Create = "create";
    public const string Modify = "modify";
    public const string Interact = "interact";
    public const string UI = "ui";
    public const string Camera = "camera";
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
    public const string MilliMeter = "mm";
    public const string CentiMeter = "cm";
    public const string Meter = "m";
    public const string Feet = "f";
    public const string OU = "OU";
}

public class LaunchArgs
{
    public const string ConfigPathShort = "-c";
    public const string ConfigPathLong = "--config-file";
    public const string VerboseShort = "-v";
    public const string VerboseLong = "--verbose";
    public const string FullScreenShort = "-fs";
    public const string FullScreenLong = "--fullscreen";
    public static readonly ReadOnlyCollection<string> Args = new ReadOnlyCollection<string>(new List<string>() { ConfigPathShort,ConfigPathLong,VerboseShort,VerboseLong,FullScreenShort,FullScreenLong });
}

public class DefaultValues
{

}