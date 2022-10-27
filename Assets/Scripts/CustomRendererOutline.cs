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
        if (GameManager.gm.allItems.ContainsValue(gameObject))
            isActive = true;

        if (isActive)
            SubscribeEvents();
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
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);

        EventManager.Instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.Instance.AddListener<HighlightEvent>(ToggleHighlight);

        EventManager.Instance.AddListener<TemperatureColorEvent>(OnTemperatureColorEvent);

        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// Unsubscribe the GameObject to Events
    ///</summary>
    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.Instance.RemoveListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.RemoveListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.RemoveListener<EditModeOutEvent>(OnEditModeOut);

        EventManager.Instance.RemoveListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.RemoveListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.Instance.RemoveListener<HighlightEvent>(ToggleHighlight);

        EventManager.Instance.RemoveListener<TemperatureColorEvent>(OnTemperatureColorEvent);

        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }


    ///<summary>
    /// When called checks if he is the GameObject selected and change its material.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (GameManager.gm.currentItems.Contains(gameObject))
        {
            if (!isFocused)
                SetMaterial(GameManager.gm.selectMat);
            isSelected = true;
            return;
        }
        if (GameManager.gm.previousItems.Contains(gameObject))
        {
            if (isHighlighted)
                SetMaterial(GameManager.gm.highlightMat);
            else if (isFocused)
                SetMaterial(GameManager.gm.focusMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.gm.alphaMat);
                else if (GameManager.gm.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.gm.defaultMat);
                if (!GameManager.gm.tempMode)
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
            SetMaterial(GameManager.gm.focusMat);
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
                SetMaterial(GameManager.gm.highlightMat);
            else if (isSelected)
                SetMaterial(GameManager.gm.selectMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.gm.alphaMat);
                else if (GameManager.gm.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.gm.defaultMat);
                if (!GameManager.gm.tempMode)
                    transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
            isFocused = false;

            if (GameManager.gm.focus.Count > 0)
            {
                GameObject newFocus = GameManager.gm.focus[GameManager.gm.focus.Count - 1];
                newFocus.GetComponent<CustomRendererOutline>().SetMaterial(GameManager.gm.focusMat);
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
            SetMaterial(GameManager.gm.editMat);
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and revert its material to the previously used.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnEditModeOut(EditModeOutEvent _e)
    {
        if (_e.obj.Equals(gameObject))
            SetMaterial(GameManager.gm.focusMat);
    }

    ///<summary>
    /// When called checks if he is the GameObject hovered on and change its material.
    ///</summary>
    ///<param name="_e">The event's instance</param>
    private void OnMouseHover(OnMouseHoverEvent _e)
    {
        if (GameManager.gm.focus.Count > 0
            && (!transform.parent || GameManager.gm.focus[GameManager.gm.focus.Count - 1] != transform.parent.gameObject))
            return;

        if (_e.obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            Color temp = transform.GetChild(0).GetComponent<Renderer>().material.color;
            SetMaterial(GameManager.gm.mouseHoverMat);
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
                SetMaterial(GameManager.gm.highlightMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.gm.alphaMat);
                else if (GameManager.gm.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.gm.defaultMat);
                if (!GameManager.gm.tempMode)
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
            EventManager.Instance.Raise(new HighlightEvent { obj = transform.parent.gameObject });
            if (isHighlighted)
                SetMaterial(GameManager.gm.highlightMat);
            else
            {
                if (GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(GameManager.gm.alphaMat);
                else if (GameManager.gm.tempMode)
                    SetMaterial(GetTemperatureMaterial());
                else
                    SetMaterial(GameManager.gm.defaultMat);
                if (!GameManager.gm.tempMode)
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

    private Material GetTemperatureMaterial()
    {
        STemp temp = GetComponent<OObject>().GetTemperatureInfos();
        Material mat = Instantiate(GameManager.gm.defaultMat);
        if (temp.mean is float.NaN)
        {
            mat.color = Color.gray;
        }
        else
        {
            (int tempMin, int tempMax) = GameManager.gm.configLoader.GetTemperatureLimit(temp.unit);
            float blue = Utils.MapAndClamp(temp.mean, tempMin, tempMax, 1, 0);
            float red = Utils.MapAndClamp(temp.mean, tempMin, tempMax, 0, 1);
            mat.color = new Color(red, 0, blue);
        }
        return mat;
    }

    private void OnTemperatureColorEvent(TemperatureColorEvent _e)
    {
        if (GameManager.gm.tempMode)
        {
            if (!isSelected && !isFocused && !isHighlighted)
                SetMaterial(GetTemperatureMaterial());
        }
        else
        {
            if (isSelected)
                SetMaterial(GameManager.gm.selectMat);
            else if (isFocused)
                SetMaterial(GameManager.gm.focusMat);
            else if (isHighlighted)
                SetMaterial(GameManager.gm.highlightMat);
            else
            {
                SetMaterial(GameManager.gm.defaultMat);
                transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
        }
    }

    private void OnImportFinished(ImportFinishedEvent _e)
    {
        if (GameManager.gm.tempMode)
        {
            if (!isSelected && !isFocused && !isHighlighted)
                SetMaterial(GetTemperatureMaterial());
        }
        else
        {
            if (isSelected)
                SetMaterial(GameManager.gm.selectMat);
            else if (isFocused)
                SetMaterial(GameManager.gm.focusMat);
            else if (isHighlighted)
                SetMaterial(GameManager.gm.highlightMat);
            else
            {
                SetMaterial(GameManager.gm.defaultMat);
                transform.GetChild(0).GetComponent<Renderer>().material.color = GetComponent<OObject>().color;
            }
        }
    }
}
