using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBehaviour : MonoBehaviour
{
    private void OnEnable()
    {
        transform.parent.localScale = Vector3.one;
    }

    public void ResetScale(GameObject obj)
    {
        StartCoroutine(Reset(obj));

    }

    private IEnumerator Reset(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        obj.transform.localScale = Vector3.one;

    }
}
