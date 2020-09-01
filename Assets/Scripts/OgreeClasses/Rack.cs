using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rack : Object
{
    public Rack()
    {
        family = EObjFamily.rack;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Filters.instance.rackRowsList.Remove(name[0].ToString());
        // Filters.instance.racks.Remove(gameObject);
        Filters.instance.UpdateDropdownFromList(Filters.instance.dropdownRackRows, Filters.instance.rackRowsList);
    }

    ///<summary>
    /// Update rack's color according to its Tenant.
    ///</summary>
    public void UpdateColor()
    {
        if (tenant == null)
            return;

        Material mat = transform.GetChild(0).GetComponent<Renderer>().material;
        Color myColor = new Color();
        ColorUtility.TryParseHtmlString(tenant.color, out myColor);
        mat.color = myColor;
        // Debug.Log($"{tenant.color} => {myColor}");
    }

    ///<summary>
    /// Move the rack in its room's orientation.
    ///</summary>
    ///<param name="_v">The translation vector</param>
    public void MoveRack(Vector2 _v)
    {
        Room room = transform.parent.GetComponent<Room>();
        switch (room.orientation)
        {
            case EOrientation.N:
                transform.localPosition += new Vector3(_v.x, 0, _v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.x, _v.y);
                break;
            case EOrientation.W:
                transform.localPosition += new Vector3(_v.y, 0, -_v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(_v.y, -_v.x);
                break;
            case EOrientation.S:
                transform.localPosition += new Vector3(-_v.x, 0, -_v.y) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.x, -_v.y);
                break;
            case EOrientation.E:
                transform.localPosition += new Vector3(-_v.y, 0, _v.x) * GameManager.gm.tileSize;
                posXY += new Vector2(-_v.y, _v.x);
                break;
        }
    }
}
