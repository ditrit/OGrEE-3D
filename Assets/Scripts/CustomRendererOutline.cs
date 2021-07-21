using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRendererOutline : MonoBehaviour
{
    public Material selectedMaterial;
    public Material mouseHoverMaterial;
    private Material defaultMaterial;
    private Material transparentMaterial;

    public bool isActive = false;

    public bool isSelected = false;
    public bool isHovered = false;

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

        EventManager.Instance.AddListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.AddListener<OnMouseUnHoverEvent>(OnMouseUnHover);
    }

    ///<summary>
    /// Unsubscribe the GameObject to Events
    ///</summary>
    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.RemoveListener<OnDeselectItemEvent>(OnDeselectItem);

        EventManager.Instance.RemoveListener<OnMouseHoverEvent>(OnMouseHover);
        EventManager.Instance.RemoveListener<OnMouseUnHoverEvent>(OnMouseUnHover);
    }


    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (e._obj.Equals(gameObject))
        {
            SetMaterial(transform.GetChild(0).GetComponent<Renderer>(), selectedMaterial);
            isSelected = true;
        }

    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true deactivates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent e)
    {
        if (e._obj.Equals(gameObject))
        {
            Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
            if (e._obj.GetComponent<OObject>().category.Equals("corridor"))
                SetMaterial(renderer, transparentMaterial);
            else
                SetMaterial(renderer, defaultMaterial);

            renderer.material.color = e._obj.GetComponent<OObject>().color;
            isSelected = false;
        }
    }

    private void OnMouseHover(OnMouseHoverEvent e)
    {
        if (e._obj.Equals(gameObject) && !isSelected)
        {
            SetMaterial(transform.GetChild(0).GetComponent<Renderer>(), mouseHoverMaterial);
            isHovered = true;
        }
    }

    private void OnMouseUnHover(OnMouseUnHoverEvent e)
    {
        if (e._obj.Equals(gameObject) && !isSelected)
        {
            Renderer renderer = transform.GetChild(0).GetComponent<Renderer>();
            if (e._obj.GetComponent<OObject>().category.Equals("corridor"))
                SetMaterial(renderer, transparentMaterial);
            else
                SetMaterial(renderer, defaultMaterial);

            renderer.material.color = e._obj.GetComponent<OObject>().color;
            isHovered = false;
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
