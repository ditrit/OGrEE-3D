using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDisplayController : MonoBehaviour
{
    public bool isTemplate = false;

    /// <summary>
    /// the collider and the renderer of the first child of this gameobject (the box)
    /// </summary>
    private (Collider col, MeshRenderer rend) cube;
    public bool Shown { get => cube.rend.enabled; }
    private DisplayObjectData displayObjectData;
    private Item item;
    private Sensor sensor;
    private Slot slot;
    private Group group;

    /// <summary>
    /// If this object is an oobject and is a referent
    /// </summary>
    private bool isReferent;

    /// <summary>
    /// This bool is on if a parent of this object has its scatter plot enabled, not if this one's scatter plot is enabled
    /// </summary>
    public bool scatterPlotOfOneParent = false;
#pragma warning disable IDE1006 // Styles d'affectation de noms
    /// <summary>
    /// Check if the object should be listening to events, ie if it is an oobject or a sensor not from a template or an unused slot
    /// </summary>
    private bool listening => item || (sensor && !sensor.fromTemplate) || (slot && !slot.used);
#pragma warning restore IDE1006 // Styles d'affectation de noms

    public bool isHidden = false;
    public bool isHiddenInGroup = false;

    private bool isHovered = false;
    public bool isHighlighted = false;
    private GameObject highlightCube;
    private Color highlightColor;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        ObjectDisplayController customRendererParent = transform.parent?.GetComponent<ObjectDisplayController>();
        OgreeObject ogreeObjectParent = transform.parent?.GetComponent<OgreeObject>();
        scatterPlotOfOneParent = customRendererParent && customRendererParent.scatterPlotOfOneParent || ogreeObjectParent && ogreeObjectParent.scatterPlot;
        item = GetComponent<Item>();
        isReferent = item && item.referent == item;
        slot = GetComponent<Slot>();
        sensor = GetComponent<Sensor>();
        group = GetComponent<Group>();
        if (!isTemplate)
        {
            EventManager.instance.Highlight.Add(OnHighLight);
            SubscribeEvents();
        }
        if (scatterPlotOfOneParent && sensor && sensor.fromTemplate)
            Display(true, true);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        EventManager.instance.Highlight.Remove(OnHighLight);
    }

    ///<summary>
    /// Subscribe the GameObject to Events. Subscribe to different events with different function whether it's a sensor, a slot, a group or any other oobject
    ///</summary>
    public void SubscribeEvents()
    {
        if (slot || sensor)
        {
            EventManager.instance.OnSelectItem.Add(OnSelectOther);
            EventManager.instance.ImportFinished.Add(OnImportFinishedOther);
        }
        else
        {
            EventManager.instance.OnSelectItem.Add(OnSelectBasic);

            EventManager.instance.EditModeIn.Add(OnEditModeInBasic);
            EventManager.instance.EditModeOut.Add(OnEditModeOutBasic);

            if (group)
            {
                EventManager.instance.ImportFinished.Add(OnImportFinishedGroup);
                ObjectDisplayController customRendererParent = transform.parent?.GetComponent<ObjectDisplayController>();
                OgreeObject ogreeObjectParent = transform.parent?.GetComponent<OgreeObject>();
                scatterPlotOfOneParent = customRendererParent && customRendererParent.scatterPlotOfOneParent || ogreeObjectParent && ogreeObjectParent.scatterPlot;
                Display(!scatterPlotOfOneParent, !scatterPlotOfOneParent, !scatterPlotOfOneParent);
            }
            else
                EventManager.instance.ImportFinished.Add(OnImportFinishedBasic);

            EventManager.instance.TemperatureColor.Add(OnTemperatureColorEvent);

            EventManager.instance.OnMouseHover.Add(OnMouseHover);
            EventManager.instance.OnMouseUnHover.Add(OnMouseUnHover);
        }
        EventManager.instance.OnFocus.Add(OnFocus);
        EventManager.instance.OnUnFocus.Add(OnUnFocus);
        EventManager.instance.TemperatureDiagram.Add(OnTemperatureDiagram);
        EventManager.instance.TemperatureScatterPlot.Add(OnTemperatureScatterPlot);
        EventManager.instance.GetCoordModeToggle.Add(OnGetCoordModeToggle);
    }

    ///<summary>
    /// Unsubscribe the GameObject to the event it subscribed when initialised
    ///</summary>
    public void UnsubscribeEvents()
    {
        if (slot || sensor)
        {
            EventManager.instance.OnSelectItem.Remove(OnSelectOther);
            EventManager.instance.ImportFinished.Remove(OnImportFinishedOther);
        }
        else
        {
            EventManager.instance.OnSelectItem.Remove(OnSelectBasic);

            EventManager.instance.EditModeIn.Remove(OnEditModeInBasic);
            EventManager.instance.EditModeOut.Remove(OnEditModeOutBasic);

            if (group)
                EventManager.instance.ImportFinished.Remove(OnImportFinishedGroup);
            else
                EventManager.instance.ImportFinished.Remove(OnImportFinishedBasic);

            EventManager.instance.TemperatureColor.Remove(OnTemperatureColorEvent);

            EventManager.instance.OnMouseHover.Remove(OnMouseHover);
            EventManager.instance.OnMouseUnHover.Remove(OnMouseUnHover);
        }
        EventManager.instance.OnFocus.Remove(OnFocus);
        EventManager.instance.OnUnFocus.Remove(OnUnFocus);
        EventManager.instance.TemperatureDiagram.Remove(OnTemperatureDiagram);
        EventManager.instance.TemperatureScatterPlot.Remove(OnTemperatureScatterPlot);
        EventManager.instance.GetCoordModeToggle.Remove(OnGetCoordModeToggle);
    }

    /// <summary>
    /// When selected, toggles the renderer, the collider and the labels of the object depending on if it is selected, focused, a referent, its parent is selected, if focus mode is on and if it has a tempBar or scatterplot enabled, then call <see cref="HandleMaterial"/>
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnSelectBasic(OnSelectItemEvent _e)
    {
        List<Item> selectionrefs = GameManager.instance.GetSelectedReferents();
        if (!item.tempBar && !scatterPlotOfOneParent)
        {
            List<GameObject> selection = GameManager.instance.GetSelected();
            bool isSelected = selection.Contains(gameObject);
            bool colAndLabels = !isHidden && ((isReferent && !(selectionrefs.Contains(item) || GameManager.instance.focusMode)) || selection.Contains(transform.parent?.gameObject));
            bool rend = !isHidden && (isSelected || colAndLabels);

            Display(rend, colAndLabels, colAndLabels);

            if (isSelected)
            {
                SetMaterial(GameManager.instance.GetFocused().Contains(gameObject) ? GameManager.instance.focusMat : GameManager.instance.selectMat);
                return;
            }
        }
        if (!GameManager.instance.GetPrevious().Contains(gameObject))
            return;

        if (GameManager.instance.GetFocused().Contains(gameObject))
            SetMaterial(GameManager.instance.focusMat);
        else
            HandleMaterial();

    }

    /// <summary>
    /// Whenn seletec, toggles the renderer, the collider and the labels of the object depending on if it is listening, has a scatterPlot enabled, has its parent selected
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnSelectOther(OnSelectItemEvent _e)
    {
        if (!listening || scatterPlotOfOneParent)
            return;

        if (GameManager.instance.GetSelected().Contains(transform.parent?.gameObject))
            Display(true, true);
        else
            Display(false, false);
    }

    /// <summary>
    /// When called, call <see cref="ToggleRoomsAndBuildings"/> and set its material to <see cref="GameManager.focusMat"/> if it is the object being focused, else toggles its renderer, labels and collider depending on if it is a refent,a sensor from a template, if its parent is focused, and if it is in focus
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnFocus(OnFocusEvent _e)
    {
        if (_e.obj == gameObject)
        {
            ToggleRoomsAndBuildings(false);
            SetMaterial(GameManager.instance.focusMat);
        }
        else if (_e.obj.transform != transform.parent)
        {
            if (item is Rack rack && rack.uRoot && !GameManager.instance.GetSelectedReferents().Contains(item))
                rack.uRoot.gameObject.SetActive(false);
            if ((!sensor || !sensor.fromTemplate || scatterPlotOfOneParent) && !DistantChildOf(_e.obj))
                Display(false, false, false);
        }
    }

    /// <summary>
    /// Whenn called, call <see cref="ToggleRoomsAndBuildings"/> and <see cref="HandleMaterial"/> if it is the object being focused, else toggles its renderer, labels and collider depending on if it is a refent,a sensor from a template, if its parent is focused, and if it has a scatterPlot enabled
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnUnFocus(OnUnFocusEvent _e)
    {
        if (!listening)
            return;
        if (_e.obj == gameObject)
        {
            item?.ResetTransform();
            ToggleRoomsAndBuildings(true);
            if (GameManager.instance.GetSelected().Contains(gameObject))
                SetMaterial(GameManager.instance.selectMat);
            else
                HandleMaterial();
        }
        else if (_e.obj == transform.parent.gameObject)
            item?.ResetTransform();
        else if ((sensor && sensor.fromTemplate && scatterPlotOfOneParent)
            || (isReferent && !GameManager.instance.GetSelectedReferents().Contains(item) && !isHidden))
            Display(true, true, true);
        else if (isHighlighted && !isHidden)
            highlightCube?.SetActive(true);
        if (item is Rack rack && rack.uRoot && rack.uRoot.gameObject.activeSelf != rack.areUHelpersToggled)
            rack.uRoot.gameObject.SetActive(rack.areUHelpersToggled);
    }

    /// <summary>
    /// When called, disables its collider and set its material to <see cref="GameManager.editMat"/> if it is the object being edited in
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnEditModeInBasic(EditModeInEvent _e)
    {
        if (GameManager.instance.GetSelected().Contains(gameObject))
        {
            cube.col.enabled = true;
            SetMaterial(GameManager.instance.editMat);
        }
    }

    /// <summary>
    /// When called, enables its collider and set its material to <see cref="GameManager.focusMat"/> if it is the object being edited out
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnEditModeOutBasic(EditModeOutEvent _e)
    {
        if (GameManager.instance.GetSelected().Contains(gameObject))
        {
            cube.col.enabled = false;
            SetMaterial(GameManager.instance.focusMat);
        }
    }

    /// <summary>
    /// When called, toggles its renderer, labels and collider depending on if it is selected, has a tempBar or a scatterPlot enabled, has its parent selected or is a referent then call <see cref="HandleMaterial"/>
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnImportFinishedBasic(ImportFinishedEvent _e)
    {
        List<GameObject> selection = GameManager.instance.GetSelected();

        if (selection.Contains(gameObject) || item.tempBar)
            return;
        if (scatterPlotOfOneParent)
        {
            Display(false, false, false);
            return;
        }
        List<Item> selectionrefs = GameManager.instance.GetSelectedReferents();
        bool rendAndCol = !isHidden && ((isReferent && !selectionrefs.Contains(item) && !GameManager.instance.focusMode) || selection.Contains(transform.parent?.gameObject));
        bool hideLabels = item is GenericObject && item.transform.parent?.GetComponent<Room>() is Room room && !room.genNamesDisplayed;
        Display(rendAndCol, !hideLabels, rendAndCol);

        foreach (string tagName in item.tags)
        {
            if (GameManager.instance.GetTag(tagName) is Tag tag && tag.objHightlighted)
            {
                isHighlighted = true;
                highlightColor = tag.color;
            }
        }

        HandleMaterial();
    }

    /// <summary>
    /// When called, toggles its renderer, labels and collider depending of if it is selected, if is is displayed, if it is a referent or if focusMode is on
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnImportFinishedGroup(ImportFinishedEvent _e)
    {
        List<GameObject> selection = GameManager.instance.GetSelected();

        if (selection.Contains(gameObject))
            return;
        if (!group.isDisplayed || scatterPlotOfOneParent)
        {
            Display(false, false, false);
            return;
        }
        List<Item> selectionrefs = GameManager.instance.GetSelectedReferents();
        bool RendColAndLabels = (isReferent && !selectionrefs.Contains(item) && !GameManager.instance.focusMode) || selection.Contains(transform.parent?.gameObject);
        Display(RendColAndLabels, RendColAndLabels, RendColAndLabels);
        if (isHighlighted)
            HighlightObject();
        else
        {
            SetMaterial(GameManager.instance.defaultMat);
            cube.rend.material.color = item.color;
        }
    }

    /// <summary>
    /// When called, toggles its renderer and labels depending on if it is a sensor from template, if it is listening, on scatterplotEnabled, or if its parent is selected
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnImportFinishedOther(ImportFinishedEvent _e)
    {
        if (sensor && sensor.fromTemplate && scatterPlotOfOneParent && (!GameManager.instance.focusMode || DistantChildOf(GameManager.instance.GetFocused()[^1])))
        {
            Display(true, true);
            return;
        }
        if (!listening || scatterPlotOfOneParent)
        {
            Display(false, false);
            return;
        }
        if (GameManager.instance.GetSelected().Contains(transform.parent?.gameObject))
            Display(true, true);
        else
            Display(false, false);
    }

    /// <summary>
    /// When called, toggles its renderer, labels and collider depending on if it is listening, if it has a referent, if it is in the room where the bar chart is, if it is selected, if it is a referent or if it is a slot or a sensor
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnTemperatureDiagram(TemperatureDiagramEvent _e)
    {
        if (sensor && sensor.fromTemplate && scatterPlotOfOneParent)
            Display(_e.room.barChart, _e.room.barChart);
        if (!listening)
            return;

        Item extendedReferent = item ? item.referent : transform.parent?.GetComponent<Item>().referent;

        if (!extendedReferent || _e.room.transform != extendedReferent.transform.parent)
            return;

        List<GameObject> selection = GameManager.instance.GetSelected();
        bool labels = (isReferent && !GameManager.instance.GetSelectedReferents().Contains(item)) || (transform.parent && !scatterPlotOfOneParent && selection.Contains(transform.parent.gameObject));
        bool rend = labels || selection.Contains(gameObject);
        bool col = labels && !slot && !sensor;
        Display(_e.room.barChart && rend, _e.room.barChart && labels, _e.room.barChart && col);
        if (item is Rack rack && rack.uRoot)
            rack.uRoot.gameObject.SetActive(_e.room.barChart && rack.areUHelpersToggled);
        if (item && item.clearanceHandler != null && item.clearanceHandler.clearanceWrapper)
            item.clearanceHandler.clearanceWrapper.SetActive(_e.room.barChart);
    }

    /// <summary>
    /// When called, call <see cref="HandleMaterial"/> if it is the object whose scatter plot is toggled and not selected or focused, else call <see cref="ScatterPlotToggle(bool)"/> if its parent is
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnTemperatureScatterPlot(TemperatureScatterPlotEvent _e)
    {
        if (sensor && sensor.fromTemplate && transform.parent == _e.ogreeObject.transform)
            scatterPlotOfOneParent = _e.ogreeObject.scatterPlot;
        if (!listening)
            return;

        if (gameObject == _e.ogreeObject.gameObject && !GameManager.instance.GetSelected().Contains(gameObject) && !GameManager.instance.GetFocused().Contains(gameObject))
            HandleMaterial();
        else if (transform.parent == _e.ogreeObject.transform)
            ScatterPlotToggle(_e.ogreeObject.scatterPlot);
    }

    /// <summary>
    /// When called, call <see cref="HandleMaterial"/> if it is not selected or focused
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnTemperatureColorEvent(TemperatureColorEvent _e)
    {
        if (GameManager.instance.GetSelected().Contains(gameObject) || GameManager.instance.GetFocused().Contains(gameObject) || isHighlighted)
            return;
        HandleMaterial();
    }

    /// <summary>
    /// When called, invert this object's color if it is the object hovered and not selected
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnMouseHover(OnMouseHoverEvent _e)
    {
        if (_e.obj == gameObject && !GameManager.instance.GetSelected().Contains(gameObject))
        {
            Color temp = cube.rend.material.color;
            cube.rend.material.color = Utils.InvertColor(temp);
            isHovered = true;
        }
    }

    /// <summary>
    /// When called, call <see cref="HandleMaterial"/> if it the object being unhovered and it is not selected
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnMouseUnHover(OnMouseUnHoverEvent _e)
    {
        if (_e.obj == gameObject && !GameManager.instance.GetSelected().Contains(gameObject))
        {
            HandleMaterial();
            isHovered = false;
        }
    }

    /// <summary>
    /// When called, call <see cref="ToggleHighlight(bool)"/> if it the object being highlighted
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnHighLight(HighlightEvent _e)
    {
        highlightColor = _e.color;
        if (_e.obj == gameObject)
            ToggleHighlight(!isHighlighted);
    }

    /// <summary>
    /// When called, set <see cref="cube.col"/> according to <see cref="GameManager.getCoordsMode"/> 
    /// </summary>
    /// <param name="_e">The event's instance</param>
    private void OnGetCoordModeToggle(GetCoordModeToggleEvent _e)
    {
        //cube.col.enabled = !GameManager.instance.getCoordsMode;
        if (GameManager.instance.getCoordsMode)
            cube.col.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        else
            cube.col.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    ///<summary>
    /// Setup collider and renderer
    ///</summary>
    public void Initialize()
    {
        item = GetComponent<Item>();
        cube = (transform.GetChild(0).GetComponent<Collider>(), transform.GetChild(0).GetComponent<MeshRenderer>());
        displayObjectData = GetComponent<DisplayObjectData>();
    }

    /// <summary>
    /// Call itself from its parent CustomRenderer if it has one, change the boolean <see cref="isHighlighted"/> then call <see cref="HandleMaterial"/>
    /// </summary>
    /// <param name="_highlighted"></param>
    private void ToggleHighlight(bool _highlighted)
    {
        isHighlighted = _highlighted;
        if (GameManager.instance.GetSelected().Contains(gameObject))
            return;
        HandleMaterial();
    }

    /// <summary>
    /// Handles the activation/deactivation of its renderer, collider and labels and its material wether <paramref name="_scatterPlot"/> is true or false then call itself from all of its children who have a CustomRenderer
    /// </summary>
    /// <param name="_scatterPlot">the state of the scatter plot of one of its parents</param>
    private void ScatterPlotToggle(bool _scatterPlot)
    {
        scatterPlotOfOneParent = _scatterPlot;
        if (!listening)
            return;
        if (_scatterPlot)
        {
            Display(false, false, false);
        }
        else if (GameManager.instance.GetSelected().Contains(gameObject))
        {
            if (item && item.referent)
                item.scatterPlot = false;
            Display(true, false, false);
            UHelpersManager.instance.ToggleU(gameObject, true);
        }
        else if ((isReferent && !GameManager.instance.GetSelectedReferents().Contains(item)) || GameManager.instance.GetSelected().Contains(transform.parent?.gameObject))
        {
            Display(true, true, !slot && !sensor);
            if (item)
            {
                item.scatterPlot = false;
                HandleMaterial();
            }
        }
        if (item is Rack rack && rack.uRoot)
            rack.uRoot.gameObject.SetActive(!_scatterPlot && rack.areUHelpersToggled);
        if (item && item.clearanceHandler != null && item.clearanceHandler.clearanceWrapper)
            item.clearanceHandler.clearanceWrapper.SetActive(!_scatterPlot);
        foreach (Transform child in transform)
            child.GetComponent<ObjectDisplayController>()?.ScatterPlotToggle(_scatterPlot);
    }

    /// <summary>
    /// Assign the right material depending on the state in this order :
    /// <br/> - <b>if the scatter plot of the object is enabled (not the boolean <see cref="scatterPlotOfOneParent"/> of this script)</b> : <see cref="GameManager.scatterPlotMat"/>
    /// <br/> - <b>if this object is highlighted</b> : <see cref="GameManager.highlightMat"/>
    /// <br/> - <b>if temp color mode is activated and this object is an oobject but is not a group or a corridor</b> : <see cref="GetTemperatureMaterial"/>
    /// <br/> - <b>if this object is a corridor</b> : <see cref="GameManager.alphaMat"/> with the color of the object
    /// <br/> - <see cref="GameManager.defaultMat"/> with the color of the object
    /// </summary>
    public void HandleMaterial()
    {
        if (item && item.scatterPlot)
            SetMaterial(GameManager.instance.scatterPlotMat);
        else if (isHighlighted)
            HighlightObject();
        else if (GameManager.instance.tempColorMode && !group && item && item.category != Category.Corridor)
            SetMaterial(GetTemperatureMaterial());
        else
        {
            if (item && item.category == Category.Corridor)
                SetMaterial(GameManager.instance.alphaMat);
            else
                SetMaterial(GameManager.instance.defaultMat);
            cube.rend.material.color = item.color;
        }
    }

    /// <summary>
    /// Set material to <see cref="GameManager.highlightMat"/>, generate or activate <see cref="highlightCube"/> and change their color using <see cref="highlightColor"/> if needed.
    /// </summary>
    private void HighlightObject()
    {
        SetMaterial(GameManager.instance.highlightMat);
        if (!highlightCube)
        {
            highlightCube = Instantiate(GameManager.instance.highlightCubeModel, transform.GetChild(0));
            highlightCube.transform.localScale = Vector3.one * 1.1f;
        }
        highlightCube.SetActive(GameManager.instance.focusMode ? cube.rend.enabled : !isHidden);

        Material highlightMat = highlightCube.GetComponent<Renderer>().material;
        if (highlightColor != Color.clear)
        {
            cube.rend.material.color = highlightColor;
            highlightMat.color = highlightColor.WithAlpha(highlightMat.color.a);
            highlightMat.SetColor("_EmissionColor", highlightColor);
        }
        else
        {
            Color defaultColor = Utils.ParseHtmlColor(GameManager.instance.configHandler.GetColor("highlight"));
            highlightMat.color = defaultColor.WithAlpha(highlightMat.color.a);
            highlightMat.SetColor("_EmissionColor", defaultColor);
        }
    }

    ///<summary>
    /// Assign a Material to <see cref="cube.rend"/> keeping textures. Then hide cube & highlightCube if needed.
    ///</summary>
    ///<param name="_newMat">The Material to assign</param>
    private void SetMaterial(Material _newMat)
    {
        Material mat = cube.rend.material;
        cube.rend.material = Instantiate(_newMat);

        cube.rend.material.SetTexture("_BaseMap", mat.GetTexture("_BaseMap"));
        cube.rend.material.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
        cube.rend.material.SetTexture("_MetallicGlossMap", mat.GetTexture("_MetallicGlossMap"));
        cube.rend.material.SetTexture("_OcclusionMap", mat.GetTexture("_OcclusionMap"));

        if (isHidden)
        {
            cube.rend.enabled = false;
            displayObjectData.ToggleLabel(false);
        }
        highlightCube?.SetActive(cube.rend.enabled && _newMat == GameManager.instance.highlightMat);
    }

    ///<summary>
    /// Toggle renderer of all items which are not in ogreeChildObjects.
    ///</summary>
    ///<param name="_value">The value to give to all MeshRenderer</param>
    private void ToggleRoomsAndBuildings(bool _value)
    {
        foreach (DictionaryEntry de in GameManager.instance.allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (!go)
                continue;
            OgreeObject obj = go.GetComponent<OgreeObject>();
            if (obj.localCS)
                obj.localCS.SetActive(_value);
            if (obj is Item deOObject && item != deOObject && deOObject.clearanceHandler != null && deOObject.clearanceHandler.isToggled)
                deOObject.clearanceHandler.clearanceWrapper.SetActive(_value);
            switch (obj)
            {
                case Building bd and not Room:
                    bd.transform.GetChild(0).GetComponent<Renderer>().enabled = _value;
                    bd.transform.GetChild(0).GetComponent<Collider>().enabled = _value;
                    bd.nameText.GetComponent<Renderer>().enabled = _value;
                    foreach (Transform wall in bd.walls)
                    {
                        wall.GetComponent<Renderer>().enabled = _value && bd.displayWalls;
                        wall.GetComponent<Collider>().enabled = _value && bd.displayWalls;
                    }
                    break;
                case Room ro:
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
                    ro.nameText.GetComponent<Renderer>().enabled = _value && !ro.tileName;
                    foreach (Transform wall in ro.walls)
                    {
                        wall.GetComponentInChildren<Renderer>().enabled = _value && ro.displayWalls;
                        wall.GetComponentInChildren<Collider>().enabled = _value && ro.displayWalls;
                    }
                    if (go.transform.Find("Floor"))
                    {
                        foreach (Transform child in go.transform.Find("Floor"))
                        {
                            child.GetComponent<Renderer>().enabled = _value;
                            child.GetChild(0).GetComponent<Renderer>().enabled = _value && ro.tileName;
                        }
                        break;
                    }
                    if (go.transform.Find("tilesNameRoot"))
                        foreach (Renderer rend in go.transform.Find("tilesNameRoot").GetComponentsInChildren<Renderer>())
                            rend.enabled = _value && ro.tileName;
                    if (go.transform.Find("tilesColorRoot"))
                        foreach (Renderer rend in go.transform.Find("tilesColorRoot").GetComponentsInChildren<Renderer>())
                            rend.enabled = _value && ro.tileColor;
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Create a material with a color corresponding to the object's temperature
    /// </summary>
    /// <returns>the temperature material</returns>
    private Material GetTemperatureMaterial()
    {
        if (!cube.rend.enabled) //templace placeholder
            return GameManager.instance.defaultMat;

        STemp temp = item.GetTemperatureInfos();
        Material mat = Instantiate(GameManager.instance.defaultMat);
        if (float.IsNaN(temp.mean))
            mat.color = Color.gray;
        else
        {
            (int tempMin, int tempMax) = GameManager.instance.configHandler.GetTemperatureLimit(temp.unit);
            Texture2D text = TempDiagram.instance.heatMapGradient;
            float pixelX = Utils.MapAndClamp(temp.mean, tempMin, tempMax, 0, text.width);
            mat.color = text.GetPixel(Mathf.FloorToInt(pixelX), text.height / 2);
        }
        return mat;
    }

    /// <summary>
    /// Change the color but keep the alpha of the material of its renderer if the object is not hovered or highlighted or selected or focused
    /// </summary>
    /// <param name="_color">the color replacing this object's one</param>
    public void ChangeColor(Color _color)
    {
        if (!isHovered && !isHighlighted && !GameManager.instance.GetSelected().Contains(gameObject) && !GameManager.instance.GetFocused().Contains(gameObject))
            cube.rend.material.color = _color.WithAlpha(cube.rend.material.color.a);
    }

    ///<summary>
    /// Update object's alpha according to _input, true or false.
    ///</summary>
    ///<param name="_value">Alpha wanted for the rack</param>
    public void ToggleAlpha(bool _value)
    {
        Display(!_value, !_value);
        isHidden = _value;
    }

    ///<summary>
    /// Toggles the Renderer and the labels.
    ///</summary>
    ///<param name="_rend">Determines whether the Renderer should be displayed or hidden.</param>
    ///<param name="_label">Determines whether the labels should be displayed or hidden.</param>
    public void Display(bool _rend, bool _label)
    {
        cube.rend.enabled = _rend;
        highlightCube?.SetActive(GameManager.instance.focusMode ? _rend : _rend && isHighlighted && !GameManager.instance.GetSelected().Contains(gameObject));
        displayObjectData.ToggleLabel(_label);
        if (item && item.heatMap)
            item.heatMap.GetComponent<Renderer>().enabled = _rend;
    }

    /// <summary>
    /// Force <see cref="highlightCube"/> to enable itself according to <see cref="isHighlighted"/> value.
    /// </summary>
    public void ForceHighlightCube()
    {
        highlightCube?.SetActive(isHighlighted);
    }

    ///<summary>
    /// Toggles the Renderer, the Collider and the labels.
    ///</summary>
    ///<param name="_rend">Determines whether the Renderer should be displayed or hidden.</param>
    ///<param name="_label">Determines whether the labels should be displayed or hidden.</param>
    ///<param name="_col">Determines whether the collider should be enabled or disabled.</param>
    public void Display(bool _rend, bool _label, bool _col)
    {
        Display(_rend, _label);
        cube.col.enabled = _col;
    }

    /// <summary>
    /// Check if this object is a child (loose definition) of an object
    /// </summary>
    /// <param name="_possibleParent">The possible parent (loose definition) of this object</param>
    /// <returns>true if <paramref name="_possibleParent"/> is found while going up the parent chain</returns>
    private bool DistantChildOf(GameObject _possibleParent)
    {
        Transform parent = transform.parent;
        while (parent)
        {
            if (parent == _possibleParent.transform)
                return true;
            parent = parent.parent;
        }
        return false;
    }

    /// <summary>
    /// Hide this object, unload children
    /// </summary>
    public async void HideObject()
    {
        if ((group && !group.isDisplayed) || isHiddenInGroup)
            return;
        Display(false, false, false);
        isHidden = true;
        await item.LoadChildren(0, true);
    }

    /// <summary>
    /// Display this object, load children if selected
    /// </summary>
    public async void DisplayObject()
    {
        if ((group && !group.isDisplayed) || isHiddenInGroup)
            return;
        if (GameManager.instance.GetSelected().Contains(gameObject))
        {
            Display(true, false, false);
            if (item && item.currentLod == 0)
                await item.LoadChildren(1);
        }
        else
            Display(true, true, true);
        isHidden = false;
    }
}