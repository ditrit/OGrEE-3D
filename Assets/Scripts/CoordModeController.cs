using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Leaving it there in case I think of a good way to use it
public static class RectTransformExtensions
{
    /// <summary>
    /// Check if two rect overlaps
    /// </summary>
    /// <param name="_a">first rect</param>
    /// <param name="_b">second rect</param>
    /// <returns></returns>
    public static bool Overlaps(this RectTransform _a, RectTransform _b)
    {
        return _a.WorldRect().Overlaps(_b.WorldRect());
    }

    /// <summary>
    /// Checks if two rect overlaps
    /// </summary>
    /// <param name="a">first rect</param>
    /// <param name="b">second rect</param>
    /// <param name="allowInverse">Does the test allow the widths and heights of the Rects to be negative?</param>
    /// <returns></returns>
    public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse)
    {
        return a.WorldRect().Overlaps(b.WorldRect(), allowInverse);
    }

    /// <summary>
    /// Make a Rect with no parent (at root) at the same place and with the same dimensions
    /// </summary>
    /// <param name="rectTransform">the rect to copy</param>
    /// <returns></returns>
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
    private Vector3 textSize;

    /// <summary>
    /// Initialize the data of the script according to the currently selected building/room
    /// </summary>
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
                    diagonalForNonDefaultOrientations.localPosition = ro.transform.position + ro.technicalZone.localScale.x * 10 * ro.transform.TransformDirection(Vector3.right);
                    break;
                case AxisOrientation.YMinus:
                    diagonalForNonDefaultOrientations.localPosition = ro.transform.position + ro.technicalZone.localScale.z * 10 * ro.transform.TransformDirection(Vector3.forward);
                    break;
                case AxisOrientation.BothMinus:
                    diagonalForNonDefaultOrientations.localPosition = ro.transform.position + 10 * (ro.technicalZone.localScale.z * ro.transform.TransformDirection(Vector3.forward) + ro.technicalZone.localScale.x * ro.transform.TransformDirection(Vector3.right));
                    break;
            }
        }
    }

    private void OnDisable()
    {
        diagonal.gameObject.SetActive(false);
        diagonalText.transform.parent.gameObject.SetActive(false);
        diagonalForNonDefaultOrientations.gameObject.SetActive(false);
        diagonalForNonDefaultOrientationsText.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Scale and place the half of an axis along with its text
    /// </summary>
    /// <param name="text">the text of the axis</param>
    /// <param name="axis">the axis</param>
    /// <param name="hit">the RayCastHit which gives the length of the semi axis</param>
    /// <param name="rayDirection">the direction of the hit</param>
    /// <param name="cameraDirection">the direction of the camera used to rotate the text</param>
    /// <param name="axisDirection">the direction to scale the axis</param>
    /// <param name="color">the color of the text</param>
    private void PlaceSemiAxisWithText(TextMeshPro text, Transform axis, RaycastHit hit, Vector3 rayDirection, Vector3 cameraDirection, Vector3 axisDirection, string color)
    {
        float length = maxLength;
        if (hit.collider)
        {
            length = hit.distance;
            text.transform.parent.localScale = textSize;
            text.transform.parent.localPosition = 0.5f * hit.distance * rayDirection + 0.002f * Vector3.up;
            text.text = $"<color=\"{color}\">{Utils.FloatToRefinedStr(hit.distance)}";
            //The text is always aligned with the axis, so it rotate in 180 degrees steps (else the Round(x/180) * 180)
            text.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(cameraDirection).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            text.transform.parent.localScale = Vector3.zero;

        axis.localPosition += 0.5f * length * rayDirection;
        axis.localScale += Vector3.one + (length - 1) * axisDirection;
    }

    /// <summary>
    /// Scale and place the axis of the coordinate system and their texts
    /// </summary>
    private void HandleAxis()
    {
        axisX.localPosition = Vector3.zero;
        axisX.localScale = Vector3.zero;
        axisZ.localPosition = Vector3.zero;
        axisZ.localScale = Vector3.zero;

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        PlaceSemiAxisWithText(textXLeft, axisX, raycastHit, Vector3.forward, -1 * GameManager.instance.cameraControl.transform.right, Vector3.right, "green");

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.back), out RaycastHit raycastHit2, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        PlaceSemiAxisWithText(textXRight, axisX, raycastHit2, Vector3.back, -1 * GameManager.instance.cameraControl.transform.right, Vector3.right, "green");

        axisX.localPosition += 0.001f * Vector3.up;

        if (raycastHit.collider && raycastHit2.collider)
        {
            textXTotal.transform.parent.parent.localScale = textSize;
            textXTotal.text = $"<color=\"green\">{Utils.FloatToRefinedStr(raycastHit.distance + raycastHit2.distance)}";
            //The text is always aligned with the axis, so it rotate in 180 degrees steps (else the Round(x/180) * 180)
            textXTotal.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(-1 * GameManager.instance.cameraControl.transform.right).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            textXTotal.transform.parent.localScale = Vector3.zero;

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        PlaceSemiAxisWithText(textZUp, axisZ, raycastHit, Vector3.right, GameManager.instance.cameraControl.transform.up, Vector3.up, "red");

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out raycastHit2, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        PlaceSemiAxisWithText(textZDown, axisZ, raycastHit2, Vector3.left, GameManager.instance.cameraControl.transform.up, Vector3.up, "red");

        if (raycastHit.collider && raycastHit2.collider)
        {
            textZTotal.transform.parent.parent.localScale = textSize;
            textZTotal.text = $"<color=\"red\">{Utils.FloatToRefinedStr(raycastHit.distance + raycastHit2.distance)}";
            //The text is always aligned with the axis, so it rotate in 180 degrees steps (else the Round(x/180) * 180)
            textZTotal.transform.parent.eulerAngles = Mathf.Round(Quaternion.LookRotation(GameManager.instance.cameraControl.transform.up).eulerAngles.y / 180f) * 180 * Vector3.up;
        }
        else
            textZTotal.transform.parent.localScale = Vector3.zero;
    }

    /// <summary>
    /// Place and scale the diagonals and their texts
    /// </summary>
    private void HandleDiagonals()
    {
        PlaceDiagonalWithText(diagonal, diagonalText);
        if (hasNonDefaultOrientation)
            PlaceDiagonalWithText(diagonalForNonDefaultOrientations, diagonalForNonDefaultOrientationsText);
    }
    
    /// <summary>
    /// Place and scale a diagonal along with its text
    /// </summary>
    /// <param name="diagonal">the diagonal</param>
    /// <param name="text">the text of the diagonal</param>
    private void PlaceDiagonalWithText(Transform diagonal, TextMeshPro text)
    {
        diagonal.localScale = Vector3.Scale(transform.localPosition - diagonal.localPosition, Vector3.one - 2 * Vector3.forward);
        text.transform.parent.localPosition = diagonal.transform.GetChild(0).position;
        text.transform.parent.eulerAngles = Mathf.Rad2Deg * Mathf.Atan2(diagonal.localScale.z, diagonal.localScale.x) * Vector3.up;
        text.transform.parent.localScale = textSize;
        text.text = $"<color=\"red\">{Utils.FloatToRefinedStr(Mathf.Abs(diagonal.transform.localScale.x))}</color>|<color=\"green\">{Utils.FloatToRefinedStr(Mathf.Abs(diagonal.transform.localScale.z))}</color>";
    }

    ///<summary>
    /// Move the coordSystem plane to the hit point, aligned with the hitted object and handle its axis, diagonals and texts
    ///</summary>
    ///<param name="_hit">The hit data</param>
    public void MoveCSToHit(RaycastHit _hit)
    {
        textSize = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
        transform.position = _hit.point + new Vector3(0, 0.001f, 0);
        transform.eulerAngles = _hit.collider.transform.parent.eulerAngles;
        HandleAxis();
        HandleDiagonals();
    }
}
