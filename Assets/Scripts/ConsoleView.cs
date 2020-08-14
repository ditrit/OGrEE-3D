using UnityEngine;
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
        // console.RunCommandString(".cmds:K:\\_Orness\\CmdsOgree3D\\testCmds.txt");
        console.RunCommandString(".cmds:K:\\_Orness\\CmdsOgree3D\\EDF.NOE_Ced.ocli");
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

}
