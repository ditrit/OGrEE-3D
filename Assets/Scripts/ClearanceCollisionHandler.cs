using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearanceCollisionHandler : MonoBehaviour
{
    [SerializeField] private new Renderer renderer;
    public Transform ownObject;
    [SerializeField] private int collisionCount = 0;
    private void OnTriggerEnter(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);
        if (other.transform.IsChildOf(ownObject) || other.transform.parent?.GetComponent<Building>())
            return;
        print($"{other.transform.parent?.parent?.parent?.name}/{other.transform.parent?.parent?.name}/{other.transform.parent?.name}/{other.gameObject?.name}");
        collisionCount++;
        renderer.material.color = Color.red;
    }

    private void OnTriggerExit(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);
        if (other.transform.IsChildOf(ownObject) || other.transform.parent?.GetComponent<Building>())
            return;
        collisionCount--;
        if (collisionCount == 0)
            renderer.material.color = Color.green;
    }
}
