using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DebugText : MonoBehaviour
{
    [SerializeField] private LocalizeStringEvent localizedStr;
    private readonly int[] last100FPS = new int[100];
    private int currentIndex = 0;
#pragma warning disable IDE1006 // Name assignment styles
    public int averageFPS { get; private set; }
    public int goCount { get; private set; }
    public int ooCount { get; private set; }
#pragma warning restore IDE1006 // Name assignment styles

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
        localizedStr.RefreshString();
    }

    ///<summary>
    /// Count the number of OgreeObject and GameObject in the scene.
    /// Display count of each OgreeObject type in CLI.
    ///</summary>
    public void CountObjects()
    {
        int sitesCount = 0;
        int buildingsCount = 0;
        int roomsCount = 0;
        int racksCount = 0;
        int devicesCount = 0;
        int componentCount = 0;

        ooCount = GameManager.instance.allItems.Count;
        goCount = FindObjectsOfType<GameObject>().Length;

        foreach (DictionaryEntry de in GameManager.instance.allItems)
        {
            OgreeObject obj = ((GameObject)de.Value).GetComponent<OgreeObject>();
            switch (obj)
            {
                case Site:
                    sitesCount++;
                    break;
                case Building and not Room:
                    buildingsCount++;
                    break;
                case Room:
                    roomsCount++;
                    break;
                case Rack:
                    racksCount++;
                    break;
                case Device device:
                    if (device.isComponent)
                        componentCount++;
                    else
                        devicesCount++;
                    break;
            }
        }
        List<string> countList = new()
        {
            sitesCount.ToString(),
            buildingsCount.ToString(),
            roomsCount.ToString(),
            racksCount.ToString(),
            devicesCount.ToString(),
            componentCount.ToString()
        };
        GameManager.instance.AppendLogLine(new ExtendedLocalizedString("Logs", "Objects count", countList), ELogTarget.both);
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
