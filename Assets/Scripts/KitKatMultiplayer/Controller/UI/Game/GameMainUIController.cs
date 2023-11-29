// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Newtonsoft.Json;
// using UnityEngine;
// using UnityEngine.UI;

// public enum KayKitBrawlDynamicInfo
// {
//     GameTimer,
//     GameCoinInfo,
// }

// public class GameMainUIController : MonoBehaviour
// {
//     [Header("PlayerPrefab")]
//     public GamePlayerPrefab gamePlayerPrefab;

//     [Header("CoinPrefab")]
//     public GameCoinPrefab gameCoinPrefab;


//     [Header("Game Main Panel")]
//     [SerializeField] private Text gameTimerText;
//     [SerializeField] private Text gameServerMsText;
//     private const float UPDATE_MS_TEXT_TIMER = 1f;

//     private float updateMsTextTimer = 0f;

//     [Header("Game Result Panel")]
//     [SerializeField] private GameObject gameResultPanel;
//     [SerializeField] private Vector3 winnerlocalEulerAngles;
//     [SerializeField] private Transform rankOneTransform;
//     [SerializeField] private Transform rankTwoTransform;
//     [SerializeField] private Transform rankThreeTransform;

//     private Dictionary<long, GamePlayerPrefab> accountIdGamePlayerTable = new Dictionary<long, GamePlayerPrefab>();
//     private Dictionary<int, GameCoinPrefab> coinIdGameCoinTable = new Dictionary<int, GameCoinPrefab>();

//     private void OnEnable()
//     {
//         NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.LobbyPlayerPrepareEnterRespond, OnLobbyPlayerPrepareEnterRespond);
//         NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GameConnectedRespond, OnGameConnectedRespond);

//         NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GamePlayerSyncRespond, OnGamePlayerSyncRespond);
//         NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GamePlayerNetworkInputRespond, OnGamePlayerNetworkInputRespond);

//         NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GameRoomStart, OnGameRoomStart);
//         NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.GameRoomOver, OnGameRoomOver);

//         EventManager.Instance.RegisterEventListener<ServerConnectedEvent>((int)RemoteConnetionType.Game, OnGameServerConnected);
//     }

//     private void OnDisable()
//     {
//         NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.LobbyPlayerPrepareEnterRespond, OnLobbyPlayerPrepareEnterRespond);
//         NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GameConnectedRespond, OnGameConnectedRespond);

//         NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GamePlayerSyncRespond, OnGamePlayerSyncRespond);
//         NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GamePlayerNetworkInputRespond, OnGamePlayerNetworkInputRespond);

//         NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GameRoomStart, OnGameRoomStart);
//         NetworkHandler.Instance.UnRegisterMessageListener(ClientHandlerMessage.GameRoomOver, OnGameRoomOver);

//         EventManager.Instance.UnRegisterEventListener<ServerConnectedEvent>((int)RemoteConnetionType.Game, OnGameServerConnected);
//     }
//     private void Awake()
//     {
//         updateMsTextTimer = 0;

//         Cursor.lockState = CursorLockMode.Locked;
//         Cursor.visible = false;
//     }
//     private void Update()
//     {
//         updateMsTextTimer -= Time.deltaTime;
//     }
//     #region Network Message Callback
//     private void OnGameServerConnected(IEvent @event)
//     {
//         MessageBuilder msgBuilder;
//         msgBuilder = new MessageBuilder();
//         msgBuilder.AddMsg((int)GameConnectedRequest.SessionId, ClientData.Instance.SessionId, NetMsgFieldType.Int);
//         NetworkHandler.Instance.Send(RemoteConnetionType.Game, ClientHandlerMessage.GameConnectedRequest, msgBuilder);
//     }
//     private void OnLobbyPlayerPrepareEnterRespond(int connectionId, Dictionary<int, object> message)
//     {
//         Debug.Log("OnLobbyPlayerPrepareEnterRespond" + JsonConvert.SerializeObject(message));

