using UnityEngine;

public class Corridor : Item
{
    ///<summary>
    /// Set a Color with an hexadecimal value
    ///</summary>
    ///<param name="_hex">The hexadecimal value, without '#'</param>
    public new void SetColor(string _hex)
    {
        bool validColor = ColorUtility.TryParseHtmlString($"#{_hex}", out Color newColor);
        newColor.a = 0.5f;
        if (validColor)
        {
            color = newColor;
            GetComponent<ObjectDisplayController>().ChangeColor(color);
        }
    }
}
