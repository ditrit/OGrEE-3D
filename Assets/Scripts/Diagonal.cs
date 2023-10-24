using UnityEngine;

public class Diagonal : MonoBehaviour
{
    public Transform coordSystem;

    private void Update()
    {
            FollowtheSystem();
    }
    private void FollowtheSystem()
    {
        transform.localScale = Vector3.Scale(coordSystem.localPosition - transform.localPosition, Vector3.one - 2 * Vector3.forward);
    }
}
