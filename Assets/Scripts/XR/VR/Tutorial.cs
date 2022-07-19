using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;
using System.Text.RegularExpressions;
using Microsoft.MixedReality.Toolkit.UI;
using System.Threading.Tasks;

public class Tutorial : MonoBehaviour
{
    public int currentStep = 0;
    public GameObject arrow;
    public GameObject directionnalIndicator;
    public GameObject tutorialWindow;
    public GameObject mixedRealtyPlaySpace;
    public GameObject mainCamera;

    public string[] templates;
    private Vector3 targetSize;
    private Vector3 offset;
    private ParentConstraint parentConstraint;

    public TutorialStep[] tutorialSteps;


    private readonly ReadFromJson rfJson = new ReadFromJson();

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(maxLines: 4, minLines: 2)]
        [Tooltip("Text show in the tutorial window")]
        public string text;

        [Tooltip("GameObject pointed at by the arrow")]
        public GameObject arrowTargetGameObject;

        [Tooltip("GameObject pointed at by the arrow")]
        public string arrowTargetHierarchyName;

        public bool teleportPlayer;

        [Tooltip("Where to place the player at the start of the step")]
        public Vector3 teleportPosition = Vector3.negativeInfinity;

        public NextStepEvent nextStepEvent;

        public ArrowTargetType arrowTargetType;

        public GameObject buttonNextStep;

        public string nextStepObjectHierarchyName;

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
        public enum NextStepEvent
        {
            Select,
            Deselect,
            Focus,
            Unfocus,
            EditIn,
            EditOut,
            ButtonPress
        }

        public enum ArrowTargetType
        {
            GameObject,
            HierarchyName
        }

        public object test;

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
                    helper.sApiObject.attributes[attribute.key] = attribute.value;
            }

        foreach (string template in templates)
            if (Regex.IsMatch(template, "\"category\"[ ]*:[ ]*\"room\""))
                rfJson.CreateRoomTemplateJson(template);
            else
                rfJson.CreateObjTemplateJson(template);

        EventManager.Instance.AddListener<OnSelectItemEvent>(OnSelectItem);

        EventManager.Instance.AddListener<OnFocusEvent>(OnFocusItem);
        EventManager.Instance.AddListener<OnUnFocusEvent>(OnUnFocusItem);

        EventManager.Instance.AddListener<EditModeInEvent>(OnEditModeIn);
        EventManager.Instance.AddListener<EditModeOutEvent>(OnEditModeOut);
    }

    private void OnSelectItem(OnSelectItemEvent _e)
    {
        if (currentStep > 0 && tutorialSteps[currentStep - 1].nextStepEvent == TutorialStep.NextStepEvent.Select)
        {
            foreach (GameObject obj in GameManager.gm.currentItems)
                if (tutorialSteps[currentStep - 1].nextStepObjectHierarchyName == obj.GetComponent<OgreeObject>().hierarchyName)
                {
                    NextStep();
                    break;
                }
        }
        if (currentStep > 0 && tutorialSteps[currentStep - 1].nextStepEvent == TutorialStep.NextStepEvent.Deselect)
        {
            foreach (GameObject obj in GameManager.gm.previousItems)
                if (tutorialSteps[currentStep - 1].nextStepObjectHierarchyName == obj.GetComponent<OgreeObject>().hierarchyName)
                {
                    NextStep();
                    break;
                }
        }
    }

    private void OnFocusItem(OnFocusEvent _e)
    {
        if (currentStep == 0 || tutorialSteps[currentStep - 1].nextStepEvent != TutorialStep.NextStepEvent.Focus)
            return;
        if (_e.obj.GetComponent<OgreeObject>().hierarchyName == tutorialSteps[currentStep - 1].nextStepObjectHierarchyName)
            NextStep();
    }
    private void OnUnFocusItem(OnUnFocusEvent _e)
    {
        if (currentStep == 0 || tutorialSteps[currentStep - 1].nextStepEvent != TutorialStep.NextStepEvent.Unfocus)
            return;
        if (_e.obj.GetComponent<OgreeObject>().hierarchyName == tutorialSteps[currentStep - 1].nextStepObjectHierarchyName)
            NextStep();
    }
    private void OnEditModeIn(EditModeInEvent _e)
    {
        if (currentStep == 0 || tutorialSteps[currentStep - 1].nextStepEvent != TutorialStep.NextStepEvent.EditIn)
            return;
        if (_e.obj.GetComponent<OgreeObject>().hierarchyName == tutorialSteps[currentStep - 1].nextStepObjectHierarchyName)
            NextStep();
    }
    private void OnEditModeOut(EditModeOutEvent _e)
    {
        if (currentStep == 0 || tutorialSteps[currentStep - 1].nextStepEvent != TutorialStep.NextStepEvent.EditOut)
            return;
        if (_e.obj.GetComponent<OgreeObject>().hierarchyName == tutorialSteps[currentStep - 1].nextStepObjectHierarchyName)
            NextStep();
    }

    private void Update()
    {
        if (arrow.activeInHierarchy)
            MoveArrow();
    }


    private void PlaceArrow(GameObject _target)
    {
        arrow.SetActive(false);
        directionnalIndicator.SetActive(false);
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

        directionnalIndicator.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.Solvers.DirectionalIndicator>().DirectionalTarget = _target.transform;

        arrow.SetActive(true);
        directionnalIndicator.SetActive(true);
    }

    private void PlaceArrow(string id)
    {
        PlaceArrow(GameManager.gm.FindByAbsPath(id)?.transform.GetChild(0).gameObject);
    }

    public async void NextStep()
    {
        if (currentStep < tutorialSteps.Length)
        {
            foreach (GameObject obj in tutorialSteps[currentStep].stepObjectsShown)
                if (obj != null)
                    obj.SetActive(true);
            foreach (GameObject obj in tutorialSteps[currentStep].stepObjectsHidden)
                if (obj != null)
                    obj.SetActive(false);

            foreach (TutorialStep.SApiObjectHelper helper in tutorialSteps[currentStep].stepSApiObjectsInstantiated)
                await OgreeGenerator.instance.CreateItemFromSApiObject(helper.sApiObject);
        }
        PlaceArrow(tutorialSteps[currentStep].arrowTargetGameObject);
        if (!tutorialSteps[currentStep].arrowTargetGameObject && tutorialSteps[currentStep].arrowTargetHierarchyName != null)
            PlaceArrow(tutorialSteps[currentStep].arrowTargetHierarchyName);

        ChangeText(tutorialSteps[currentStep].text);

        if (tutorialSteps[currentStep].teleportPlayer)
        {
            Vector3 targetPos = tutorialSteps[currentStep].teleportPosition;
            float height = targetPos.y;
            targetPos -= mainCamera.transform.position - mixedRealtyPlaySpace.transform.position;
            targetPos.y = height;

            mixedRealtyPlaySpace.transform.position = targetPos;
        }

        if (tutorialSteps[currentStep].nextStepEvent == TutorialStep.NextStepEvent.ButtonPress)
        {
            tutorialSteps[currentStep].buttonNextStep.GetComponent<ButtonConfigHelper>().OnClick.RemoveAllListeners();
            tutorialSteps[currentStep].buttonNextStep.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() => NextStep());
        }

        currentStep++;
        if (currentStep == tutorialSteps.Length)
            currentStep = 0;

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
