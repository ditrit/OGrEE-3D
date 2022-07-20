using System.Collections.Generic;
using UnityEngine;
#if VR
using UnityEngine.Windows.WebCam;
#endif
using System.Linq;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.IO;
using UnityEngine.UI;
using System.Collections;

public class Label
{
    public string site;
    public string room;
    public string rack;
}

public class ApiListener : MonoBehaviour
{
    public static ApiListener instance;
    private string currentHost;
    public string customer;
    public string site = "NOE";
    public string deviceType = "rack";
    public string building;
    public string room;
    public string rack;
    public string customerAndSite;
#if VR
    private PhotoCapture photoCaptureObject = null;
#endif
    public bool PictureCoolDown = true;
    public GameObject buttonPicture;
    private bool isDemo = true;
    private WebCamTexture webCamTexture;
    private byte[] bytes;
    public RawImage background;
    private bool isCamAvailable = false;
    private Texture defaultBackground;
    public AspectRatioFitter fit;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }

    // Start is called before the first frame update
    private async void Start()
    {
        while (String.IsNullOrEmpty(customer))
        {
            await Task.Delay(50);
            try
            {
                customer = GameManager.gm.configLoader.GetTenant();
                currentHost = GameManager.gm.configLoader.GetPythonApiUrl();
            }
            catch { }
        }
        while (!ApiManager.instance.isInit)
        {
            await Task.Delay(50);
        }
#if !VR
        WebCamDevice[] devices = WebCamTexture.devices;
        customerAndSite = customer + "." + site;
        for (int i = 0; i < devices.Length; i++)
        {
            GameManager.gm.AppendLogLine(devices[i].name + devices[i].isFrontFacing, false, eLogtype.warning);
            if (!devices[i].isFrontFacing)
            {
                webCamTexture = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
            }
        }
        // GetComponent<Renderer>().material.mainTexture = webCamTexture; //Add Mesh Renderer to the GameObject to which this script is attached to
        if (webCamTexture)
        {
            webCamTexture.Play();
            background.texture = webCamTexture;
            isCamAvailable = true;
        }
#endif
    }
    private void Update()
    {
        if (!isCamAvailable)
            return;
        float ratio = (float)webCamTexture.width / webCamTexture.height;
        fit.aspectRatio = ratio;

        // float scaleY = webCamTexture.videoVerticallyMirrored ? -1f: -1f;
        // background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -webCamTexture.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
    }

    public async void TakePhotoButton()
    {
        StartCoroutine(TakePhoto());
        if (bytes != null)
        {
            await UploadByteAsync(bytes);
        }
    }

    public IEnumerator TakePhoto()
    {
        yield return new WaitForEndOfFrame();
        if (webCamTexture)
        {
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);

            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();
            photo = rotateTexture(photo);
            bytes = photo.EncodeToPNG();
            Destroy(photo);
        }
    }

     public Texture2D rotateTexture(Texture2D image )
     {
 
         Texture2D target = new Texture2D(image.height, image.width, image.format, false);    //flip image width<>height, as we rotated the image, it might be a rect. not a square image
         
         Color32[] pixels = image.GetPixels32(0);
         pixels = rotateTextureGrid(pixels, image.width, image.height);
         target.SetPixels32(pixels);
         target.Apply();
 
         //flip image width<>height, as we rotated the image, it might be a rect. not a square image
 
         return target;
      }
 
 
     public Color32[] rotateTextureGrid(Color32[] tex, int wid, int hi)
     {
         Color32[] ret = new Color32[wid * hi];      //reminder we are flipping these in the target
 
         for (int y = 0; y < hi; y++)
         {
             for (int x = 0; x < wid; x++)
             {
                 ret[(hi-1)-y + x * hi] = tex[x + y * wid];         //juggle the pixels around
                 
             }
         }
 
         return ret;
     }

