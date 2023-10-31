using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public static class RectTransformExtensions
{

    public static bool Overlaps(this RectTransform a, RectTransform b)
    {
        return a.WorldRect().Overlaps(b.WorldRect());
    }
    public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
    {
        return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
    }

    public static Rect WorldRect(this RectTransform rectTransform)
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        Vector3 position = rectTransform.position;
        return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
    }
}
public class CoordModeController : MonoBehaviour
{
    [SerializeField] private float scale;
    [SerializeField] private float maxLength;
    [SerializeField] private Transform axisX;
    [SerializeField] private Transform axisZ;
    [SerializeField] private TextMeshPro textXLeft;
    [SerializeField] private TextMeshPro textXRight;
    [SerializeField] private TextMeshPro textXTotal;
    [SerializeField] private TextMeshPro textZUp;
    [SerializeField] private TextMeshPro textZDown;
    [SerializeField] private TextMeshPro textZTotal;
    [SerializeField] private TextMeshPro diagonalText;
    [SerializeField] private Transform diagonal;
    [SerializeField] private Transform diagonalForNonDefaultOrientations;
    [SerializeField] private TextMeshPro diagonalForNonDefaultOrientationsText;
    private bool hasNonDefaultOrientation = false;

    private void OnEnable()
    {
        Building bd = GameManager.instance.GetSelected()[0].GetComponent<Building>();
        diagonal.gameObject.SetActive(true);
        diagonalText.transform.parent.gameObject.SetActive(true);
        diagonal.localPosition = bd.transform.position;
        Room ro = (Room)bd;
        hasNonDefaultOrientation = ro && ro.attributes["axisOrientation"] != AxisOrientation.Default;
        if (hasNonDefaultOrientation)
        {
            diagonalForNonDefaultOrientations.gameObject.SetActive(true);
            diagonalForNonDefaultOrientationsText.transform.parent.gameObject.SetActive(true);
            switch (ro.attributes["axisOrientation"])
            {
                case AxisOrientation.XMinus:
                    diagonalForNonDefaultOrientations.localPosition = ro.transform.position + ro.technicalZone.localScale.x * 10 * Vector3.right;
                    break;
                case AxisOrientation.YMinus:
                    diagonalForNonDefaultOrientations.localPosition = ro.transform.position + ro.technicalZone.localScale.y * 10 * Vector3.forward;
                    break;
                case AxisOrientation.BothMinus:
                    diagonalForNonDefaultOrientations.localPosition = ro.transform.position + 10 * (ro.technicalZone.localScale.y * Vector3.forward + ro.technicalZone.localScale.x * Vector3.right);
                    break;
            }
        }
    }

    private void OnDisable()
    {
        diagonal.gameObject.SetActive(false);
        diagonalForNonDefaultOrientations.gameObject.SetActive(false);
    }

