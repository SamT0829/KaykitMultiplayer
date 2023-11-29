using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class LobbyGameUIController : MonoBehaviour
{
    [Header("Lobby Button")]
    [SerializeField] Button CreateRoomButton;
    [SerializeField] Button JoinRoomButton;

    [Header("Lobby Panel UI")]
    [SerializeField] GameObject LobbyGamePanel;
    [SerializeField] GameObject LobbyMainPanel;

    [Header("CreateRoomUI")]
    [SerializeField] GameObject CreateRoomPanel;
    [SerializeField] InputField RoomNameInputField;
    [SerializeField] Dropdown MaxPlayerDropDown;
    [SerializeField] Dropdown GameTypeDropDown;
    [SerializeField] Button CreateRoomConfirmButton;

    [Header("JoinRoomUI")]
    [SerializeField] GameObject JoinRoomPanel;
    [SerializeField] InputField RoomNameJoinInputField;
    [SerializeField] InputField MaxPlayerJoinInputField;
    [SerializeField] Button JoinRoomConfirmButton;

    [Header("Lobby Room Prefab")]
    [SerializeField] LobbyRoomPrefab lobbyRoomPrefab;
    [SerializeField] Transform LobbyRoomListPanelTransform;

    [SerializeField] LobbyRoomPrefab[] GameNameRoomPrefabArray = new LobbyRoomPrefab[10];

    int roomId;

    // 不用經過 LoginUI
    // [SerializeField] bool TestMode = false;

    private void OnEnable()
    {
        // BackgroundThread
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerBackgroundThread, OnLobbyPlayerBackgroundThread);

        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerCreateLobbyRoomRespond, OnLobbyPlayerCreateLobbyRoomRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerJoinLobbyRoomRespond, OnLobbyPlayerJoinLobbyRoomRespond);

        // Lobby Button Callback
        CreateRoomButton.onClick.AddListener(OnClickCreateRoomButton);
        JoinRoomButton.onClick.AddListener(OnClickJoinRoomButton);

        CreateRoomConfirmButton.onClick.AddListener(OnClickCreateRoomConfirmButton);
    }
    private void OnDisable()
    {
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerBackgroundThread, OnLobbyPlayerBackgroundThread);

        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerCreateLobbyRoomRespond, OnLobbyPlayerCreateLobbyRoomRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerJoinLobbyRoomRespond, OnLobbyPlayerJoinLobbyRoomRespond);
    }

    #region Button Callback
    private void OnClickCreateRoomButton()
    {
        CreateRoomPanel.SetActive(true);
    }
    private void OnClickJoinRoomButton()
    {
        CreateRoomPanel.SetActive(false);
        // GOTO : Show JoinRoomPanel 
    }
    private void OnClickCreateRoomConfirmButton()
    {
        var roomName = RoomNameInputField.text;
        var maxPlayer = MaxPlayerDropDown.captionText.text;
        var roomType = GameTypeDropDown.captionText.text;

        if (!Enum.TryParse(roomType, out GameType gametype))
        {
            gametype = GameType.None;
        }

        //Send Network Message
        MessageBuilder msgBuilder = new MessageBuilder();
        msgBuilder.AddMsg((int)LobbyPlayerCreateLobbyRoomRequest.RoomType, (int)gametype, NetMsgFieldType.Int);
        msgBuilder.AddMsg((int)LobbyPlayerCreateLobbyRoomRequest.RoomName, roomName, NetMsgFieldType.String);
        msgBuilder.AddMsg((int)LobbyPlayerCreateLobbyRoomRequest.RoomPassword, "", NetMsgFieldType.String);
        msgBuilder.AddMsg((int)LobbyPlayerCreateLobbyRoomRequest.MaxPlayer, Convert.ToInt32(maxPlayer), NetMsgFieldType.Int);
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerCreateLobbyRoomRequest, msgBuilder);
    }
    #endregion

    #region Net Message Callback
    private void OnLobbyPlayerBackgroundThread(int connectionId, Dictionary<int, object> message)
    {
        object[] roomListData;
        if (!message.RetrieveMessageItem(LobbyPlayerBackgroundThread.LobbyRoomListData, out roomListData))
            return;

        List<LobbyRoomInfo> gameRoomDataList = new List<LobbyRoomInfo>();
        foreach (Dictionary<int, object> roomData in roomListData)
        {
            if (roomData == null)
                continue;

            LobbyRoomInfo gameRoomData = new LobbyRoomInfo();
            gameRoomData.DeserializeObject(roomData);
            gameRoomDataList.Add(gameRoomData);
        }

        UpdateLobbyRoomUI(gameRoomDataList);
    }
    private void OnLobbyPlayerCreateLobbyRoomRespond(int connectionId, Dictionary<int, object> message)
    {
        if (!message.RetrieveMessageItem(LobbyPlayerCreateLobbyRoomRespond.ErrorCode, out ErrorCode err))
            return;

        if (err == ErrorCode.Success)
        {
            if (message.RetrieveMessageItem(LobbyPlayerCreateLobbyRoomRespond.RoomData, out Dictionary<int, object> roomData))
            {
                LobbyRoomInfo lobbyRoomInfo = new LobbyRoomInfo();
                lobbyRoomInfo.DeserializeObject(roomData);

                GameControlEvent e = new GameControlEvent(GameControlEventMessage.PlayerEnterLobbyRoom, lobbyRoomInfo);
                EventManager.Instance.SendEvent(e);

                CreateRoomPanel.SetActive(false);
            }
        }
        else
        {
            UIManager.Instance.CreateMessageTipsUI("ErrorMessage", err.ToString(), "Confirm", null, null);
        }
    }
    private void OnLobbyPlayerJoinLobbyRoomRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnLobbyPlayerJoinLobbyRoomRespond" + JsonConvert.SerializeObject(message));

        ErrorCode err;
        if (!message.RetrieveMessageItem(LobbyPlayerJoinLobbyRoomRespond.ErrorCode, out err))
            return;

        if (err == ErrorCode.Success)
        {
            if (message.RetrieveMessageItem(LobbyPlayerCreateLobbyRoomRespond.RoomData, out Dictionary<int, object> roomData))
            {
                LobbyRoomInfo lobbyRoomInfo = new LobbyRoomInfo();
                lobbyRoomInfo.DeserializeObject(roomData);

                GameControlEvent e = new GameControlEvent(GameControlEventMessage.PlayerEnterLobbyRoom, lobbyRoomInfo);
                EventManager.Instance.SendEvent(e);
            }
        }
        else
        {
            UIManager.Instance.CreateMessageTipsUI("ErrorMessage", err.ToString(), "Confirm", null, null);
        }
    }
    #endregion
    // Lobby Create Game Room UI
    private void UpdateLobbyRoomUI(List<LobbyRoomInfo> lobbyRoomInfoList)
    {
        if (LobbyRoomListPanelTransform == null)
        {
            var gameListPanel = transform.Find("LobbyPanel").Find("GameRoomListPanel");
            if (gameListPanel != null)
                LobbyRoomListPanelTransform = gameListPanel;
            else
                return;
        }

        for (int i = 0; i < GameNameRoomPrefabArray.Length; i++)
        {
            if (lobbyRoomInfoList.Count <= i)
            {
                if (GameNameRoomPrefabArray[i] != null)
                    Destroy(GameNameRoomPrefabArray[i].gameObject);

                continue;
            }

            if (GameNameRoomPrefabArray[i] != null && GameNameRoomPrefabArray[i].Info.RoomId == lobbyRoomInfoList[i].RoomId)
                continue;

            if (GameNameRoomPrefabArray[i] != null)
                Destroy(GameNameRoomPrefabArray[i].gameObject);

            LobbyRoomPrefab gameRoom = Instantiate(lobbyRoomPrefab, LobbyRoomListPanelTransform);
            gameRoom.SettingRoomPrefab(lobbyRoomInfoList[i]);
            GameNameRoomPrefabArray[i] = gameRoom;
        }
    }

    #region Event Message Callback

    // private void OnLeaveGameRoomEvent(IEvent obj)
    // {
    //     GetComponentInParent<LobbyMainUIController>().ActivePanel(GetComponent<RectTransform>());
    // }
    // private void OnReconnectGame(IEvent obj)
    // {
    //     Action OnBeforTeleportEvent = () =>
    //     {

    //     };

    //     Action OnAfterTeleportEvent = () =>
    //     {
    //         UIManager.Instance.CreateLoadingMission(new ServerLobbyReconnectGameMission());
    //         UIManager.Instance.CreateLoadingMission(new ServerGameEnteredMission() { OnFinish = LoadingFinish });
    //         UIManager.Instance.StartLoadingMission();
    //     };

    //     GameManager.Instance.TeleportToScene(GameScene.Lobby.ToString(), GameScene.Game.ToString(), OnBeforTeleportEvent, OnAfterTeleportEvent);
    // }
    // private Task LoadingFinish()
    // {
    //     Debug.Log("finish");
    //     UIManager.Instance.FinishLoadingMission();
    //     return Task.CompletedTask;
    // }
    #endregion

    private void ResetLobbyUI()
    {
        // LobbyGamePanel.SetActive(false);
        CreateRoomPanel.SetActive(false);
        // JoinRoomPanel.SetActive(false);
        RoomNameInputField.text = string.Empty;
        MaxPlayerDropDown.value = 0;

        Array.ForEach(GameNameRoomPrefabArray, prefab => { if (prefab != null) Destroy(prefab.gameObject); });
    }
}
