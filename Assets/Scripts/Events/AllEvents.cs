using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFocusEvent : CustomEvent
{
    public GameObject obj;
}

public class OnUnFocusEvent : CustomEvent
{
    public GameObject obj;
}

public class OnSelectItemEvent : CustomEvent
{
}

public class OnMouseHoverEvent : CustomEvent
{
    public GameObject obj;
}

public class OnMouseUnHoverEvent : CustomEvent
{
    public GameObject obj;
}

public class HighlightEvent : CustomEvent
{
    public GameObject obj;
}

public class ImportFinishedEvent : CustomEvent
{

}

public class ChangeCursorEvent : CustomEvent
{
    public CursorChanger.CursorType type;
}

public class UpdateDomainEvent : CustomEvent
{
    public string name;
}

public class SwitchLabelEvent : CustomEvent
{
    public ELabelMode value;
}

public class EditModeInEvent : CustomEvent
{
    public GameObject obj;
}

public class EditModeOutEvent : CustomEvent
{
    public GameObject obj;
}

public class ConnectApiEvent : CustomEvent
{
}

public class TemperatureDiagramEvent : CustomEvent
{
    public GameObject obj;
}
public class TemperatureColorEvent : CustomEvent
{
}

public class TemperatureScatterPlotEvent : CustomEvent
{
    public GameObject obj;
}
