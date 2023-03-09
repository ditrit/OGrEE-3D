using System;
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

/// <summary>
/// States of the application : they are <b>not</b> exclusive
/// </summary>
[Flags]
public enum State
{
    /// <summary>
    /// Nothing is selected
    /// </summary>
    None = 0, // b0

    /// <summary>
    /// Something is selected
    /// </summary>
    Select = 1, // b1

    /// <summary>
    /// Something is selected and focused
    /// </summary>
    Focus = 3, // Select, b10 <=> b11

    /// <summary>
    /// Something is selected, focused and edited
    /// </summary>
    Edit = 7, // Select, Focus, b100 <=> b111

    /// <summary>
    /// A room is selected
    /// </summary>
    RoomSelect = 9, // Select, b1000 <=> b1001

    /// <summary>
    /// A rack is Selected
    /// </summary>
    RackSelect = 17, // Select, b1_0000 <=> b1_0001

    /// <summary>
    /// A rack is selected and focused
    /// </summary>
    RackFocus = Focus | RackSelect,

    /// <summary>
    /// A rack is selected, focused and edited
    /// </summary>
    RackEdit = Edit | RackSelect,

    /// <summary>
    /// A device is selected
    /// </summary>
    DeviceSelect = 33, // Select, b10_0000 <=> b10_0001

    /// <summary>
    /// A device is selected and focused
    /// </summary>
    DeviceFocus = Focus | DeviceSelect,

    /// <summary>
    /// A device is selected, focused and edited
    /// </summary>
    DeviceEdit = Edit | DeviceSelect,

    /// <summary>
    /// Racks and devices' color matches their temperature
    /// </summary>
    TempColor = 64, // b100_0000
}