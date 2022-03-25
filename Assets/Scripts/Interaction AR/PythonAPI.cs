using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonAPI : MonoBehaviour
{
    private void Start() {
        {
            run_cmd("C:/Users/vince/Desktop/Ogree_Unity/DevVincent/Python/main.py", "-i C:/Users/vince/Desktop/Ogree_Unity/DevVincent/Python/Images/Hololens_NOEC8-C03b2.jpg -t EDF");
        }
    }

    private void run_cmd(string cmd, string args)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "C:/Users/vince/AppData/Local/Programs/Python/Python39/python.exe";
        start.Arguments = string.Format("{0} {1}", cmd, args);
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        using(Process process = Process.Start(start))
        {
            using(StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log(result);
            }
        }
    }
}
