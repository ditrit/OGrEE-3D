using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TabletteTouchManager : MonoBehaviour
{
    [SerializeField] ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    Camera arCam;

    // Start is called before the first frame update
    void Start()
    {
        arCam = GameObject.Find("AR Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0)
            return;

        RaycastHit hit;
        Ray ray = arCam.ScreenPointToRay(Input.GetTouch(0).position);
        if (m_RaycastManager.Raycast(Input.GetTouch(0).position, m_Hits))
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject)
                    {
                        hit.collider.gameObject.GetComponent<HandInteractionHandler>().SelectThis();
                    }
                    else
                    {
                    }
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
            }
        }
    }
}