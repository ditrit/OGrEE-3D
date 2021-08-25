using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// Class responsible for increasing performance by culling the child's MeshRenderers when the GameObject isnt Focused by the user.
///</summary>
public class FocusHandler : MonoBehaviour
{
    public List<GameObject> OwnObjectsList;
    public List<GameObject> ogreeChildObjects;
    public List<GameObject> slotsChildObjects;

    public List<MeshRenderer> ogreeChildMeshRendererList;
    public List<MeshRenderer> slotChildMeshRendererList;

    public bool isSelected = false;
    public bool isHovered = false;
    public bool isFocused = false;

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
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

        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);
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

        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (e._obj.Equals(gameObject))
        {
            UpdateChildMeshRenderers(true);
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
            if (transform.GetChild(0).GetComponent<Renderer>().enabled && !isFocused)
                UpdateChildMeshRenderers(false);
            isSelected = false;
        }
    }

    ///
    private void OnFocusItem(OnFocusEvent e)
    {
        if (e._obj == gameObject)
        {
            UpdateChildMeshRenderers(true);
            UpdateOtherChildrenMeshRenderers(false);
            isFocused = true;
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
        else if (GameManager.gm.focus.Count >= 2 && gameObject == GameManager.gm.focus[GameManager.gm.focus.Count - 2])
        {
            if (GetComponent<OObject>().category == "rack" || GetComponent<OObject>().category == "device")
            {
                GetComponent<OObject>().UpdateAlpha("true");
                GetComponent<OObject>().ToggleSlots("false");
            }
        }
    }

    ///
    private void OnUnFocusItem(OnUnFocusEvent e)
    {
        if (e._obj == gameObject)
        {
            UpdateOtherChildrenMeshRenderers(true);
            UpdateChildMeshRenderers(false);
            isFocused = false;
            transform.GetChild(0).GetComponent<Collider>().enabled = true;

            if (GameManager.gm.focus.Count > 0)
            {
                // EventManager.Instance.Raise(new OnFocusEvent() { _obj = GameManager.gm.focus[GameManager.gm.focus.Count - 1] });
                GameObject newFocus = GameManager.gm.focus[GameManager.gm.focus.Count - 1];
                newFocus.GetComponent<OObject>().UpdateAlpha("false");
                newFocus.GetComponent<OObject>().ToggleSlots("true");
            }
        }
    }

    ///
    private void OnMouseHover(OnMouseHoverEvent e)
    {
        if (GameManager.gm.focus.Count > 0 && GameManager.gm.focus[GameManager.gm.focus.Count - 1] != transform.parent.gameObject)
            return;
        if (e._obj.Equals(gameObject) && !isSelected && !isFocused)
        {
            UpdateChildMeshRenderers(true);
            isHovered = true;
        }

    }

    ///
    private void OnMouseUnHover(OnMouseUnHoverEvent e)
    {
        if (e._obj.Equals(gameObject) && transform.GetChild(0).GetComponent<Renderer>().enabled
            && !isSelected && !isFocused)
        {
            UpdateChildMeshRenderers(false);
            isHovered = false;
        }
    }

    ///<summary>
    /// When called, fills all the lists and does a ManualUnFocus to deactivate all useless mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent e)
    {
        FillListsWithChildren();
        FillMeshRendererLists();
        if (transform.GetChild(0).GetComponent<Renderer>().enabled
            && transform.GetChild(0).GetComponent<Renderer>().material.color.a == 1f)
            UpdateChildMeshRenderers(false);
    }

    ///<summary>
    /// Fills the 3 Child list with their corresponding content.
    ///</summary>
    private void FillListsWithChildren()
    {
        ogreeChildObjects.Clear();
        OwnObjectsList.Clear();
        slotsChildObjects.Clear();

        foreach (Transform child in transform)
        {
            if (child.GetComponent<OgreeObject>())
            {
                ogreeChildObjects.Add(child.gameObject);
            }
            else if (child.GetComponent<Slot>())
            {
                slotsChildObjects.Add(child.gameObject);
            }
            else
            {
                OwnObjectsList.Add(child.gameObject);
            }
        }
    }

    ///<summary>
    /// Fills the Mesh renderer Lists from the OgreeChildObjects and SlotsChildObjects lists.
    ///</summary>
    private void FillMeshRendererLists()
    {
        ogreeChildMeshRendererList.Clear();
        slotChildMeshRendererList.Clear();

        foreach (GameObject gameObject in ogreeChildObjects)
        {
            MeshRenderer[] OgreeChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in OgreeChildMeshRenderers)
            {
                ogreeChildMeshRendererList.Add(meshRenderer);
            }
        }

        foreach (GameObject gameObject in slotsChildObjects)
        {
            MeshRenderer[] SlotChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in SlotChildMeshRenderers)
            {
                slotChildMeshRendererList.Add(meshRenderer);
            }
        }
    }

    ///<summary>
    /// When called enables/disables the child MeshRenderers located in the OgreeChildMeshRendererList and SlotChildMeshRendererList depending on the boolean argument.
    ///</summary>
    ///<param name="_value">Boolean value assigned to the meshRenderer.enabled </param>
    private void UpdateChildMeshRenderers(bool _value)
    {
        foreach (MeshRenderer meshRenderer in ogreeChildMeshRendererList)
        {
            meshRenderer.enabled = _value;
        }

        foreach (MeshRenderer meshRenderer in slotChildMeshRendererList)
        {
            if (!meshRenderer.transform.parent.GetComponent<Slot>().used)
                meshRenderer.enabled = _value;
        }
    }

    ///
    private void UpdateOtherChildrenMeshRenderers(bool _value)
    {
        List<Transform> others = new List<Transform>();
        foreach (Transform child in transform.parent)
        {
            if (child.GetComponent<OgreeObject>() && child != transform)
                others.Add(child);
        }

        List<MeshRenderer> renderers = new List<MeshRenderer>();
        foreach (Transform obj in others)
        {
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in meshRenderers)
                renderers.Add(mr);
        }

        foreach (MeshRenderer mr in renderers)
            mr.enabled = _value;
    }
}
