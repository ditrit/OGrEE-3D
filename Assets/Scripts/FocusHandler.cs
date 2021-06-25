using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SubscribeEvents() {

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocus);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocus);
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }



    public void UnsubscribeEvents() {

        EventManager.Instance.RemoveListener<OnFocusEvent>(OnFocus);
        EventManager.Instance.RemoveListener<OnUnFocusEvent>(OnUnFocus);
        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    //when we focus on the object we react to the event and enable all renderers
    private void OnFocus(OnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            UpdateChildMeshRenderers(true);
        }

    }

    //when we unfocus on the object we react to the event and disable all renderers
    private void OnUnFocus(OnUnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            UpdateChildMeshRenderers(false);
        }
    }

    private void OnImportFinished(ImportFinishedEvent e) {

        FillListsWithChildren();
        FillMeshRendererLists();

        //UpdateChildRendererList();
        ManualUnFocus();
    }

    private void ManualUnFocus() {
        UpdateChildMeshRenderers(false);
    }

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

    private void UpdateChildMeshRenderers(bool value) {
        foreach(MeshRenderer meshRenderer in OgreeChildMeshRendererList) {
            meshRenderer.enabled = value;
        }

        foreach(MeshRenderer meshRenderer in SlotChildMeshRendererList) {
            meshRenderer.enabled = value;
        }
    }

}
