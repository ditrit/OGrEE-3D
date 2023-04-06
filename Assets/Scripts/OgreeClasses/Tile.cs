using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Material defaultMat;
    public string color;
    public string texture;
    public bool modified = false;
    public Vector2 coord;

    ///<summary>
    /// Set the color of the tile
    ///</summary>
    ///<param name="_customColors">List of custom colors</param>
    public void SetColor(List<SColor> _customColors)
    {
        Material mat = GetComponent<Renderer>().material;
        Color customColor = new Color();
        if (color.StartsWith("@"))
        {
            foreach (SColor custColor in _customColors)
            {
                if (custColor.name == color.Substring(1))
                    ColorUtility.TryParseHtmlString($"#{custColor.value}", out customColor);
            }
        }
        else
            ColorUtility.TryParseHtmlString($"#{color}", out customColor);
        mat.color = customColor;
    }

    ///<summary>
    /// Set the texture of the tile
    ///</summary>
    ///<param name="">HierarchyName of the parent room, for error message</param>
    public void SetTexture(string _hierarchyName)
    {
        if (GameManager.instance.textures.ContainsKey(texture))
        {
            Renderer rend = GetComponent<Renderer>();
            rend.material = new Material(defaultMat)
            {
                mainTexture = GameManager.instance.textures[texture]
            };
        }
        else
            GameManager.instance.AppendLogLine($"[{_hierarchyName}] Unknow tile texture: {texture}", ELogTarget.logger, ELogtype.warning);
    }
}
