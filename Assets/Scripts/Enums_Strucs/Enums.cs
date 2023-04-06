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