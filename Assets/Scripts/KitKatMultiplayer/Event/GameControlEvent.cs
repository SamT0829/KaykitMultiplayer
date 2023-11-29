using System;


public class GameControlEvent : IEvent
{
    private GameControlEventMessage _message;
    private object _param;

    public object Param { get => _param; }

    public GameControlEvent(GameControlEventMessage message, object param = null)
    {
        _message = message;
        _param = param;
    }

    int IEvent.GetMessageKey()
    {
        return Convert.ToInt32(_message);
    }

    bool IEvent.GetSendAll()
    {
        return false;
    }
}
