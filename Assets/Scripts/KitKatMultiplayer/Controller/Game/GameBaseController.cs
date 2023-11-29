using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public abstract class GameBaseController : MonoBehaviour
{
    protected enum KayKitBaseDynamicInfo
    {
        GamePlayerRoomInfo,
    }

    [Header("PlayerPrefab")]
    public GamePlayerPrefab gamePlayerPrefab;

    [Header("Game Panel")]
    [SerializeField] private Text gameServerMsText;


    [Header("Game Result Panel")]
    [SerializeField] private GameObject gameResultPanel;
    [SerializeField] private Vector3 winnerlocalEulerAngles;
    [SerializeField] private Transform rankOneTransform;
    [SerializeField] private Transform rankTwoTransform;
    [SerializeField] private Transform rankThreeTransform;

    private const float UPDATE_MS_TEXT_TIMER = 1f;
    private float updateMsTextTimer = 0f;
    private int cameraIndex = 0;

    // game player table
    private Dictionary<long, GamePlayerPrefab> accountIdGamePlayerTable = new Dictionary<long, GamePlayerPrefab>();

    private Dictionary<int, Action<GameDynamicInfo>> gameDynamicInfoReceiverAction = new Dictionary<int, Action<GameDynamicInfo>>();
    protected List<GamePlayerPrefab> GetAllGamePlayer => accountIdGamePlayerTable.Values.ToList();

    private void OnEnable()
    {
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GameConnectedRespond, OnGameConnectedRespond);

        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GamePlayerSyncRespond, OnGamePlayerSyncRespond);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GamePlayerNetworkInputRespond, OnGamePlayerNetworkInputRespond);

        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GameRoomStart, OnGameRoomStart);
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GameRoomOver, OnGameRoomOver);
    }
    private void OnDisable()
    {
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GameConnectedRespond, OnGameConnectedRespond);

        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GamePlayerSyncRespond, OnGamePlayerSyncRespond);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GamePlayerNetworkInputRespond, OnGamePlayerNetworkInputRespond);

        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GameRoomStart, OnGameRoomStart);
        NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GameRoomOver, OnGameRoomOver);
    }

    private void Awake()
    {
        updateMsTextTimer = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected virtual void Update()
    {
        updateMsTextTimer -= Time.deltaTime;
    }

    private void GameServerMS(long serverTime)
    {
        if (updateMsTextTimer <= 0)
        {
            var timeNow = DateTime.Now.Ticks;
            var ms = timeNow - serverTime;
            var time = TimeSpan.FromTicks(ms);
            gameServerMsText.text = string.Format("game ms : {0:0.00}", time.TotalMilliseconds);
            updateMsTextTimer = UPDATE_MS_TEXT_TIMER;
        }
    }
    private void RetrieveGamePlayerData(GameDynamicInfo gameDynamicInfo)
    {
        if (gameDynamicInfo.RetrieveDynamicData(KayKitBaseDynamicInfo.GamePlayerRoomInfo, out object[] gamePlayerRoomInfos, true))
        {
            Array.ForEach(gamePlayerRoomInfos, gamePlayerInfo =>
            {
                GamePlayerRoomInfo gamePlayerRoomInfo = new GamePlayerRoomInfo();
                gamePlayerRoomInfo.DeserializeObject((object[])gamePlayerInfo);

                GamePlayerPrefab gamePlayer;
                if (!accountIdGamePlayerTable.TryGetValue(gamePlayerRoomInfo.AccountId, out gamePlayer))
                {
                    gamePlayer = Instantiate(gamePlayerPrefab, gamePlayerRoomInfo.PlayerPosition, Quaternion.identity);
                    gamePlayer.NetworkSyncInit(gamePlayerRoomInfo);
                    accountIdGamePlayerTable.Add(gamePlayerRoomInfo.AccountId, gamePlayer);
                }
                else
                {
                    gamePlayer.NetworkSyncUpdate(gamePlayerRoomInfo);
                }
            });
        }
    }

    #region Network Message Callback
    private void OnGameConnectedRespond(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnGameEnteredRespond" + JsonConvert.SerializeObject(message));
        if (!message.RetrieveMessageItem(GameConnectedRespond.ErrorCode, out ErrorCode errorCode))
            return;

        if (errorCode == ErrorCode.Success)
        {
            // Reconnect Game
            // if (message.RetrieveMessageItem(GamePlayerSyncRespond.GameStaticInfo, out Dictionary<int, object> gameStaticData, false))
            // {
            //     GameStaticInfo gameStaticInfo = new GameStaticInfo();
            //     gameStaticInfo.DeserializeObject(gameStaticData);
            //     RetrieveGameStaticInfo(gameStaticInfo);
            // }
        }
        else
        {
            UIManager.Instance.CreateMessageTipsUI("Error Message", errorCode.ToString(), "Confirm", null, null);
        }
    }
    private void OnGameRoomStart(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnGameRoomStart" + JsonConvert.SerializeObject(message));
        if (message.RetrieveMessageItem(GameRoomStart.GameDynamicInfo, out Dictionary<int, object> gameDynamicData, false))
        {
            GameDynamicInfo gameDynamicIndo = new GameDynamicInfo();
            gameDynamicIndo.DeserializeObject(gameDynamicData);
            RetrivieGameDynamicInfo(gameDynamicIndo);
        }
    }
    private void OnGameRoomOver(int connectionId, Dictionary<int, object> msg)
    {
        Debug.Log("OnOnGameOver" + JsonConvert.SerializeObject(msg));
        if (msg.RetrieveMessageItem(GameRoomOver.LobbyRoomInfo, out Dictionary<int, object> lobbyRoomData))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UIManager.Instance.CreateMessageTipsUI("Game Over", "Return To Lobby Room", "Confirm", null, () => TeleportToMainLobby(lobbyRoomData));
        }
    }
    private void OnGamePlayerSyncRespond(int connectionId, Dictionary<int, object> msg)
    {
        // Debug.Log("OnGamePlayerSyncRespond" + JsonConvert.SerializeObject(msg));

        if (msg.RetrieveMessageItem(GamePlayerSyncRespond.GameStaticInfo, out Dictionary<int, object> gameStaticData, false))
        {
            GameStaticInfo gameStaticInfo = new GameStaticInfo();
            gameStaticInfo.DeserializeObject(gameStaticData);
            RetrieveGameStaticInfo(gameStaticInfo);
        }

        if (msg.RetrieveMessageItem(GamePlayerSyncRespond.GameDynamicInfo, out Dictionary<int, object> gameDynamicData, false))
        {
            GameDynamicInfo gameDynamicInfo = new GameDynamicInfo();
            gameDynamicInfo.DeserializeObject(gameDynamicData);
            RetrivieGameDynamicInfo(gameDynamicInfo);
        }

        if (msg.RetrieveMessageItem(GamePlayerSyncRespond.GameResultInfo, out object[] gameResultData, false))
        {
            GameResultInfo gameResultInfo = new GameResultInfo();
            gameResultInfo.DeserializeObject(gameResultData);
            RetrivieGameResultInfo(gameResultInfo);
        }
    }
    private void OnGamePlayerNetworkInputRespond(int connectionId, Dictionary<int, object> message)
    {
        if (message.RetrieveMessageItem(GamePlayerNetworkInputRespond.PlayerNetworkInput, out object[] gamePlayerNetworkData))
        {
            if (message.RetrieveMessageItem(GamePlayerNetworkInputRespond.AccountId, out long accountId))
            {
                if (accountIdGamePlayerTable.TryGetValue(accountId, out GamePlayerPrefab gamePlayer))
                {
                    PlayerNetworkInput playerNetworkInput = new PlayerNetworkInput();
                    playerNetworkInput.DeserializeObject(gamePlayerNetworkData);
                    gamePlayer.NetworkInputUpdate(playerNetworkInput);
                }
                else
                {
                    Debug.LogError("cant find accountId " + accountId);
                }
            }
        }
    }
    #endregion

    public virtual GamePlayerPrefab GetAnotherAlivePlayerPrefab()
    {
        int index = cameraIndex;
        GamePlayerPrefab gamePlayer = null;
        while (cameraIndex < GetAllGamePlayer.Count)
        {
            gamePlayer = GetAllGamePlayer[cameraIndex];
            if (!gamePlayer.isPlayerDie)
                return gamePlayer;
            else
                cameraIndex++;
        }

        if (cameraIndex >= GetAllGamePlayer.Count)
        {
            cameraIndex = 0;
            while (cameraIndex < index)
            {
                gamePlayer = GetAllGamePlayer[cameraIndex];
                if (!gamePlayer.isPlayerDie)
                    return gamePlayer;
                else
                    cameraIndex++;
            }
        }

        return gamePlayer;
    }
    // Game Static Info
    protected virtual void RetrieveGameStaticInfo(GameStaticInfo gameStaticInfo)
    {
        // GameServerMS(gameStaticInfo.ServerNowTime);
    }
    // Game Dynamic Info
    protected virtual void RetrivieGameDynamicInfo(GameDynamicInfo gameDynamicInfo)
    {
        RetrieveGamePlayerData(gameDynamicInfo);
    }
    // Game Result Info
    protected abstract void RetrivieGameResultInfo(GameResultInfo gameResultInfo);
    protected void GamePlayerAction(Action<GamePlayerPrefab> gamePlayerAction)
    {
        foreach (GamePlayerPrefab gamePlayer in accountIdGamePlayerTable.Values)
        {
            gamePlayerAction.Invoke(gamePlayer);
        }
    }
    protected void GameFinish()
    {
        gameResultPanel.SetActive(true);

        foreach (var gamePlayerRoomInfo in accountIdGamePlayerTable.Values)
        {
            gamePlayerRoomInfo.GameFinish();
        }
    }
    protected void ShowWinnerPlayer(GameResultInfo gameResultInfo)
    {
        Team winnerTeam = gameResultInfo.WinnerTeam;
        Debug.Log(winnerTeam);

        foreach (var gamePlayerRoomInfo in gameResultInfo.winnerInfo)
        {
            if (accountIdGamePlayerTable.TryGetValue(gamePlayerRoomInfo.AccountId, out var gamePlayer))
            {
                gamePlayer.PlayerWinnerGame(rankOneTransform.position, winnerlocalEulerAngles);
            }
        }
    }
    protected void TeleportToMainLobby(Dictionary<int, object> lobbyRoomData)
    {
        void afterTeleportAction()
        {
            GameControlEvent e = new GameControlEvent(GameControlEventMessage.GameRoomEnterToLobbyRoom, lobbyRoomData);
            EventManager.Instance.SendEvent(e);
        }

        void beforeTeleportAction()
        {
            NetworkHandler.Instance.Disconnect(RemoteConnetionType.Game);
        }

        // GoTo Lobby
        GameManager.Instance.TeleportToScene(GameScene.GameScene, GameScene.LobbyScene, beforeTeleportAction, afterTeleportAction);
    }
    protected void RegisterGameStateDynmaicInfoFunction<E>(E gameState, Action<GameDynamicInfo> function) where E : Enum
    {
        Action<GameDynamicInfo> listener;
        if (!gameDynamicInfoReceiverAction.TryGetValue(gameState.GetHashCode(), out listener))
            gameDynamicInfoReceiverAction[gameState.GetHashCode()] = function;

        else
        {
            if (gameDynamicInfoReceiverAction[gameState.GetHashCode()] != function)
                gameDynamicInfoReceiverAction[gameState.GetHashCode()] += function;
        }
    }
    protected void UnRegisterGameStateDynmaicInfoFunction<E>(E gameState, Action<GameDynamicInfo> function) where E : Enum
    {
        Action<GameDynamicInfo> listener;
        if (gameDynamicInfoReceiverAction.TryGetValue(gameState.GetHashCode(), out listener))
        {
            gameDynamicInfoReceiverAction[gameState.GetHashCode()] -= function;

            if (gameDynamicInfoReceiverAction[gameState.GetHashCode()] == null)
                gameDynamicInfoReceiverAction.Remove(gameState.GetHashCode());
        }
    }
}
