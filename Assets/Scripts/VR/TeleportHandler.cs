using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportHandler : MonoBehaviour
{

    public Room lastRoomLoaded = null;
    public static TeleportHandler instance;
    public GameObject mixedRealtyPlaySpace;
    public GameObject mainCamera;

    private bool shouldTP = false;
    private Room previousRoomLoaded = null;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);

    }

    ///<summary>
    /// When called, checks if children of the last loaded room are loaded and teleport to the last loaded room if yes.
    ///</summary>
    ///<param name="e">The event's instance</param>
    private void OnImportFinished(ImportFinishedEvent e)
    {
        if (shouldTP)
        {
            StartCoroutine(TeleportToRoomDelayed(lastRoomLoaded));
            //mixedRealtyPlaySpace.transform.position = GetEmptyPosInRoom() + Vector3.up * mixedRealtyPlaySpace.transform.GetChild(0).position.y;
            shouldTP = false;
        }
        if (lastRoomLoaded != previousRoomLoaded)
        {
            shouldTP = true;
            previousRoomLoaded = lastRoomLoaded;
        }
    }


    ///<summary>
    /// Wait for the end of the frame to teleport
    ///</summary>
    private IEnumerator TeleportToRoomDelayed(Room _room)
    {
        yield return new WaitForFixedUpdate();
        TeleportToRoom(_room);

    }


    ///<summary>
    /// Teleport to an empty tile of the room
    ///</summary>
    ///<param name="_room">The room to be teleported in</param>
    public void TeleportToRoom(Room _room)
    {
        Vector3 targetPosition = GetEmptyPosInRoom(_room);
        float height = targetPosition.y;
        targetPosition -= mainCamera.transform.position - mixedRealtyPlaySpace.transform.position;
        targetPosition.y = height;

        mixedRealtyPlaySpace.transform.position = targetPosition;
    }


    ///<summary>
    /// Return an empty tile in the usable zone of the room
    ///</summary>
    ///<param name="_room">The room checked</param>
    ///<returns> A Vector3 containing the global position of the empty tile, or the center of the room if all tiles are occupied</returns>
    private Vector3 GetEmptyPosInRoom(Room _room)
    {
        float x = (_room.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[0].x - _room.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120].x) / GameManager.gm.tileSize * _room.usableZone.transform.localScale.x;
        float z = (_room.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[0].z - _room.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120].z) / GameManager.gm.tileSize * _room.usableZone.transform.localScale.z;

        Vector3 rootPos = _room.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120];
        rootPos += new Vector3(GameManager.gm.tileSize / _room.usableZone.transform.lossyScale.x, 0.002f, GameManager.gm.tileSize / _room.usableZone.transform.lossyScale.z) / 2;

        for (int j = 0; j < z; j++)
        {
            for (int i = 0; i < x; i++)
            {
                Vector3 pos = _room.usableZone.TransformPoint(rootPos + new Vector3(i / _room.usableZone.transform.lossyScale.x, 0, j / _room.usableZone.transform.lossyScale.z) * GameManager.gm.tileSize);
                if (IsTileFree(pos))
                    return pos;
            }
        }
        return _room.usableZone.position;
    }


    ///<summary>
    /// Check if there are no collider on top of a position
    ///</summary>
    ///<param name="_pos">The position checked</param>
    ///<<returns> If the position if free or not</returns>
    private bool IsTileFree(Vector3 _pos)
    {
        Collider[] hitColliders = Physics.OverlapSphere(_pos + GameManager.gm.tileSize * Vector3.up, GameManager.gm.tileSize);
        return hitColliders.Length > 0;
    }
  
}
