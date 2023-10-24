using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextScaler : MonoBehaviour
{
    [SerializeField] private float scale;
    private Transform text;
    private void Awake()
    {
        text = transform.GetChild(0);
    }
    private void Update()
    {
        Scale();
    }

    private void Scale()
    {
        text.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.forward) + 0.001f * Vector3.forward;
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit raycastHit);
        Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward),Color.yellow,0.2f,false);
        if (raycastHit.collider && raycastHit.distance > text.lossyScale.x)
            text.localPosition = raycastHit.distance / 2 * Vector3.forward;
        else
            text.localPosition = new Vector3(0.02f, 0, 0.02f);
    }
}