#if VR

    ///<summary>
    /// Call the function that takes a picture
    ///</summary>
    public async void CapturePhoto()
    {
        if (isDemo)
        {
            //byte[] data = Resources.Load<Texture2D>("LabelDemo").GetRawTextureData();
            string path = Application.persistentDataPath;
            if (!path.EndsWith("/"))
                path += "/";
            byte [] data = System.IO.File.ReadAllBytes(path + "LabelDemo.jpg");
            await UploadByteAsync(data);
        }
        else
        {
            if (PictureCoolDown)
            {
                PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
                PictureCoolDown = false;
            }
        }
    }

    ///<summary>
    /// Set parameters and format of the picture to take
    ///</summary>
    ///<param name="_captureObject">Photo</param>
    private void OnPhotoCaptureCreated(PhotoCapture _captureObject)
    {
        photoCaptureObject = _captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;

        _captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    ///<summary>
    /// Release the camera
    ///</summary>
    ///<param name="_result">result of the pipe</param>
    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult _result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    ///<summary>
    /// take a picture
    ///</summary>
    ///<param name="_result">result of the pipe</param>
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult _result)
    {
        if (_result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    ///<summary>
    /// process the photo on memory
    ///</summary>
    ///<param name="_result">result of the pipe</param>
    ///<param name="_photoCaptureFrame">frame captured</param>
    private async void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult _result, PhotoCaptureFrame _photoCaptureFrame)
    {
        if (_result.success)
        {
            List<byte> imageBufferList = new List<byte>();
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            _photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);
            byte[] imageByteArray = imageBufferList.ToArray();
            if (imageByteArray != null)
            {
                await UploadByteAsync(imageByteArray);
            }
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
#endif

    ///<summary>
    /// Send picture to API and receive json
    ///</summary>
    ///<param name="_byteArray"> image in the format of a byte array</param>
    public async Task UploadByteAsync(byte[] _byteArray)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("Label_Rack", _byteArray);
        form.AddField("Tenant_Name", customerAndSite);
        //form.AddField("deviceType", deviceType);
        // UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.yellow);
        UiManagerVincent.instance.UpdateText($"Start POST request with url = {currentHost} and site = {customerAndSite}");
        using (UnityWebRequest www = UnityWebRequest.Post($"http://{currentHost}", form))
        {
            www.SendWebRequest();
            while (!www.isDone)
                await Task.Delay(50);
            PictureCoolDown = true;
            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                // UiManagerVincent.instance.UpdateText(www.error);
                UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
            }
            else
            {
                string text = www.downloadHandler.text;
                try
                {
                    JsonUtility.FromJson<Label>(text);
                }
                catch (System.Exception e)
                {
                    // UiManagerVincent.instance.UpdateText(e.ToString());
                    UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
                }

                Label labelReceived = JsonUtility.FromJson<Label>(text);
                site = labelReceived.site;

                if (site == "PCY")
                {
                    room = "UC" + labelReceived.room;
                }
                room = labelReceived.room;
                if (room.Length > 3)
                    room = room.Substring(3);
                rack = labelReceived.rack;
                // UiManagerVincent.instance.UpdateText($"The label read is {site}{room}-{rack}");
                if (string.IsNullOrEmpty(room) && string.IsNullOrEmpty(rack))
                {
                    UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
                    // UiManagerVincent.instance.UpdateText(UiManagerVincent.instance.apiResponseTMP.text + "\nCould not read the room and the rack label");

                }
                else if (string.IsNullOrEmpty(room))
                {
                    UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
                    // UiManagerVincent.instance.UpdateText(UiManagerVincent.instance.apiResponseTMP.text + "\nCould not read the room label");
                }

                else if (string.IsNullOrEmpty(rack))
                {
                    UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
                    // UiManagerVincent.instance.UpdateText(UiManagerVincent.instance.apiResponseTMP.text + "\nCould not read the rack label");

                }

                else if (!string.IsNullOrEmpty(rack))
                {
                    await SetBuilding(room);
                    if (building == "error" || building == null)
                    {
                        UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
                        // UiManagerVincent.instance.UpdateText(UiManagerVincent.instance.apiResponseTMP.text + "\nError while getting the parent building of the room");

                    }
                    else
                    {
                        UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.green);
                        // UiManagerVincent.instance.UpdateText(UiManagerVincent.instance.apiResponseTMP.text + "\nLoading Rack please wait...");
                        UiManagerVincent.instance.DeactivateButtonAndText();
                    }
                }

                else
                {
                    UiManagerVincent.instance.ChangeButtonColor(buttonPicture, Color.red);
                }
            }
        }
    }

    ///<summary>
    /// Retrieve parent building of a room
    ///</summary>
    ///<param name="_room">string refering to a room name</param>
    public async Task SetBuilding(string _room)
    {
        SApiObject obj = await ApiManager.instance.GetObject($"rooms?name=" + _room, ApiManager.instance.GetFirstSApiObject);
        string parentId = obj.parentId;
        obj = await ApiManager.instance.GetObject($"buildings/" + parentId, ApiManager.instance.GetFirstSApiObject);
        building = obj.name;
    }

    ///<summary>
    /// Activate a Gameobject if it is not active. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void ToggleGameobject(GameObject _g)
    {
        if (_g.activeSelf)
        {
            _g.SetActive(false);
        }
        else
        {
            _g.SetActive(true);
        }
    }

    ///<summary>
    /// Activate a Gameobject if it is not active and move it in front of the camera. Deactivate it if it is active.
    ///</summary>
    ///<param name="_g">Gameobject to activate/deactivate</param>
    public void ToggleAndMoveGameobject(GameObject _g)
    {
        if (_g.activeSelf)
        {
            _g.SetActive(false);
        }
        else
        {
            _g.SetActive(true);
            Utils.MoveObjectToCamera(_g, Camera.main, 0.6f, -0.25f, 0, 25);
        }
    }

    ///<summary>
    /// Load the 3D model of a rack
    ///</summary>
    ///<param name="_customer">string refering to a customer</param>
    ///<param name="_site">string refering to a site</param>
    ///<param name="_building">string refering to a building</param>
    ///<param name="_room">string refering to a room</param>
    ///<param name="_rack">string refering to a rack</param>
    public async Task LoadSingleRack(string _customer, string _site, string _building, string _room, string _rack)
    {
        GameObject customer = GameManager.gm.FindByAbsPath(_customer);
        await GameManager.gm.DeleteItem(customer, false);
        await ApiManager.instance.GetObject($"sites/" + _site, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room, ApiManager.instance.DrawObject);
        await ApiManager.instance.GetObject($"tenants/" + _customer + "/sites/" + _site + "/buildings/" + _building + "/rooms/" + _room + "/racks/" + _rack, ApiManager.instance.DrawObject);

        GameObject rack = GameManager.gm.FindByAbsPath(_customer + "." + _site + "." + _building + "." + _room + "." + _rack);
        if (rack == null)
            GameManager.gm.AppendLogLine("Rack NOT Found in the scene after loading from API", false, eLogtype.error);
        else
        {
            GameManager.gm.AppendLogLine("Rack Found in the scene after loading from API", false, eLogtype.success);
#if !VR
            Utils.MoveObjectInFrontOfCamera(rack, Camera.main, 2.2f, 0);
            customer = GameManager.gm.FindByAbsPath(_customer);
            customer.transform.SetParent(Camera.main.transform);

#endif
#if VR
            Utils.MoveObjectToCamera(rack, Camera.main, 1.5f, -0.7f, 90, 0);
#endif
            OgreeObject ogree = rack.GetComponent<OgreeObject>();
            ogree.originalLocalRotation = rack.transform.localRotation;  //update the originalLocalRotation to not mess up when using reset button from TIM
            ogree.originalLocalPosition = rack.transform.localPosition;
            var goArray = FindObjectsOfType<GameObject>();
            for (var i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == LayerMask.NameToLayer("Rack"))
                {
                    if (goArray[i].transform.GetChild(2))
                    {
                        await GameManager.gm.SetCurrentItem(goArray[i].transform.GetChild(2).gameObject);  //Select this
                        return;
                    }
                }
            }
        }
    }

}
