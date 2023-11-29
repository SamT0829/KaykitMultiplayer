using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IEvent
{
    object Param { get; }

    Type GetEventType()
    {
        return this.GetType();
    }

    T GetEventObject<T>() where T : IEvent
    {
        return (T)this;
    }

    bool GetEventObject<T>(out T outEventObject) where T : IEvent
    {
        if (typeof(T) == this.GetType())
        {
            outEventObject = (T)this;
            return true;
        }

        outEventObject = default;
        return false;
    }

    bool GetSendAll();

    int GetMessageKey();
}

public class EventCaller
{
    private Type eventType;
    private Action<IEvent> _functionCall;
    private int _messageKey;

    public EventCaller(Type type, Action<IEvent> callerObject, int messageKey)
    {
        eventType = type;
        _functionCall = callerObject;
        _messageKey = messageKey;
    }

    public Type EventType()
    {
        return eventType;
    }

    public Action<IEvent> FunctionCall()
    {
        return _functionCall;
    }

    public int GetMessageKey()
    {
        return _messageKey;
    }
}