using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

public class NetworkHandler : MonoBehaviour
{
    private static NetworkHandler _instance;
    public static NetworkHandler Instance { get { return _instance; } }

    private Dictionary<RemoteConnetionType, ClientPeer> remoteConnectorTable = new Dictionary<RemoteConnetionType, ClientPeer>();
    private Dictionary<ClientHandlerMessage, Action<int, Dictionary<int, object>>> messageDispatchTable = new();

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            _instance = this;


        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        NetworkUpdate();
    }

    private void NetworkUpdate()
    {
        if (remoteConnectorTable.Count > 0)
        {
            var peerList = remoteConnectorTable.Values.ToList();
            peerList.ForEach(peer => peer.Update());
        }
    }

    public void Connect(RemoteConnetionType connectionId, string serverAddreas, string serverName)
    {
        if (remoteConnectorTable.TryGetValue(connectionId, out ClientPeer peer))
        {
            peer.Disconnect();
            remoteConnectorTable.Remove(connectionId);
            Debug.Log("peer Disconnect with ID: " + serverAddreas + connectionId);
        }

        remoteConnectorTable[connectionId] = new ClientPeer(connectionId);
        remoteConnectorTable[connectionId].Connect(serverAddreas, serverName);
        Debug.Log("peer connect with ID: " + serverAddreas + connectionId);
    }

    public void Disconnect(RemoteConnetionType connectionId)
    {
        if (remoteConnectorTable.TryGetValue(connectionId, out ClientPeer peer))
        {
            peer.Disconnect();
            remoteConnectorTable.Remove(connectionId);
            Debug.Log("peer Disconnect with ID: " + connectionId);
        }
    }

    public void DisconnectAll()
    {
        if (remoteConnectorTable.Count <= 0)
            return;

        var keys = remoteConnectorTable.Keys.ToList();

        foreach (var remoteConnector in keys)
        {
            Disconnect(remoteConnector);
        }
    }

    public bool IsConnect(RemoteConnetionType connectionId)
    {
        return remoteConnectorTable.ContainsKey(connectionId);
    }

    /**註冊ClientCallBack訊息*/
    public void RegisterMessageListener(ClientHandlerMessage msgType, Action<int, Dictionary<int, object>> callback)
    {
        Action<int, Dictionary<int, object>> listener;
        if (!messageDispatchTable.TryGetValue(msgType, out listener))
            messageDispatchTable[msgType] = callback;

        else
        {
            if (messageDispatchTable[msgType] != callback)
                messageDispatchTable[msgType] += callback;
        }
    }

    public void UnRegisterMessageListener(ClientHandlerMessage msgType, Action<int, Dictionary<int, object>> callback)
    {
        if (messageDispatchTable.TryGetValue(msgType, out Action<int, Dictionary<int, object>> listener))
        {
            messageDispatchTable[msgType] -= callback;

            if (messageDispatchTable[msgType] == null)
                messageDispatchTable.Remove(msgType);
        }
    }

    public void Send(RemoteConnetionType connectionId, ClientHandlerMessage msgType, MessageBuilder message)
    {
        if (!IsConnect(connectionId))
        {
            Debug.Log($"Peer {connectionId} Disconnect");
            return;
        }

        MessageBuilder builtMessage = message;

        //<-----------------判斷是否為玩家訊息------------------------>
        if (msgType > ClientHandlerMessage.LobbyPlayerMessageBegin && msgType < ClientHandlerMessage.LobbyPlayerMessageEnd)
        {
            MessageBuilder playerMessage = new MessageBuilder();
            playerMessage.AddMsg(((int)PlayerMessage.HandlerMessageType), (int)msgType, NetMsgFieldType.Int);
            playerMessage.AddMsg(((int)PlayerMessage.HandlerMessageData), message.BuildMsg(), NetMsgFieldType.Object);

            msgType = ClientHandlerMessage.LobbyPlayerMessage;
            builtMessage = playerMessage;
        }

        if (msgType > ClientHandlerMessage.GamePlayerMessageBegin && msgType < ClientHandlerMessage.GamePlayerMessageEnd)
        {
            MessageBuilder gamePlayerMessage = new MessageBuilder();
            gamePlayerMessage.AddMsg(((int)PlayerMessage.HandlerMessageType), (int)msgType, NetMsgFieldType.Int);
            gamePlayerMessage.AddMsg(((int)PlayerMessage.HandlerMessageData), message.BuildMsg(), NetMsgFieldType.Object);
            msgType = ClientHandlerMessage.GamePlayerMessage;
            builtMessage = gamePlayerMessage;
        }

        MessageBuilder outMessage = new MessageBuilder();
        outMessage.AddNetMsg(((byte)NetOperationType.MessageType), (int)MessageType.ClientHandlerMessage, NetMsgFieldType.Int);
        outMessage.AddNetMsg(((byte)NetOperationType.MessageID), (int)msgType, NetMsgFieldType.Int);
        outMessage.AddNetMsg(((byte)NetOperationType.RemoteType), (int)RemoteConnetionType.Client, NetMsgFieldType.Int);
        outMessage.AddNetMsg(((byte)NetOperationType.Data), builtMessage.BuildMsg(), NetMsgFieldType.Object);
        outMessage.AddNetMsg(((byte)NetOperationType.SelfDefinedType), true, NetMsgFieldType.Boolean);
        ((ClientPeer)remoteConnectorTable[connectionId]).Send(NetOperationCode.ClientServer, outMessage.BuildNetMsg());
    }

    public void OnMessageArrived(Dictionary<byte, object> rowMessage)
    {
        ClientHandlerMessage msgType = (ClientHandlerMessage)rowMessage[((byte)NetOperationType.MessageID)];
        RemoteConnetionType remoteConnetionType = (RemoteConnetionType)rowMessage[((byte)NetOperationType.RemoteType)];
        Dictionary<int, object> userData = (Dictionary<int, object>)rowMessage[((byte)NetOperationType.Data)];

        if (msgType == ClientHandlerMessage.LobbyPlayerMessage)
        {
            msgType = (ClientHandlerMessage)userData[(((int)PlayerMessage.HandlerMessageType))];
            userData = (Dictionary<int, object>)userData[((int)PlayerMessage.HandlerMessageData)];
        }

        if (msgType == ClientHandlerMessage.GamePlayerMessage)
        {
            msgType = (ClientHandlerMessage)userData[((int)PlayerMessage.HandlerMessageType)];
            userData = (Dictionary<int, object>)userData[((int)PlayerMessage.HandlerMessageData)];
        }

        DispatchEvent(msgType, remoteConnetionType, userData);
    }

    private void DispatchEvent(ClientHandlerMessage msgType, RemoteConnetionType remoteConnetionType, Dictionary<int, object> message)
    {
        if (messageDispatchTable.ContainsKey(msgType))
            messageDispatchTable[msgType].Invoke(((byte)remoteConnetionType), message);
    }
}
