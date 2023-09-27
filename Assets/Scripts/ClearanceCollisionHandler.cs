using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearanceCollisionHandler : MonoBehaviour
{
    [SerializeField] private new Renderer renderer;
    public Transform ownObject;
    [SerializeField] private int collisionCount = 0;

    /// <summary>
    /// Check if the other collider should be reacted to. If it is, give it a <see cref="TriggerExitOnDisable"/><br/>
    /// if it hasn't got one already and set it to notify us should the other gameobject be disabled.<br/>
    /// Then add one to the collision count and set the clearance color to red.
    /// </summary>
    /// <param name="other">the collider colliding with us</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.IsChildOf(ownObject) || other.transform.parent?.GetComponent<Building>())
            return;
        if (!other.TryGetComponent(out TriggerExitOnDisable trigger))
            trigger = other.gameObject.AddComponent<TriggerExitOnDisable>();
        trigger.shouldFire = true;
        trigger.collisionHandler = this;
        collisionCount++;
        renderer.material.color = Color.red;
    }

    /// <summary>
    /// Check if the other collider should be reacted to. If it is, deactivate its <see cref="TriggerExitOnDisable"/>
    /// and substract one to the collision count and set the clearance color to green if it reaches 0
    /// </summary>
    /// <param name="other">the collider colliding with us</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.IsChildOf(ownObject) || other.transform.parent?.GetComponent<Building>())
            return;
        other.GetComponent<TriggerExitOnDisable>().shouldFire = false;
        SubstractCollision();
    }

    /// <summary>
    /// Substract one to the collision count and set the clearance color to green if it reaches 0
    /// </summary>
    public void SubstractCollision()
    {
        collisionCount--;
        if (collisionCount == 0)
            renderer.material.color = Color.green;
    }


    /// <summary>
    /// Reset the collision count and the clearane color on disabling
    /// </summary>
    private void OnDisable()
    {
        collisionCount = 0;
        renderer.material.color = Color.green;
    }
}
