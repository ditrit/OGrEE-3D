using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;
using System.Text.RegularExpressions;
public partial class Tutorial : MonoBehaviour
{
    public int step = 0;
    public GameObject arrow;
    public GameObject tutorialWindow;
    private Vector3 targetSize;
    private Vector3 offset;
    private ParentConstraint parentConstraint;

    public string[] templates;

    [NonReorderable]
    public TutorialStep[] tutorialSteps;


    private readonly ReadFromJson rfJson = new ReadFromJson();

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(maxLines: 4, minLines: 2)]
        [Tooltip("Text show in the tutorial window")]
        public string text;

        [Tooltip("Object pointed at by the arrow")]
        public GameObject arrowTarget;

        [Tooltip("Objects hidden during the step")]
        public GameObject[] stepObjectsHidden;

        [Tooltip("Objects shown during the step")]
        public GameObject[] stepObjectsShown;


        [Tooltip("SApiObject instantiated during the step")]
        public SApiObjectHelper[] stepSApiObjectsInstantiated;


        [System.Serializable]
        public class SApiObjectHelper
        {
            [System.Serializable]
            public class Attribute
            {
                public string key;
                public string value;
            }

            public SApiObject sApiObject;

            public List<Attribute> attributes = new List<Attribute>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        parentConstraint = arrow.GetComponent<ParentConstraint>();

        foreach (TutorialStep step in tutorialSteps)
            foreach (TutorialStep.SApiObjectHelper helper in step.stepSApiObjectsInstantiated)
            {
                helper.sApiObject.attributes = new Dictionary<string, string>();
                foreach (TutorialStep.SApiObjectHelper.Attribute attribute in helper.attributes)
                {
                    if (helper == null)
                        print("helper");
                    else if (helper.sApiObject.attributes == null)
                        print("helper.sapiobject.attributes");
                    else if (attribute == null)
                        print("attribute");
                    helper.sApiObject.attributes[attribute.key] = attribute.value;
                }
            }

        foreach (string template in templates)
            if (Regex.IsMatch(template, "\"category\"[ ]*:[ ]*\"room\""))
                rfJson.CreateRoomTemplateJson(template);
            else
                rfJson.CreateObjTemplateJson(template);
    }

    private void Update()
    {
        if (arrow.activeInHierarchy)
            MoveArrow();
    }

    private void PlaceArrow(GameObject _target)
    {
        arrow.SetActive(false);
        if (!_target)
            return;
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

    public async void NextStep()
    {
        if (step == 0)
            tutorialWindow.transform.GetChild(3).GetComponent<Microsoft.MixedReality.Toolkit.UI.ButtonConfigHelper>().OnClick.AddListener(() => NextStep());

        PlaceArrow(tutorialSteps[step].arrowTarget);
        ChangeText(tutorialSteps[step].text);
        if (step < tutorialSteps.Length)
        {
            foreach (GameObject obj in tutorialSteps[step].stepObjectsShown)
                obj?.SetActive(true);
            foreach (GameObject obj in tutorialSteps[step].stepObjectsHidden)
                obj?.SetActive(false);
            foreach (TutorialStep.SApiObjectHelper helper in tutorialSteps[step].stepSApiObjectsInstantiated)
               await OgreeGenerator.instance.CreateItemFromSApiObject(helper.sApiObject);
        }
        step++;
        if (step == tutorialSteps.Length)
            step = 0;

    }

    private void MoveArrow()
    {
        parentConstraint.SetTranslationOffset(0, offset + ((Mathf.PingPong(Time.time, 1)) * (0.05f * new Vector3(1, 1, 0))));
    }

    private void ChangeText(string _text)
    {
        tutorialWindow.transform.GetChild(2).GetComponent<TextMeshPro>().text = _text;
    }
}
