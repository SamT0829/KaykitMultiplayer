using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;

public class AccountLoginUIController : MonoBehaviour
{
    [Header("AccountUI")]
    [SerializeField] RectTransform LoginUI;
    [SerializeField] RectTransform RegisterUI;
    [SerializeField] RectTransform RegisterPlayerUI;

    [Header("GameLoginUI")]
    [SerializeField] InputField LoginIdInputField;
    [SerializeField] InputField LoginPasswordInputField;
    [SerializeField] Button LoginConfirmButton;
    [SerializeField] Button RegisterButton;

    [Header("Register Account UI")]
    [SerializeField] InputField RegisterIdInputField;
    [SerializeField] InputField RegisterPasswordInputField;
    [SerializeField] Button RegisterConfirmButton;
    [SerializeField] Button LoginButton;

    [Header("Register Player UI")]
    [SerializeField] InputField PlayerNicknameInputField;
    [SerializeField] Button RegisterPlayerConfirmButton;
    [SerializeField] Button BackToLoginButton;

    private string gameId;
    private string gamePassword;
    [SerializeField] bool OnTest;

    private int FPSCount;
    private DateTime FPSTimer;

    private void Start()
    {
        NetworkHandler.Instance.Connect(RemoteConnetionType.Account, GameManager.Instance.ServerAddreas, GameManager.Instance.ServerName);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.AccountConnectedRespond, OnAccountConnectRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.AccountRegisterRespond, OnAccountRegisterRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.AccountLoginRespond, OnAccountLoginRespond);

        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyConnectedRespond, OnLobbyLoginRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerRegisterRespond, OnLobbyPlayerRegisterPlayerRespond);

        EventManager.Instance.RegisterEventListener<ServerConnectedEvent>(RemoteConnetionType.Account.GetHashCode(), OnAccountServerConnected);
        EventManager.Instance.RegisterEventListener<ServerConnectedEvent>(RemoteConnetionType.Lobby.GetHashCode(), OnLobbyServerConnected);

        LoginConfirmButton.onClick.AddListener(OnClickLoginConfirmButton);
        RegisterButton.onClick.AddListener(OnClickRegisterButton);
        RegisterConfirmButton.onClick.AddListener(OnClickRegisterConfirmButton);
        LoginButton.onClick.AddListener(OnClickLoginButton);
        RegisterPlayerConfirmButton.onClick.AddListener(OnClickRegisterPlayerConfirmButton);
        // BackToLoginButton.onClick.AddListener(OnClickBackToLoginButton);
    }

    private void OnDisable()
    {
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.AccountConnectedRespond, OnAccountConnectRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.AccountRegisterRespond, OnAccountRegisterRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.AccountLoginRespond, OnAccountLoginRespond);

        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyConnectedRespond, OnLobbyLoginRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerRegisterRespond, OnLobbyPlayerRegisterPlayerRespond);

        EventManager.Instance.UnRegisterEventListener<ServerConnectedEvent>(RemoteConnetionType.Account.GetHashCode(), OnAccountServerConnected);
        EventManager.Instance.UnRegisterEventListener<ServerConnectedEvent>(RemoteConnetionType.Lobby.GetHashCode(), OnLobbyServerConnected);
    }

    private void FixedUpdate()
    {
        FPSCount++;
        if (FPSTimer == null)
            FPSTimer = DateTime.Now;


        var timer = DateTime.Now;
        if ((timer - FPSTimer) > TimeSpan.FromSeconds(1))
        {
            Debug.Log(FPSCount);
            FPSCount = 0;
            FPSTimer = timer;
        }

    }

    // Button
    private void OnClickLoginConfirmButton()
    {
        gameId = LoginIdInputField.text;
        gamePassword = LoginPasswordInputField.text;
        SendAccountLoginRequest(GameType.KaykitGame, gameId, gamePassword);
    }
    private void OnClickRegisterButton()
    {
        RegisterUI.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuint);
        LoginUI.DOLocalMoveY(400, 0.5f).SetEase(Ease.OutSine);
    }
    private void OnClickRegisterConfirmButton()
    {
        var gameId = RegisterIdInputField.text;
        var gamePassword = RegisterPasswordInputField.text;

        MessageBuilder messageBuilder = new MessageBuilder();
        messageBuilder.AddMsg(((int)AccountRegisterRequest.GameType), GameType.KaykitGame.GetHashCode(), NetMsgFieldType.Int);
        messageBuilder.AddMsg(((int)AccountRegisterRequest.GameId), gameId, NetMsgFieldType.String);
        messageBuilder.AddMsg(((int)AccountRegisterRequest.Password), gamePassword, NetMsgFieldType.String);
        NetworkHandler.Instance.Send(RemoteConnetionType.Account, ClientHandlerMessage.AccountRegisterRequest, messageBuilder);
    }
    private void OnClickLoginButton()
    {
        LoginUI.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuint);
        RegisterUI.DOLocalMoveY(400, 0.5f).SetEase(Ease.OutSine);
    }
    private void OnClickRegisterPlayerConfirmButton()
    {
        string playerNickname = PlayerNicknameInputField.text;

        MessageBuilder messageBuilder = new MessageBuilder();
        messageBuilder.AddMsg((int)LobbyPlayerRegisterRequest.SessionId, ClientData.Instance.SessionId, NetMsgFieldType.Int);
        messageBuilder.AddMsg((int)LobbyPlayerRegisterRequest.GameType, GameType.KaykitGame.GetHashCode(), NetMsgFieldType.Int);
        messageBuilder.AddMsg((int)LobbyPlayerRegisterRequest.Nickname, playerNickname, NetMsgFieldType.String);
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerRegisterRequest, messageBuilder);
    }

    // Event    
    private void OnAccountServerConnected(IEvent @event)
    {
        MessageBuilder messageBuilder = new MessageBuilder();
        NetworkHandler.Instance.Send(RemoteConnetionType.Account, ClientHandlerMessage.AccountConnectedRequest, messageBuilder);
    }
    private void OnLobbyServerConnected(IEvent @event)
    {
        Debug.Log("OnLobbyServerConnected");
        MessageBuilder messageBuilder = new MessageBuilder();
        messageBuilder.AddMsg((int)LobbyLoginRequest.SessionId, ClientData.Instance.SessionId, NetMsgFieldType.Int);
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyConnectedRequest, messageBuilder);
    }

    // Account
    private void OnAccountConnectRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnAccountConnectRespond");

        if (OnTest)
            SendAccountLoginRequest(GameType.KaykitGame, "sam", "sam");
    }
    private void OnAccountRegisterRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnAccountRegisterRespond" + JsonConvert.SerializeObject(message));
    }
    private void OnAccountLoginRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnAccountLoginRespond" + JsonConvert.SerializeObject(message));

        //處理Server傳來的登入資訊
        if (!message.RetrieveMessageItem(AccountLoginRespond.ErrorCode, out ErrorCode errorCode)) return;
        if (!message.RetrieveMessageItem(AccountLoginRespond.SessionId, out int sessionId)) return;
        if (!message.RetrieveMessageItem(AccountLoginRespond.LobbyServerIP, out string lobbyServerIp)) return;
        if (!message.RetrieveMessageItem(AccountLoginRespond.LobbyServerPort, out int lobbyServerPort)) return;

        ClientData.Instance.SessionId = sessionId;
        ClientData.Instance.LobbyServerAddreas = string.Format("{0}:{1}", lobbyServerIp, lobbyServerPort);

        if (errorCode == ErrorCode.Success)
        {
            NetworkHandler.Instance.Connect(RemoteConnetionType.Lobby, ClientData.Instance.LobbyServerAddreas, GameManager.Instance.ServerName);
            // UIManager.Instance.CreateLoadingMission(new ServerLobbyEnterMission()
            // {
            //     OnFinish = () =>
            //     {
            //         GameUIEvent e = new GameUIEvent(GameUIMessageEvent.EnterGameLobby);
            //         EventManager.Instance.SendEvent(e);
            //         return Task.CompletedTask;
            //     }
            // });
            // UIManager.Instance.StartLoadingMission();
        }
        else
        {
            Debug.Log(errorCode);
            UIManager.Instance.CreateMessageTipsUI("Error Message", errorCode.ToString(), "Confirm", null, null);
        }
    }

    // Lobby
    private void OnLobbyLoginRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnLobbyLoginRespond" + JsonConvert.SerializeObject(message));
        if (!message.RetrieveMessageItem(LobbyConnectedRespond.ErrorCode, out ErrorCode errorCode)) return;

        if (errorCode == ErrorCode.Success)
        {
            bool isPlayerInfoDataNotExist = false;

            message.RetrieveMessageItem(LobbyConnectedRespond.IsPlayerInfoDataNotExist, out isPlayerInfoDataNotExist, false);

            if (isPlayerInfoDataNotExist)
            {
                // Register PlayerInfo
                RegisterPlayerUI.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InOutQuad);
                LoginUI.DOLocalMoveY(400, 0.5f).SetEase(Ease.OutSine);
            }
            else
            {
                if (!message.RetrieveMessageItem(LobbyConnectedRespond.LobbyPlayerInfo, out string lobbyPlayerInfoData)) return;
                if (!message.RetrieveMessageItem(LobbyConnectedRespond.LobbyGameInfo, out string lobbyGameInfoData)) return;

                ClientData.Instance.LobbyPlayerInfo = JsonConvert.DeserializeObject<LobbyPlayerInfo>(lobbyPlayerInfoData);

                GameControlEvent e = new GameControlEvent(GameControlEventMessage.PlayerEnterLobbyScene);

                GameManager.Instance.TeleportToScene(GameScene.AccountScene, GameScene.LobbyScene, null, () => EventManager.Instance.SendEvent(e));

                Debug.Log("OnLobbyLoginRespond" + JsonConvert.SerializeObject(message));
            }
        }
    }
    private void OnLobbyPlayerRegisterPlayerRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnLobbyRegisterPlayerRespond" + JsonConvert.SerializeObject(message));

        if (!message.RetrieveMessageItem(LobbyPlayerRegisterRespond.ErrorCode, out ErrorCode errorCode)) return;
        if (errorCode == ErrorCode.Success)
        {
            bool isNickNameAlreadyUsed;

            if (!message.RetrieveMessageItem(LobbyPlayerRegisterRespond.NickNameAlreadyUsed, out isNickNameAlreadyUsed)) return;

            if (isNickNameAlreadyUsed)
            {
                UIManager.Instance.CreateMessageTipsUI("Error Message", "Player Nickname already used please try again !!", "Confirm", null,
                () => PlayerNicknameInputField.text = "");
            }
            else
            {
                MessageBuilder messageBuilder = new MessageBuilder();
                messageBuilder.AddMsg(((int)LobbyLoginRequest.SessionId), ClientData.Instance.SessionId, NetMsgFieldType.Int);
                NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyConnectedRequest, messageBuilder);
            }
        }
    }
    private void SendAccountLoginRequest(GameType gameType, string gameId, string gamePassword)
    {
        MessageBuilder messageBuilder = new MessageBuilder();
        messageBuilder.AddMsg(((int)AccountLoginRequest.GameType), gameType.GetHashCode(), NetMsgFieldType.Int);
        messageBuilder.AddMsg(((int)AccountLoginRequest.GameId), gameId, NetMsgFieldType.String);
        messageBuilder.AddMsg(((int)AccountLoginRequest.Password), gamePassword, NetMsgFieldType.String);
        NetworkHandler.Instance.Send(RemoteConnetionType.Account, ClientHandlerMessage.AccountLoginRequest, messageBuilder);
    }
}
