using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.UI;
public class Move_Rack : MonoBehaviour
{
    private float m_Speed;
    private PinchSlider sliderX;
    private PinchSlider sliderY;
    public GameObject Target;
    
    private void Start()
    {
        GameObject sliderxGameObject = GameObject.Find("Menu/Sliders/SliderX");
        sliderX = sliderxGameObject.GetComponent<PinchSlider>();
        GameObject slideryGameObject = GameObject.Find("Menu/Sliders/SliderY");
        sliderY = slideryGameObject.GetComponent<PinchSlider>();
    }
    /*public void Move()
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
                    t.position += new Vector3(0.0f, sliderY.SliderValue, 0.0f);
                    t.position += new Vector3(sliderX.SliderValue, 0.0f, 0.0f);
                    return;
                }
                t = t.parent.transform;
            }
            GameManager.gm.AppendLogLine($"Cannot rotate other object than rack", "red");
        }
    }*/
    
    public void Move()
    {
        Target.transform.position += new Vector3(0.0f, (2 * sliderY.SliderEndDistance * sliderY.SliderValue + sliderY.SliderStartDistance) /100, 0.0f);
        Target.transform.position += new Vector3((2 * sliderX.SliderEndDistance * sliderX.SliderValue - sliderX.SliderStartDistance) /100, 0.0f, 0.0f);

        /*float localAngleCameraRadian = Mathf.Deg2Rad * GameManager.gm.m_camera.transform.eulerAngles.y;
        Vector3 offset = new Vector3(Mathf.Sin(localAngleCameraRadian) * 1.5f, -0.2f, Mathf.Cos(localAngleCameraRadian) * 1.5f);

        Vector3 newPostion = new Vector3(GameManager.gm.m_camera.transform.position.x, 0.0f, GameManager.gm.m_camera.transform.position.z);

        Target.transform.position = newPostion + offset;*/
    }
}