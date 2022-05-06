using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;
public class Move_Rack : MonoBehaviour
{
    private float m_Speed;
    private PinchSlider sliderX;
    private PinchSlider sliderY;
    
    private void Start()
    {
        GameObject sliderxGameObject = GameObject.Find("Menu/Sliders/SliderX");
        sliderX = sliderxGameObject.GetComponent<PinchSlider>();
        GameObject slideryGameObject = GameObject.Find("Menu/Sliders/SliderY");
        sliderY = slideryGameObject.GetComponent<PinchSlider>();

    }

    public void MoveY()
    {
        Transform t = GameManager.gm.currentItems[0].transform;

        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack or one of its child to rotate it", "yellow");
        }

        else
        {
            while(t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    t.position += new Vector3(0.0f, (2 * sliderY.SliderEndDistance * sliderY.SliderValue + sliderY.SliderStartDistance) /20, 0.0f);
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
        }
    }
    
    public void MoveX()
    {
        Transform t = GameManager.gm.currentItems[0].transform;

        float localAngleCameraRadian = Mathf.Deg2Rad * t.eulerAngles.y;
        float offset = (2 * sliderX.SliderEndDistance * sliderX.SliderValue + sliderX.SliderStartDistance) /20;

        if (GameManager.gm.currentItems.Count == 0)
        {
            GameManager.gm.AppendLogLine($"Select a rack or one of its child to rotate it", "yellow");
        }

        else
        {
            while(t != null)
            {
                if (t.GetComponent<OgreeObject>().category == "rack")
                {
                    t.position += new Vector3(-Mathf.Cos(localAngleCameraRadian) * offset, 0.0f, Mathf.Sin(localAngleCameraRadian) * offset);
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
        }
    }
}