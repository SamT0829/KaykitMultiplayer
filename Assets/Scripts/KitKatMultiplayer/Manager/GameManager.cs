using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameScene
{
    AccountScene,
    LobbyScene,
    GameScene,
}


public class GameManager : MonoBehaviour
{
    public readonly string ServerAddreas = "192.168.8.111:5055";
    public readonly string ServerName = "KayKitMultiplayerServer";

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            _instance = this;


        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);

        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.KickPlayer, OnKickPlayer);
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.GameDisconnected, OnGameDisconnected);
    }

    private void Start()
    {
        NetworkHandler.Instance.RegisterMessageListener(ClientHandlerMessage.KickPlayer, OnKickPlayer);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        NetworkHandler.Instance.DisconnectAll();
    }

    private void OnKickPlayer(int connectionId, Dictionary<int, object> message)
    {
        Debug.Log("OnKickPlayer");
    }

    public void TeleportToScene(GameScene from, GameScene to, Action beforeTeleportAction = null, Action afterTeleportAction = null)
    {
        StartCoroutine(TransitionToScene(from.ToString(), to.ToString(), beforeTeleportAction, afterTeleportAction));
    }

    private void OnGameDisconnected(IEvent @event)
    {
        var remoteConnector = (RemoteConnetionType)@event.GetEventObject<GameControlEvent>().Param;
        NetworkHandler.Instance.Disconnect(remoteConnector);

        if (remoteConnector == RemoteConnetionType.Lobby || remoteConnector == RemoteConnetionType.Game)
            UIManager.Instance.CreateMessageTipsUI("Error Message", "Game Disconnected", "Quit Game", null, Application.Quit);
    }

    private IEnumerator TransitionToScene(string from, string to, Action beforeTeleportAction, Action afterTeleportAction)
    {
        if (SceneManager.GetActiveScene().name != to)
        {
            if (beforeTeleportAction != null)
                beforeTeleportAction.Invoke();

            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Single);

            if (afterTeleportAction != null)
                afterTeleportAction.Invoke();
        }
    }

}
