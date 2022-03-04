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
        OgreeObject rack = GameManager.gm.currentItems[0].GetComponent<OgreeObject>();
        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack to place it again", "yellow");
        }

        else
        {
            if (rack.category == "rack")
            {
                rack.GetComponent<BoundsControl>().enabled = true;
                rack.GetComponent<ObjectManipulator>().enabled = true;
            }

            else
            {
                GameManager.gm.AppendLogLine($"Cannot place other object than rack", "red");
            }
        }
    }
}
