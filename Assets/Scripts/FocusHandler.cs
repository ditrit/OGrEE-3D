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
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

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

        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            UpdateChildMeshRenderers(true, true);
            isSelected = true;
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
            UpdateParentRenderers(gameObject, false);
            if (GetComponent<OObject>().category == "rack")
                UpdateOwnMeshRenderers(false);
            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true deactivates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnDeselectItem(OnDeselectItemEvent e)
    {
        if (e.obj.Equals(gameObject))
        {
            isSelected = false;
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
            ResetToRack();
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    /// If he is the previously focused GameObject, use OObject methods to hide it.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnFocusItem(OnFocusEvent e)
    {
        if (e.obj == gameObject)
        {
            transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            UpdateChildMeshRenderers(true, true);
            UpdateOtherObjectsMeshRenderers(false);
            isFocused = true;
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
            GetComponent<DisplayObjectData>()?.ToggleLabel(false);
        }
    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true deactivates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnUnFocusItem(OnUnFocusEvent e)
    {
        if (e.obj == gameObject)
        {
            // UpdateChildMeshRenderers(false);
            UpdateOtherObjectsMeshRenderers(true);
            isFocused = false;
            transform.GetChild(0).GetComponent<Collider>().enabled = false;
            GetComponent<DisplayObjectData>()?.ToggleLabel(true);
        }
    }

    ///<summary>
    /// When called, fills all the lists and does a ManualUnFocus to deactivate all useless mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent e)
    {
        if (GetComponent<OObject>().category != "device")
            UpdateChildMeshRenderersRec(false);
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
                ogreeChildObjects.Add(child.gameObject);
            else if (child.GetComponent<Slot>())
                slotsChildObjects.Add(child.gameObject);
            else
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
        transform.GetChild(0).GetComponent<Collider>().enabled = _value;

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
            if (meshRenderer.GetComponent<Collider>() && !meshRenderer.transform.parent.GetComponent<Slot>())
                meshRenderer.GetComponent<Collider>().enabled = _collider;
        }

        foreach (MeshRenderer meshRenderer in slotChildMeshRendererList)
        {
            if (!meshRenderer.transform.parent.GetComponent<Slot>().used)
                meshRenderer.enabled = _value;
        }
    }

    ///<summary>
    /// When called fills lists and enables/disables children's MeshRenderers recursively.
    ///</summary>
    ///<param name="_value">Boolean value used when calling UpdateChildMeshRenderers</param>
    private void UpdateChildMeshRenderersRec(bool _value)
    {
        FillListsWithChildren();
        foreach (GameObject child in ogreeChildObjects)
            child.GetComponent<FocusHandler>().UpdateChildMeshRenderersRec(_value);

        FillMeshRendererLists();
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
            return;
        }
        transform.parent.GetComponent<FocusHandler>().ResetToRack();
    }
}
