using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class LobbyRoomUIController : MonoBehaviour
{
    [Header("Lobby Room UI")]
    [SerializeField] GameObject LobbyRoomPanel;
    [SerializeField] Text LobbyRoomID;
    [SerializeField] Text LobbyRoomType;
    [SerializeField] Text LobbyRoomName;
    [SerializeField] LobbyRoomPlayerPrefab[] LobbyRoomPlayerPrefabArray;

    [Header("Lobby Room Chat UI")]
    [SerializeField] InputField LobbyRoomChatInputField;
    [SerializeField] bool onSubmit;


    [Header("Lobby Room Button")]
    [SerializeField] Button StartGameButton;
    [SerializeField] Button LeaveGameButton;
    [SerializeField] Button ReadyGameButton;
    [SerializeField] Button ChangeTeamButton;

    [Header("GameRoomPlayerPrefab")]
    [SerializeField] LobbyRoomPlayerPrefab lobbyRoomPlayerPrefab;
    [SerializeField] Transform LobbyRoomPlayerListPanelTransform;                 // brawl game
    [SerializeField] Transform LobbyRoomPlayerRedTeamListPanelTransform;          // team game
    [SerializeField] Transform LobbyRoomPlayerBlueTeamListPanelTransform;          // team game


    [Header("ChatTextPrefab")]
    [SerializeField] ChatMessagePrefab chatMessagePrefab;
    [SerializeField] Transform GameRoomMessageContentTransform;
    [SerializeField] List<ChatMessagePrefab> ChatMessagePrefabList = new List<ChatMessagePrefab>();
    [SerializeField] int maxMessage;

    private void OnEnable()
    {
        // BackgroundThread
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyRoomBackgroundThread, OnLobbyRoomBackgroundThread);

        // Lobby Player
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerLeaveLobbyRoomRespond, OnLobbyPlayerLeaveLobbyRoomRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerReadyLobbyRoomRespond, OnLobbyPlayerReadyLobbyRoomRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerStartLobbyRoomRespond, OnLobbyPlayerStartLobbyRoomRespond);

        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyRoomEnterGameRoom, OnLobbyRoomEnterGameRoom);

        // Event
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterLobbyRoom, OnPlayerEnterLobbyRoomEvent);
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.GameRoomEnterToLobbyRoom, OnGameRoomEnterToLobbyRoom);
    }
    private void OnDisable()
    {
        // BackgroundThread
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyRoomBackgroundThread, OnLobbyRoomBackgroundThread);

        // Lobby Player
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerLeaveLobbyRoomRespond, OnLobbyPlayerLeaveLobbyRoomRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerReadyLobbyRoomRespond, OnLobbyPlayerReadyLobbyRoomRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerStartLobbyRoomRespond, OnLobbyPlayerStartLobbyRoomRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerChangeTeamLobbyRoomRespond, OnLobbyPlayerStartLobbyRoomRespond);

        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyRoomEnterGameRoom, OnLobbyRoomEnterGameRoom);

        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterLobbyRoom, OnPlayerEnterLobbyRoomEvent);
        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.GameRoomEnterToLobbyRoom, OnGameRoomEnterToLobbyRoom);
    }
    private void Awake()
    {
        StartGameButton.onClick.AddListener(OnClickStartGameButton);
        LeaveGameButton.onClick.AddListener(OnClickLeaveGameButton);
        ReadyGameButton.onClick.AddListener(OnClickReadyGameButton);
    }
    private void Update()
    {
        if (LobbyRoomPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                LobbyRoomChatInputField.ActivateInputField();
                SendLobbyPlayerChatLobbyRoomRequest();
            }
        }
    }
    private void SendLobbyPlayerChatLobbyRoomRequest()
    {
        if (LobbyRoomChatInputField.text != string.Empty)
        {
            MessageBuilder msgBuilder = new MessageBuilder();
            msgBuilder.AddMsg(((int)LobbyPlayerChatLobbyRoomRequest.ChatMsg), LobbyRoomChatInputField.text, NetMsgFieldType.String);
            NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerChatLobbyRoomRequest, msgBuilder);

            LobbyRoomChatInputField.text = string.Empty;
        }
    }
    private void ResetGameRoomUI()
    {
        LobbyRoomChatInputField.text = string.Empty;
        ChatMessagePrefabList.ForEach(prefab => { if (prefab != null) Destroy(prefab.gameObject); });
        Array.ForEach(LobbyRoomPlayerPrefabArray, prefab => { if (prefab != null) Destroy(prefab.gameObject); });
    }
    private void ShowLobbyRoomUI(LobbyRoomInfo lobbyRoomInfo)
    {
        LobbyRoomID.text = lobbyRoomInfo.RoomId.ToString();
        LobbyRoomType.text = Enum.ToObject(typeof(GameType), lobbyRoomInfo.RoomType).ToString();
        LobbyRoomName.text = lobbyRoomInfo.RoomName;
        LobbyRoomPlayerPrefabArray = new LobbyRoomPlayerPrefab[lobbyRoomInfo.MaxPlayer];

        if (lobbyRoomInfo.RoomType >= GameType.KayKitTeamBrawl.GetHashCode())
        {
            ChangeTeamButton.gameObject.SetActive(true);
            ChangeTeamButton.onClick.AddListener(OnClickChangeTeamButton);
        }
        else
        {
            ChangeTeamButton.gameObject.SetActive(false);
        }

        UpdateGameRoomPlayerUI(lobbyRoomInfo);
    }
    private void UpdateGameRoomPlayerUI(LobbyRoomInfo lobbyRoomInfo)
    {
        List<LobbyPlayerRoomInfo> lobbyPlayerRoomData = lobbyRoomInfo.LobbyPlayerRoomInfos;

        if (LobbyRoomPlayerListPanelTransform == null)
        {
            var gamePlayerListPanel = transform.Find("GameRoomPanel").Find("GameRoomPlayerListPanel");
            if (gamePlayerListPanel != null)
                LobbyRoomPlayerListPanelTransform = gamePlayerListPanel;
            else
            {
                Debug.Log("GameRoomPlayerListPanelTransform is Null");
                return;
            }
        }

        for (int i = 0; i < LobbyRoomPlayerPrefabArray.Length; i++)
        {
            if (lobbyPlayerRoomData.Count <= i)
            {
                if (LobbyRoomPlayerPrefabArray[i] != null)
                    Destroy(LobbyRoomPlayerPrefabArray[i].gameObject);

                continue;
            }

            if (LobbyRoomPlayerPrefabArray[i] != null && LobbyRoomPlayerPrefabArray[i].LobbyPlayerRoomInfo.NickName == lobbyPlayerRoomData[i].NickName)
            {
                LobbyRoomPlayerPrefabArray[i].UpdateNetworkSetting(lobbyRoomInfo, lobbyPlayerRoomData[i]);
                SetLobbyRoomPlayerTeam(LobbyRoomPlayerPrefabArray[i]);
                continue;
            }

            if (LobbyRoomPlayerPrefabArray[i] != null)
            {
                Destroy(LobbyRoomPlayerPrefabArray[i].gameObject);
            }

            LobbyRoomPlayerPrefab lobbyRoomPlayer = Instantiate(lobbyRoomPlayerPrefab, LobbyRoomPlayerListPanelTransform);
            lobbyRoomPlayer.SettingGameRoomPlayerPrefab(lobbyPlayerRoomData[i]);
            lobbyRoomPlayer.UpdateNetworkSetting(lobbyRoomInfo, lobbyPlayerRoomData[i]);
            SetLobbyRoomPlayerTeam(lobbyRoomPlayer);
            LobbyRoomPlayerPrefabArray[i] = lobbyRoomPlayer;
        }
    }
    private void SetLobbyRoomPlayerTeam(LobbyRoomPlayerPrefab lobbyRoomPlayerPrefab)
    {
        if (lobbyRoomPlayerPrefab.LobbyPlayerRoomInfo.Team == Team.RedTeam
            && lobbyRoomPlayerPrefab.transform.parent != LobbyRoomPlayerRedTeamListPanelTransform)
        {
            lobbyRoomPlayerPrefab.transform.SetParent(LobbyRoomPlayerRedTeamListPanelTransform);
        }
        else if (lobbyRoomPlayerPrefab.LobbyPlayerRoomInfo.Team == Team.BlueTeam
           && lobbyRoomPlayerPrefab.transform.parent != LobbyRoomPlayerBlueTeamListPanelTransform)
        {
            lobbyRoomPlayerPrefab.transform.SetParent(LobbyRoomPlayerBlueTeamListPanelTransform);
        }
    }
    private void UpdateGameRoomMessageUI(List<object> chatData)
    {
        if (ChatMessagePrefabList.Count >= maxMessage)
        {
            Destroy(ChatMessagePrefabList[0].gameObject);
            ChatMessagePrefabList.Remove(ChatMessagePrefabList[0]);
        }

        var messagePrefab = Instantiate(chatMessagePrefab, GameRoomMessageContentTransform);
        messagePrefab.BuildChatMessage(chatData);
        ChatMessagePrefabList.Add(messagePrefab);
    }

    // Teleport to Game Scene
    private void TeleportToGameScene(GameScene teleportToGameScene, GameType game)
    {
        GameManager.Instance.TeleportToScene(GameScene.LobbyScene, teleportToGameScene, OnBeforTeleportEvent, () => OnAfterLobbyTeleportToGame(game));
    }
    private void OnBeforTeleportEvent()
    {

    }
    private void OnAfterLobbyTeleportToGame(GameType game)
    {
        GameControlEvent e = new(GameControlEventMessage.PlayerEnterGame, game);
        EventManager.Instance.SendEvent(e);

        MessageBuilder msgBuilder = new MessageBuilder();
        msgBuilder.AddMsg((int)LobbyPlayerPrepareEnterRequest.AccountId, ClientData.Instance.LobbyPlayerInfo.AccountId, NetMsgFieldType.Long);
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerPrepareEnterRequest, msgBuilder);
    }
    #region Click Button Event
    private void OnClickStartGameButton()
    {
        MessageBuilder msgBuilder = new MessageBuilder();
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerStartLobbyRoomRequest, msgBuilder);
    }
    private void OnClickLeaveGameButton()
    {
        MessageBuilder msgBuilder = new MessageBuilder();
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerLeaveLobbyRoomRequest, msgBuilder);
    }
    private void OnClickReadyGameButton()
    {
        MessageBuilder msgBuilder = new MessageBuilder();
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerReadyLobbyRoomRequest, msgBuilder);
    }
    private void OnClickChangeTeamButton()
    {
        MessageBuilder msgBuilder = new MessageBuilder();
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerChangeTeamLobbyRoomRequest, msgBuilder);
    }
    #endregion
    // Event Function
    private void OnPlayerEnterLobbyRoomEvent(IEvent @event)
    {
        var lobbyRoomInfo = (LobbyRoomInfo)@event.GetEventObject<GameControlEvent>().Param;

        ShowLobbyRoomUI(lobbyRoomInfo);
    }
    private void OnGameRoomEnterToLobbyRoom(IEvent @event)
    {
        var lobbyRoomData = (Dictionary<int, object>)@event.GetEventObject<GameControlEvent>().Param;
        LobbyRoomInfo lobbyRoomInfo = new LobbyRoomInfo();
        lobbyRoomInfo.DeserializeObject(lobbyRoomData);
        ShowLobbyRoomUI(lobbyRoomInfo);
    }
    // Network Function
    private void OnLobbyRoomBackgroundThread(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log(JsonConvert.SerializeObject(message));
        Dictionary<int, object> lobbyRoomData;
        if (message.RetrieveMessageItem(LobbyRoomBackgroundThread.LobbyRoomData, out lobbyRoomData))
        {
            LobbyRoomInfo lobbyRoomInfo = new LobbyRoomInfo();
            lobbyRoomInfo.DeserializeObject(lobbyRoomData);
            UpdateGameRoomPlayerUI(lobbyRoomInfo);
        }

        if (message.RetrieveMessageItem(LobbyRoomBackgroundThread.LobbyRoomMessage, out string chatMessage, false))
        {
            List<object> chatData = JsonConvert.DeserializeObject<List<object>>(chatMessage);
            UpdateGameRoomMessageUI(chatData);
        }
    }
    private void OnLobbyPlayerLeaveLobbyRoomRespond(int connectionId, Dictionary<int, object> message)
    {
        if (message.RetrieveMessageItem(LobbyPlayerLeaveLobbyRoomRespond.Succes, out bool succes))
        {
            var e = new GameControlEvent(GameControlEventMessage.PlayerLeaveLobbyRoom);
            EventManager.Instance.SendEvent(e);

            ResetGameRoomUI();
        }
    }
    private void OnLobbyPlayerReadyLobbyRoomRespond(int connectionId, Dictionary<int, object> message)
    {

    }
    private void OnLobbyPlayerStartLobbyRoomRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnLobbyPlayerStartLobbyRoomRespond" + JsonConvert.SerializeObject(message));
    }
    private void OnLobbyRoomEnterGameRoom(int connectionId, Dictionary<int, object> message)
    {
        if (!message.RetrieveMessageItem(LobbyPlayerRoomEntered.ErrorCode, out ErrorCode errorCode)) return;

        if (errorCode == ErrorCode.Success)
        {
            if (!message.RetrieveMessageItem(LobbyPlayerRoomEntered.GameScene, out int gameId)) return;

            GameType game = (GameType)gameId;

            // GoTo Game Scene
            TeleportToGameScene(GameScene.GameScene, game);
        }
        else
        {
            UIManager.Instance.CreateMessageTipsUI("ErrorMessage", errorCode.ToString(), "Confirm", null, null);
        }
    }
}
