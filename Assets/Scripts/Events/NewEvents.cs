using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public abstract class EventParam {
    public abstract object Key();
}

public class OnFocusEvent : EventParam
{
    public GameObject obj;
    public OnFocusEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}


public class OnUnFocusEvent : EventParam
{
    public GameObject obj;

    public OnUnFocusEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class OnSelectItemEvent : EventParam
{
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class OnMouseHoverEvent : EventParam
{
    public GameObject obj;

    public OnMouseHoverEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class OnMouseUnHoverEvent : EventParam
{
    public GameObject obj;

    public OnMouseUnHoverEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}
public class HighlightEvent : EventParam
{
    public GameObject obj;

    public HighlightEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class ImportFinishedEvent : EventParam
{
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class ChangeCursorEvent : EventParam
{
    public CursorChanger.CursorType type;

    public ChangeCursorEvent(CursorChanger.CursorType _type)
    {
        type = _type;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class UpdateDomainEvent : EventParam
{
    public string name;

    public UpdateDomainEvent(string _name)
    {
        name = _name;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class SwitchLabelEvent : EventParam
{
    public ELabelMode value;

    public SwitchLabelEvent(ELabelMode _value)
    {
        value = _value;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class EditModeInEvent : EventParam
{
    public GameObject obj;

    public EditModeInEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class EditModeOutEvent : EventParam
{
    public GameObject obj;

    public EditModeOutEvent(GameObject _obj)
    {
        obj = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class ConnectApiEvent : EventParam
{
    public Dictionary<string, string> apiData;

    public ConnectApiEvent(Dictionary<string, string> _data)
    {
        apiData = _data;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class TemperatureDiagramEvent : EventParam
{
    public Room room;

    public TemperatureDiagramEvent(Room _room)
    {
        room = _room;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class TemperatureColorEvent : EventParam
{
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class TemperatureScatterPlotEvent : EventParam
{
    public OgreeObject ogreeObject;

    public TemperatureScatterPlotEvent(OgreeObject _obj)
    {
        ogreeObject = _obj;
    }
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class RightClickEvent : EventParam
{
    public static readonly object key = new object();
    public override object Key() { return key; }
}

public class CancelGenerateEvent : EventParam
{
    public static readonly object key = new object();
    public override object Key() { return key; }
}
