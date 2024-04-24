using UnityEngine;
using TMPro;

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
        if (hasNonDefaultOrientation && ro.isSquare)
        {
            diagonalForNonDefaultOrientations.gameObject.SetActive(true);
            diagonalForNonDefaultOrientationsText.transform.parent.gameObject.SetActive(true);
            diagonalForNonDefaultOrientations.localPosition = ro.childrenOrigin.transform.position;
        }
    }

    private void OnDisable()
    {
        diagonal.gameObject.SetActive(false);
        diagonalText.transform.parent.gameObject.SetActive(false);
        diagonalForNonDefaultOrientations.gameObject.SetActive(false);
        diagonalForNonDefaultOrientationsText.transform.parent.gameObject.SetActive(false);
        UiManager.instance.previousClick = false;
    }

    /// <summary>
    /// Scale and place the half of an axis along with its text
    /// </summary>
    /// <param name="_text">the text of the axis</param>
    /// <param name="_axis">the axis</param>
    /// <param name="_hit">the RayCastHit which gives the length of the semi axis</param>
    /// <param name="_rayDirection">the direction of the hit</param>
    /// <param name="_cameraDirection">the direction of the camera used to rotate the text</param>
    /// <param name="_axisDirection">the direction to scale the axis</param>
    /// <param name="_color">the color of the text</param>
    private void PlaceSemiAxisWithText(TextMeshPro _text, Transform _axis, RaycastHit _hit, Vector3 _rayDirection, Vector3 _cameraDirection, Vector3 _axisDirection, string _color)
    {
        float length = maxLength;
        if (_hit.collider)
        {
            length = _hit.distance;
            _text.transform.parent.localPosition = 0.5f * _hit.distance * _rayDirection + 0.002f * Vector3.up;
            _text.text = $"<color=\"{_color}\">{_hit.distance:0.##}";
            //The text is always aligned with the axis, so it rotate in 180 degrees steps (else the Round(x/180) * 180)
            _text.transform.parent.eulerAngles = (Mathf.Round(Quaternion.LookRotation(transform.rotation * _cameraDirection).eulerAngles.y / 180f) * 180 - transform.eulerAngles.y) * Vector3.up;
        }
        else
            _text.transform.parent.localScale = Vector3.zero;

        _axis.localPosition += 0.5f * length * _rayDirection;
        _axis.localScale += Vector3.one + (length - 1) * _axisDirection;
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
            textXTotal.gameObject.SetActive(true);
            textXTotal.text = $"<color=\"green\">{raycastHit.distance + raycastHit2.distance:0.##}";
            //The text is always aligned with the axis, so it rotate in 180 degrees steps (else the Round(x/180) * 180)
            textXTotal.transform.parent.eulerAngles = (Mathf.Round(Quaternion.LookRotation(transform.rotation * GameManager.instance.cameraControl.transform.right * -1).eulerAngles.y / 180f) * 180 - transform.eulerAngles.y) * Vector3.up;
        }
        else
            textXTotal.gameObject.SetActive(false);

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.right), out raycastHit, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        PlaceSemiAxisWithText(textZUp, axisZ, raycastHit, Vector3.right, GameManager.instance.cameraControl.transform.up, Vector3.up, "red");

        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.left), out raycastHit2, float.MaxValue, ~LayerMask.NameToLayer("Ignore Raycast"));
        PlaceSemiAxisWithText(textZDown, axisZ, raycastHit2, Vector3.left, GameManager.instance.cameraControl.transform.up, Vector3.up, "red");

        if (raycastHit.collider && raycastHit2.collider)
        {
            textZTotal.gameObject.SetActive(true);
            textZTotal.text = $"<color=\"red\">{raycastHit.distance + raycastHit2.distance:0.##}";
            //The text is always aligned with the axis, so it rotate in 180 degrees steps (else the Round(x/180) * 180)
            textZTotal.transform.parent.eulerAngles = (Mathf.Round(Quaternion.LookRotation(transform.rotation * GameManager.instance.cameraControl.transform.up).eulerAngles.y / 180f) * 180 - transform.eulerAngles.y) * Vector3.up;
        }
        else
            textZTotal.gameObject.SetActive(false);
    }

    /// <summary>
    /// Place and scale the diagonals and their texts
    /// </summary>
    private void HandleDiagonals()
    {
        PlaceDiagonalWithText(diagonal, diagonalText);
        if (hasNonDefaultOrientation)
            PlaceDiagonalWithText(diagonalForNonDefaultOrientations, diagonalForNonDefaultOrientationsText, true);
    }

    /// <summary>
    /// Place and scale a diagonal along with its text
    /// </summary>
    /// <param name="_diagonal">the diagonal</param>
    /// <param name="_text">the text of the diagonal</param>
    private void PlaceDiagonalWithText(Transform _diagonal, TextMeshPro _text, bool cym = false)
    {
        _diagonal.localScale = Vector3.Scale(transform.localPosition - _diagonal.localPosition, Vector3.one - 2 * Vector3.forward);
        _text.transform.parent.localPosition = _diagonal.transform.GetChild(0).position;
        _text.transform.parent.eulerAngles = Mathf.Rad2Deg * Mathf.Atan2(_diagonal.localScale.z, _diagonal.localScale.x) * Vector3.up;
        _text.text = $"<color={(cym ? "yellow" : "red")}>{Mathf.Abs(_diagonal.transform.localScale.x):0.##}</color>|<color={(cym ? "#ff00ff" : "green")}>{Mathf.Abs(_diagonal.transform.localScale.z):0.##}</color>";
    }

    ///<summary>
    /// Move the coordSystem plane to the hit point, aligned with the hitted object and handle its axis, diagonals and texts
    ///</summary>
    ///<param name="_hit">The hit data</param>
    public void MoveCSToHit(RaycastHit _hit)
    {
        transform.position = _hit.point + new Vector3(0, 0.001f, 0);
        transform.eulerAngles = _hit.collider.transform.parent.eulerAngles;
        HandleAxis();
        HandleDiagonals();
    }

    /// <summary>
    /// Resize all the texts of the coord mode depending on the distance of the coord mode object from the camera
    /// </summary>
    public void RescaleTexts()
    {
        textSize = scale * Vector3.Distance(transform.position, Camera.main.transform.position) * (Vector3.one - Vector3.up) + Vector3.up;
        textXLeft.transform.parent.localScale = textSize;
        textXRight.transform.parent.localScale = textSize;
        textXTotal.transform.parent.parent.localScale = textSize;
        textZTotal.transform.parent.parent.localScale = textSize;
        textZDown.transform.parent.localScale = textSize;
        textZUp.transform.parent.localScale = textSize;
        diagonalText.transform.parent.localScale = textSize;
        diagonalForNonDefaultOrientationsText.transform.parent.localScale = textSize;
    }
}
