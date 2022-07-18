using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TabletteTouchManager : MonoBehaviour
{
    [SerializeField] ARRaycastManager m_RaycastManager;
    List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    Camera arCam;

    [Range(5, 20)]
    public float moveSpeed = 15;
    [Range(20, 100)]
    public float rotationSpeed = 50;
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
                FreeModeControls();
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
            }
        }
    }

    ///<summary>
    /// Controls for "AntMan" mode.
    ///</summary>
    private void FreeModeControls()
    {
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.Rotate(-Input.GetAxis("Vertical") * Time.deltaTime * rotationSpeed, 0, 0);
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * rotationSpeed, 0, Space.World);
        }
    }

    /* Rajouter bouton select parent, focus, edit
    Rajouter inputs
    Fix le placement de l'objet qui n'est toujours pas bon
    Fix le placement sur un plan ?
    Comment gérer la photo ?
    Enlever la cli
    */

}
