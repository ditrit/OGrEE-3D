using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class PostVideo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UploadCoroutine("C:/Users/vince/Desktop/Ogree_Unity/DevVincent/Python/Images/rack.jpg"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /* Upload the chosen video file to the movie server */
IEnumerator UploadCoroutine(string filePath)
{
    WWWForm form = new WWWForm();
    form.AddBinaryData("Label_Rack", File.ReadAllBytes(filePath));
    form.AddField("Tenant_Name", "EDF");
    //form.AddBinaryData("Label_Rack", bytes, "EDF","application/json");
    UnityWebRequest www = UnityWebRequest.Post("http://192.168.1.38:6000", form);
    yield return www.SendWebRequest();

    if (www.result == UnityWebRequest.Result.ConnectionError)
    {
        Debug.Log(www.error);
    }
    else
    {
        Debug.Log("Form upload complete!");
        Debug.Log(www.downloadHandler.text);
    }
}
}
