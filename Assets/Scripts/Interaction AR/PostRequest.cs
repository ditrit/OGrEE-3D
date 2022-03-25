using System.Collections;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class PostRequest : MonoBehaviour
{
    #region private members 	
	private TcpClient socketConnection; 	
	private Thread clientReceiveThread; 
    private string updateText;	
    private HttpClient httpClient = new HttpClient();
	#endregion  

    public void Start()
    {
        //await Post("http://192.168.1.38:6000", "tenantName = EDF");
        StartCoroutine(Post("http://192.168.1.38:6000", "tenantName = EDF"));
    }
    
    void Update()
    {

    }

    IEnumerator Post(string url, string bodyJsonString)
    {
        WWWForm form = new WWWForm();
        form.AddField("myField", "myData");
        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {

            //byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
            //request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
            //request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            //request.SetRequestHeader("Content-Type", "application/json");
            Debug.Log("before yield return");
            yield return request.SendWebRequest();

            Debug.Log("after yield return");
            if (request.result == UnityWebRequest.Result.ConnectionError) // Error
            {
                Debug.Log(request.error);
            }

            else // Success
            {
                Debug.Log(request.result);
                Debug.Log("Status Code: " + request.responseCode);
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

/*
    public async Task Post(string url, string bodyJsonString)
    {

        UnityWebRequest request = UnityWebRequest.Post(url, bodyJsonString);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        Debug.Log("before yield return");
        request.SendWebRequest();
        Debug.Log("after yield return");
        if (request.result == UnityWebRequest.Result.ConnectionError) // Error
        {
            Debug.Log(request.error);
        }

        else // Success
        {
            Debug.Log(request.result);
            Debug.Log("Status Code: " + request.responseCode);
            Debug.Log(request.downloadHandler.text);
        }
    }
*/
/*
    public async Task Post(string fullPath, string json)
    {

        StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        try
        {
            Debug.Log("Before Post");
            HttpResponseMessage response = await httpClient.PostAsync(fullPath, content);
            Debug.Log("After Post");
            string responseStr = response.Content.ReadAsStringAsync().Result;
            Debug.Log("Response");
            Debug.Log(responseStr);

            if (responseStr.Contains("success"))
                Debug.Log("sucess");
            else
                Debug.Log("Fail to post on server");
        }
        catch (HttpRequestException e)
        {
            Debug.Log("HttpRequestException");
            Debug.Log(e.Message);
        }
    }

    /*

    // Start is called before the first frame update
    // Start is called before the first frame update
    /// <summary>   
    /// Setup socket connection.    
    /// </summary>  
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }

    }

    /// <summary>   
    /// Runs in background clientReceiveThread; Listens for incomming data.     
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient("172.30.146.51", 5000);
            Debug.Log("Connection successful");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                // Get a stream object for reading              
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary.                  
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message.                        
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("server message received as: " + serverMessage);
                        updateText = serverMessage;
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
*/
}