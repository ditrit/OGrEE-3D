using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRendererOutline : MonoBehaviour
{
    public Material selectedMaterial;
    public Material defaultMaterial;

    public bool isActive = false;

    private void Start()
    {
        if (GetComponent<OObject>() || GetComponent<Rack>() || GetComponent<Group>())
        {
            isActive = true;
        }

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
    }

    ///<summary>
    /// Unsubscribe the GameObject to Events
    ///</summary>
    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.RemoveListener<OnDeselectItemEvent>(OnDeselectItem);
    }


    ///<summary>
    /// When called checks if he is the GameObject focused on and if true activates all of his child's mesh renderers.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnSelectItem(OnSelectItemEvent e)
    {
        if (e._obj.Equals(gameObject))
        {
            transform.GetChild(0).GetComponent<Renderer>().material = selectedMaterial;
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
            transform.GetChild(0).GetComponent<Renderer>().material = defaultMaterial;
        }
    }
}
