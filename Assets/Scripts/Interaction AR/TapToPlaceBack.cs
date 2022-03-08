using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class TapToPlaceBack : MonoBehaviour
{
    // Start is called before the first frame update

    public void TtpAgain()
    {
        Transform t = GameManager.gm.currentItems[0].transform;

        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack or one of its child to place it again", "yellow");
        }

        else
        {
            while(t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    t.GetChild(0).GetComponent<BoundsControl>().enabled = true;
                    t.GetChild(0).GetComponent<ObjectManipulator>().enabled = true;
                    //t.GetChild(0).GetComponent<BoxCollider>().enabled = true;
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot place other object than rack", "red");
        }
    }
}
