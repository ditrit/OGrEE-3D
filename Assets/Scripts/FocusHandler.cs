using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// Class responsible for increasing performance by culling the child's MeshRenderers when the GameObject isnt Focused by the user.
///</summary>
public class FocusHandler : MonoBehaviour {

    public List<GameObject> rackOwnObjectsList;
    public List<GameObject> ogreeChildObjects;
    public List<GameObject> slotsChildObjects;

    public List<MeshRenderer> ogreeChildMeshRendererList;
    public List<MeshRenderer> slotChildMeshRendererList;

    public Material focusedMaterial;
    public Material defaultMaterial;

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

            transform.GetChild(0).GetComponent<Renderer>().material = focusedMaterial;
        }

    }

    ///<summary>
    /// When called checks if he is the GameObject focused on and if true deactivates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnUnFocus(OnUnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            UpdateChildMeshRenderers(false);

            transform.GetChild(0).GetComponent<Renderer>().material = defaultMaterial;
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
        ogreeChildObjects.Clear();
        rackOwnObjectsList.Clear();
        slotsChildObjects.Clear();

        foreach(Transform child in transform) {
            if(child.GetComponent<OgreeObject>()) {
                ogreeChildObjects.Add(child.gameObject);
            } else if(child.GetComponent<Slot>()) {
                slotsChildObjects.Add(child.gameObject);
            } else {
                rackOwnObjectsList.Add(child.gameObject);
            }
        }
    }

    ///<summary>
    /// Fills the Mesh renderer Lists from the OgreeChildObjects and SlotsChildObjects lists.
    ///</summary>
    private void FillMeshRendererLists() {
        ogreeChildMeshRendererList.Clear();
        slotChildMeshRendererList.Clear();

        foreach(GameObject gameObject in ogreeChildObjects) {
            MeshRenderer[] OgreeChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer meshRenderer in OgreeChildMeshRenderers) {
                ogreeChildMeshRendererList.Add(meshRenderer);
            }
        }

        foreach(GameObject gameObject in slotsChildObjects) {
            MeshRenderer[] SlotChildMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer meshRenderer in SlotChildMeshRenderers) {
                slotChildMeshRendererList.Add(meshRenderer);
            }
        }
    }

    ///<summary>
    /// When called enables/disables the child MeshRenderers located in the OgreeChildMeshRendererList and SlotChildMeshRendererList depending on the boolean argument.
    ///</summary>
    ///<param name="value">Boolean value assigned to the meshRenderer.enabled </param>
    private void UpdateChildMeshRenderers(bool value) {
        foreach(MeshRenderer meshRenderer in ogreeChildMeshRendererList) {
            meshRenderer.enabled = value;
        }

        foreach(MeshRenderer meshRenderer in slotChildMeshRendererList) {
            meshRenderer.enabled = value;
        }
    }

}