//         if (!message.RetrieveMessageItem(LobbyPlayerPrepareEnterRespond.ErrorCode, out ErrorCode errorCode))
//             return;

//         if (errorCode == ErrorCode.Success)
//         {
//             if (!message.RetrieveMessageItem(LobbyPlayerPrepareEnterRespond.GameServerIP, out string gameServerIP)) return;
//             if (!message.RetrieveMessageItem(LobbyPlayerPrepareEnterRespond.GameServerPort, out int gameServerPort)) return;

//             ClientData.Instance.GameServerAddreas = string.Format("{0}:{1}", gameServerIP, gameServerPort);
//             NetworkHandler.Instance.Connect(RemoteConnetionType.Game, ClientData.Instance.GameServerAddreas, GameManager.Instance.ServerName);
//         }
//         else
//         {
//             UIManager.Instance.CreateMessageTipsUI("Error Message", errorCode.ToString(), "Confirm", null, null);
//         }
//     }
//     private void OnGameConnectedRespond(int connectionId, Dictionary<int, object> message)
//     {
//         Debug.Log("OnGameEnteredRespond" + JsonConvert.SerializeObject(message));
//         if (!message.RetrieveMessageItem(GameConnectedRespond.ErrorCode, out ErrorCode errorCode))
//             return;

//         if (errorCode == ErrorCode.Success)
//         {
//             // Reconnect Game
//             if (message.RetrieveMessageItem(GameConnectedRespond.GameStaticInfo, out string gameStaticData, false))
//             {
//                 GameStaticInfo gameStaticInfo = JsonConvert.DeserializeObject<GameStaticInfo>(gameStaticData);
//                 gameStaticInfo.GamePlayerRoomInfos.ForEach(GamePlayerRoomInfo =>
//                 {
//                     GamePlayerPrefab gamePlayer;
//                     if (!accountIdGamePlayerTable.TryGetValue(GamePlayerRoomInfo.AccountId, out gamePlayer))
//                     {
//                         gamePlayer = Instantiate(gamePlayerPrefab, GamePlayerRoomInfo.PlayerPosition, Quaternion.identity);
//                         gamePlayer.NetworkSyncInit(GamePlayerRoomInfo);

//                         accountIdGamePlayerTable.Add(GamePlayerRoomInfo.AccountId, gamePlayer);
//                     }
//                     else
//                     {
//                         Debug.LogErrorFormat("accountIdGamePlayerTable is have gameplayer for {0} accountid", GamePlayerRoomInfo.AccountId);
//                     }
//                 });
//             }
//         }
//         else
//         {
//             UIManager.Instance.CreateMessageTipsUI("Error Message", errorCode.ToString(), "Confirm", null, null);
//         }
//     }
//     private void OnGameRoomStart(int connectionId, Dictionary<int, object> message)
//     {
//         Debug.Log("OnGameRoomStart" + JsonConvert.SerializeObject(message));
//         if (message.RetrieveMessageItem(GameRoomStart.GameStaticInfo, out object[] gameStaticData, false))
//         {
//             GameStaticInfo gameStaticInfo = new GameStaticInfo();
//             gameStaticInfo.DeserializeObject(gameStaticData);
//             GameServerMS(gameStaticInfo.ServerNowTime);
//             gameStaticInfo.GamePlayerRoomInfos.ForEach(gamePlayerInfo =>
//             {
//                 GamePlayerPrefab gamePlayer;
//                 if (!accountIdGamePlayerTable.TryGetValue(gamePlayerInfo.AccountId, out gamePlayer))
//                 {
//                     gamePlayer = Instantiate(gamePlayerPrefab, gamePlayerInfo.PlayerPosition, Quaternion.identity);
//                     gamePlayer.NetworkSyncInit(gamePlayerInfo);

