using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rack : Object
{
    private Vector3 originalLocalPos;
    private Vector3 originalPosXY;

    public Rack()
    {
        family = EObjFamily.rack;
    }

    // protected override void OnDestroy()
    // {
    //     base.OnDestroy();
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rack>() && GameManager.gm.currentItems.Contains(gameObject))
        {
            // Debug.Log($"{name}.OnTriggerEnter() with {other.name}");
            GameManager.gm.AppendLogLine($"Cannot move {name}, it will overlap {other.name}", "yellow");
            transform.localPosition = originalLocalPos;
            posXY = originalPosXY;
        }
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
    }

    ///<summary>
    /// Move the rack in its room's orientation.
    ///</summary>
    ///<param name="_v">The translation vector</param>
    public void MoveRack(Vector2 _v)
    {
        originalLocalPos = transform.localPosition;
        originalPosXY = posXY;
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = false;
        }

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
        StartCoroutine(ReactiveCollider());
    }

    ///<summary>
    /// Coroutine: enable Rack's Collider after finish move (end of next frame)
    ///</summary>
    private IEnumerator ReactiveCollider()
    {
        // yield return new WaitForSeconds(1);
        yield return new WaitForEndOfFrame(); // end of current frame
        yield return new WaitForEndOfFrame(); // end of next frame
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>())
                child.GetComponent<Collider>().enabled = true;
        }
    }
}
