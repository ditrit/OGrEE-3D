using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFocusEvent : CustomEvent
{
    public GameObject _obj { get; set; }
}

public class OnUnFocusEvent : CustomEvent
{
    public GameObject _obj { get; set; }
}

public class OnSelectItemEvent : CustomEvent
{
    public GameObject _obj { get; set; }
}

public class OnDeselectItemEvent : CustomEvent
{
    public GameObject _obj { get; set; }
}

public class OnMouseHoverEvent : CustomEvent
{
    public GameObject _obj { get; set; }
}

public class OnMouseUnHoverEvent : CustomEvent
{
    public GameObject _obj { get; set; }
}

public class ImportFinishedEvent : CustomEvent
{

}

public class ChangeCursorEvent : CustomEvent
{
    public CursorChanger.CursorType type;
}

