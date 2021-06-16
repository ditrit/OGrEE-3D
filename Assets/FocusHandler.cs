using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusHandler : MonoBehaviour {
    public List<MeshRenderer> meshRendererList;

    private void Start() {
        UpdateChildRendererList();

        //manual unfocus on start
        ManualUnFocus();

    }

    private void Awake() {
        SubscribeEvents();
    }

    private void OnDestroy() {
        UnsubscribeEvents();
    }

    public void SubscribeEvents() {

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocus);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocus);
    }



    public void UnsubscribeEvents() {

        EventManager.Instance.RemoveListener<OnFocusEvent>(OnFocus);
        EventManager.Instance.RemoveListener<OnUnFocusEvent>(OnUnFocus);
    }

    //when we focus on the object we react to the event and enable all renderers
    private void OnFocus(OnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            Debug.Log("focus");
            foreach(MeshRenderer meshRenderer in meshRendererList) {
                meshRenderer.enabled = true;
            }
        }
        
    }

    //when we unfocus on the object we react to the event and disable all renderers
    private void OnUnFocus(OnUnFocusEvent e) {
        if(e._obj.Equals(gameObject)) {
            Debug.Log("unfocus");
            foreach(MeshRenderer meshRenderer in meshRendererList) {
                meshRenderer.enabled = false;
            }
        }
    }

    private void ManualUnFocus() {
        Debug.Log("Manual unfocus");
        foreach(MeshRenderer meshRenderer in meshRendererList) {
            meshRenderer.enabled = false;
        }
    }



    //We update the list of renderers 
    private void UpdateChildRendererList() {
        meshRendererList.Clear();

        MeshRenderer[] meshRendererArray = GetComponentsInChildren<MeshRenderer>();

        //checking if it has more renderers than 7 since 7 is the minimum amount for an empty rack with no sons
        if(meshRendererArray.Length > 7) {
            foreach(MeshRenderer meshRenderer in meshRendererArray) {
                meshRendererList.Add(meshRenderer);
            }

            //the first ones are always the box and the labels so i remove them
            for(int i = 0; i < 7; i++)
                meshRendererList.Remove(meshRendererArray[i]);
        }

        
    }
}
