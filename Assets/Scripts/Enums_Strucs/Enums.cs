using System.Collections;
using System.Collections.Generic;
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

public enum EPromptStatus
{
    wait,
    accept,
    refuse
}
public enum ELabelMode
{
    FrontAndRear,
    FloatingOnTop,
    Hidden
}