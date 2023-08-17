using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public static EventManager instance
    {
        get
        {
            if (eventManagerInstance == null)
            {
                eventManagerInstance = new EventManager();
            }

            return eventManagerInstance;
        }
    }
    private static EventManager eventManagerInstance = null;


    public delegate void EventDelegate<T>(T _eventParam) where T : CustomEvent;

    public class EventWrap<T> where T : CustomEvent
    {
        private readonly HashSet<EventDelegate<T>> handlers = new HashSet<EventDelegate<T>>();
        private event EventDelegate<T> EventDelegate
        {
            add => handlers.Add(value);
            remove => handlers.Remove(value);
        }
        public void Add(EventDelegate<T> _event)
        {
            EventDelegate += _event;
        }
        public void Remove(EventDelegate<T> _event)
        {
            EventDelegate -= _event;
        }
        public void Invoke(T _param)
        {
            foreach (var handler in handlers)
                handler(_param);
        }
    }

    public EventWrap<OnFocusEvent> OnFocus = new EventWrap<OnFocusEvent>();
    public EventWrap<OnUnFocusEvent> OnUnFocus = new EventWrap<OnUnFocusEvent>();
    public EventWrap<OnSelectItemEvent> OnSelectItem = new EventWrap<OnSelectItemEvent>();
    public EventWrap<OnMouseHoverEvent> OnMouseHover = new EventWrap<OnMouseHoverEvent>();
    public EventWrap<OnMouseUnHoverEvent> OnMouseUnHover = new EventWrap<OnMouseUnHoverEvent>();
    public EventWrap<HighlightEvent> Highlight = new EventWrap<HighlightEvent>();
    public EventWrap<ImportFinishedEvent> ImportFinished = new EventWrap<ImportFinishedEvent>();
    public EventWrap<ChangeCursorEvent> ChangeCursor = new EventWrap<ChangeCursorEvent>();
    public EventWrap<UpdateDomainEvent> UpdateDomain = new EventWrap<UpdateDomainEvent>();
    public EventWrap<SwitchLabelEvent> SwitchLabel = new EventWrap<SwitchLabelEvent>();
    public EventWrap<EditModeInEvent> EditModeIn = new EventWrap<EditModeInEvent>();
    public EventWrap<EditModeOutEvent> EditModeOut = new EventWrap<EditModeOutEvent>();
    public EventWrap<ConnectApiEvent> ConnectApi = new EventWrap<ConnectApiEvent>();
    public EventWrap<TemperatureDiagramEvent> TemperatureDiagram = new EventWrap<TemperatureDiagramEvent>();
    public EventWrap<TemperatureColorEvent> TemperatureColor = new EventWrap<TemperatureColorEvent>();
    public EventWrap<TemperatureScatterPlotEvent> TemperatureScatterPlot = new EventWrap<TemperatureScatterPlotEvent>();
    public EventWrap<RightClickEvent> RightClick = new EventWrap<RightClickEvent>();
    public EventWrap<CancelGenerateEvent> CancelGenerate = new EventWrap<CancelGenerateEvent>();

    /// <summary>
    /// Raise the event to all the listeners
    /// </summary>
    public void Raise<T>(T _param) where T : CustomEvent
    {
        switch (_param)
        {
            case OnFocusEvent e:
                OnFocus.Invoke(e);
                break;
            case OnUnFocusEvent e:
                OnUnFocus.Invoke(e);
                break;
            case OnSelectItemEvent e:
                OnSelectItem.Invoke(e);
                break;
            case OnMouseHoverEvent e:
                OnMouseHover.Invoke(e);
                break;
            case OnMouseUnHoverEvent e:
                OnMouseUnHover.Invoke(e);
                break;
            case HighlightEvent e:
                Highlight.Invoke(e);
                break;
            case ImportFinishedEvent e:
                ImportFinished.Invoke(e);
                break;
            case ChangeCursorEvent e:
                ChangeCursor.Invoke(e);
                break;
            case UpdateDomainEvent e:
                UpdateDomain.Invoke(e);
                break;
            case SwitchLabelEvent e:
                SwitchLabel.Invoke(e);
                break;
            case EditModeInEvent e:
                EditModeIn.Invoke(e);
                break;
            case EditModeOutEvent e:
                EditModeOut.Invoke(e);
                break;
            case ConnectApiEvent e:
                ConnectApi.Invoke(e);
                break;
            case TemperatureDiagramEvent e:
                TemperatureDiagram.Invoke(e);
                break;
            case TemperatureColorEvent e:
                TemperatureColor.Invoke(e);
                break;
            case TemperatureScatterPlotEvent e:
                TemperatureScatterPlot.Invoke(e);
                break;
            case RightClickEvent e:
                RightClick.Invoke(e);
                break;
            case CancelGenerateEvent e:
                CancelGenerate.Invoke(e);
                break;
            default:
                Debug.LogError($"UNKNOWN EVENT :{typeof(T)}");
                break;
        }
    }
}