using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportHandler : MonoBehaviour
{

    public static TeleportHandler instance;
    public Camera VRcamera;
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    public GameObject lastRoomLoaded = null;
    private GameObject previousRoomLoaded = null;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);

    }

    private void OnImportFinished(ImportFinishedEvent e)
    {
        if (lastRoomLoaded != previousRoomLoaded)
        {
            VRcamera.transform.position = (lastRoomLoaded.GetComponent<Room>().usableZone.position + Vector3.up);
        }
        previousRoomLoaded = lastRoomLoaded;
    }
}
