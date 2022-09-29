using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFocusEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class OnUnFocusEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class OnSelectItemEvent : CustomEvent
{
}

public class OnMouseHoverEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class OnMouseUnHoverEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class HighlightEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class ImportFinishedEvent : CustomEvent
{

}

public class ChangeCursorEvent : CustomEvent
{
    public CursorChanger.CursorType type;
}

public class UpdateTenantEvent : CustomEvent
{
    public string name;
}

public class ToggleLabelEvent : CustomEvent
{
    public enum ELabelMode
    {
        FrontAndRear,
        FloatingOnTop,
        Hidden
    }
    public ELabelMode value;
}

public class EditModeInEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class EditModeOutEvent : CustomEvent
{
    public GameObject obj { get; set; }
}

public class ConnectApiEvent : CustomEvent
{

}
