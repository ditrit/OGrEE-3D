using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Linq;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class DeltaPositionManager : MonoBehaviour
{
    public float yPositionDelta = 0.0f;
    public float initialYPosition = 0.0f;
    public Vector3 initialScale = Vector3.one;
    public Vector3 finalScale = Vector3.one;
    public float halfheightBox = 0f;
    public float centerbefore;
    public float centerafter;
    public bool isFirstMove = true;
    public float yRotation;

    private void Start()
    {
        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);
    }

    private void OnEditModeIn(EditModeInEvent _e)
    {
        Transform t = transform.GetChild(0);
        centerbefore = t.position.y;
        if (t.GetComponent<BoxCollider>().enabled)
            halfheightBox = t.GetComponent<BoxCollider>().bounds.extents.y;
        else
        {
            t.GetComponent<BoxCollider>().enabled = true;
            halfheightBox = t.GetComponent<BoxCollider>().bounds.extents.y;  
            t.GetComponent<BoxCollider>().enabled = false;
        }
    }

    private void OnEditModeOut(EditModeOutEvent _e)
    {
        finalScale = _e.obj.transform.lossyScale;
        Transform t = transform.GetChild(0);
        centerafter = t.position.y;
        yRotation = transform.eulerAngles.y;

    }

    public void OnHoverIn()
    {
        if (transform.parent.GetComponent<DeltaPositionManager>())
        {
            DeltaPositionManager delta = transform.parent.GetComponent<DeltaPositionManager>();
            if (isFirstMove)
            {
                initialYPosition = delta.initialYPosition + (transform.position.y - transform.parent.position.y);
                isFirstMove = false;
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

    public void OnHoverOut()
    {
        yPositionDelta = transform.position.y - initialYPosition;
    }
}
