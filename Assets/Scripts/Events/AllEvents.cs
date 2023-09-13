using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base event for all EventManager events.
/// </summary>
public class CustomEvent
{

}

public class OnFocusEvent : CustomEvent
{
    public GameObject obj;

    public OnFocusEvent(GameObject _obj)
    {
        obj = _obj;
    }
}

public class OnUnFocusEvent : CustomEvent
{
    public GameObject obj;

    public OnUnFocusEvent(GameObject _obj)
    {
        obj = _obj;
    }
}

public class OnSelectItemEvent : CustomEvent
{
}

public class OnMouseHoverEvent : CustomEvent
{
    public GameObject obj;

    public OnMouseHoverEvent(GameObject _obj)
    {
        obj = _obj;
    }

}

public class OnMouseUnHoverEvent : CustomEvent
{
    public GameObject obj;

    public OnMouseUnHoverEvent(GameObject _obj)
    {
        obj = _obj;
    }
}

public class HighlightEvent : CustomEvent
{
    public GameObject obj;

    public HighlightEvent(GameObject _obj)
    {
        obj = _obj;
    }
}

public class ImportFinishedEvent : CustomEvent
{
}

public class ChangeCursorEvent : CustomEvent
{
    public CursorChanger.CursorType type;

    public ChangeCursorEvent(CursorChanger.CursorType _type)
    {
        type = _type;
    }
}

public class UpdateDomainEvent : CustomEvent
{
    public string name;

    public UpdateDomainEvent(string _name)
    {
        name = _name;
    }
}

public class SwitchLabelEvent : CustomEvent
{
    public ELabelMode value;

    public SwitchLabelEvent(ELabelMode _value)
    {
        value = _value;
    }
}

public class EditModeInEvent : CustomEvent
{
    public GameObject obj;

    public EditModeInEvent(GameObject _obj)
    {
        obj = _obj;
    }
}

public class EditModeOutEvent : CustomEvent
{
    public GameObject obj;

    public EditModeOutEvent(GameObject _obj)
    {
        obj = _obj;
    }
}

public class ConnectApiEvent : CustomEvent
{
    public Dictionary<string, string> apiData;

    public ConnectApiEvent(Dictionary<string, string> _data)
    {
        apiData = _data;
    }
}

public class TemperatureDiagramEvent : CustomEvent
{
    public Room room;

    public TemperatureDiagramEvent(Room _room)
    {
        room = _room;
    }
}
public class TemperatureColorEvent : CustomEvent
{
}

public class TemperatureScatterPlotEvent : CustomEvent
{
    public OgreeObject ogreeObject;

    public TemperatureScatterPlotEvent(OgreeObject _obj)
    {
        ogreeObject = _obj;
    }
}

public class RightClickEvent : CustomEvent
{
}

public class CancelGenerateEvent : CustomEvent
{
}