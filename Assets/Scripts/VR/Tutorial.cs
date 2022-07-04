using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;
public partial class Tutorial : MonoBehaviour
{
    public int step = 0;
    public GameObject arrow;
    public GameObject tutorialWindow;
    private Vector3 targetSize;
    private Vector3 offset;
    private ParentConstraint parentConstraint;

    public TutorialStep[] tutorialSteps;

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

    }
    // Start is called before the first frame update
    void Start()
    {
        parentConstraint = arrow.GetComponent<ParentConstraint>();
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

    public void NextStep()
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
