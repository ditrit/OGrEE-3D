using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using TMPro;
using ZXing.Common;
using UnityEngine.Windows.WebCam;
using System.Linq;
using ZXing.Datamatrix;

public class QRScanner : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private string QrCode = string.Empty;
    public TMP_Text TextMesh = null;
    private bool qrCodeFound = false;
    public UnityEngine.UI.Image Cursor;
    private float ratioImageSize = 0.9f;
    private int h;
    private int w;


    void Start()
    {

        WebCamDevice device = WebCamTexture.devices[0];

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.cameraResolutionWidth = (int) Math.Round(cameraResolution.width * ratioImageSize);
        c.cameraResolutionHeight = (int) Math.Round(cameraResolution.height * ratioImageSize);
        Cursor.rectTransform.sizeDelta = new Vector2(c.cameraResolutionWidth, c.cameraResolutionHeight);

        var renderer = GetComponent<RawImage>();
        webcamTexture = new WebCamTexture(device.name);
        w = c.cameraResolutionWidth;
        h = c.cameraResolutionHeight;
        
        Debug.Log($"The {webcamTexture} camera resolution is {w} x {h} pixels", this);
        Cursor.transform.localScale = new Vector3(ratioImageSize, ratioImageSize, 0f);
        renderer.texture = webcamTexture;
        StartCoroutine(GetQRCode());
    }

    IEnumerator GetQRCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                //TryInverted = true,
                PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.All_1D,
                    BarcodeFormat.QR_CODE,
					BarcodeFormat.UPC_A,
					BarcodeFormat.UPC_E,
					BarcodeFormat.EAN_8,
					BarcodeFormat.EAN_13,
					BarcodeFormat.CODE_39,
					BarcodeFormat.CODE_93,
					BarcodeFormat.CODE_128,
					BarcodeFormat.CODABAR,
					BarcodeFormat.ITF,
					BarcodeFormat.RSS_14,
					BarcodeFormat.PDF_417,
					BarcodeFormat.RSS_EXPANDED,
					BarcodeFormat.DATA_MATRIX
                }
            }
        };

        DataMatrixReader barCodeReader2 = new DataMatrixReader();

        /*var testBarCodeReader = new ZXing.ImageSharp.BarcodeReader<Rgba32>
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
            }
        };*/

        webcamTexture.Play();
        var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
        while (!qrCodeFound)
        {
            try
            {
                snap.SetPixels32(webcamTexture.GetPixels32());
                var Result = barCodeReader.Decode(snap.GetRawTextureData(), webcamTexture.width, webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);

                if (Result == null)
                {
                    LuminanceSource colLumSource = new RGBLuminanceSource(snap.GetRawTextureData(), w, h, RGBLuminanceSource.BitmapFormat.RGB32);
                    HybridBinarizer hybridBinarizer = new HybridBinarizer(colLumSource);
                    BinaryBitmap bitMap = new BinaryBitmap(hybridBinarizer);
                    var result = barCodeReader2.decode(bitMap);
                }

                //var testResult = testBarCodeReader.Decode(snap);
                if (Result != null)
                {
                    QrCode = Result.Text;
                    var test = Result.ResultMetadata;
                    foreach(var element in test.Keys)
                    {
                        Debug.Log($"The key: {element} is mapped with the value: {test[element]}");
                    }
                    TextMesh.text = QrCode;
                    if (!qrCodeFound)
                    {
                        Debug.Log("DECODED TEXT FROM QR: " + QrCode);
                        //break;
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
            yield return null;
        }
        //webcamTexture.Stop();
    }
}