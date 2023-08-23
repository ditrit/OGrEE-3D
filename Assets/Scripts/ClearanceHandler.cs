using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClearanceHandler
{
    [System.Serializable]
    private class Clearance
    {
        public float length;
        public GameObject gameObject;
        public Vector3 direction;
    }

    [SerializeField] private Clearance front = new Clearance();
    [SerializeField] private Clearance rear = new Clearance();
    [SerializeField] private Clearance left = new Clearance();
    [SerializeField] private Clearance right = new Clearance();
    [SerializeField] private Clearance top = new Clearance();
    [SerializeField] private Transform parent;

    private bool isCreated = false;
    public bool toggled = false;
    [SerializeField] private List<Clearance> clearances = new List<Clearance>();

    public ClearanceHandler(float _front, float _rear, float _left, float _right, float _top, Transform _parent)
    {
        front.length = _front / 1000;
        front.direction = Vector3.forward;
        rear.length = _rear / 1000;
        rear.direction = Vector3.back;
        left.length = _left / 1000;
        left.direction = Vector3.left;
        right.length = _right / 1000;
        right.direction = Vector3.right;
        top.length = _top / 1000;
        top.direction = Vector3.up;
        parent = _parent;
        ToggleClearance(true);
    }

    public void ToggleClearance(bool _toggle)
    {
        string verb = _toggle ? "Display" : "Hide";
        toggled = _toggle;
        if (!isCreated)
        {
            if (_toggle)
                BuildClearance();
            return;
        }
        foreach (Clearance clearance in clearances)
            clearance.gameObject.SetActive(_toggle);
        GameManager.instance.AppendLogLine($"{verb} local Clearance for {parent.name}", ELogTarget.logger, ELogtype.success);
    }

    public void ToggleClearance()
    {
        ToggleClearance(!toggled);
    }

    public void BuildClearance()
    {
        if (front.length > 0)
            clearances.Add(front);
        if (rear.length > 0)
            clearances.Add(rear);
        if (left.length > 0)
            clearances.Add(left);
        if (right.length > 0)
            clearances.Add(right);
        if (top.length > 0)
            clearances.Add(top);
        foreach (Clearance clearance in clearances)
        {
            Vector3 absDirection = Vector3.Scale(clearance.direction, clearance.direction);
            clearance.gameObject = Object.Instantiate(GameManager.instance.clearanceModel, parent.transform);
            clearance.gameObject.transform.localPosition = 0.5f * (Vector3.Scale(clearance.direction, parent.GetChild(0).localScale) + clearance.length * clearance.direction);
            clearance.gameObject.transform.localScale = Vector3.Scale(Vector3.one - absDirection, parent.GetChild(0).localScale) + clearance.length * absDirection;
        }
        isCreated = true;
    }
}
