using UnityEngine;

public class Corridor : Item
{
    public override void UpdateFromSApiObject(SApiObject _src)
    {
        if ((HasAttributeChanged(_src, "posXYZ")
            || HasAttributeChanged(_src, "posXYUnit")
            || HasAttributeChanged(_src, "rotation"))
            && transform.parent)
        {
            PlaceInRoom(_src);
            group?.ShapeGroup();
        }

        if (HasAttributeChanged(_src, "temperature"))
            SetColor(_src.attributes["temperature"] == "cold" ? "000099" : "990000");
        base.UpdateFromSApiObject(_src);
    }

    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    public new void SetColor(string _hex)
    {
        if (ColorUtility.TryParseHtmlString($"#{_hex}", out Color newColor))
        {
            color = newColor.WithAlpha(0.5f);
            GetComponent<ObjectDisplayController>().ChangeColor(color);
        }
    }
}
