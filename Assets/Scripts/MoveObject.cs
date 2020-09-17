using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    [SerializeField] private Vector3 origin;
    [SerializeField] private Vector3 dest;
    public bool hasDrag = false;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50))
                origin = hit.point;
        }
        else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0)
                    && Input.GetAxis("Mouse X") != 0 && Input.GetAxis("Mouse Y") != 0)
        {
            hasDrag = true;
            // Debug.LogWarning($"Drag x={Input.GetAxis("Mouse X")} y={Input.GetAxis("Mouse Y")}");
        }
        else if (Input.GetMouseButtonUp(0) && hasDrag)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50))
            {
                dest = hit.point;
                Move();
            }
            hasDrag = false;
        }
    }

    ///<summary>
    /// Compute a vector from origin to dest and call currentItems's Rack.MoveRack().
    /// If multi selection, move only racks in the same room than currentItems[0].
    ///</summary>
    private void Move()
    {
        Vector3 moveVect = dest - origin;
        int x = Mathf.RoundToInt(moveVect.x / GameManager.gm.tileSize);
        int y = Mathf.RoundToInt(moveVect.z / GameManager.gm.tileSize);
        if (GameManager.gm.currentItems.Count == 1)
        {
            GameObject obj = GameManager.gm.currentItems[0];
            if (obj.GetComponent<Rack>())
                obj.GetComponent<Rack>().MoveRack(new Vector2(x, y));
        }
        else if (GameManager.gm.currentItems.Count > 1)
        {
            Transform targetRoom = GameManager.gm.currentItems[0].transform.parent;
            foreach (GameObject obj in GameManager.gm.currentItems)
            {
                if (obj.transform.parent == targetRoom && obj.GetComponent<Rack>())
                    obj.GetComponent<Rack>().MoveRack(new Vector2(x, y));
            }
        }
        GameManager.gm.UpdateGuiInfos();
    }

}

