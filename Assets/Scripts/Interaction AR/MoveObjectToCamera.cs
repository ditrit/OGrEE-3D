using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectToCamera : MonoBehaviour
{
    public Camera m_camera;

    void Start()
    {
        m_camera = Camera.main;
    }

    ///<summary>
    /// Modify an object position and rotation to place it in front of the main camera.
    ///</summary>
    ///<param name="_obj">Object to place in front of the camera</param>
    public void MoveObjectToCameraFunction(GameObject _obj)
    {
        
        float speed = 10f * Time.deltaTime;
        Vector3 offset = new Vector3(0.0f, 0.0f, 1.0f);
        _obj.transform.localPosition = Vector3.MoveTowards(_obj.transform.localPosition, m_camera.transform.localPosition + offset, speed);
        _obj.transform.localRotation = Quaternion.Slerp(_obj.transform.localRotation, Quaternion.Euler(m_camera.transform.localEulerAngles),
                                                    speed / Vector3.Distance(_obj.transform.localPosition, m_camera.transform.localPosition + offset));
    }
}
