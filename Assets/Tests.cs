using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tests : MonoBehaviour
{
    public bool pause = false;

    [Header("Spawner")]
    public int limit = 50000;
    public GameObject prefab;
    public bool reset = false;

    private bool twoScenes = false;
    private GameObject cam;

    private void Start()
    {
        // Multilines();
        cam = Camera.main.gameObject;
    }


    private void Update()
    {
        if (pause)
            return;

        Spawner();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!twoScenes)
                StartCoroutine(LoadAsyncScene());
            else
                StartCoroutine(UnloadAsyncScene());

        }
    }

    private void Multilines()
    {
        string str = $@"1 {
            2
            }


    	3";

        Debug.Log(str);
        List<string> lst = new List<string>(Regex.Split(str, System.Environment.NewLine));
        Debug.Log($"Before cleaning empty lines : {lst.Count}");
        lst.RemoveAll(string.IsNullOrEmpty);
        Debug.Log($"After cleaning empty lines : {lst.Count}");
    }

    private void Spawner()
    {
        if (reset)
        {
            pause = true;
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            reset = false;
        }
        else
        {
            int count = GameObject.FindObjectsOfType<GameObject>().Length;
            if (count < limit)
            {
                Instantiate(prefab, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100)),
                    Quaternion.identity, transform);
            }
        }
    }

    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;
        cam.SetActive(false);
        twoScenes = true;
    }

    IEnumerator UnloadAsyncScene()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(1);
        while (!asyncUnload.isDone)
            yield return null;
        cam.SetActive(true);
        twoScenes = false;
    }

}
