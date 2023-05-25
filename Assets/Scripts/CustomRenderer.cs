using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRenderer : MonoBehaviour
{

    public List<MeshRenderer> ownMeshRenderers;
    public Dictionary<Slot,FocusHandler> slots;
    public List<FocusHandler> children;

    public bool isActive = false;
    public bool isSelected = false;
    public bool isFocused = false;
    public bool isHovered = false;
    public bool isHighlighted = false;

    private void Start()
    {
        if (GameManager.instance.allItems.ContainsValue(gameObject))
        {
            isActive = true;
            SubscribeEvents();
        }
    }

    private void OnDestroy()
    {
        if (isActive)
            UnsubscribeEvents();
    }

    ///<summary>
    /// Subscribe the GameObject to Events
    ///</summary>
    public void SubscribeEvents()
    {
        EventManager.instance.AddListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.instance.AddListener<EditModeOutEvent>(OnEditModeOut);

        EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinished);

        EventManager.instance.AddListener<TemperatureDiagramEvent>(OnTemperatureDiagram);
        EventManager.instance.AddListener<TemperatureScatterPlotEvent>(OnTemperatureScatterPlot);
        EventManager.instance.AddListener<TemperatureColorEvent>(OnTemperatureColorEvent);

        EventManager.instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.instance.AddListener<HighlightEvent>(ToggleHighlight);
    }

    ///<summary>
    /// Unsubscribe the GameObject to Events
    ///</summary>
    public void UnsubscribeEvents()
    {
        EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.instance.RemoveListener<OnFocusEvent>(OnFocusItem);
        EventManager.instance.RemoveListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.instance.RemoveListener<EditModeOutEvent>(OnEditModeOut);

        EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);

        EventManager.instance.RemoveListener<TemperatureDiagramEvent>(OnTemperatureDiagram);
        EventManager.instance.RemoveListener<TemperatureScatterPlotEvent>(OnTemperatureScatterPlot);
        EventManager.instance.RemoveListener<TemperatureColorEvent>(OnTemperatureColorEvent);

        EventManager.instance.RemoveListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.instance.RemoveListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.instance.RemoveListener<HighlightEvent>(ToggleHighlight);
    }

    private void OnSelectItem(OnSelectItemEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnFocusItem(OnFocusEvent _e)
    {
        if (_e.obj == gameObject)
        {
            isFocused = true;

            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            UpdateChildMeshRenderers(true, true);
            UpdateOtherObjectsMeshRenderers(false);
            ToggleCollider(gameObject, false);
            GetComponent<DisplayObjectData>()?.ToggleLabel(false);
            SetMaterial(GameManager.instance.focusMat);
        }
    }

    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnEditModeIn(EditModeInEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnEditModeOut(EditModeOutEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnImportFinished(ImportFinishedEvent _e)
    {
        Init();
    }

    private void OnTemperatureDiagram(TemperatureDiagramEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnTemperatureScatterPlot(TemperatureScatterPlotEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnTemperatureColorEvent(TemperatureColorEvent _e)
    {
        throw new NotImplementedException();
    }

    private void OnMouseHover(OnMouseHoverEvent _e)
    {
        throw new NotImplementedException();
    }
    private void OnMouseUnHover(OnMouseUnHoverEvent _e)
    {
        throw new NotImplementedException();
    }

    private void ToggleHighlight(HighlightEvent _e)
    {
        throw new NotImplementedException();
    }

    ///<summary>
    /// Initialise the renderer and gameobject lists of this and all children recursively
    /////</summary>
    public void Init()
    {
        foreach (Transform child in transform)
        {
            FocusHandler focusHandler = child.GetComponent<FocusHandler>();
            if (!focusHandler)
            {
                ownMeshRenderers.AddRange(child.GetComponentsInChildren<MeshRenderer>());
                continue;
            }

            Slot slot = child.GetComponent<Slot>();
            if (slot)
            {
                slots.Add(slot,focusHandler);
                continue;
            }
            if ((child.GetComponent<Sensor>()?.fromTemplate) != true)
                children.Add(focusHandler); 
        }
    }

    ///<summary>
    /// Assign a Material to given Renderer keeping textures.
    ///</summary>
    ///<param name="_renderer">The Renderer of the object to modify</param>
    ///<param name="_newMat">The Material to assign</param>
    private void SetMaterial(Material _newMat)
    {
        Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
        Material mat = renderer.material;
        renderer.material = Instantiate(_newMat);

        renderer.material.SetTexture("_BaseMap", mat.GetTexture("_BaseMap"));
        renderer.material.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
        renderer.material.SetTexture("_MetallicGlossMap", mat.GetTexture("_MetallicGlossMap"));
        renderer.material.SetTexture("_OcclusionMap", mat.GetTexture("_OcclusionMap"));

        if (GetComponent<OObject>().isHidden)
        {
            transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            GetComponent<DisplayObjectData>()?.ToggleLabel(false);
        }
    }

    ///<summary>
    /// Toggle renderer of all items which are not in ogreeChildObjects.
    ///</summary>
    ///<param name="_value">The value to give to all MeshRenderer</param>
    private void UpdateOtherObjectsMeshRenderers(bool _value)
    {
        foreach (DictionaryEntry de in GameManager.instance.allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (!ogreeChildObjects.Contains(go) && go != gameObject)
            {
                switch (go.GetComponent<OgreeObject>().category)
                {
                    case "domain":
                        break;
                    case "site":
                        break;
                    case "building":
                        Building bd = go.GetComponent<Building>();
                        bd.transform.GetChild(0).GetComponent<Renderer>().enabled = _value;
                        bd.transform.GetChild(0).GetComponent<Collider>().enabled = _value;
                        bd.nameText.GetComponent<Renderer>().enabled = _value;
                        foreach (Transform wall in bd.walls)
                        {
                            wall.GetComponent<Renderer>().enabled = _value;
                            wall.GetComponent<Collider>().enabled = _value;
                        }
                        break;
                    case "room":
                        Room ro = go.GetComponent<Room>();
                        if (ro.usableZone)
                        {
                            ro.usableZone.GetComponent<Renderer>().enabled = _value;
                            ro.usableZone.GetComponent<Collider>().enabled = _value;
                        }
                        if (ro.reservedZone)
                        {
                            ro.reservedZone.GetComponent<Renderer>().enabled = _value;
                            ro.reservedZone.GetComponent<Collider>().enabled = _value;
                        }
                        if (ro.technicalZone)
                        {
                            ro.technicalZone.GetComponent<Renderer>().enabled = _value;
                            ro.technicalZone.GetComponent<Collider>().enabled = _value;
                        }
                        if (ro.tilesGrid)
                        {
                            ro.tilesGrid.GetComponent<Renderer>().enabled = _value;
                            ro.tilesGrid.GetComponent<Collider>().enabled = _value;
                        }
                        ro.nameText.GetComponent<Renderer>().enabled = _value;
                        foreach (Transform wall in ro.walls)
                        {
                            wall.GetComponentInChildren<Renderer>().enabled = _value;
                            wall.GetComponentInChildren<Collider>().enabled = _value;
                        }
                        if (go.transform.Find("tilesNameRoot"))
                        {
                            foreach (Transform child in go.transform.Find("tilesNameRoot"))
                                child.GetComponent<Renderer>().enabled = _value;
                        }
                        if (go.transform.Find("tilesColorRoot"))
                        {
                            foreach (Transform child in go.transform.Find("tilesColorRoot"))
                                child.GetComponent<Renderer>().enabled = _value;
                        }
                        break;
                    case "rack":
                        go.GetComponent<FocusHandler>().UpdateOwnMeshRenderers(_value);
                        go.GetComponent<OObject>()?.ToggleSlots(false);
                        break;
                    case "device":
                        go.GetComponent<FocusHandler>().UpdateOwnMeshRenderers(false);
                        break;
                    case "group":
                        if (go.GetComponent<Group>().isDisplayed)
                            go.GetComponent<FocusHandler>().UpdateOwnMeshRenderers(_value);
                        break;
                    case "corridor":
                        go.GetComponent<FocusHandler>().UpdateOwnMeshRenderers(_value);
                        break;
                    default:
                        go.transform.GetChild(0).GetComponent<Renderer>().enabled = _value;
                        break;
                }
            }
        }
    }
}