//                     accountIdGamePlayerTable.Add(gamePlayerInfo.AccountId, gamePlayer);
//                 }
//                 else
//                 {
//                     Debug.LogErrorFormat("gameNameGamePlayerTable is has have player by {0}", gamePlayerInfo.NickName);
//                 }
//             });
//         }
//     }
//     private void OnGameRoomOver(int connectionId, Dictionary<int, object> msg)
//     {
//         Debug.Log("OnOnGameOver" + JsonConvert.SerializeObject(msg));
//         if (msg.RetrieveMessageItem(GameRoomOver.LobbyRoomInfo, out Dictionary<int, object> lobbyRoomData))
//         {
//             Cursor.lockState = CursorLockMode.Confined;
//             Cursor.visible = true;
//             UIManager.Instance.CreateMessageTipsUI("Game Over", "Return To Lobby Room", "Confirm", null, () => TeleportToMainLobby(lobbyRoomData));
//         }
//     }
//     private void OnGamePlayerSyncRespond(int connectionId, Dictionary<int, object> msg)
//     {
//         // Debug.Log("OnGamePlayerSyncRespond" + JsonConvert.SerializeObject(msg));
//         if (msg.RetrieveMessageItem(GamePlayerSyncRespond.GameStaticInfo, out object[] gameStaticData, false))
//         {
//             RetrieveGameStaticData(gameStaticData);
//         }

//         if (msg.RetrieveMessageItem(GamePlayerSyncRespond.GameDynamicInfo, out Dictionary<string, object> gameDynamicData, false))
//         {
//             RetrivieGameDynamicData((Dictionary<string, object>)gameDynamicData);
//         }

//         if (msg.RetrieveMessageItem(GamePlayerSyncRespond.GameResultInfo, out object[] gameResult, false))
//         {
//             RetrivieGameResultData(gameResult);
//         }
//     }
//     private void OnGamePlayerNetworkInputRespond(int connectionId, Dictionary<int, object> message)
//     {
//         if (message.RetrieveMessageItem((GamePlayerNetworkInputRespond.PlayerNetworkInput), out object[] gamePlayerNetworkData))
//         {
//             if (message.RetrieveMessageItem((GamePlayerNetworkInputRespond.AccountId), out long accountId))
//             {
//                 if (accountIdGamePlayerTable.TryGetValue(accountId, out GamePlayerPrefab gamePlayer))
//                 {
//                     PlayerNetworkInput playerNetworkInput = new PlayerNetworkInput();
//                     playerNetworkInput.DeserializeObject(gamePlayerNetworkData);
//                     gamePlayer.NetworkInputUpdate(playerNetworkInput);

//                     // object bulletData;
//                     // if (message.TryGetValue(((int)GamePlayerNetworkInputRespond.PlayerBulletData), out bulletData))
//                     // {
//                     //     BulletInfo bulletInfo = new BulletInfo();
//                     //     bulletInfo.DeserializeObject((object[])bulletData);
//                     //     var bullet = gamePlayer.FireBullet(playerNetworkInput, bulletInfo);
//                     //     bulletIdGameBulletTable.Add(bullet.BulletInfo.BulletId, bullet);
//                     // }
//                 }
//                 else
//                 {
//                     Debug.LogError("cant find accountId " + accountId);
//                 }
//             }
//         }
//     }
//     #endregion

//     // Game Static Data
//     private void RetrieveGameStaticData(object[] gameStaticData)
//     {
//         GameStaticInfo gameStaticInfo = new GameStaticInfo();
//         gameStaticInfo.DeserializeObject(gameStaticData);
//         GameServerMS(gameStaticInfo.ServerNowTime);
//         gameStaticInfo.GamePlayerRoomInfos.ForEach(gamePlayerInfo =>
//         {
//             GamePlayerPrefab gamePlayer;
//             if (!accountIdGamePlayerTable.TryGetValue(gamePlayerInfo.AccountId, out gamePlayer))
//             {
//                 gamePlayer = Instantiate(gamePlayerPrefab, gamePlayerInfo.PlayerPosition, Quaternion.identity);
//                 gamePlayer.NetworkSyncInit(gamePlayerInfo);
//                 accountIdGamePlayerTable.Add(gamePlayerInfo.AccountId, gamePlayer);
//             }
//             else
//             {
//                 gamePlayer.NetworkSyncUpdate(gamePlayerInfo);
//             }
//         });
//     }
//     private void GameServerMS(long serverTime)
//     {
//         if (updateMsTextTimer <= 0)
//         {
//             var timeNow = DateTime.Now.Ticks;
//             var ms = timeNow - serverTime;
//             var time = TimeSpan.FromTicks(ms);
//             gameServerMsText.text = string.Format("game ms : {0:0.00}", time.TotalMilliseconds);
//             updateMsTextTimer = UPDATE_MS_TEXT_TIMER;
//         }
//     }

