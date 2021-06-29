using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// Class responsible for increasing performance by culling the child's MeshRenderers when the GameObject isnt Focused by the user.
///</summary>
public class FocusHandler : MonoBehaviour {

    public List<GameObject> RackOwnObjectsList;
    public List<GameObject> OgreeChildObjects;
    public List<GameObject> SlotsChildObjects;

    public List<MeshRenderer> OgreeChildMeshRendererList;
    public List<MeshRenderer> SlotChildMeshRendererList;

    private Rack rack;

    private void Awake() {
        SubscribeEvents();
        rack = GetComponent<Rack>();
    }

    private void OnDestroy() {
        UnsubscribeEvents();
    }

    ///<summary>
    /// Subscribe the GameObject to Events
    ///</summary>
    public void SubscribeEvents() {

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocus);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocus);
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    ///<summary>
    /// Unsubscribe the GameObject to Events
    ///</summary>
    public void UnsubscribeEvents() {

        EventManager.Instance.RemoveListener<OnFocusEvent>(OnFocus);
        EventManager.Instance.RemoveListener<OnUnFocusEvent>(OnUnFocus);
        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }
    

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnFocus(OnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            UpdateChildMeshRenderers(true);
        }

    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true deactivates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnUnFocus(OnUnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            UpdateChildMeshRenderers(false);
        }
    }

    ///<summary>
    /// When called, fills all the lists and does a ManualUnFocus to deactivate all useless mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent e) {

        FillListsWithChildren();
        FillMeshRendererLists();
        ManualUnFocus();
    }

    ///<summary>
    /// Used to imitate an Unfocus to disable all useless mesh renderers.
    ///</summary>
    private void ManualUnFocus() {
        UpdateChildMeshRenderers(false);
    }

    ///<summary>
    /// Fills the 3 Child list with their corresponding content.
    ///</summary>
    private void FillListsWithChildren() {
        OgreeChildObjects.Clear();
        RackOwnObjectsList.Clear();
        SlotsChildObjects.Clear();

        foreach(Transform child in transform) {
            if(child.GetComponent<OgreeObject>()) {
                OgreeChildObjects.Add(child.gameObject);
            } else if(child.GetComponent<Slot>()) {
                SlotsChildObjects.Add(child.gameObject);
            } else {
                RackOwnObjectsList.Add(child.gameObject);
            }
        }
    }

    ///<summary>
    /// Fills the Mesh renderer Lists from the OgreeChildObjects and SlotsChildObjects lists.
    ///</summary>
    private void FillMeshRendererLists() {
        OgreeChildMeshRendererList.Clear();
        SlotChildMeshRendererList.Clear();

        foreach(GameObject gameObject in OgreeChildObjects) {
            MeshRenderer[] OgreeChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer meshRenderer in OgreeChildMeshRenderers) {
                OgreeChildMeshRendererList.Add(meshRenderer);
            }
        }

        foreach(GameObject gameObject in SlotsChildObjects) {
            MeshRenderer[] SlotChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer meshRenderer in SlotChildMeshRenderers) {
                SlotChildMeshRendererList.Add(meshRenderer);
            }
        }
    }

    ///<summary>
    /// When called enables/disables the child MeshRenderers located in the OgreeChildMeshRendererList and SlotChildMeshRendererList depending on the boolean argument.
    ///</summary>
    ///<param name="value">Boolean value assigned to the meshRenderer.enabled </param>
    private void UpdateChildMeshRenderers(bool value) {
        foreach(MeshRenderer meshRenderer in OgreeChildMeshRendererList) {
            meshRenderer.enabled = value;
        }

        foreach(MeshRenderer meshRenderer in SlotChildMeshRendererList) {
            meshRenderer.enabled = value;
        }
    }

}