    private void HandleAxis()
    {
        float length = maxLength;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast")))
        {
            length = raycastHit.distance;
            textXLeft.transform.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
            textXLeft.transform.parent.localPosition = 0.5f * raycastHit.distance * Vector3.forward + 0.002f * Vector3.up;
            textXLeft.text = $"<color=\"green\">{Utils.FloatToRefinedStr(raycastHit.distance)}";
            textXLeft.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(-1 * GameManager.instance.cameraControl.transform.right).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            textXLeft.transform.parent.localScale = Vector3.zero;

        axisX.localPosition = 0.5f * length * Vector3.forward;
        axisX.localScale = Vector3.one + (length - 1) * Vector3.right;

        length = maxLength;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out RaycastHit raycastHit2, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast")))
        {
            length = raycastHit2.distance;
            textXRight.transform.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
            textXRight.transform.parent.localPosition = 0.5f * raycastHit2.distance * Vector3.back + 0.002f * Vector3.up;
            textXRight.text = $"<color=\"green\">{Utils.FloatToRefinedStr(raycastHit2.distance)}";
            textXRight.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(-1 * GameManager.instance.cameraControl.transform.right).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            textXRight.transform.parent.localScale = Vector3.zero;

        axisX.localPosition += 0.5f * length * Vector3.back;
        axisX.localScale += Vector3.one + (length - 1) * Vector3.right;

        axisX.localPosition += 0.001f * Vector3.up;

        if (raycastHit.collider && raycastHit2.collider)
        {
            textXTotal.transform.parent.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
            textXTotal.text = $"<color=\"green\">{Utils.FloatToRefinedStr(raycastHit.distance + raycastHit2.distance)}";
            textXTotal.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(-1 * GameManager.instance.cameraControl.transform.right).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            textXTotal.transform.parent.localScale = Vector3.zero;


        length = maxLength;
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        if (raycastHit.collider)
        {
            length = raycastHit.distance;
            textZUp.transform.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
            textZUp.transform.parent.localPosition = 0.5f * raycastHit.distance * Vector3.right + 0.002f * Vector3.up;
            textZUp.text = $"<color=\"red\">{Utils.FloatToRefinedStr(raycastHit.distance)}";
            textZUp.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(GameManager.instance.cameraControl.transform.up).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
        {
            textZUp.transform.parent.localScale = Vector3.zero;
        }

        axisZ.localPosition = 0.5f * length * Vector3.right;
        axisZ.localScale = length * Vector3.up + (Vector3.one - Vector3.up);

        length = maxLength;
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out raycastHit2, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        if (raycastHit2.collider)
        {
            length = raycastHit2.distance;
            textZDown.transform.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
            textZDown.transform.parent.localPosition = 0.5f * raycastHit2.distance * Vector3.left + 0.002f * Vector3.up;
            textZDown.text = $"<color=\"red\">{Utils.FloatToRefinedStr(raycastHit2.distance)}";
            textZDown.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(GameManager.instance.cameraControl.transform.up).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
        {
            textZDown.transform.parent.localScale = Vector3.zero;
        }

        axisZ.localPosition += 0.5f * length * Vector3.left;
        axisZ.localScale += length * Vector3.up + (Vector3.one - Vector3.up);

        if (raycastHit.collider && raycastHit2.collider)
        {
            textZTotal.transform.parent.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
            textZTotal.text = $"<color=\"red\">{Utils.FloatToRefinedStr(raycastHit.distance + raycastHit2.distance)}";
            textZTotal.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(-1 * GameManager.instance.cameraControl.transform.right).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            textZTotal.transform.parent.localScale = Vector3.zero;
    }

    private void HandleDiagonals()
    {

        //// Set axis texts
        diagonal.localScale = Vector3.Scale(transform.localPosition - diagonal.localPosition, Vector3.one - 2 * Vector3.forward);
        diagonalText.transform.parent.localPosition = diagonal.transform.GetChild(0).position;
        diagonalText.transform.parent.eulerAngles = Mathf.Rad2Deg * Mathf.Atan2(diagonal.localScale.z, diagonal.localScale.x) * Vector3.up;
        diagonalText.transform.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
        diagonalText.text = $"<color=\"red\">{Utils.FloatToRefinedStr(Mathf.Abs(diagonal.transform.localScale.x))}</color>|<color=\"green\">{Utils.FloatToRefinedStr(Mathf.Abs(diagonal.transform.localScale.z))}</color>";

        if (!hasNonDefaultOrientation)
            return;
        diagonalForNonDefaultOrientations.localScale = Vector3.Scale(transform.localPosition - diagonalForNonDefaultOrientations.localPosition, Vector3.one - 2 * Vector3.forward);
        diagonalForNonDefaultOrientationsText.transform.parent.localPosition = diagonalForNonDefaultOrientations.transform.GetChild(0).position;
        diagonalForNonDefaultOrientationsText.transform.parent.eulerAngles = Mathf.Rad2Deg * Mathf.Atan2(diagonalForNonDefaultOrientations.localScale.z, diagonalForNonDefaultOrientations.localScale.x) * Vector3.up;
        diagonalForNonDefaultOrientationsText.transform.parent.localScale = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
        diagonalForNonDefaultOrientationsText.text = $"<color=\"red\">{Utils.FloatToRefinedStr(Mathf.Abs(diagonalForNonDefaultOrientations.transform.localScale.x))}</color>|<color=\"green\">{Utils.FloatToRefinedStr(Mathf.Abs(diagonalForNonDefaultOrientations.transform.localScale.z))}</color>";
    }

    ///<summary>
    /// Move the coordSystem plane to the hit point, aligned with the hitted object
    ///</summary>
    ///<param name="_hit">The hit data</param>
    public void MoveCSToHit(RaycastHit _hit)
    {
        transform.position = _hit.point + new Vector3(0, 0.001f, 0);
        transform.eulerAngles = _hit.collider.transform.parent.eulerAngles;
        HandleAxis();
        HandleDiagonals();
    }
}
