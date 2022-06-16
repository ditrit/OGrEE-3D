using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Filters))]
public abstract class AUiManager
{
    public abstract void UpdateGuiInfos();    
    public abstract void UpdateFocusText();
    public abstract void ChangeApiButton(string _str, Color _color);
    public abstract void SetApiUrlText(string _str);
    //public abstract async Task ConnectToApi();
    public abstract void Stop();
}

