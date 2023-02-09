using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRendererOutline : MonoBehaviour
{
    public bool isActive = false;

    public bool isSelected = false;
    public bool isHovered = false;
    public bool isHighlighted = false;
    public bool isFocused = false;

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

        EventManager.instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.instance.AddListener<HighlightEvent>(ToggleHighlight);

        EventManager.instance.AddListener<TemperatureColorEvent>(OnTemperatureColorEvent);
        EventManager.instance.AddListener<ImportFinishedEvent>(OnImportFinished);
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

        EventManager.instance.RemoveListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.instance.RemoveListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.instance.RemoveListener<HighlightEvent>(ToggleHighlight);

        EventManager.instance.RemoveListener<TemperatureColorEvent>(OnTemperatureColorEvent);
        EventManager.instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }


    ///<summary>
    /// When called checks if he is the GameObject selected and change its material.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (GameManager.instance.currentItems.Contains(gameObject))
        {
            if (!isFocused)
                SetMaterial(GameManager.instance.selectMat);
            isSelected = true;
            return;
        }
        if (GameManager.instance.previousItems.Contains(gameObject))
        {
            if (isHighlighted)
                SetMaterial(GameManager.instance.highlightMat);
            else if (isFocused)
                SetMaterial(GameManager.instance.focusMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.instance.alphaMat);
                else if (GameManager.instance.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.instance.defaultMat);
                if (!GameManager.instance.tempMode)
                    transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
            isSelected = false;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and change its material.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent _e)
    {
        if (_e.obj.Equals(gameObject))
        {
            SetMaterial(GameManager.instance.focusMat);
            isFocused = true;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and revert its material to the previously used.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        if (_e.obj.Equals(gameObject))
        {
            if (isHighlighted)
                SetMaterial(GameManager.instance.highlightMat);
            else if (isSelected)
                SetMaterial(GameManager.instance.selectMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.instance.alphaMat);
                else if (GameManager.instance.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.instance.defaultMat);
                if (!GameManager.instance.tempMode)
                    transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
            isFocused = false;

            if (GameManager.instance.focus.Count > 0)
            {
                GameObject newFocus = GameManager.instance.focus[GameManager.instance.focus.Count - 1];
                newFocus.GetComponent<CustomRendererOutline>().SetMaterial(GameManager.instance.focusMat);
            }
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and change its material.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnEditModeIn(EditModeInEvent _e)
    {
        if (_e.obj.Equals(gameObject))
            SetMaterial(GameManager.instance.editMat);
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and revert its material to the previously used.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnEditModeOut(EditModeOutEvent _e)
    {
        if (_e.obj.Equals(gameObject))
            SetMaterial(GameManager.instance.focusMat);
    }

    ///<summary>
    /// When called checks if he is the GameObject hovered on and change its material.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnMouseHover(OnMouseHoverEvent _e)
    {
        if (GameManager.instance.focus.Count > 0
            && (!transform.parent || GameManager.instance.focus[GameManager.instance.focus.Count - 1] != transform.parent.gameObject))
            return;

        if (_e.obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            Color temp = transform.GetChild(0).GetComponent<Renderer>().material.color;
            SetMaterial(GameManager.instance.mouseHoverMat);
            isHovered = true;
            transform.GetChild(0).GetComponent<Renderer>().material.color = Utils.InvertColor(temp);
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject hovered on and revert its material to the previously used.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnMouseUnHover(OnMouseUnHoverEvent _e)
    {
        if (_e.obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            if (isHighlighted)
                SetMaterial(GameManager.instance.highlightMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.instance.alphaMat);
                else if (GameManager.instance.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.instance.defaultMat);
                if (!GameManager.instance.tempMode)
                    transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }

            isHovered = false;
        }
    }

    ///<summary>
    /// Change the material for highlightMaterial if this gameObject is highlighted.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void ToggleHighlight(HighlightEvent _e)
    {
        if (_e.obj.Equals(gameObject))
        {
            isHighlighted = !isHighlighted;
            EventManager.instance.Raise(new HighlightEvent { obj = transform.parent.gameObject });
            if (isHighlighted)
                SetMaterial(GameManager.instance.highlightMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.instance.alphaMat);
                else if (GameManager.instance.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.instance.defaultMat);
                if (!GameManager.instance.tempMode)
                    transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
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

    /// <summary>
    /// Create a material with a color corresponding to the object's temperature
    /// </summary>
    /// <returns>the temperature material</returns>
    private Material GetTemperatureMaterial()
    {
        if (!transform.GetChild(0).GetComponent<MeshRenderer>().enabled) //templace placeholder
            return GameManager.instance.defaultMat;

        STemp temp = GetComponent<OObject>().GetTemperatureInfos();
        Material mat = Instantiate(GameManager.instance.defaultMat);
        if  (float.IsNaN(temp.mean))
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
    /// When called, update the object's material according to the temperature color mode being on or not
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void OnTemperatureColorEvent(TemperatureColorEvent _e)
    {
        if (GameManager.instance.tempMode)
        {
            if (!isSelected && !isFocused && !isHighlighted)
                SetMaterial(GetTemperatureMaterial());
        }
        else
        {
            if (isSelected)
                SetMaterial(GameManager.instance.selectMat);
            else if (isFocused)
                SetMaterial(GameManager.instance.focusMat);
            else if (isHighlighted)
                SetMaterial(GameManager.instance.highlightMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.instance.alphaMat);
                else
                    SetMaterial(GameManager.instance.defaultMat);
                transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
        }
    }

    /// <summary>
    /// When called, recompute the temperature material if the temperature color mode is on then set the object's material accordingly
    /// </summary>
    /// <param name="_e">the event's instance</param>
    private void OnImportFinished(ImportFinishedEvent _e)
    {
        if (GameManager.instance.tempMode)
        {
            if (!isSelected && !isFocused && !isHighlighted)
                SetMaterial(GetTemperatureMaterial());
        }
        else
        {
            if (isFocused)
                SetMaterial(GameManager.instance.focusMat);
            else if (isSelected)
                SetMaterial(GameManager.instance.selectMat);
            else if (isHighlighted)
                SetMaterial(GameManager.instance.highlightMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.instance.alphaMat);
                else
                    SetMaterial(GameManager.instance.defaultMat);
                transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
        }
    }
}
