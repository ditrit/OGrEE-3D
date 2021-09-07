using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRendererOutline : MonoBehaviour
{
    public Material selectedMaterial;
    public Material mouseHoverMaterial;
    public Material highlightMaterial;
    public Material focusMaterial;
    private Material defaultMaterial;
    private Material transparentMaterial;

    public bool isActive = false;

    public bool isSelected = false;
    public bool isHovered = false;
    public bool isHighlighted = false;
    public bool isFocused = false;

    private void Start()
    {
        defaultMaterial = GameManager.gm.defaultMat;
        transparentMaterial = GameManager.gm.alphaMat;

        if (GetComponent<OObject>())
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
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.Instance.AddListener<HighlightEvent>(ToggleHighlight);
    }

    ///<summary>
    /// Unsubscribe the GameObject to Events
    ///</summary>
    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.RemoveListener<OnDeselectItemEvent>(OnDeselectItem);

        EventManager.Instance.RemoveListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.RemoveListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.RemoveListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.RemoveListener<OnMouseUnHoverEvent>(OnMouseUnHover);

        EventManager.Instance.RemoveListener<HighlightEvent>(ToggleHighlight);
    }


    ///<summary>
    /// When called checks if he is the GameObject selected and change its material.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            SetMaterial(transform.GetChild(0).GetComponent<Renderer>(), selectedMaterial);
            isSelected = true;
        }

    }

    ///<summary>
    /// When called checks if he is the GameObject selected and revert its material to the previously used.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
            if (isHighlighted)
                SetMaterial(renderer, highlightMaterial);
            else if (isFocused)
                SetMaterial(renderer, focusMaterial);
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(renderer, transparentMaterial);
                else
                    SetMaterial(renderer, defaultMaterial);

                renderer.material.color = e.obj.GetComponent<OObject>().color;
            }
            isSelected = false;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and change its material.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            SetMaterial(transform.GetChild(0).GetComponent<Renderer>(), focusMaterial);
            isFocused = true;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and revert its material to the previously used.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
            if (isHighlighted)
                SetMaterial(renderer, highlightMaterial);
            else if (isSelected)
                SetMaterial(renderer, selectedMaterial);
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(renderer, transparentMaterial);
                else
                    SetMaterial(renderer, defaultMaterial);

                renderer.material.color = e.obj.GetComponent<OObject>().color;
            }
            isFocused = false;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject hovered on and change its material.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnMouseHover(OnMouseHoverEvent e)
    {
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] != transform.parent.gameObject)
            return;

        if (e.obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            SetMaterial(transform.GetChild(0).GetComponent<Renderer>(), mouseHoverMaterial);
            isHovered = true;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject hovered on and revert its material to the previously used.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnMouseUnHover(OnMouseUnHoverEvent e)
    {
        if (e.obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
            if (isHighlighted)
            {
                SetMaterial(renderer, highlightMaterial);
            }
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(renderer, transparentMaterial);
                else
                    SetMaterial(renderer, defaultMaterial);
                renderer.material.color = e.obj.GetComponent<OObject>().color;
            }

            isHovered = false;
        }
    }

    ///<summary>
    /// Change the material for highlightMaterial if this gameObject is highlighted.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void ToggleHighlight(HighlightEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            isHighlighted = !isHighlighted;
            Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
            EventManager.Instance.Raise(new HighlightEvent { obj = transform.parent.gameObject });
            if (isHighlighted)
            {
                SetMaterial(renderer, highlightMaterial);
            }
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(renderer, transparentMaterial);
                else
                    SetMaterial(renderer, defaultMaterial);
                renderer.material.color = e.obj.GetComponent<OObject>().color;
            }
        }
    }

    ///<summary>
    /// Assign a Material to given Renderer keeping textures.
    ///</summary>
    ///<param name="_renderer">The Renderer of the object to modify</param>
    ///<param name="_newMat">The Material to assign</param>
    private void SetMaterial(Renderer _renderer, Material _newMat)
    {
        Material mat = _renderer.material;
        _renderer.material = Instantiate(_newMat);

        _renderer.material.SetTexture("_BaseMap", mat.GetTexture("_BaseMap"));
        _renderer.material.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
        _renderer.material.SetTexture("_MetallicGlossMap", mat.GetTexture("_MetallicGlossMap"));
        _renderer.material.SetTexture("_OcclusionMap", mat.GetTexture("_OcclusionMap"));
    }
}
