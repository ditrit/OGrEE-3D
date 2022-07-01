using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;
public class Tutorial : MonoBehaviour
{
    public int step = 0;
    public GameObject arrow;
    public GameObject buttonTuto;
    public GameObject tutorialWindow;
    public GameObject buttonAPI;
    public GameObject buttonList;
    public GameObject APIMenu;
    public GameObject rack;
    public GameObject chassis;
    public GameObject ButtonWrapper;
    public GameObject menuContent;
    public static Tutorial instance;
    private Vector3 targetSize;
    private Vector3 offset;
    private ParentConstraint parentConstraint;
    // Start is called before the first frame update
    void Start()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        parentConstraint = arrow.GetComponent<ParentConstraint>();
    }

    private void Update()
    {
        if (arrow.activeInHierarchy)
            MoveArrow();
    }

    private void PlaceArrow(GameObject _target)
    {
        if (parentConstraint.sourceCount > 0)
            parentConstraint.RemoveSource(0);
        parentConstraint.locked = false;
        arrow.transform.position = _target.transform.position;
        ConstraintSource source = new ConstraintSource
        {
            sourceTransform = _target.transform,
            weight = 1
        };
        parentConstraint.AddSource(source);

        targetSize = _target.GetComponent<Renderer>() ? _target.GetComponent<Renderer>().bounds.size : (_target.GetComponent<Collider>() ? _target.GetComponent<Collider>().bounds.size : _target.transform.lossyScale);
        offset = (targetSize.y / 2) * Vector3.up + (targetSize.x / 2) * Vector3.right;
        parentConstraint.SetTranslationOffset(0, offset);
        Quaternion rot = Quaternion.LookRotation(_target.transform.position - parentConstraint.GetTranslationOffset(0) - arrow.transform.position, Vector3.up);
        parentConstraint.SetRotationOffset(0, rot.eulerAngles);

        arrow.SetActive(true);
    }

    public void NextStep()
    {
        step++;
        switch (step)
        {
            case 1:
                buttonTuto.SetActive(false);
                tutorialWindow.SetActive(true);
                menuContent.SetActive(false);
                ChangeText("1");
                break;
            case 2:
                menuContent.SetActive(true);
                if (!ApiManager.instance.isInit)
                    PlaceArrow(buttonAPI);
                else
                    PlaceArrow(buttonList);
                ChangeText("2");
                tutorialWindow.transform.GetChild(3).gameObject.SetActive(false);
                break;
            case 3:
                PlaceArrow(APIMenu.transform.GetChild(0).GetChild(0).GetChild(0).gameObject);
                ChangeText("3");
                break;
            case 4:
                PlaceArrow(APIMenu.transform.GetChild(0).GetChild(0).GetChild(1).gameObject);
                ChangeText("4");
                break;
            case 5:
                PlaceArrow(rack);
                ChangeText("5");
                break;
            case 6:
                PlaceArrow(chassis);
                ChangeText("6");
                break;
            case 7:
                PlaceArrow(ButtonWrapper.transform.GetChild(0).gameObject);
                ChangeText("7");
                break;
            case 8:
                PlaceArrow(ButtonWrapper.transform.GetChild(3).gameObject);
                ChangeText("8");
                buttonTuto.SetActive(true);
                break;
            case 9:
                arrow.SetActive(false);
                break;
            default: break;
        }
        if (step >= 9)
        {
            step = 0;
        }
    }

    private void MoveArrow()
    {
        parentConstraint.SetTranslationOffset(0, offset + ((Mathf.PingPong(Time.time, 1)) * (targetSize - targetSize.z * Vector3.forward)));
    }

    private void ChangeText(string _text)
    {
        tutorialWindow.transform.GetChild(2).GetComponent<TextMeshPro>().text = _text;
    }
}
