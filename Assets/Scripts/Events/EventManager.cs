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


    public delegate void Event<T>(T _eventParam) where T : CustomEvent;
    public event Event<OnFocusEvent> OnFocus;
    public event Event<OnUnFocusEvent> OnUnFocus;
    public event Event<OnSelectItemEvent> OnSelectItem;
    public event Event<OnMouseHoverEvent> OnMouseHover;
    public event Event<OnMouseUnHoverEvent> OnMouseUnHover;
    public event Event<HighlightEvent> Highlight;
    public event Event<ImportFinishedEvent> ImportFinished;
    public event Event<ChangeCursorEvent> ChangeCursor;
    public event Event<UpdateDomainEvent> UpdateDomain;
    public event Event<SwitchLabelEvent> SwitchLabel;
    public event Event<EditModeInEvent> EditModeIn;
    public event Event<EditModeOutEvent> EditModeOut;
    public event Event<ConnectApiEvent> ConnectApi;
    public event Event<TemperatureDiagramEvent> TemperatureDiagram;
    public event Event<TemperatureColorEvent> TemperatureColor;
    public event Event<TemperatureScatterPlotEvent> TemperatureScatterPlot;
    public event Event<RightClickEvent> RightClick;
    public event Event<CancelGenerateEvent> CancelGenerate;

    /// <summary>
    /// Raise the event to all the listeners
    /// </summary>
    public void Raise<T>(T _param) where T : CustomEvent
    {
        switch (_param)
        {
            case OnFocusEvent e:
                OnFocus?.Invoke(e);
                break;
            case OnUnFocusEvent e:
                OnUnFocus?.Invoke(e);
                break;
            case OnSelectItemEvent e:
                OnSelectItem?.Invoke(e);
                break;
            case OnMouseHoverEvent e:
                OnMouseHover?.Invoke(e);
                break;
            case OnMouseUnHoverEvent e: 
                OnMouseUnHover?.Invoke(e); 
                break;
            case HighlightEvent e: 
                Highlight?.Invoke(e); 
                break;
            case ImportFinishedEvent e: 
                ImportFinished?.Invoke(e); 
                break;
            case ChangeCursorEvent e: 
                ChangeCursor?.Invoke(e); 
                break;
            case UpdateDomainEvent e: 
                UpdateDomain?.Invoke(e); 
                break;
            case SwitchLabelEvent e: 
                SwitchLabel?.Invoke(e); 
                break;
            case EditModeInEvent e: 
                EditModeIn?.Invoke(e); 
                break;
            case EditModeOutEvent e: 
                EditModeOut?.Invoke(e); 
                break;
            case ConnectApiEvent e: 
                ConnectApi?.Invoke(e); 
                break;
            case TemperatureDiagramEvent e: 
                TemperatureDiagram?.Invoke(e); 
                break;
            case TemperatureColorEvent e: 
                TemperatureColor?.Invoke(e); 
                break;
            case TemperatureScatterPlotEvent e: 
                TemperatureScatterPlot?.Invoke(e); 
                break;
            case RightClickEvent e: 
                RightClick?.Invoke(e); 
                break;
            case CancelGenerateEvent e: 
                CancelGenerate?.Invoke(e); 
                break;
            default: 
                Debug.LogError($"UNKNOWN EVENT :{typeof(T)}"); 
                break;
        }
    }
}