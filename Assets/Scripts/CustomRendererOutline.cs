using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRendererOutline : MonoBehaviour
{
    public Material selectedMaterial;
    public Material mouseHoverMaterial;
    public Material highlightMaterial;
    public Material focusMaterial;
    public Material editMaterial;
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
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);

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

        EventManager.Instance.RemoveListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.RemoveListener<EditModeOutEvent>(OnEditModeOut);

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
            if (!isFocused)
                SetMaterial(selectedMaterial);
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
            if (isHighlighted)
                SetMaterial(highlightMaterial);
            else if (isFocused)
                SetMaterial(focusMaterial);
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(transparentMaterial);
                else
                    SetMaterial(defaultMaterial);

                transform.GetChild(0).GetComponent<Renderer>().material.color = e.obj.GetComponent<OObject>().color;
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
            SetMaterial(focusMaterial);
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
            if (isHighlighted)
                SetMaterial(highlightMaterial);
            else if (isSelected)
                SetMaterial(selectedMaterial);
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(transparentMaterial);
                else
                    SetMaterial(defaultMaterial);
                transform.GetChild(0).GetComponent<Renderer>().material.color = e.obj.GetComponent<OObject>().color;
            }
            isFocused = false;

            if (GameManager.gm.focus.Count > 0)
            {
                GameObject newFocus = GameManager.gm.focus[GameManager.gm.focus.Count - 1];
                newFocus.GetComponent<CustomRendererOutline>().SetMaterial(focusMaterial);
            }
        }
    }

    private void OnEditModeIn(EditModeInEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            SetMaterial(editMaterial);
        }
    }

    private void OnEditModeOut(EditModeOutEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            SetMaterial(focusMaterial);
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject hovered on and change its material.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnMouseHover(OnMouseHoverEvent e)
    {
        if (GameManager.gm.focus.Count > 0
            && (!transform.parent || GameManager.gm.focus[GameManager.gm.focus.Count - 1] != transform.parent.gameObject))
            return;

        if (e.obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            SetMaterial(mouseHoverMaterial);
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
            if (isHighlighted)
                SetMaterial(highlightMaterial);
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(transparentMaterial);
                else
                    SetMaterial(defaultMaterial);
                transform.GetChild(0).GetComponent<Renderer>().material.color = e.obj.GetComponent<OObject>().color;
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
            EventManager.Instance.Raise(new HighlightEvent { obj = transform.parent.gameObject });
            if (isHighlighted)
                SetMaterial(highlightMaterial);
            else
            {
                if (e.obj.GetComponent<OObject>().category.Equals("corridor"))
                    SetMaterial(transparentMaterial);
                else
                    SetMaterial(defaultMaterial);
                transform.GetChild(0).GetComponent<Renderer>().material.color = e.obj.GetComponent<OObject>().color;
            }
        }
    }

    ///<summary>
    /// Assign a Material to given Renderer keeping textures.
    ///</summary>
    ///<param name="_renderer">The Renderer of the object to modify</param>
    ///<param name="_newMat">The Material to assign</param>
    public void SetMaterial(Material _newMat)
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
}
