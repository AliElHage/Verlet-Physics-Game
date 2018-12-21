using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private static Dictionary<string, UnityEvent> _events;

    public void Connect(string eventName, UnityAction listener)
    {
        if(_events == null)
        {
            _events = new Dictionary<string, UnityEvent>();
        }

        UnityEvent currentEvent;
        if(_events.TryGetValue(eventName, out currentEvent))
        {
            currentEvent.AddListener(listener);
        }
        else
        {
            currentEvent = new UnityEvent();
            currentEvent.AddListener(listener);
            _events.Add(eventName, currentEvent);
        }
    }

    public void Disconnect(string eventName, UnityAction listener)
    {
        if(_events == null)
        {
            throw new UnityException("Event dictionary was not initialized or was destroyed.");
        }

        UnityEvent currentEvent;
        if (_events.TryGetValue(eventName, out currentEvent))
        {
            currentEvent.RemoveListener(listener);
        }
        else
        {
            Debug.LogWarning("No event to remove listeners from.");
        }
    }

    public void Invoke(string eventName)
    {
        if (_events == null)
        {
            throw new UnityException("Event dictionary was not initialized or was destroyed.");
        }

        UnityEvent currentEvent;
        if (_events.TryGetValue(eventName, out currentEvent))
        {
            currentEvent.Invoke();
        }
        else
        {
            Debug.LogWarning("No event to invoke.");
        }
    }
}
