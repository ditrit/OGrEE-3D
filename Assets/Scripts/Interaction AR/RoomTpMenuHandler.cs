using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;

public class RoomTpMenuHandler : GridMenuHandler
{
    public GameObject buttonPrefab;
    List<Room> rooms;
    // Start is called before the first frame update
    void Start()
    {
        numberOfResultsPerPage = gridCollection.Rows;

    }

    private void OnEnable()
    {
        rooms = GetRooms();
        pageNumber = 0;
        UpdateGrid(rooms.Count, AssignButtonFunction);
    }



    ///<summary>
    /// Load the 3D model of an OObject
    ///</summary>
    ///<returns>A list containing all loaded rooms</returns>
    private List<Room> GetRooms()
    {
        List<Room> rooms = new List<Room>();
        foreach (DictionaryEntry de in GameManager.gm.allItems)
        {
            GameObject obj = (GameObject)de.Value;
            if (obj.GetComponent<Room>())
            {
                rooms.Add(obj.GetComponent<Room>());
            }
        }
        return rooms;
    }

    ///<summary>
    /// Instantiates a button which teleport to the corresponding room on an empty tile
    ///</summary>
    ///<param name="_index">the element index in rooms</param>
    private void AssignButtonFunction(int _index)
    {
        string fullname = rooms[_index].hierarchyName;
        GameObject button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, parentList.transform);
        button.name = fullname;
        button.transform.Find("IconAndText/TextMeshPro").GetComponent<TextMeshPro>().text = fullname;

        button.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
        {
            TeleportHandler.instance.TeleportToRoom(rooms[_index]);
        });
    }
}
