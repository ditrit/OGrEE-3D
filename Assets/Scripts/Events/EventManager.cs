using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Event Manager manages publishing raised events to subscribing/listening classes.
///
/// @example subscribe
///     EventManager.Instance.AddListener<SomethingHappenedEvent>(OnSomethingHappened);
///
/// @example unsubscribe
///     EventManager.Instance.RemoveListener<SomethingHappenedEvent>(OnSomethingHappened);
///
/// @example publish an event
///     EventManager.Instance.Raise(new SomethingHappenedEvent());
///
/// This class is a minor variation on <http://www.willrmiller.com/?p=87>
/// </summary>
public class EventManager
{
#pragma warning disable IDE1006 // Name assignment styles
    public static EventManager instance
#pragma warning restore IDE1006 // Name assignment styles
    {
        get
        {
            if (eventManagerInstance == null)
            {
                eventManagerInstance = new EventManager();
            }

            return eventManagerInstance;
        }
    }
    private static EventManager eventManagerInstance = null;

    public delegate void EventDelegate<T>(T e) where T : CustomEvent;
    private delegate void EventDelegate(CustomEvent e);

    /// <summary>
    /// The actual delegate, there is one delegate per unique event. Each
    /// delegate has multiple invocation list items.
    /// </summary>
    private readonly Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();

    /// <summary>
    /// Lookups only, there is one delegate lookup per listener
    /// </summary>
    private readonly Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();

    /// <summary>
    /// Add the delegate.
    /// </summary>
    public void AddListener<T>(EventDelegate<T> del) where T : CustomEvent
    {

        if (delegateLookup.ContainsKey(del))
        {
            return;
        }

        // Create a new non-generic delegate which calls our generic one.
        // This is the delegate we actually invoke.
        EventDelegate internalDelegate = (e) => del((T)e);
        delegateLookup[del] = internalDelegate;

        if (delegates.TryGetValue(typeof(T), out EventDelegate tempDel))
        {
            delegates[typeof(T)] = tempDel += internalDelegate;
        }
        else
        {
            delegates[typeof(T)] = internalDelegate;
        }
    }

    /// <summary>
    /// Remove the delegate. Can be called multiple times on same delegate.
    /// </summary>
    public void RemoveListener<T>(EventDelegate<T> del) where T : CustomEvent
    {

        if (delegateLookup.TryGetValue(del, out EventDelegate internalDelegate))
        {
            if (delegates.TryGetValue(typeof(T), out EventDelegate tempDel))
            {
                tempDel -= internalDelegate;
                if (tempDel == null)
                {
                    delegates.Remove(typeof(T));
                }
                else
                {
                    delegates[typeof(T)] = tempDel;
                }
            }

            delegateLookup.Remove(del);
        }
    }

    /// <summary>
    /// The count of delegate lookups. The delegate lookups will increase by
    /// one for each unique AddListener. Useful for debugging and not much else.
    /// </summary>
    public int DelegateLookupCount { get { return delegateLookup.Count; } }

    /// <summary>
    /// Raise the event to all the listeners
    /// </summary>
    public void Raise(CustomEvent e)
    {
        if (delegates.TryGetValue(e.GetType(), out EventDelegate del))
        {
            del.Invoke(e);
        }
    }

}
