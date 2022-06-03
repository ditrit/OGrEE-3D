using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportHandler : MonoBehaviour
{

    public static TeleportHandler instance;
    public GameObject mixedRealtyPlaySpace;
    public GameObject debugCube;
    private bool shouldTP = false;
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    public Room lastRoomLoaded = null;
    private Room previousRoomLoaded = null;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);

    }

    private void OnImportFinished(ImportFinishedEvent e)
    {
        if (shouldTP)
        {
            print("second imp");
            StartCoroutine(Test());
            //mixedRealtyPlaySpace.transform.position = GetEmptyPosInRoom() + Vector3.up * mixedRealtyPlaySpace.transform.GetChild(0).position.y;
            shouldTP = false;
        }
        if (lastRoomLoaded != previousRoomLoaded)
        {
            print("first imp");
            shouldTP = true;
            previousRoomLoaded = lastRoomLoaded;
        }
    }

    private IEnumerator Test()
    {
        yield return new WaitForFixedUpdate();
        mixedRealtyPlaySpace.transform.position = GetEmptyPosInRoom() + Vector3.up * mixedRealtyPlaySpace.transform.GetChild(0).position.y;

    }

    private Vector3 GetEmptyPosInRoom()
    {
        float x = (lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[0].x - lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120].x) / GameManager.gm.tileSize * lastRoomLoaded.usableZone.transform.localScale.x;
        float z = (lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[0].z - lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120].z) / GameManager.gm.tileSize * lastRoomLoaded.usableZone.transform.localScale.z;

        Vector3 rootPos = lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120];
        rootPos += new Vector3(GameManager.gm.tileSize / lastRoomLoaded.usableZone.transform.lossyScale.x, 0.002f, GameManager.gm.tileSize / lastRoomLoaded.usableZone.transform.lossyScale.z) / 2;

        for (int j = 0; j < z; j++)
        {
            for (int i = 0; i < x; i++)
            {
                Vector3 pos = lastRoomLoaded.usableZone.TransformPoint(rootPos + new Vector3(i / lastRoomLoaded.usableZone.transform.lossyScale.x, 0, j / lastRoomLoaded.usableZone.transform.lossyScale.z) * GameManager.gm.tileSize);
                Instantiate(debugCube, pos + GameManager.gm.tileSize * Vector3.up, Quaternion.identity);
                print(pos);
                if (IsTileFree(pos))
                    return pos;
            }
        }
        return lastRoomLoaded.usableZone.position;
    }

    private bool IsTileFree(Vector3 _pos)
    {
        Collider[] hitColliders = Physics.OverlapSphere(_pos +  GameManager.gm.tileSize * Vector3.up, GameManager.gm.tileSize / 2);
        for (int i = 0; i < hitColliders.Length; i++)
            print("collider :" + hitColliders[i].name);
        return false;
    }
    private void OnDrawGizmos()
    {
        //if (lastRoomLoaded != null)
        //{
        //    float x = (lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[0].x - lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120].x) / GameManager.gm.tileSize * lastRoomLoaded.usableZone.transform.localScale.x;
        //    float z = (lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[0].z - lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120].z) / GameManager.gm.tileSize * lastRoomLoaded.usableZone.transform.localScale.z;

        //    Vector3 rootPos = lastRoomLoaded.usableZone.GetComponent<MeshFilter>().sharedMesh.vertices[120];
        //    rootPos += new Vector3(GameManager.gm.tileSize / lastRoomLoaded.usableZone.transform.lossyScale.x, 0.002f, GameManager.gm.tileSize / lastRoomLoaded.usableZone.transform.lossyScale.z) / 2;

        //    for (int j = 0; j < z; j++)
        //    {
        //        for (int i = 0; i < x; i++)
        //        {
        //            Vector3 pos = lastRoomLoaded.usableZone.TransformPoint(rootPos + new Vector3(i / lastRoomLoaded.usableZone.transform.lossyScale.x, 0, j / lastRoomLoaded.usableZone.transform.lossyScale.z) * GameManager.gm.tileSize);
        //            Gizmos.DrawSphere(pos + GameManager.gm.tileSize * Vector3.up, GameManager.gm.tileSize/2);
        //        }
        //    }
        //}
    }

}
