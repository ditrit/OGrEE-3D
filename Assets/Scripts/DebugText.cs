using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DebugText : MonoBehaviour
{
    private TextMeshProUGUI txt;
    private int[] last100FPS = new int[100];
    private int currentIndex = 0;
    public int averageFPS { get; private set; }
    public int goCount { get; private set; }
    public int ooCount { get; private set; }

    private void Start()
    {
        txt = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        int currentFPS = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        if (currentIndex < last100FPS.Length)
        {
            last100FPS[currentIndex] = currentFPS;
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
            last100FPS[currentIndex] = currentFPS;
        }
        averageFPS = Average(last100FPS);

        txt.text = $"Objs: {ooCount}/{goCount}\nFPS: {averageFPS}";
    }

    ///<summary>
    /// Count the number of OgreeObject and GameObject in the scene.
    /// Display count of each OgreeObject type in CLI.
    ///</summary>
    public void CountObjects()
    {
        int tenantsCount = 0;
        int sitesCount = 0;
        int buildingsCount = 0;
        int roomsCount = 0;
        int racksCount = 0;
        int devicesCount = 0;

        ooCount = GameManager.instance.allItems.Count;
        goCount = GameObject.FindObjectsOfType<GameObject>().Length;

        foreach (DictionaryEntry de in GameManager.instance.allItems)
        {
            OgreeObject obj = ((GameObject)de.Value).GetComponent<OgreeObject>();
            switch (obj.category)
            {
                case "tenant":
                    tenantsCount++;
                    break;
                case "site":
                    sitesCount++;
                    break;
                case "building":
                    buildingsCount++;
                    break;
                case "room":
                    roomsCount++;
                    break;
                case "rack":
                    racksCount++;
                    break;
                case "device":
                    devicesCount++;
                    break;
            }
        }
        GameManager.instance.AppendLogLine($"Tenants: {tenantsCount}", true);
        GameManager.instance.AppendLogLine($"Sites: {sitesCount}", true);
        GameManager.instance.AppendLogLine($"Buildings: {buildingsCount}", true);
        GameManager.instance.AppendLogLine($"Rooms: {roomsCount}", true);
        GameManager.instance.AppendLogLine($"Racks: {racksCount}", true);
        GameManager.instance.AppendLogLine($"Devices: {devicesCount}", true);
    }

    ///<summary>
    /// Compute the average value of an array.
    ///</summary>
    ///<param name="_array">The array to compute</param>
    ///<returns>The average og the array</returns>
    private int Average(int[] _array)
    {
        int sum = 0;
        for (int i = 0; i < _array.Length; i++)
            sum += _array[i];

        return sum / _array.Length;
    }
}
