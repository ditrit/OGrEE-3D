using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diagonal : MonoBehaviour
{
    public Transform center;
    public bool isActive;
    // Update is called once per frame
    void Update()
    {
        if (isActive)
            FollowTheCenter();
    }
    private void FollowTheCenter()
    {
        transform.localScale = center.localPosition - transform.localPosition;
        transform.localScale = Vector3.Scale(transform.localScale, Vector3.one - 2 * Vector3.forward);
    }
}
