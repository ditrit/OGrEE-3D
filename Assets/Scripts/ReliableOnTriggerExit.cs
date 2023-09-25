using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// OnTriggerExit is not called if the triggering object is destroyed, set inactive, or if the collider is disabled. This script fixes that
//
// Usage: Wherever you read OnTriggerEnter() and want to consistently get OnTriggerExit
// In OnTriggerEnter() call ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);
// In OnTriggerExit call ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);
//
// Algorithm: Each ReliableOnTriggerExit is associated with a collider, which is added in OnTriggerEnter via NotifyTriggerEnter
// Each ReliableOnTriggerExit keeps track of OnTriggerEnter calls
// If ReliableOnTriggerExit is disabled or the collider is not enabled, call all pending OnTriggerExit calls
public class ReliableOnTriggerExit : MonoBehaviour
{
    public delegate void _OnTriggerExit(Collider c);

    Collider thisCollider;
    bool ignoreNotifyTriggerExit = false;

    // Target callback
    readonly Dictionary<GameObject, _OnTriggerExit> waitingForOnTriggerExit = new();

    public static void NotifyTriggerEnter(Collider _c, GameObject _caller, _OnTriggerExit _onTriggerExit)
    {
        ReliableOnTriggerExit thisComponent = null;
        ReliableOnTriggerExit[] ftncs = _c.GetComponents<ReliableOnTriggerExit>();
        foreach (ReliableOnTriggerExit ftnc in ftncs)
        {
            if (ftnc.thisCollider == _c)
            {
                thisComponent = ftnc;
                break;
            }
        }
        if (!thisComponent)
        {
            thisComponent = _c.gameObject.AddComponent<ReliableOnTriggerExit>();
            thisComponent.thisCollider = _c;
        }
        // Unity bug? (!!!!): Removing a Rigidbody while the collider is in contact will call OnTriggerEnter twice, so I need to check to make sure it isn't in the list twice
        // In addition, force a call to NotifyTriggerExit so the number of calls to OnTriggerEnter and OnTriggerExit match up
        if (!thisComponent.waitingForOnTriggerExit.ContainsKey(_caller))
        {
            thisComponent.waitingForOnTriggerExit.Add(_caller, _onTriggerExit);
            thisComponent.enabled = true;
        }
        else
        {
            thisComponent.ignoreNotifyTriggerExit = true;
            thisComponent.waitingForOnTriggerExit[_caller].Invoke(_c);
            thisComponent.ignoreNotifyTriggerExit = false;
        }
    }

    public static void NotifyTriggerExit(Collider _c, GameObject _caller)
    {
        if (!_c)
            return;

        ReliableOnTriggerExit thisComponent = null;
        ReliableOnTriggerExit[] ftncs = _c.GetComponents<ReliableOnTriggerExit>();
        foreach (ReliableOnTriggerExit ftnc in ftncs)
        {
            if (ftnc.thisCollider == _c)
            {
                thisComponent = ftnc;
                break;
            }
        }
        if (thisComponent && !thisComponent.ignoreNotifyTriggerExit)
        {
            thisComponent.waitingForOnTriggerExit.Remove(_caller);
            if (thisComponent.waitingForOnTriggerExit.Count == 0)
                thisComponent.enabled = false;
        }
    }
    private void OnDisable()
    {
        if (!gameObject.activeInHierarchy)
            CallCallbacks();
    }
    private void Update()
    {
        if (!thisCollider)
        {
            // Will GetOnTriggerExit with null, but is better than no call at all
            CallCallbacks();

            Destroy(this);
        }
        else if (!thisCollider.enabled)
            CallCallbacks();
    }
    void CallCallbacks()
    {
        ignoreNotifyTriggerExit = true;
        foreach (KeyValuePair<GameObject,_OnTriggerExit> v in waitingForOnTriggerExit)
        {
            if (!v.Key)
                continue;
            v.Value.Invoke(thisCollider);
        }
        ignoreNotifyTriggerExit = false;
        waitingForOnTriggerExit.Clear();
        enabled = false;
    }
}

