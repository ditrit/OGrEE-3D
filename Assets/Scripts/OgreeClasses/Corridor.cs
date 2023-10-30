using UnityEngine;

public class Corridor : Item
{
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
