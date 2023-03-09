using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class States : MonoBehaviour
{
    private void Start()
    {
        print(DeviceEdit.Add(TempColor).Remove(EditOnly));
    }

    public static Hashtable test = new Hashtable();

    public abstract class State
    {
        public string name { get; protected set; }
        public int id { get; protected set; }

        public abstract State Ancester();

        public abstract bool Contains(State _state);

        public abstract State Remove(State _state);

        public abstract State Add(State _state);

        public override string ToString()
        {
            return $"State : {name}";
        }
    }
    public class StandAloneState : State
    {
        public StandAloneState parent;

        public StandAloneState(int _id, StandAloneState _parent = null, [CallerMemberName] string _name = null)
        {
            parent = _parent;
            name = _name;
            id = _id;
            test.Add(id, this);
        }

        public override bool Contains(State _state)
        {
            if (this == _state)
                return true;
            if (_state is StandAloneState aloneState)
                return Contains(aloneState.parent);
            return false;
        }

        public override State Ancester()
        {
            if (parent == null)
                return this;
            return parent.Ancester();
        }

        public override string ToString()
        {
            string Aux(StandAloneState s, int i)
            {
                if (parent != null)
                    return $"{new string(' ', i * 2)}{name}\n    <={(s.parent != null ? Aux(s.parent, i + 1) : "")}";
                return $"{new string(' ', i * 2)}{s.name}";
            }
            return Aux(this, 0);
        }

        public override State Add(State _state)
        {
            if (_state.Contains(this))
                return _state;
            if (Contains(_state))
                return this;
            if (test.ContainsKey(id | _state.id))
                return (State)test[id | _state.id];
            if (_state is CompoundState cState)
            {
                List<State> states = cState.impliedStates.GetRange(0, cState.impliedStates.Count);
                states.Add(this);
                return new CompoundState(states, $"{name} | {_state.name}");
            }
            return new CompoundState(new List<State> { this, _state }, $"{name} | {_state.name}");
        }

        public override State Remove(State _state)
        {
            if (_state.Contains(this))
                return None;
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is State state)
                return id == state.id;
            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }

    public class CompoundState : State
    {
        public List<State> impliedStates { get; protected set; }

        public CompoundState(List<State> _impliedStates, [CallerMemberName] string _name = null)
        {
            impliedStates = _impliedStates;
            name = _name;
            id = 0;
            impliedStates.ForEach(s => id |= s.id);
        }

        public override State Ancester()
        {
            return this;
        }

        public override bool Contains(State _state)
        {
            if (_state == this)
                return true;
            foreach (State state in impliedStates)
                if (state.Contains(_state))
                    return true;
            return false;
        }

        public override State Add(State _state)
        {
            if (_state.Contains(this))
                return _state;
            if (Contains(_state))
                return this;
            if (test.ContainsKey(id | _state.id))
                return (State)test[id | _state.id];
            List<State> states = impliedStates.GetRange(0, impliedStates.Count);
            states.Add(_state);
            CompoundState newState = new CompoundState(states, $"{name} | {_state.name}");
            return newState;
        }

        public override State Remove(State _state)
        {
            if (_state == this)
                return None;
            List<State> states = impliedStates.GetRange(0, impliedStates.Count);
            states = states.Select(s => s.Remove(_state)).ToList();
            states.RemoveAll(s => s == None);
            int id = 0;
            states.ForEach(s => id |= s.id);
            if (test.ContainsKey(id))
                return (State)test[id];
            string name = "";
            states.ForEach(s => name += $" | {s.name}");
            return new CompoundState(states,name.Trim(new char[] { '|',' '}));
        }
        public override string ToString()
        {
            string s = $"{name}: [";
            foreach (State state in impliedStates)
                s += $"{state.name}, ";
            return $"{s.Trim(new char[] { ',', ' ' })}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is State state)
                return id == state.id;
            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }

    public static StandAloneState None = new StandAloneState(0);
    public static StandAloneState Select = new StandAloneState(1);
    public static StandAloneState FocusOnly = new StandAloneState(2);
    public static StandAloneState EditOnly = new StandAloneState(4);
    public static StandAloneState OObjectSelect = new StandAloneState(8, Select);
    public static StandAloneState DeviceSelect = new StandAloneState(16, OObjectSelect);
    public static StandAloneState RackSelect = new StandAloneState(32, OObjectSelect);
    public static StandAloneState RoomSelect = new StandAloneState(64, Select);
    public static StandAloneState BuildingSelect = new StandAloneState(128, Select);
    public static StandAloneState SiteSelect = new StandAloneState(256, Select);
    public static StandAloneState TenantSelect = new StandAloneState(512, Select);
    public static CompoundState DeviceFocus = new CompoundState(new List<State> { FocusOnly, DeviceSelect });
    public static CompoundState DeviceEdit = new CompoundState(new List<State> { EditOnly, DeviceFocus });
    public static CompoundState RackFocus = new CompoundState(new List<State> { FocusOnly, RackSelect });
    public static CompoundState RackEdit = new CompoundState(new List<State> { EditOnly, RackFocus });
    public static StandAloneState TempColor = new StandAloneState(1024);
    public static StandAloneState TempDiagram = new StandAloneState(2048);
    public static StandAloneState ScatterPlot = new StandAloneState(4096);
}
