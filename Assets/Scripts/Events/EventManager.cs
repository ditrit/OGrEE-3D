using System.ComponentModel;
using UnityEngine;

public class EventManager
{
    public static EventManager instance
    {
        get
        {
            if (newEventManagerInstance == null)
            {
                newEventManagerInstance = new EventManager();
            }

            return newEventManagerInstance;
        }
    }
    private static EventManager newEventManagerInstance = null;

    public EventHandlerList listEventDelegates = new EventHandlerList();


    public delegate void Event<T>(T _eventParam) where T : EventParam;
    public event Event<OnFocusEvent> OnFocus
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(OnFocusEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(OnFocusEvent.key, value);
        }
    }
    public event Event<OnUnFocusEvent> OnUnFocus
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(OnUnFocusEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(OnUnFocusEvent.key, value);
        }
    }
    public event Event<OnSelectItemEvent> OnSelectItem
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(OnSelectItemEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(OnSelectItemEvent.key, value);
        }
    }
    public event Event<OnMouseHoverEvent> OnMouseHover
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(OnMouseHoverEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(OnMouseHoverEvent.key, value);
        }
    }
    public event Event<OnMouseUnHoverEvent> OnMouseUnHover
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(OnMouseUnHoverEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(OnMouseUnHoverEvent.key, value);
        }
    }
    public event Event<HighlightEvent> Highlight
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(HighlightEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(HighlightEvent.key, value);
        }
    }
    public event Event<ImportFinishedEvent> ImportFinished
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(ImportFinishedEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(ImportFinishedEvent.key, value);
        }
    }
    public event Event<ChangeCursorEvent> ChangeCursor
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(ChangeCursorEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(ChangeCursorEvent.key, value);
        }
    }
    public event Event<UpdateDomainEvent> UpdateDomain
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(UpdateDomainEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(UpdateDomainEvent.key, value);
        }
    }
    public event Event<SwitchLabelEvent> SwitchLabel
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(SwitchLabelEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(SwitchLabelEvent.key, value);
        }
    }
    public event Event<EditModeInEvent> EditModeIn
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(EditModeInEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(EditModeInEvent.key, value);
        }
    }
    public event Event<EditModeOutEvent> EditModeOut
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(EditModeOutEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(EditModeOutEvent.key, value);
        }
    }
    public event Event<ConnectApiEvent> ConnectApi
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(ConnectApiEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(ConnectApiEvent.key, value);
        }
    }
    public event Event<TemperatureDiagramEvent> TemperatureDiagram
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(TemperatureDiagramEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(TemperatureDiagramEvent.key, value);
        }
    }
    public event Event<TemperatureColorEvent> TemperatureColor
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(TemperatureColorEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(TemperatureColorEvent.key, value);
        }
    }
    public event Event<TemperatureScatterPlotEvent> TemperatureScatterPlot
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(TemperatureScatterPlotEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(TemperatureScatterPlotEvent.key, value);
        }
    }
    public event Event<RightClickEvent> RightClick
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(RightClickEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(RightClickEvent.key, value);
        }
    }
    public event Event<CancelGenerateEvent> CancelGenerate
    {
        // Add the input delegate to the collection.
        add
        {
            listEventDelegates.AddHandler(CancelGenerateEvent.key, value);
        }
        // Remove the input delegate from the collection.
        remove
        {
            listEventDelegates.RemoveHandler(CancelGenerateEvent.key, value);
        }
    }
    public void Raise<T>(T _param) where T : EventParam
    {
        if (listEventDelegates[_param.Key()] == null)
            Debug.Log(typeof(T));
        ((Event<T>)listEventDelegates[_param.Key()])?.Invoke(_param);
    }

}