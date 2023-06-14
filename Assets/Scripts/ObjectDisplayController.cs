using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDisplayController : MonoBehaviour
{
    /// <summary>
    /// the collider and the renderer of the first child of this gameobject (the box)
    /// </summary>
    private (Collider col, MeshRenderer rend) cube;
    private DisplayObjectData displayObjectdata;
    private OObject oobject;
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
    private bool scatterPlotOfOneParent = false;
#pragma warning disable IDE1006 // Styles d'affectation de noms
    /// <summary>
    /// Check if the object should be listening to events, ie if it is an oobject or a sensor not from a template or an unused slot
    /// </summary>
    private bool listening => oobject || (sensor && !sensor.fromTemplate) || (slot && !slot.used);
#pragma warning restore IDE1006 // Styles d'affectation de noms

    private bool isHovered = false;
    private bool isHighlighted = false;

    private void Awake()
    {
        cube = (transform.GetChild(0).GetComponent<Collider>(), transform.GetChild(0).GetComponent<MeshRenderer>());
        displayObjectdata = GetComponent<DisplayObjectData>();
    }

    private void Start()
    {
        ObjectDisplayController customRendererParent = transform.parent?.GetComponent<ObjectDisplayController>();
        OgreeObject ogreeObjectParent = transform.parent?.GetComponent<OgreeObject>();
        scatterPlotOfOneParent = customRendererParent && customRendererParent.scatterPlotOfOneParent || ogreeObjectParent && ogreeObjectParent.scatterPlot;
        oobject = GetComponent<OObject>();
        isReferent = oobject && oobject.referent == oobject;
        slot = GetComponent<Slot>();
        sensor = GetComponent<Sensor>();
        group = GetComponent<Group>();
        SubscribeEvents();
        if (scatterPlotOfOneParent && sensor && sensor.fromTemplate)
            Display(true, true);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    ///<summary>
    /// Subscribe the GameObject to Events. Subscribe to different events with different function whether it's a sensor, a slot, a group or any other oobject
    ///</summary>
    public void SubscribeEvents()
    {
        if (slot || sensor)
        {
            EventManager.instance.AddListener<OnSelectItemEvent>(OnSelectOther);
            EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinishedOther);
        }
        else
        {
            EventManager.instance.AddListener<OnSelectItemEvent>(OnSelectBasic);

            EventManager.instance.AddListener<EditModeInEvent>(OnEditModeInBasic);
            EventManager.instance.AddListener<EditModeOutEvent>(OnEditModeOutBasic);

            if (group)
                EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinishedGroup);
            else
                EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinishedBasic);

            EventManager.instance.AddListener<TemperatureColorEvent>(OnTemperatureColorEvent);

            EventManager.instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
            EventManager.instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);
        }
        EventManager.instance.AddListener<OnFocusEvent>(OnFocus);
        EventManager.instance.AddListener<OnUnFocusEvent>(OnUnFocus);
        EventManager.instance.AddListener<TemperatureDiagramEvent>(OnTemperatureDiagram);
        EventManager.instance.AddListener<TemperatureScatterPlotEvent>(OnTemperatureScatterPlot);
        EventManager.instance.AddListener<HighlightEvent>(OnHighLight);
    }

    ///<summary>
    /// Unsubscribe the GameObject to the event it subscribed when initialised
    ///</summary>
    public void UnsubscribeEvents()
    {
        if (slot || sensor)
        {
            EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelectOther);
            EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinishedOther);
        }
        else
        {
            EventManager.instance.RemoveListener<OnSelectItemEvent>(OnSelectBasic);

            EventManager.instance.RemoveListener<EditModeInEvent>(OnEditModeInBasic);
            EventManager.instance.RemoveListener<EditModeOutEvent>(OnEditModeOutBasic);

            if (group)
                EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinishedGroup);
            else
                EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinishedBasic);

            EventManager.instance.RemoveListener<TemperatureColorEvent>(OnTemperatureColorEvent);

            EventManager.instance.RemoveListener<OnMouseHoverEvent>(OnMouseHover);
            EventManager.instance.RemoveListener<OnMouseUnHoverEvent>(OnMouseUnHover);
        }
        EventManager.instance.RemoveListener<OnFocusEvent>(OnFocus);
        EventManager.instance.RemoveListener<OnUnFocusEvent>(OnUnFocus);
        EventManager.instance.RemoveListener<TemperatureDiagramEvent>(OnTemperatureDiagram);
        EventManager.instance.RemoveListener<TemperatureScatterPlotEvent>(OnTemperatureScatterPlot);
        EventManager.instance.RemoveListener<HighlightEvent>(OnHighLight);
    }

    /// <summary>
    /// When selected, toggles the renderer, the collider and the labels of the object depending on if it is selected, focused, a referent, its parent is selected, if focus mode is on and if it has a tempBar or scatterplot enabled, then call <see cref="HandleMaterial"/>
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnSelectBasic(OnSelectItemEvent _e)
    {
        List<OObject> selectionrefs = GameManager.instance.GetSelectedReferents();
        if (!selectionrefs.Contains(oobject.referent))
            UHelpersManager.instance.ToggleU(gameObject, false);
        if (!oobject.tempBar && !scatterPlotOfOneParent)
        {
            List<GameObject> selection = GameManager.instance.GetSelected();
            bool isSelected = selection.Contains(gameObject);
            bool colAndLabels = selection.Contains(transform.parent.gameObject) || (isReferent && !(selectionrefs.Contains(oobject) || GameManager.instance.focusMode));
            bool rend = isSelected || colAndLabels;

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
    /// <param name="_e">The event's intance</param>
    private void OnSelectOther(OnSelectItemEvent _e)
    {
        if (!listening || scatterPlotOfOneParent)
            return;

        if (GameManager.instance.GetSelected().Contains(transform.parent.gameObject))
            Display(true, true);
        else
            Display(false, false);
    }

    /// <summary>
    /// When called, call <see cref="ToggleRoomsAndBuildings"/> and set its material to <see cref="GameManager.focusMat"/> if it is the object being focused, else toggles its renderer, labels and collider depending on if it is a refent,a sensor from a template, if its parent is focused, and if it in focus
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnFocus(OnFocusEvent _e)
    {
        if (_e.obj == gameObject)
        {
            ToggleRoomsAndBuildings(false);
            SetMaterial(GameManager.instance.focusMat);
        }
        else if (_e.obj != transform.parent.gameObject)
        {
            if (isReferent && !GameManager.instance.GetSelectedReferents().Contains(oobject))
                UHelpersManager.instance.ToggleU(gameObject, false);
            if (!sensor || !sensor.fromTemplate || !GameManager.instance.IsInFocus(gameObject))
                Display(false, false, false);
        }
    }

    /// <summary>
    /// Whenn called, call <see cref="ToggleRoomsAndBuildings"/> and <see cref="HandleMaterial"/> if it is the object being focused, else toggles its renderer, labels and collider depending on if it is a refent,a sensor from a template, if its parent is focused, and if it has a scatterPlot enabled
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnUnFocus(OnUnFocusEvent _e)
    {
        if (_e.obj == gameObject)
        {
            oobject.ResetTransform();
            ToggleRoomsAndBuildings(true);
            if (GameManager.instance.GetSelected().Contains(gameObject))
                SetMaterial(GameManager.instance.selectMat);
            else
                HandleMaterial();
        }
        else if ((sensor && sensor.fromTemplate && scatterPlotOfOneParent) || (isReferent && !GameManager.instance.GetSelectedReferents().Contains(oobject)))
            Display(true, true, true);
    }

    /// <summary>
    /// When called, disables its collider and set its material to <see cref="GameManager.editMat"/> if it is the object being edited in
    /// </summary>
    /// <param name="_e">The event's intance</param>
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
    /// <param name="_e">The event's intance</param>
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
    /// <param name="_e">The event's intance</param>
    private void OnImportFinishedBasic(ImportFinishedEvent _e)
    {
        List<GameObject> selection = GameManager.instance.GetSelected();

        if (selection.Contains(gameObject) || oobject.tempBar)
            return;
        if (scatterPlotOfOneParent)
        {
            Display(false, false, false);
            return;
        }

        List<OObject> selectionrefs = GameManager.instance.GetSelectedReferents();
        bool RendColAndLabels = selection.Contains(transform.parent.gameObject) || (isReferent && !(selectionrefs.Contains(oobject) || GameManager.instance.focusMode));
        Display(RendColAndLabels, RendColAndLabels, RendColAndLabels);
        HandleMaterial();
    }

    /// <summary>
    /// When called, toggles its renderer, labels and collider depending of if it is selected, if is is displayed, if it is a referent or if focusMode is on
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnImportFinishedGroup(ImportFinishedEvent _e)
    {
        List<GameObject> selection = GameManager.instance.GetSelected();

        if (selection.Contains(gameObject))
            return;
        if (!GetComponent<Group>().isDisplayed)
        {
            Display(false, false, false);
            return;
        }
        List<OObject> selectionrefs = GameManager.instance.GetSelectedReferents();
        bool RendColAndLabels = selection.Contains(transform.parent.gameObject) || (isReferent && !(selectionrefs.Contains(oobject) || GameManager.instance.focusMode));
        Display(RendColAndLabels, RendColAndLabels, RendColAndLabels);
        if (isHighlighted)
            SetMaterial(GameManager.instance.highlightMat);
        else
        {
            SetMaterial(GameManager.instance.defaultMat);
            cube.rend.material.color = oobject.color;
        }
    }

    /// <summary>
    /// When called, toggles its renderer and labels depending on if it is a sensor from template, if it is listening, on scatterplotEnabled, or if its parent is selected
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnImportFinishedOther(ImportFinishedEvent _e)
    {
        if (sensor && sensor.fromTemplate && scatterPlotOfOneParent)
        {
            Display(true, true);
            return;
        }
        if (!listening || scatterPlotOfOneParent)
        {
            Display(false, false);
            return;
        }
        if (GameManager.instance.GetSelected().Contains(transform.parent.gameObject))
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
        if (!listening)
            return;

        OObject extendedReferent = oobject ? oobject.referent : transform.parent.GetComponent<OObject>().referent;

        if (!extendedReferent || _e.room.transform != extendedReferent.transform.parent)
            return;

        if (!_e.room.barChart)
        {
            Display(false, false, false);
            if (isReferent && GameManager.instance.GetSelected().Contains(gameObject))
                UHelpersManager.instance.ToggleU(gameObject, false);
        }
        else if (GameManager.instance.GetSelected().Contains(gameObject))
        {
            Display(true, false, false);
            if (isReferent)
                UHelpersManager.instance.ToggleU(gameObject, true);
        }
        else if (isReferent || GameManager.instance.GetSelected().Contains(transform.parent.gameObject))
            Display(true, true, !slot && !sensor);
    }

    /// <summary>
    /// When called, call <see cref="HandleMaterial"/> if it is the object whose scatter plot is toggled and not selected or focused, else call <see cref="ScatterPlotToggle(bool)"/> if its parent is
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnTemperatureScatterPlot(TemperatureScatterPlotEvent _e)
    {
        if (!listening)
            return;

        if (gameObject == _e.ogreeObject.gameObject && !GameManager.instance.GetSelected().Contains(gameObject) && !GameManager.instance.GetFocused().Contains(gameObject))
            HandleMaterial();
        else if (transform.parent.gameObject == _e.ogreeObject.gameObject)
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
    /// When called, Set the material of the object to <see cref="GameManager.mouseHoverMat"/> then invert its color if it is the object hovered and not selected
    /// </summary>
    /// <param name="_e">The event's intance</param>
    private void OnMouseHover(OnMouseHoverEvent _e)
    {
        if (_e.obj == gameObject && !GameManager.instance.GetSelected().Contains(gameObject))
        {
            Color temp = cube.rend.material.color;
            SetMaterial(GameManager.instance.mouseHoverMat);
            cube.rend.material.color = Utils.InvertColor(temp);
            isHovered = true;
        }
    }

    /// <summary>
    /// When called, call <see cref="HandleMaterial"/> if it the object being unhovered and it is not selected
    /// </summary>
    /// <param name="_e">The event's intance</param>
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
    /// <param name="_e">The event's intance</param>
    private void OnHighLight(HighlightEvent _e)
    {
        if (_e.obj == gameObject)
            ToggleHighlight(!isHighlighted);
    }

    /// <summary>
    /// Call itself from its parent CustomRenderer if it has one, change the boolean <see cref="isHighlighted"/> then call <see cref="HandleMaterial"/>
    /// </summary>
    /// <param name="_highlighted"></param>
    private void ToggleHighlight(bool _highlighted)
    {
        isHighlighted = _highlighted;
        transform.parent?.GetComponent<ObjectDisplayController>()?.ToggleHighlight(_highlighted);
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
            if (GameManager.instance.GetSelected().Contains(gameObject))
                UHelpersManager.instance.ToggleU(gameObject, false);
        }
        else if (GameManager.instance.GetSelected().Contains(gameObject))
        {
            if (oobject && oobject.referent)
                oobject.scatterPlot = false;
            Display(true, false, false);
            UHelpersManager.instance.ToggleU(gameObject, true);
        }
        else if ((isReferent && !GameManager.instance.GetSelectedReferents().Contains(oobject)) || GameManager.instance.GetSelected().Contains(transform.parent.gameObject))
        {
            Display(true, true, !slot && !sensor);
            if (oobject)
            {
                oobject.scatterPlot = false;
                HandleMaterial();
            }
        }
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
    private void HandleMaterial()
    {
        if (oobject && oobject.scatterPlot)
            SetMaterial(GameManager.instance.scatterPlotMat);
        else if (isHighlighted)
            SetMaterial(GameManager.instance.highlightMat);
        else if (GameManager.instance.tempColorMode && !group && oobject && oobject.category != "corridor")
            SetMaterial(GetTemperatureMaterial());
        else
        {
            if (oobject && oobject.category == "corridor")
                SetMaterial(GameManager.instance.alphaMat);
            else
                SetMaterial(GameManager.instance.defaultMat);
            cube.rend.material.color = oobject.color;
        }
    }

    ///<summary>
    /// Assign a Material to given Renderer keeping textures.
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

        if (oobject && oobject.isHidden)
        {
            cube.rend.enabled = false;
            GetComponent<DisplayObjectData>()?.ToggleLabel(false);
        }
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
            if (go.GetComponent<OObject>())
                continue;
            switch (go.GetComponent<OgreeObject>())
            {
                case var tmp when tmp is Building bd && !(tmp is Room):
                    bd.transform.GetChild(0).GetComponent<Renderer>().enabled = _value;
                    bd.transform.GetChild(0).GetComponent<Collider>().enabled = _value;
                    bd.nameText.GetComponent<Renderer>().enabled = _value;
                    foreach (Transform wall in bd.walls)
                    {
                        wall.GetComponent<Renderer>().enabled = _value;
                        wall.GetComponent<Collider>().enabled = _value;
                    }
                    break;
                case var tmp when tmp is Room ro:
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

        STemp temp = oobject.GetTemperatureInfos();
        Material mat = Instantiate(GameManager.instance.defaultMat);
        if (float.IsNaN(temp.mean))
        {
            mat.color = Color.gray;
        }
        else
        {
            (int tempMin, int tempMax) = GameManager.instance.configLoader.GetTemperatureLimit(temp.unit);
            Texture2D text = TempDiagram.instance.heatMapGradient;
            float pixelX = Utils.MapAndClamp(temp.mean, tempMin, tempMax, 0, text.width);
            mat.color = text.GetPixel(Mathf.FloorToInt(pixelX), text.height / 2);
        }
        return mat;
    }

    /// <summary>
    /// Change the color of the material of its renderer if the object is not hovered or highlighted or selected or focused
    /// </summary>
    /// <param name="_r">red component</param>
    /// <param name="_g">green component</param>
    /// <param name="_b">blue component</param>
    /// <returns>a Color created from the three component with the alpha of the previous color of the material of this object's renderer</returns>
    public Color ChangeColor(float _r, float _g, float _b)
    {
        if (!isHovered && !isHighlighted && !GameManager.instance.GetSelected().Contains(gameObject) && !GameManager.instance.GetFocused().Contains(gameObject))
            cube.rend.material.color = new Color(_r, _g, _b, cube.rend.material.color.a);
        return cube.rend.material.color;
    }

    /// <summary>
    /// call and returns <see cref="ChangeColor(float, float, float)"/> with the red, green and blue component of the image (the alpha is ignored)
    /// </summary>
    /// <param name="c">the color replacing this object's one</param>
    /// <returns><see cref="ChangeColor(float, float, float)"/></returns>
    public Color ChangeColor(Color c)
    {
        return ChangeColor(c.r, c.g, c.b);
    }

    ///<summary>
    /// Update object's alpha according to _input, true or false.
    ///</summary>
    ///<param name="_value">Alpha wanted for the rack</param>
    public void ToggleAlpha(bool _value)
    {
        Display(!_value, !_value);
        oobject.isHidden = _value;
    }

    ///<summary>
    /// Display or not the Renderer and all TMP
    ///<summary>
    ///<param name="_value">If the slot is hiden or not</param>
    public void Display(bool _rend, bool _label)
    {
        cube.rend.enabled = _rend;
        displayObjectdata.ToggleLabel(_label);
        if (oobject && oobject.heatMap)
            oobject.heatMap.GetComponent<Renderer>().enabled = _rend;
    }

    ///<summary>
    /// Display or not the Renderer and all TMP
    ///<summary>
    ///<param name="_value">If the slot is hiden or not</param>
    public void Display(bool _rend, bool _label, bool _col)
    {
        Display(_rend, _label);
        cube.col.enabled = _col;
    }
}