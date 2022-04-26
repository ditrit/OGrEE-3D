using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfTest : MonoBehaviour
{
    public GameObject prefab = null;
    public int minFPS = 20;

    private DebugText debugText = null;
    [SerializeField] private bool isReady = false;

    private void Start()
    {
        debugText = GameObject.FindObjectOfType<DebugText>();
        StartCoroutine(UnlockSpawner());
    }

    private void Update()
    {
        if (isReady)
            Spawner();
    }

    ///<summary>
    /// Instantiate a prefab at a random position until the averageFPS drops below minFPS.
    ///</summary>
    private void Spawner()
    {
        Vector3 pos = new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
        Instantiate(prefab, pos, Quaternion.identity, transform);

        if (debugText.averageFPS < minFPS)
            isReady = false;
    }

    ///<summary>
    /// Turn isReady to true when averageFPS exceeds minFPS.
    ///</summary>
    private IEnumerator UnlockSpawner()
    {
        yield return new WaitUntil(() => debugText.averageFPS > minFPS);
        isReady = true;
    }
}
