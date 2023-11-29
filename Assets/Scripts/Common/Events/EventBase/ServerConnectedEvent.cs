using System;

public class ServerConnectedEvent : IEvent
{
    private RemoteConnetionType serverId;

    public ServerConnectedEvent(RemoteConnetionType serverType)
    {
        serverId = serverType;
    }

    public object Param => null;

    public int GetMessageKey()
    {
        return Convert.ToInt32(serverId);
    }

    public bool GetSendAll()
    {
        return false;
    }
}
