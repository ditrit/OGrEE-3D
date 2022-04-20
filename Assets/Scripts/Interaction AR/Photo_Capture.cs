using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.WebCam;
using System.Linq;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;
using System.Net.Http;
using System;



public class Label
{
    public string site;
    public string room;
    public string rack;
}

public class Photo_Capture : MonoBehaviour
{
    public HttpClient httpClient;
    private string currentHost = "";
    public string maisonHost = "192.168.1.38";
    public string telephoneHost = "192.168.17.231";
    public string customer = "EDF";
    public string site = "PCY";
    private string building;
    private string room;
    private string rack;
    private string customerAndSite;
    public string port = "5000";
    public GameObject quadButtonNoe;
    public GameObject quadButtonPcy;
    public GameObject quadButtonMaison;
    public GameObject quadButtonTelephone;
    public GameObject quadButtonPhoto;
    public Material red;
    public Material green;
    public Material yellow;
    private PhotoCapture photoCaptureObject = null;
    public TextMeshPro apiResponseTMP = null;

    // Start is called before the first frame update
    private void Start()
    {
        customerAndSite = customer + '.' + site;
        SetHostTelephone();
        if (currentHost == maisonHost)
        {
            SetHostMaison();
        }

        if (currentHost == telephoneHost)
        {
            SetHostTelephone();
        }

        if (customerAndSite == "EDF.PCY")
        {
            SetSitePcy();
        }

        if (customerAndSite == "EDF.NOE")
        {
            SetSiteNoe();
        }
    }


    public void SetHostMaison()
    {
        currentHost = maisonHost;
        quadButtonMaison.GetComponent<Renderer>().material = green;
        quadButtonTelephone.GetComponent<Renderer>().material = red;
    }

    public void SetHostTelephone()
    {
        currentHost = telephoneHost;
        quadButtonMaison.GetComponent<Renderer>().material = red;
        quadButtonTelephone.GetComponent<Renderer>().material = green;
    }

    public void SetSiteNoe()
    {
        customerAndSite = "EDF.NOE";
        quadButtonNoe.GetComponent<Renderer>().material = green;
        quadButtonPcy.GetComponent<Renderer>().material = red;
    }

    public void SetSitePcy()
    {
        customerAndSite = "EDF.PCY";
        quadButtonNoe.GetComponent<Renderer>().material = red;
        quadButtonPcy.GetComponent<Renderer>().material = green;
    }

    public void RequestSentColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material = yellow;
    }

    public void RequestFailColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material = red;
    }

    public void RequestSuccessColor()
    {
        quadButtonPhoto.GetComponent<Renderer>().material = green;
    }
    public async Task SetBuilding(string _room)
    {
        string data = await ApiManager.instance.GetObjectParentId($"rooms?name=" + _room);
        building = await ApiManager.instance.GetObjectName($"buildings/" + data);
    }
    
    public async Task LoadSingleRack(string _customer, string _site, string _building, string _room, string _rack)
    {
        GameObject room = GameManager.gm.FindByAbsPath(_customer + "." + _site + "." + _building + "." + _room);
        GameManager.gm.DeleteItem(room, false);
        await ApiManager.instance.GetObject($"sites/"+ _site);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" +_building);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room + "/racks/" + _rack);
        await GameManager.gm.LoadDetailsRackAPI(_customer, _site, _building, _room, _rack);
    }

    public void CapturePhoto()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
    
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    async void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            byte[] imageByteArray = imageBufferList.ToArray();
            if (imageByteArray != null)
            {
                UploadByteAsync(imageByteArray);
            }
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    public async void UploadByteAsync(byte[] byteArray)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("Label_Rack", byteArray);
        form.AddField("Tenant_Name", customerAndSite);
        RequestSentColor();
        apiResponseTMP.text = string.Format("Start POST request with url = {0} and site = {1}", currentHost, customerAndSite);
        using (UnityWebRequest www = UnityWebRequest.Post("http://" + currentHost + ":" + port, form))
        {
            www.SendWebRequest();
            while (!www.isDone)
                await Task.Delay(100);
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                apiResponseTMP.text = www.error;
                RequestFailColor();
            }
            else
            {
                RequestSuccessColor();
                string text = www.downloadHandler.text;
                Label labelReceived = JsonUtility.FromJson<Label>(text);
                
                site = labelReceived.site;

                if (site == "PCY")
                {
                    room = "UC" + labelReceived.room;
                }
                else
                {
                    room = labelReceived.room;
                }
                room = labelReceived.room;
                rack = labelReceived.rack;

                apiResponseTMP.text = string.Format("The label read is {0}{1}-{2}", site, room, rack);
                if (string.IsNullOrEmpty(room))
                {
                    apiResponseTMP.text = apiResponseTMP.text + "\nCannot find parent building because room is void";
                }

                else if (!string.IsNullOrEmpty(rack))
                {
                    await SetBuilding(room);
                    if (building == "error" || building == null)
                    {
                        GameManager.gm.AppendLogLine(building);
                        apiResponseTMP.text = apiResponseTMP.text + "\nError while getting the parent building of the room";
                    }
                    else
                    {
                        apiResponseTMP.text = apiResponseTMP.text + "\nLoading Rack please wait...";
                        await LoadSingleRack(customer, site, building, room, rack);
                        apiResponseTMP.text = string.Format("The label read is {0}{1}-{2}", site, room, rack) + "\nRack Loaded in Scene";
                    }
                }
            }
        }
    }

}
