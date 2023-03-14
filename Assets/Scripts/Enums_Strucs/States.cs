using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class States : MonoBehaviour
{
    private void Start()
    {
        print(TempColor.Add(RackEdit).Add(RackFocus));
        print(RackFocus.Add(DeviceSelect));
        foreach(State state1 in states)
        {
            foreach(State state2 in states)
            {
                print($"{state1.name} + {state2.name} = {state1.Add(state2)}");
                print($"{state2.name} + {state1.name} = {state2.Add(state1)}");
            }
        }
    }

    public static Hashtable test = new Hashtable();

    public abstract class State
    {
        public string name { get; protected set; }
        public int id { get; protected set; }

        public abstract State Ancester();

        public abstract bool Contains(State _state);

        public abstract State Remove(StandAloneState _state);

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
                if (s.parent != null)
                    return $"{s.name}\n{new string(' ', i * 4)}<={Aux(s.parent, i + 1)}";
                return s.name;
            }
            return Aux(this, 1);
        }

        public override State Add(State _state)
        {
            if (_state.Contains(this))
                return this;
            if (Contains(_state))
                return _state;
            if (_state is StandAloneState SAstate && SAstate.Ancester() == Ancester())
                return _state;
            if (test.ContainsKey(id | _state.id))
                return (State)test[id | _state.id];
            return new CompoundState(new List<State> { this, _state }, $"{name} | {_state.name}");
        }

        public override State Remove(StandAloneState _state)
        {
            if (_state.Contains(this))
                return Idle;
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
            impliedStates = new List<State>();
            foreach (State state in _impliedStates)
            {
                if (state is StandAloneState)
                    impliedStates.Add(state);
                else if (state is CompoundState cState)
                    foreach (State childState in cState.impliedStates)
                        impliedStates.Add(childState);
            }
            name = _name;
            id = 0;
            impliedStates.ForEach(s => id |= s.id);
            test.Add(id, this);
        }

        public override State Ancester()
        {
            return this;
        }

        public override bool Contains(State _state)
        {
            if (id != 0 && _state.id !=0 && (id & _state.id) == _state.id)
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
            List<State> states = new List<State>();
            if (_state is StandAloneState SAState)
            {
                foreach (State state in impliedStates)
                {
                    if (state is StandAloneState aloneState && aloneState.Ancester() == SAState.Ancester())
                        continue;
                    states.Add(state);
                }
            }
            else
                states = impliedStates.GetRange(0, impliedStates.Count);
            states.Add(_state);
            int newId = 0;
            states.ForEach(s => newId |= s.id);
            if (test.ContainsKey(newId))
                return (State)test[newId];
            CompoundState newState = new CompoundState(states, $"{name} | {_state.name}");
            return newState;
        }

        public override State Remove(StandAloneState _state)
        {
            List<State> states = impliedStates.GetRange(0, impliedStates.Count);
            states = states.Select(s => s.Remove(_state)).ToList();
            states.RemoveAll(s => s == Idle);
            int id = 0;
            states.ForEach(s => id |= s.id);
            if (test.ContainsKey(id))
                return (State)test[id];
            string name = "";
            states.ForEach(s => name += $" | {s.name}");
            return new CompoundState(states, name.Trim(new char[] { '|', ' ' }));
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

    public static StandAloneState Idle = new StandAloneState(0);
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
    public static CompoundState RackFocus = new CompoundState(new List<State> { FocusOnly, OObjectSelect });
    public static CompoundState RackEdit = new CompoundState(new List<State> { EditOnly, RackFocus });
    public static StandAloneState TempColor = new StandAloneState(1024);
    public static StandAloneState TempDiagram = new StandAloneState(2048);
    public static StandAloneState ScatterPlot = new StandAloneState(4096);
    public static StandAloneState HeatMap = new StandAloneState(8192);

    public static List<State> states = new List<State>() 
    {
        Idle,
        Select,
        FocusOnly,
        EditOnly,
        OObjectSelect,
        DeviceSelect,
        RackSelect,
        RoomSelect,
        BuildingSelect,
        SiteSelect,
        TenantSelect,
        DeviceFocus,
        DeviceEdit,
        RackFocus,
        RackEdit,
        TempColor,
        TempDiagram,
        ScatterPlot,
        HeatMap
    };
}