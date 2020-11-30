using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AServerItem : MonoBehaviour
{
    public new string name;
    public int id;
    public int parentId;

    private Coroutine updatingCoroutine = null;

    ///<summary>
    /// Set id.
    ///</summary>
    ///<param name="_id">The id to set</param>
    public void UpdateId(int _id)
    {
        id = _id;
    }

    ///<summary>
    /// If a WaitAndPut coroutine is running, stop it. Then, start WaitAndPut.
    ///</summary>
    public void PutData()
    {
        if (updatingCoroutine != null)
            StopCoroutine(updatingCoroutine);
        updatingCoroutine = StartCoroutine(WaitAndPut());
    }

    ///<summary>
    /// Wait 2 seconds and call ApiManager.CreatePutRequest().
    ///</summary>
    private IEnumerator WaitAndPut()
    {
        yield return new WaitForSeconds(2f);
        string hierarchyName = GetComponent<HierarchyName>()?.fullname;
        ApiManager.instance.CreatePutRequest(hierarchyName);
    }
}
