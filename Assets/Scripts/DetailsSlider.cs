using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DetailsSlider : MonoBehaviour
{
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    ///<summary>
    /// Called when the value of the slider is changed
    ///</summary>
    ///<param name="_value">Value given by the slider</param>
    public void OnValueChanged(float _value)
    {
        List<OgreeObject> objsToUpdate = new List<OgreeObject>();
        foreach (GameObject go in GameManager.gm.currentItems)
            objsToUpdate.Add(go.GetComponent<OgreeObject>());
        foreach (OgreeObject obj in objsToUpdate)
            obj?.LoadChildren(_value.ToString());

    }

    ///<summary>
    /// Update slider with given value
    ///</summary>
    ///<param name="_value">The value to set the slider</param>
    public void UpdateSlider(int _value)
    {
        slider.value = _value;
    }

    ///<summary>
    /// Set the slider interactable or not
    ///</summary>
    ///<param name="_value">Is the slider interactible ?</param>
    public void ActiveSlider(bool _value)
    {
        slider.interactable = _value;
    }
}
