using UnityEngine;

/// <summary>
/// Unity doesn't fire a OnTriggerExit when a gameobject is disabled or destroyed, so I attach this script to
/// anything colliding with a clearance to get notified should it be disabled
/// </summary>
public class TriggerExitOnDisable : MonoBehaviour
{
    public ClearanceCollisionHandler collisionHandler;
    public bool shouldFire;

    /// <summary>
    /// Subtract a collision to the collision count of the clearance if shouldFire is on
    /// </summary>
    private void OnDisable()
    {
        if (shouldFire)
            collisionHandler.SubstractCollision();
        shouldFire = false;
    }
}
