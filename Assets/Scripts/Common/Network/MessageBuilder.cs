using System.Collections.Generic;
using UnityEngine;

public enum NetMsgFieldType
{
    Boolean,
    Byte,
    Short,
    Int,
    Float,
    Long,
    String,
    Double,
    Object,
    Array,
    UnityVector3,
}


public class MessageBuilder
{
    private Dictionary<byte, object> _collectedNetMessage = new Dictionary<byte, object>();

    private Dictionary<int, object> _collectedMessage = new Dictionary<int, object>();


    public Dictionary<byte, object> BuildNetMsg()
    {
        return _collectedNetMessage;
    }

    public Dictionary<int, object> BuildMsg()
    {
        return _collectedMessage;
    }

    public void AddNetMsg(byte msgKey, object msgValue, NetMsgFieldType msgValueType)
    {
        string functionedValue = string.Empty;

        switch (msgValueType)
        {
            case NetMsgFieldType.Byte:
                functionedValue = "#b" + msgValue.ToString();
                break;
            case NetMsgFieldType.Short:
                functionedValue = "#s" + msgValue.ToString();

                break;
            case NetMsgFieldType.Int:
                functionedValue = "#i" + msgValue.ToString();
                break;
            case NetMsgFieldType.Float:
                functionedValue = "#f" + msgValue.ToString();
                break;
            case NetMsgFieldType.Long:
                functionedValue = "#l" + msgValue.ToString();
                break;
            case NetMsgFieldType.Array:
                MessageBuilder objectMessage = new MessageBuilder();
                objectMessage.AddMsg(((int)ArrayIndicator.MessageType), true, NetMsgFieldType.Boolean);
                objectMessage.AddMsg(((int)ArrayIndicator.MessageData), msgValue, NetMsgFieldType.Object);
                this._collectedNetMessage.Add(msgKey, objectMessage.BuildMsg());
                return;
        }

        if (functionedValue != string.Empty)
        {
            this._collectedNetMessage.Add(msgKey, (object)functionedValue);
        }
        else
        {
            this._collectedNetMessage.Add(msgKey, msgValue);
        }
    }

    public void AddMsg(int msgKey, object msgValue, NetMsgFieldType msgValueType)
    {
        string functionedValue = string.Empty;

        switch (msgValueType)
        {
            case NetMsgFieldType.Byte:
                functionedValue = "#b" + msgValue.ToString();
                break;
            case NetMsgFieldType.Short:
                functionedValue = "#s" + msgValue.ToString();
                break;
            case NetMsgFieldType.Int:
                functionedValue = "#i" + msgValue.ToString();
                break;
            case NetMsgFieldType.Float:
                functionedValue = "#f" + msgValue.ToString();
                break;
            case NetMsgFieldType.Long:
                functionedValue = "#l" + msgValue.ToString();
                break;
            case NetMsgFieldType.Array:
                MessageBuilder objectMessage = new MessageBuilder();
                objectMessage.AddMsg(((int)ArrayIndicator.MessageType), true, NetMsgFieldType.Boolean);
                objectMessage.AddMsg(((int)ArrayIndicator.MessageData), msgValue, NetMsgFieldType.Object);
                this._collectedMessage.Add(msgKey, objectMessage.BuildMsg());
                return;
            case NetMsgFieldType.UnityVector3:
                functionedValue = "#v3" + msgValue.ToString();
                break;
            default:
                break;
        }

        if (functionedValue != string.Empty)
        {
            this._collectedMessage.Add(msgKey, (object)functionedValue);
        }
        else
        {
            this._collectedMessage.Add(msgKey, msgValue);
        }
    }
}
