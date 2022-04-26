using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTracker : MonoBehaviour
{

    public List<string> onSelectItemEvents;
    public List<string> onDeselectItemEvents;
    public List<string> onFocusEvents;
    public List<string> onUnFocusEvents;
    public List<string> onMouseHoverEvents;
    public List<string> onMouseUnHoverEvents;
    public List<string> highlightEvents;
    public List<string> changeCursorEvents;
    public List<string> updateTenantEvents;
    public List<string> importFinishedEvents;

    // Start is called before the first frame update
    void Start()
    {
        SubscribeEvents();
    }

    public void SubscribeEvents()
    {

        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.Instance.AddListener<HighlightEvent>(OnHighlightEvent);
        EventManager.Instance.AddListener<ChangeCursorEvent>(OnChangeCursorEvent);
        EventManager.Instance.AddListener<UpdateTenantEvent>(OnUpdateTenantEvent);
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    public void OnSelectItem(OnSelectItemEvent e)
    {
        onSelectItemEvents.Add(e.obj.ToString());
    }

    public void OnDeselectItem(OnDeselectItemEvent e)
    {
        onDeselectItemEvents.Add(e.obj.ToString());
    }

    public void OnFocusItem(OnFocusEvent e)
    {
        onFocusEvents.Add(e.obj.ToString());
    }

    public void OnUnFocusItem(OnUnFocusEvent e)
    {
        onUnFocusEvents.Add(e.obj.ToString());
    }

    public void OnMouseHover(OnMouseHoverEvent e)
    {
        onMouseHoverEvents.Add(e.obj.ToString());
    }

    public void OnMouseUnHover(OnMouseUnHoverEvent e)
    {
        onMouseUnHoverEvents.Add(e.obj.ToString());
    }

    public void OnImportFinished(ImportFinishedEvent e)
    {
        importFinishedEvents.Add(e.ToString());
    }

    public void OnHighlightEvent(HighlightEvent e)
    {
        highlightEvents.Add(e.obj.ToString());
    }

    public void OnChangeCursorEvent(ChangeCursorEvent e)
    {
        changeCursorEvents.Add(e.type.ToString());
    }
    public void OnUpdateTenantEvent(UpdateTenantEvent e)
    {
        updateTenantEvents.Add(e.name.ToString());
    }
}
