using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class PostVideo : MonoBehaviour
{
    public string host = "192.168.120.231";
    public string port = "5000";


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UploadCoroutine("C:/Users/vince/Desktop/Ogree_Unity/DevVincent/Python/Images/rack.jpg"));
    }

    IEnumerator UploadCoroutine(string filePath)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("Label_Rack", File.ReadAllBytes(filePath));
        form.AddField("Tenant_Name", "EDF");
        UnityWebRequest www = UnityWebRequest.Post("http://" + host + ":" + port, form);
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