//     // Game Dynamic Data
//     private void RetrivieGameDynamicData(Dictionary<string, object> gameDynamicData)
//     {
//         GameDynamicInfo gameDynamicInfo = new GameDynamicInfo();
//         gameDynamicInfo.DeserializeObject(gameDynamicData);

//         // if (gameDynamicInfo.RetrieveDynamicData(MultiPlayerGameDynamicInfo.GameState, out int state))
//         // {
//         //     GameState = (MultiPlayerGameState)state;
//         // }

//         if (gameDynamicInfo.RetrieveDynamicData(KayKitBrawlDynamicInfo.GameTimer, out Dictionary<string, double> timerData))
//         {
//             if (timerData.TryGetValue("GameTimer", out double gameTimer))
//             {
//                 gameTimerText.text = string.Format("Timer : {0:0.00}", gameTimer);
//             }
//         }

//         if (gameDynamicInfo.RetrieveDynamicData(KayKitBrawlDynamicInfo.GameCoinInfo, out Dictionary<int, object> coinSpawnerData))
//         {
//             foreach (var coinData in coinSpawnerData.Values)
//             {
//                 CoinInfo coinInfo = new CoinInfo();
//                 coinInfo.DeserializeObject((object[])coinData);

//                 if (!coinIdGameCoinTable.TryGetValue(coinInfo.CoinID, out GameCoinPrefab coinPrefab))
//                 {
//                     coinPrefab = Instantiate(gameCoinPrefab, coinInfo.Position, Quaternion.identity);
//                     coinPrefab.Init(coinInfo);
//                     coinIdGameCoinTable.Add(coinInfo.CoinID, coinPrefab);
//                 }
//                 else
//                 {
//                     coinPrefab.NetworkUpdate(coinInfo);
//                 }
//             }
//         }
//     }

//     // Game Result Data
//     private void RetrivieGameResultData(object[] gameResultData)
//     {
//         GameResultInfo gameResultInfo = new GameResultInfo();
//         gameResultInfo.DeserializeObject(gameResultData);

//         GameFinish();
//         ShowWinnerPlayer(gameResultInfo.winnerInfo);
//     }
//     private void GameFinish()
//     {
//         gameResultPanel.SetActive(true);

//         foreach (var gamePlayerRoomInfo in accountIdGamePlayerTable.Values)
//         {
//             gamePlayerRoomInfo.GameFinish();
//         }
//     }
//     private void ShowWinnerPlayer(List<GamePlayerRoomInfo> winnerPlayerInfo)
//     {
//         foreach (var gamePlayerRoomInfo in winnerPlayerInfo)
//         {
//             if (accountIdGamePlayerTable.TryGetValue(gamePlayerRoomInfo.AccountId, out var gamePlayer))
//             {
//                 gamePlayer.PlayerWinnerGame(rankOneTransform.position, winnerlocalEulerAngles);
//             }
//         }
//     }
//     private void TeleportToMainLobby(Dictionary<int, object> lobbyRoomData)
//     {
//         void afterTeleportAction()
//         {
//             GameControlEvent e = new GameControlEvent(GameControlEventMessage.GameRoomEnterToLobbyRoom, lobbyRoomData);
//             EventManager.Instance.SendEvent(e);
//         }

//         void beforeTeleportAction()
//         {
//         }

//         // GoTo Lobby
//         GameManager.Instance.TeleportToScene(GameScene.GameScene, GameScene.LobbyScene, beforeTeleportAction, afterTeleportAction);
//     }
// }
