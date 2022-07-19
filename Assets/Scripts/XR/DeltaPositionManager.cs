using UnityEngine;

public class DeltaPositionManager : MonoBehaviour
{
    public float yPositionDelta = 0.0f;
    public float initialYPosition = 0.0f;
    public bool isFirstMove = true;

    ///<summary>
    /// When an object is moved, keep track of the original position on the y axis
    ///</summary>
    public void OnMovingObject()
    {
        if (transform.parent.GetComponent<DeltaPositionManager>())
        {
            DeltaPositionManager delta = transform.parent.GetComponent<DeltaPositionManager>();
            if (isFirstMove)
            {
                if (delta.isFirstMove)
                {
                    initialYPosition = transform.position.y; //Problème ici pour les i helpers, déplcement sans focus du parent.
                    isFirstMove = false;
                }
                else
                {
                    initialYPosition = delta.initialYPosition + (transform.position.y - transform.parent.position.y); //Problème ici pour les i helpers, déplcement sans focus du parent.
                    isFirstMove = false;
                }
            }
        }
        else
        {
            if (isFirstMove)
            {
                initialYPosition = transform.position.y;
                isFirstMove = false;
            }
        }
    }

    ///<summary>
    /// When an object is not moving anymore, update the total translation on the y axis
    ///</summary>
    public void OnStoppingObject()
    {
        yPositionDelta = transform.position.y - initialYPosition;
    }
}
