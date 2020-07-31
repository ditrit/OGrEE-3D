﻿using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class ConsoleView : MonoBehaviour
{
    public ConsoleController console {get; private set;} = new ConsoleController();

    public GameObject viewContainer;
    public TextMeshProUGUI logTextArea;
    public TMP_InputField inputField;

    void Start()
    {
        if (console != null)
        {
            console.logChanged += OnLogChanged;
        }
        UpdateLogStr(console.log);

#if DEBUG
        StartCoroutine(Debug());
#endif
    }

    private void OnDestroy()
    {
        console.logChanged -= OnLogChanged;
    }

    void OnLogChanged(string[] newLog)
    {
        UpdateLogStr(newLog);
    }

    void UpdateLogStr(string[] newLog)
    {
        if (newLog == null)
        {
            logTextArea.text = "";
        }
        else
        {
            logTextArea.text = string.Join("\n", newLog);
        }
    }

    ///<summary>
    /// Event that should be called by anything wanting to submit the current input to the console.
    ///</summary>
    public void RunCommand()
    {
        console.RunCommandString(inputField.text);
        inputField.text = "";
    }

    private IEnumerator Debug()
    {
        List<string> cmds = new List<string>();
        cmds.Add("+customer:DEMO");
        cmds.Add("+datacenter:BETA@N");
        cmds.Add("+building:/DEMO.BETA.A@[0,80,0]@[20,30,4]");
        cmds.Add("+building:/DEMO.BETA.B@[0,20,0]@[20,30,4]");
        cmds.Add("+bd:C@[30,0,0]@[60,135,5]");
        cmds.Add("+room:R1@[0,15,0]@[60,60,5]@W");
        cmds.Add("+room:/DEMO.BETA.C.R2@[0,75,0]@[60,60,5]@W");
        cmds.Add("+ro:/DEMO.BETA.C.Office@[60,0,0]@[20,75,4]@N");
        cmds.Add("+zones:[2,1,3,3]@[4,4,4,4]");
        cmds.Add("+zones:/DEMO.BETA.C.R2@[3,3,3,3]@[5,0,0,0]");
        cmds.Add("+rack:A01@[1,1]@[60,120,42]@rear");
        cmds.Add("+rack:/DEMO.BETA.C.R1.A02@[2,1]@[60,120,42]@rear");
        cmds.Add("+rack:/DEMO.BETA.C.R1.B01@[1,3]@[60,120,42]@front");
        cmds.Add("+rack:/DEMO.BETA.C.R1.B02@[2,3]@[60,120,42]@front");
        cmds.Add("+rack:/DEMO.BETA.C.R2.A01@[1,1]@[60,120,42]@rear");
        foreach (string cmd in cmds)
        {
            console.RunCommandString(cmd);
            yield return new WaitForEndOfFrame();
        }
    }

}
