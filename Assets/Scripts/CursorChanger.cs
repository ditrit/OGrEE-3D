using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.youtube.com/watch?v=8Fm37H1Mwxw
public class CursorChanger : MonoBehaviour, IEventHandler
{
    [System.Serializable]
    public class CursorData
    {
        public CursorType cursorType;
        public Texture2D texture;
        public Vector2 offset;
    }

    public enum CursorType
    {
        Idle,
        Loading
    }

    [SerializeField] private List<CursorData> cursors;

    private void Awake()
    {
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    public void SubscribeEvents()
    {
        EventManager.instance.AddListener<ChangeCursorEvent>(OnChangeCursor);
    }

    public void UnsubscribeEvents()
    {
        EventManager.instance.RemoveListener<ChangeCursorEvent>(OnChangeCursor);
    }

    ///<summary>
    /// Change the cursor regarding given cursorType
    ///</summary>
    ///<param name="_e">Given event data</param>
    private void OnChangeCursor(ChangeCursorEvent _e)
    {
        foreach (CursorData cursorData in cursors)
        {
            if (cursorData.cursorType == _e.type)
                Cursor.SetCursor(cursorData.texture, cursorData.offset, CursorMode.Auto);
        }
    }
}
