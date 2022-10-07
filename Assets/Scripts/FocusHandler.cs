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

    public bool isActive = false;
    public bool isDeleted = false;

    public bool isSelected = false;
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

        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (isDeleted)
            return;
        if (GameManager.gm.currentItems.Contains(gameObject))
        {
            // We manage all collider and renderer changes due to the selection
            isSelected = true;
            ToggleCollider(gameObject, false);
            UpdateParentRenderers(gameObject, false);
            if (GetComponent<OObject>().category == "rack")
                UpdateOwnMeshRenderers(false);

            UpdateChildMeshRenderers(true, true);
            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            return;
        }

        // If this one is part of it and is in a rack which is not the parent of a selected object we display it again
        if (GameManager.gm.previousItems.Contains(gameObject))
        {
            isSelected = false;

            // Parent racks of previously selected objects
            OObject selectionParentRack = GameManager.gm.currentItems.Count > 0 ? GameManager.gm.currentItems[0].GetComponent<OObject>()?.referent : null;
            OObject parentRack = GetComponent<OObject>().referent;

            if (selectionParentRack != parentRack)
            {
                ToggleCollider(gameObject, false);
                ResetToRack();
            }
            else
            {
                if (!GameManager.gm.currentItems.Contains(transform.parent.gameObject))
                {
                    UpdateOwnMeshRenderers(false);
                    UpdateChildMeshRenderers(false);
                }
                else
                    UpdateChildMeshRenderers(false);
            }

        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    /// If he is the previously focused GameObject, use OObject methods to hide it.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent e)
    {
        if (isDeleted)
            return;
        if (e.obj == gameObject)
        {
            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            UpdateChildMeshRenderers(true, true);
            UpdateOtherObjectsMeshRenderers(false);
            isFocused = true;
            ToggleCollider(gameObject, false);
            GetComponent<DisplayObjectData>()?.ToggleLabel(false);

        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true deactivates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent e)
    {
        if (isDeleted)
            return;
        if (e.obj == gameObject)
        {
            UpdateOtherObjectsMeshRenderers(true);
            isFocused = false;
            ToggleCollider(gameObject, false);
            GetComponent<DisplayObjectData>()?.ToggleLabel(true);

            GetComponent<OgreeObject>().ResetTransform();
            foreach (Transform child in transform)
            {
                if (child.GetComponent<OgreeObject>())
                    child.GetComponent<OgreeObject>().ResetTransform();
            }
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and enters in Edit mode.
    /// If it is, enable the colliders used for the edit mode, instantiate the BoundControls components and disable the children collliders.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnEditModeIn(EditModeInEvent e)
    {
        if (isDeleted)
            return;
        if (e.obj == gameObject)
        {
            //enable collider used for manipulation
            transform.GetChild(0).GetComponent<Collider>().enabled = true;
            //disable children colliders
            UpdateChildMeshRenderers(true);
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and exits Edit mode.
    /// If it is, disable the colliders used for the edit mode and enable the children collliders.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnEditModeOut(EditModeOutEvent e)
    {
        if (isDeleted)
            return;
        if (e.obj == gameObject)
        {
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
            UpdateChildMeshRenderers(true, true);
        }
    }

    ///<summary>
    /// When called, fills all the lists and does a ManualUnFocus to deactivate all useless mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent e)
    {
        if (isDeleted)
            return;
        InitHandler();
        if (GameManager.gm.currentItems.Contains(gameObject))
        {
            UpdateChildMeshRenderers(true, true);
            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            return;
        }

        OObject selectionReferent = GameManager.gm.currentItems.Count > 0 ? GameManager.gm.currentItems[0].GetComponent<OObject>()?.referent : null;

        if (GetComponent<OObject>().category != "device" && selectionReferent != GetComponent<OObject>().referent)
            UpdateChildMeshRenderersRec(false);
        else if (selectionReferent == GetComponent<OObject>().referent)
        {
            if (!GameManager.gm.currentItems.Contains(gameObject) && !GameManager.gm.currentItems.Contains(transform.parent.gameObject))
            {
                ToggleCollider(gameObject, false);
                UpdateOwnMeshRenderers(false);
                UpdateChildMeshRenderers(false);
            }
            if (GameManager.gm.currentItems.Contains(transform.parent.gameObject))
                UpdateChildMeshRenderers(false);
        }
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
            if (child.GetComponent<OgreeObject>() ||child.GetComponent<Sensor>())
                ogreeChildObjects.Add(child.gameObject);
            else if (child.GetComponent<Slot>())
                slotsChildObjects.Add(child.gameObject);
            else if (child.name != "uRoot" && child.name != "GridForULocation")
                OwnObjectsList.Add(child.gameObject);
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
            foreach (GameObject ownObject in gameObject.GetComponent<FocusHandler>().OwnObjectsList)
                ogreeChildMeshRendererList.Add(ownObject.GetComponent<MeshRenderer>());
        }

        foreach (GameObject gameObject in slotsChildObjects)
        {
            MeshRenderer[] SlotChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in SlotChildMeshRenderers)
                slotChildMeshRendererList.Add(meshRenderer);
        }
    }

    ///<summary>
    /// When called enables/disables the MeshRenderers located in the OwnObjectsList depending on the boolean argument.
    ///</summary>
    ///<param name="_value">Boolean value assigned to the meshRenderer.enabled </param>
    private void UpdateOwnMeshRenderers(bool _value)
    {
        foreach (GameObject go in OwnObjectsList)
        {
            MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
                mr.enabled = _value;
        }
        GetComponent<OObject>().ToggleSlots(_value.ToString());
        ToggleCollider(gameObject, _value);

        if (GetComponent<OObject>().isHidden)
        {
            transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            GetComponent<DisplayObjectData>()?.ToggleLabel(false);
        }
    }

    ///<summary>
    /// When called enables/disables the child MeshRenderers located in the OgreeChildMeshRendererList and SlotChildMeshRendererList depending on the boolean argument.
    ///</summary>
    ///<param name="_value">Boolean value assigned to the meshRenderer.enabled </param>
    ///<param name="_collider">Boolean value assigned to the Collider.enabled, false by default </param>
    private void UpdateChildMeshRenderers(bool _value, bool _collider = false)
    {
        foreach (MeshRenderer meshRenderer in ogreeChildMeshRendererList)
        {
            meshRenderer.enabled = _value;
            if (meshRenderer.GetComponent<Collider>() && !meshRenderer.transform.parent.GetComponent<Slot>() && !meshRenderer.transform.parent.GetComponent<Sensor>())
                meshRenderer.GetComponent<Collider>().enabled = _collider;
        }

        foreach (MeshRenderer meshRenderer in slotChildMeshRendererList)
        {
            if (!meshRenderer.transform.parent.GetComponent<Slot>().used)
                meshRenderer.enabled = _value;
        }
    }

    ///<summary>
    /// When called enables/disables children's MeshRenderers recursively.
    ///</summary>
    ///<param name="_value">Boolean value used when calling UpdateChildMeshRenderers</param>
    private void UpdateChildMeshRenderersRec(bool _value)
    {
        foreach (GameObject child in ogreeChildObjects)
            child.GetComponent<FocusHandler>().UpdateChildMeshRenderersRec(_value);
        UpdateChildMeshRenderers(_value);
    }

    ///<summary>
    /// Toggle renderer of all items which are not in ogreeChildObjects.
    ///</summary>
    ///<param name="_value">The value to give to all MeshRenderer</param>
    private void UpdateOtherObjectsMeshRenderers(bool _value)
    {
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject go = (GameObject)de.Value;
            if (!ogreeChildObjects.Contains(go) && go != this.gameObject)
            {
                switch (go.GetComponent<OgreeObject>().category)
                {
                    case "tenant":
                        break;
                    case "site":
                        break;
                    case "building":
                        Building bd = go.GetComponent<Building>();
                        bd.transform.GetChild(0).GetComponent<Renderer>().enabled = _value;
                        foreach (Transform wall in bd.walls)
                        {
                            wall.GetComponent<Renderer>().enabled = _value;
                            wall.GetComponent<Collider>().enabled = _value;
                        }
                        break;
                    case "room":
                        Room ro = go.GetComponent<Room>();
                        ro.usableZone.GetComponent<Renderer>().enabled = _value;
                        ro.reservedZone.GetComponent<Renderer>().enabled = _value;
                        ro.technicalZone.GetComponent<Renderer>().enabled = _value;
                        ro.tilesEdges.GetComponent<Renderer>().enabled = _value;
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

    ///<summary>
    /// Toggle renderer of all _obj parents in the hierarchy recursively until _obj is a rack
    ///</summary>
    ///<param name="_obj">The object whose <b>parents' renderers</b> will be updated</param>
    ///<param name="_value">The value to give to all MeshRenderer</param>
    private void UpdateParentRenderers(GameObject _obj, bool _value)
    {
        if (_obj.GetComponent<OgreeObject>().category != "device")
            return;
        _obj.transform.parent.GetComponent<FocusHandler>().UpdateOwnMeshRenderers(_value);
        UpdateParentRenderers(_obj.transform.parent.gameObject, _value);
    }

    ///<summary>
    /// Disable renderer of all _obj parents in the hierarchy recursively until _obj is a rack, then enable the renderer of _obj if _obj is a rack
    ///</summary>
    ///<param name="_obj">The object whose <b>own and parents' renderers</b> will be updated</param>
    private void ResetToRack()
    {
        UpdateChildMeshRenderers(false);
        if (GetComponent<OgreeObject>().category != "device")
        {
            UpdateOwnMeshRenderers(true);
            ToggleCollider(gameObject, true);
            UpdateChildMeshRenderers(false);
            GameObject uRoot = transform.Find("uRoot")?.gameObject;
            uRoot?.SetActive(false);
            return;
        }
        transform.parent.GetComponent<FocusHandler>().ResetToRack();
    }

    ///<summary>
    /// Initialise the renderer and gameobject lists of this and all children recursively
    ///</summary>
    public void InitHandler()
    {
        FillListsWithChildren();
        foreach (GameObject child in ogreeChildObjects)
            child.GetComponent<FocusHandler>().InitHandler();
        FillMeshRendererLists();
    }

    ///<summary>
    /// Toggle the collider(s) of an object <br></br><i>Due to the change in the rack prefab, all OgreeObject don't have the same numbers and hierarchy of colliders anymore</i>
    ///</summary>
    ///<param name="_obj">The object whose collider(s) will be updated</param>
    ///<param name="_enabled">state of the collider(s)</param>
    public void ToggleCollider(GameObject _obj, bool _enabled)
    {
        _obj.transform.GetChild(0).GetComponent<Collider>().enabled = _enabled;
    }
}
