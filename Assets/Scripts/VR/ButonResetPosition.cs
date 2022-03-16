using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButonResetPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);
        EventManager.Instance.AddListener<OnDeselectItemEvent>(OnDeselectItem);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            CheckChildrenPositions();
        }
    }

    public void ResetAllPositions()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i).GetComponent<OgreeObject>() == null)
            {
                continue;
            }
            transform.parent.GetChild(i).GetComponent<OgreeObject>().ResetPosition();
        }
    }

    private void CheckChildrenPositions()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform ithChild = transform.parent.GetChild(i);
            OgreeObject ogree = ithChild.GetComponent<OgreeObject>();
            if (ogree == null)
            {
                continue;
            }
            if (ithChild.localPosition.x < ogree.originalLocalPosition.x)
            {
                ogree.ResetPosition();
            }
        }
    }

    private void InitButton(Vector3 _position, Quaternion _rotation)
    {
        gameObject.transform.localPosition = _position;
        gameObject.transform.localRotation = _rotation;
    }

    private void OnSelectItem(OnSelectItemEvent _e)
    {
        gameObject.SetActive(true);
        gameObject.transform.parent = _e.obj.transform;
        InitButton(new Vector3(_e.obj.transform.localScale.x / 2, _e.obj.transform.localScale.y / 2, _e.obj.transform.localScale.z / 2), Quaternion.Euler(0, -180, 0));

    }
    private void OnDeselectItem(OnDeselectItemEvent _e)
    {
        gameObject.SetActive(false);
    }
}
