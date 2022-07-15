using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MicTest : MonoBehaviour
{
   public static float[] MicLoudness;

    //private string _device;
    private string _device1;
    private string _device2;

    public TMP_Text log;
    //AudioClip _clipRecord;
    //AudioClip _clipRecord1;
    //AudioClip _clipRecord2;
    ////mic initialization
    //void InitMic()
    //{
    //     _clipRecord = AudioClip.Create("name", 44100 * 2, 1, 44100, true);
    //     _clipRecord1 = AudioClip.Create("name", 44100 * 2, 1, 44100, true);
    //     _clipRecord2 = AudioClip.Create("name", 44100 * 2, 1, 44100, true);
    //    if (_device == null) _device = Microphone.devices[0];
    //    _clipRecord = Microphone.Start(_device, true, 999, 44100);
    //    if (Microphone.devices.Length >= 2)
    //    {
    //        if (_device1 == null) _device1 = Microphone.devices[1];
    //        _clipRecord1 = Microphone.Start(_device1, true, 999, 44100);
    //    }
    //    if (Microphone.devices.Length >= 3)
    //    {
    //        if (_device2 == null) _device2 = Microphone.devices[2];
    //        _clipRecord2 = Microphone.Start(_device2, true, 999, 44100);
    //    }
    //}

    //void StopMicrophone()
    //{
    //    Microphone.End(_device);
    //    if (_device1 != null)
    //        Microphone.End(_device1);
    //    if (_device2 != null)
    //        Microphone.End(_device2);
    //}



    ////get data from microphone into audioclip
    float[] LevelMax()
    {
        float[] levelMax = { 0, 0, 0 };

        float[] samples = new float[AudioMic.clip.samples * AudioMic.clip.channels];
        int micPosition = Microphone.GetPosition(null);
        if (micPosition > 0)
        {
            AudioMic.clip.GetData(samples, micPosition);
            // Getting a peak on the last 128 samples
           for (int i = Mathf.Clamp(samples.Length - 128, 0, samples.Length - 128); i < samples.Length; i++)
           {
                float wavePeak = samples[i];
                if (levelMax[0] < wavePeak)
               {
                   levelMax[0] = wavePeak;
               }
            }
        }
       
        return levelMax;
    }



    void Update()
    {
        // levelMax equals to the highest normalized value power 2, a small number because < 1
        // pass the value to a static var so we can access it from anywhere
        MicLoudness = LevelMax();
        log.text = "Start Mic(pos): " + Microphone.GetPosition(null) + Microphone.devices[0]+  " " + MicLoudness[0].ToString();

    }

    //bool _isInitialized;
    //// start mic when scene starts
    //void OnEnable()
    //{
    //    InitMic();
    //    _isInitialized = true;
    //}

    ////stop mic when loading a new level or quit application
    //void OnDisable()
    //{
    //    StopMicrophone();
    //}

    //void OnDestroy()
    //{
    //    StopMicrophone();
    //}


    //// make sure the mic gets started & stopped when application gets focused
    //void OnApplicationFocus(bool focus)
    //{
    //    if (focus)
    //    {
    //        //Debug.Log("Focus");

    //        if (!_isInitialized)
    //        {
    //            //Debug.Log("Init Mic");
    //            InitMic();
    //            _isInitialized = true;
    //        }
    //    }
    //    if (!focus)
    //    {
    //        //Debug.Log("Pause");
    //        StopMicrophone();
    //        //Debug.Log("Stop Mic");
    //        _isInitialized = false;

    //    }
    //}
    AudioSource AudioMic;
    void Start()
    {
        StartCoroutine(CaptureMic());
    }

    IEnumerator CaptureMic()
    {
        if (AudioMic == null) AudioMic = GetComponent<AudioSource>();
        AudioMic.clip = Microphone.Start(null, true, 1, 44100);
        AudioMic.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        log.text = "Start Mic(pos): " + Microphone.GetPosition(null) + Microphone.devices[0];
        AudioMic.Play();

        //capture for live streaming
        //while (!stop)
        //{
        //    AddMicData();
        //    yield return null;
        //}
        //capture for live streaming
        yield return null;
    }
}
