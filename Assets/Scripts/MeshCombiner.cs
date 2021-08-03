using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// POC, NOT USED
///</summary>
public class MeshCombiner : MonoBehaviour
{

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<ImportFinishedEvent>(OnImportFinished);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<ImportFinishedEvent>(OnImportFinished);
    }

    private void OnImportFinished(ImportFinishedEvent e)
    {
        CombineRenderers();
    }

    public void CombineRenderers()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        long i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);

    }
}
