using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class ClientPeer : IPhotonPeerListener
{
    private PhotonPeer peer;
    public StatusCode StatusCode;

    private RemoteConnetionType _remoteConnetionType;
    public RemoteConnetionType RemoteConnType() { return this._remoteConnetionType; }

    private Dictionary<int, PhotonPeer> peerConnectorTable = new Dictionary<int, PhotonPeer>();

    public ClientPeer(RemoteConnetionType remoteConnetionType)
    {
        _remoteConnetionType = remoteConnetionType;
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnEvent(EventData eventData)
    {
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        if (operationResponse.OperationCode == (byte)NetOperationCode.ServerClient)
            NetworkHandler.Instance.OnMessageArrived(operationResponse.Parameters);
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        switch (statusCode)
        {
            case StatusCode.ExceptionOnConnect:
                onSocketConnectFailed();
                break;
            case StatusCode.Connect:
                OnSocketConnect();
                break;
            case StatusCode.Disconnect:
                OnSoketDisconnect();
                break;
            case StatusCode.DisconnectByServer:
            case StatusCode.DisconnectByServerLogic:
            case StatusCode.DisconnectByServerUserLimit:
            case StatusCode.TimeoutDisconnect:
            case StatusCode.EncryptionEstablished:
            default:
                break;
        }
        this.StatusCode = statusCode;
    }

    private void OnSocketConnect()
    {
        var serverConnectedEvent = new ServerConnectedEvent(_remoteConnetionType);
        EventManager.Instance.SendEvent(serverConnectedEvent);
    }

    public void onSocketConnectFailed()
    {
        // var serverDisconnectedEvent = new ServerDisconnectedEvent(this._remoteConnetionType);
        // EventManager.Instance.Send(serverDisconnectedEvent);
        Debug.Log("ConnectFailed!!");
    }

    private void OnSoketDisconnect()
    {
        Debug.Log(_remoteConnetionType + " Disconnect!!");
        Disconnect();
    }

    public void Connect(string serverAddreas, string serverName)
    {
        peer = new PhotonPeer(this, ConnectionProtocol.Tcp);
        peer.Connect(serverAddreas, serverName);
    }

    public void Disconnect()
    {
        peer.Disconnect();
        var gameDisconnectedEvent = new GameControlEvent(GameControlEventMessage.GameDisconnected, _remoteConnetionType);
        EventManager.Instance.SendEvent(gameDisconnectedEvent);
    }

    public void Send(NetOperationCode netOperationCode, Dictionary<byte, object> message)
    {
        peer.OpCustom((byte)netOperationCode, message, true);
    }

    public void Update()
    {
        if (peer != null)
            peer.Service();
    }
}
