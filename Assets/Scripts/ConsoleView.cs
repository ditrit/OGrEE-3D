using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class ConsoleView : MonoBehaviour
{
    public ConsoleController console;

    public GameObject viewContainer;
    public TextMeshProUGUI logTextArea;
    public TMP_InputField inputField;

    private void Start()
    {
        if (console != null)
        {
            console.logChanged += OnLogChanged;
        }
        UpdateLogStr(console.log);

#if DEBUG
        console.RunCommandString(".cmds:K:\\_Orness\\CmdsOgree3D\\testCmds.txt");
        // console.RunCommandString(".cmds:K:\\_Orness\\CmdsOgree3D\\EDF.NOE_Ced.ocli");
        // console.RunCommandString(".cmds:K:\\_Orness\\CmdsOgree3D\\Exaion\\EDF_EXAION.ocli");
#endif
    }

    private void OnDestroy()
    {
        console.logChanged -= OnLogChanged;
    }

    ///<summary>
    /// Called when logChanged event is called: Call UpdateLogStr
    ///</summary>
    ///<param name="_newLog">Log to display</param>
    private void OnLogChanged(string[] _newLog)
    {
        UpdateLogStr(_newLog);
    }

    ///<summary>
    /// Display log in logTextArea 
    ///</summary>
    ///<param name="_newLog">Log to display</param>
    private void UpdateLogStr(string[] _newLog)
    {
        if (_newLog == null)
        {
            logTextArea.text = "";
        }
        else
        {
            logTextArea.text = string.Join("\n", _newLog);
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

}
