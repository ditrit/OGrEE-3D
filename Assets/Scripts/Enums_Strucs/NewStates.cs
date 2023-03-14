using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewStates : MonoBehaviour
{
    public static NewStates instance;
    private State state;
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }
    private void Start()
    {
        state = new Idle();
        print(state);
    }

    public bool Is<T>() where T : IState
    {
        return Is<T>(state);
    }

    private bool Is<T>(State _state) where T : IState
    {
        if (_state is Merge)
            return Is<T>(((Merge)state).state1) || Is<T>(((Merge)state).state2);
        return _state is T;
    }

    public void SwitchTo<T>() where T : IScatterPlot, ITempColor, ITempDiagram
    {
        switch (typeof(T))
        {
            case var tmp when tmp == typeof(ScatterPlot):
                state = new ScatterPlot();
                break;
            case var tmp when tmp == typeof(TempDiagram):
                state = new TempDiagram();
                break;
            case var tmp when tmp == typeof(TempColor):
                state = new TempColor();
                break;
            default:
                state = new Idle();
                break;
        }
    }
    public void SwitchTo<T>(GameObject _obj) where T : ISelect, IFocus, IEdit
    {
        switch (typeof(T))
        {
            case var tmp when tmp == typeof(Select):
                state = new Select(_obj);
                break;
            case var tmp when tmp == typeof(Focus):
                state = new Focus(_obj);
                break;
            case var tmp when tmp == typeof(Edit):
                state = new Edit(_obj);
                break;
            default:
                break;
        }

    }
    public void SwitchTo<T>(List<GameObject> _objs) where T : ISelect
    {
        state = new Select(_objs);
    }

    public void Add<T>() where T : IScatterPlot, ITempColor, ITempDiagram
    {
        if (Is<T>())
            return;
        if (Is<Idle>())
        {
            SwitchTo<T>();
            return;
        }
        switch (typeof(T))
        {
            case var tmp when tmp == typeof(ScatterPlot):
                state = new Merge(state, new ScatterPlot());
                break;
            case var tmp when tmp == typeof(TempDiagram):
                state = new Merge(state, new TempDiagram());
                break;
            case var tmp when tmp == typeof(TempColor):
                state = new Merge(state, new TempColor());
                break;
            default:
                break;
        }
    }

    public void Add<T>(GameObject _obj) where T : ISelect, IFocus, IEdit
    {
        if (Is<Idle>())
        {
            SwitchTo<T>(_obj);
            return;
        }
        switch (typeof(T))
        {
            case var tmp when tmp == typeof(Select):
                Add<T>(new List<GameObject>() { _obj });
                break;
            case var tmp when tmp == typeof(Focus):
                if (!Is<IFocus>())
                    state = new Merge(state, new Focus(_obj));
                break;
            case var tmp when tmp == typeof(Edit):
                if (!Is<IEdit>())
                    state = new Merge(state, new Edit(_obj));
                break;
            default:
                break;
        }
    }

    public void Add<T>(List<GameObject> _obj) where T : ISelect
    {
        if (Is<Idle>())
            SwitchTo<T>(_obj);
        else if (Is<Select>())
            ((IMBSelect)state).AddSelection(_obj);
        else
            state = new Merge(state, new Select(_obj));
    }

    public void Remove<T>() where T : IScatterPlot, ITempColor, ITempDiagram, IFocus, IEdit, ISelect
    {
        if (!Is<T>())
            return;
        if (state is T)
            state = new Idle();
        else
            state = ((Merge)state).Prune<T>();

    }
    public void Remove<T>(GameObject _obj) where T : ISelect
    {
        Remove<T>(new List<GameObject>() { _obj });
    }
    public void Remove<T>(List<GameObject> _obj) where T : ISelect
    {
        if (!Is<T>())
            return;
        if (state is T)
            state = new Idle();
        else if (((Merge)state).RemoveSelection(_obj))
        {
            state = ((Merge)state).Prune<ISelect>();
        }

    }

    public bool OObjectSelected()
    {
        return state.TypeSelected<OObject>();
    }

    public bool RackSelected()
    {
        return state.TypeSelected<Rack>();
    }

    public bool DeviceSelected()
    {
        return state.TypeSelected<OObject>("device");
    }

    public bool BuildingSelected()
    {
        return state.TypeSelected<Building>();
    }

    public bool RoomSelected()
    {
        return state.TypeSelected<Room>();
    }

    public bool SiteSelected()
    {
        return state.TypeSelected<OgreeObject>("site");
    }

    public bool TenantSelected()
    {
        return state.TypeSelected<OgreeObject>("tenant");
    }

    public bool RackFocused()
    {
        return state.TypeFocused<Rack>();
    }

    public bool DeviceFocused()
    {
        return state.TypeFocused<OObject>("device");
    }

    public bool RackEdited()
    {
        return state.TypeEdited<Rack>();
    }

    public bool DeviceEdited()
    {
        return state.TypeEdited<OObject>("device");
    }

    #region interfaces
    public interface IState
    {
        public bool TypeSelected<T>(string _category = "") where T : OgreeObject;
        public bool TypeFocused<T>(string _category = "") where T : OObject;
        public bool TypeEdited<T>(string _category = "") where T : OObject;
    }
    public interface IIDle : IState
    {

    }
    public interface IMBSelect : IState
    {
        public void AddSelection(List<GameObject> _obj);
        public bool RemoveSelection(List<GameObject> _obj);

    }
    public interface ISelect : IMBSelect
    {

    }
    public interface IFocus : IState
    {

    }
    public interface IEdit : IState
    {

    }
    public interface ITempColor : IState
    {

    }
    public interface IScatterPlot : IState
    {

    }
    public interface ITempDiagram : IState
    {

    }
    public interface IMerge : IMBSelect
    {

    }
    #endregion

    #region state classes
    public abstract class State : IState
    {
        public virtual bool TypeSelected<T>(string _category = "") where T : OgreeObject
        {
            return false;
        }

        public virtual bool TypeEdited<T>(string _category = "") where T : OObject
        {
            return false;
        }

        public virtual bool TypeFocused<T>(string _category = "") where T : OObject
        {
            return false;
        }
    }

    private class Idle : State, IIDle
    {
        public Idle()
        {

        }
    }

    private class Select : State, ISelect
    {
        public List<GameObject> selectedObjects;

        public Select(GameObject _obj)
        {
            EventManager.instance.Raise(new OnSelectItemEvent());
            selectedObjects = new List<GameObject>() { _obj };
        }
        public Select(List<GameObject> _objs)
        {
            EventManager.instance.Raise(new OnSelectItemEvent());
            selectedObjects = _objs;
        }

        public void AddSelection(List<GameObject> _obj)
        {
            selectedObjects.AddRange(_obj);
        }

        public bool RemoveSelection(List<GameObject> _obj)
        {
            selectedObjects.RemoveAll(o => _obj.Contains(o));
            return selectedObjects.Count == 0;
        }

        public override bool TypeSelected<T>(string _category = "")
        {
            foreach (GameObject obj in selectedObjects)
                if (obj.GetComponent<T>() && (_category == "" || _category == obj.GetComponent<OgreeObject>().category))
                    return true;

            return false;
        }
    }

    private class Focus : State, IFocus
    {
        public GameObject focusedObject;

        public Focus(GameObject _obj)
        {
            focusedObject = _obj;
        }

        public override bool TypeFocused<T>(string _category = "")
        {
            return focusedObject.GetComponent<T>() && (_category == "" || _category == focusedObject.GetComponent<OgreeObject>().category);
        }
    }

    private class Edit : State, IEdit
    {
        public GameObject editedObject;

        public Edit(GameObject _obj)
        {
            editedObject = _obj;
        }

        public override bool TypeEdited<T>(string _category = "")
        {
            return editedObject.GetComponent<T>() && (_category == "" || _category == editedObject.GetComponent<OgreeObject>().category);
        }
    }

    private class TempColor : State, ITempColor
    {
    }

    private class ScatterPlot : State, IScatterPlot
    {
    }

    private class TempDiagram : State, ITempDiagram
    {
    }

    private class Merge : State, IMerge
    {
        public State state1;
        public State state2;

        public Merge(State _state1, State _state2)
        {
            state1 = _state1;
            state2 = _state2;
        }
        public void AddSelection(List<GameObject> _obj)
        {
            if (state1 is Select select)
                select.AddSelection(_obj);
            else if (state2 is Select select1)
                select1.AddSelection(_obj);
            else if (instance.Is<ISelect>(state1))
                ((Merge)state1).AddSelection(_obj);
            else if (instance.Is<ISelect>(state1))
                ((Merge)state1).AddSelection(_obj);
        }

        public bool RemoveSelection(List<GameObject> _obj)
        {
            if (state1 is Select select)
                return select.RemoveSelection(_obj);
            if (state2 is Select select1)
                return select1.RemoveSelection(_obj);
            if (instance.Is<ISelect>(state1))
            {
                if (((Merge)state1).RemoveSelection(_obj))
                    state1 = ((Merge)state1).Prune<ISelect>();
                return false;
            }
            if (instance.Is<ISelect>(state2))
            {
                if (((Merge)state2).RemoveSelection(_obj))
                    state2 = ((Merge)state2).Prune<ISelect>();
                return false;
            }
            return false;
        }

        public State Prune<T>() where T : IState
        {
            if (state1 is T)
                return state2;
            if (state2 is T)
                return state1;
            if (instance.Is<T>(state1))
                return new Merge(((Merge)state1).Prune<T>(), state2);
            if (instance.Is<T>(state2))
                return new Merge(state1, ((Merge)state2).Prune<T>());
            return this;

        }

        public override bool TypeSelected<T>(string _category = "")
        {
            if (state1 is Select select)
                return select.TypeSelected<T>(_category);
            else if (state2 is Select select1)
                return select1.TypeSelected<T>(_category);
            else if (instance.Is<ISelect>(state1))
                return ((Merge)state1).TypeSelected<T>(_category);
            else if (instance.Is<ISelect>(state2))
                return ((Merge)state2).TypeSelected<T>(_category);

            return false;
        }
    }

    #endregion
}




