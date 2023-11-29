using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    [SerializeField] KayKitBrawlControler kayKitBrawlControler;
    [SerializeField] KayKitTeamBrawlController kayKitTeamBrawlController;
    [SerializeField] KayKitTeamBrawlModel kayKitTeamBrawlMode;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerPrepareEnterRespond, OnLobbyPlayerPrepareEnterRespond);

        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterGame, OnPlayerEnterGame);
        EventManager.Instance.RegisterEventListener<ServerConnectedEvent>((int)RemoteConnetionType.Game, OnGameServerConnected);
    }

    private void OnDisable()
    {
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerPrepareEnterRespond, OnLobbyPlayerPrepareEnterRespond);

        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterGame, OnPlayerEnterGame);
        EventManager.Instance.UnRegisterEventListener<ServerConnectedEvent>((int)RemoteConnetionType.Game, OnGameServerConnected);
    }

    private void OnPlayerEnterGame(IEvent @event)
    {
        GameType gameType = (GameType)@event.GetEventObject<GameControlEvent>().Param;

        switch (gameType)
        {
            case GameType.KayKitCoinBrawl:
                kayKitBrawlControler.enabled = true;
                break;
            case GameType.KayKitTeamBrawl:
                kayKitTeamBrawlController.enabled = true;
                kayKitTeamBrawlMode.enabled = true;
                break;
        }
    }

    private void OnGameServerConnected(IEvent @event)
    {
        MessageBuilder msgBuilder;
        msgBuilder = new MessageBuilder();
        msgBuilder.AddMsg((int)GameConnectedRequest.SessionId, ClientData.Instance.SessionId, NetMsgFieldType.Int);
        NetworkHandler.Instance.Send(RemoteConnetionType.Game, ClientHandlerMessage.GameConnectedRequest, msgBuilder);
    }
    private void OnLobbyPlayerPrepareEnterRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnLobbyPlayerPrepareEnterRespond" + JsonConvert.SerializeObject(message));

        if (!message.RetrieveMessageItem(LobbyPlayerPrepareEnterRespond.ErrorCode, out ErrorCode errorCode))
            return;

        if (errorCode == ErrorCode.Success)
        {
            if (!message.RetrieveMessageItem(LobbyPlayerPrepareEnterRespond.GameServerIP, out string gameServerIP)) return;
            if (!message.RetrieveMessageItem(LobbyPlayerPrepareEnterRespond.GameServerPort, out int gameServerPort)) return;

            ClientData.Instance.GameServerAddreas = string.Format("{0}:{1}", gameServerIP, gameServerPort);
            NetworkHandler.Instance.Connect(RemoteConnetionType.Game, ClientData.Instance.GameServerAddreas, GameManager.Instance.ServerName);
        }
        else
        {
            UIManager.Instance.CreateMessageTipsUI("Error Message", errorCode.ToString(), "Confirm", null, null);
        }
    }
}
