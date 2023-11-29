using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LobbyMainUIController : MonoBehaviour
{
    [Header("Lobby Main Button")]
    [SerializeField] Button HomeButton;
    [SerializeField] Button GameButton;
    [SerializeField] Button NotificationButton;
    [SerializeField] Button ShopButton;
    [SerializeField] Button MailButton;

    [Header("Lobby Main Panel")]
    [SerializeField] RectTransform HomePanel;
    [SerializeField] RectTransform GamePanel;
    [SerializeField] RectTransform NotificationPanel;
    [SerializeField] RectTransform ShopPanel;
    [SerializeField] RectTransform MailPanel;
    [SerializeField] RectTransform LobbyRoomPanel;

    RectTransform nowPanel;

    [SerializeField] RectTransform ClickMarkerRectTransform;

    [Header("Lobby Panel UI")]
    [SerializeField] RectTransform LobbyGamePanel;
    [SerializeField] GameObject LobbyMainPanel;

    [Header("Lobby Main Child Component")]
    [SerializeField] private Text GoldText;
    [SerializeField] private Text DiamondText;

    [Header("Test Game")]
    [SerializeField] bool isTestGame;

    private void OnEnable()
    {
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterLobbyScene, OnPlayerEnterLobbyScene);
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterLobbyRoom, OnPlayerEnterLobbyRoomEvent);
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerLeaveLobbyRoom, OnPlayerLeaveLobbyRoomEvent);
        EventManager.Instance.RegisterEventListener<GameControlEvent>((int)GameControlEventMessage.GameRoomEnterToLobbyRoom, OnGameRoomEnterToLobbyRoom);
    }

    private void OnDisable()
    {
        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterLobbyScene, OnPlayerEnterLobbyScene);
        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerEnterLobbyRoom, OnPlayerEnterLobbyRoomEvent);
        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.PlayerLeaveLobbyRoom, OnPlayerLeaveLobbyRoomEvent);
        EventManager.Instance.UnRegisterEventListener<GameControlEvent>((int)GameControlEventMessage.GameRoomEnterToLobbyRoom, OnGameRoomEnterToLobbyRoom);
    }

    private void Awake()
    {
        // Lobby Button Callback
        HomeButton.onClick.AddListener(() => OnClickHomeButton(HomeButton));
        GameButton.onClick.AddListener(() => OnClickGameButton(GameButton));
        NotificationButton.onClick.AddListener(() => OnClickNotificationButton(NotificationButton));
        ShopButton.onClick.AddListener(() => OnClickShopButton(ShopButton));
        MailButton.onClick.AddListener(() => OnClickMailButton(MailButton));

        nowPanel = HomePanel;
    }

    private void Update()
    {
        GoldText.text = ClientData.Instance.LobbyGameInfo.Money.ToString();
        DiamondText.text = ClientData.Instance.LobbyGameInfo.Diamond.ToString();
    }

    // Button Listener
    private void OnClickHomeButton(Button buttonObject)
    {
        ActivateButton(buttonObject);
        ActivePanel(HomePanel);
    }
    private void OnClickGameButton(Button buttonObject)
    {
        ActivateButton(buttonObject);
        ActivePanel(GamePanel);
    }
    private void OnClickNotificationButton(Button buttonObject)
    {
        ActivateButton(buttonObject);
    }
    private void OnClickShopButton(Button buttonObject)
    {
        ActivateButton(buttonObject);
        // MessageBuilder msgBuilder = new MessageBuilder();
        // NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerGoToShopRequest, msgBuilder);
    }
    private void OnClickMailButton(Button buttonObject)
    {
        ActivateButton(buttonObject);
        // MessageBuilder msgBuilder = new MessageBuilder();
        // NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerGoToMailRequest, msgBuilder);
    }

    // Lobby Main Button function
    private void ActivateButton(Button buttonObject)
    {
        ResetAllButton();
        MoveClickMarker(buttonObject);

        // Change button sizeDelta
        var rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0, 0.5f);
        rectTransform.anchoredPosition = new Vector2(-100, rectTransform.anchoredPosition.y);
        rectTransform.DOSizeDelta(new Vector2(250, 150), 0.5f).SetEase(Ease.OutQuint);

        // Change Image localPosition
        var imageRect = buttonObject.transform.Find("Image").GetComponent<RectTransform>();
        imageRect.DOSizeDelta(new Vector2(100, 100), 0.5f).SetEase(Ease.OutQuint);
        imageRect.DOLocalMoveY(10, 0.5f).SetEase(Ease.OutQuint);

        //Set Active Click object text
        var text = buttonObject.transform.Find("Text");
        text.gameObject.SetActive(true);
    }
    private void ResetAllButton()
    {
        Action<Button> resetButton = buttonObject =>
        {
            // Change button RectTransform
            buttonObject.GetComponent<RectTransform>().DOSizeDelta(new Vector2(200, 150), 0.5f).SetEase(Ease.OutQuint);

            // Change Image localPosition
            var imageRect = buttonObject.transform.Find("Image").GetComponent<RectTransform>();
            imageRect.DOSizeDelta(new Vector2(120, 120), 0.5f).SetEase(Ease.OutQuint);
            imageRect.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuint);

            //Set Active Click object text
            var text = buttonObject.transform.Find("Text");
            text.gameObject.SetActive(false);
        };

        // Reset All button property
        resetButton.Invoke(HomeButton);
        resetButton.Invoke(GameButton);
        resetButton.Invoke(NotificationButton);
        resetButton.Invoke(ShopButton);
        resetButton.Invoke(MailButton);
    }
    private void MoveClickMarker(Button buttonObject)
    {
        ClickMarkerRectTransform.SetParent(buttonObject.transform);
        ClickMarkerRectTransform.localPosition = new Vector3(ClickMarkerRectTransform.localPosition.x, 0, 0);
    }
    private void ActivePanel(RectTransform panel)
    {
        if (nowPanel && nowPanel == panel)
        {
            return;
        }

        panel.gameObject.SetActive(true);

        if (nowPanel)
            nowPanel.DOLocalMove(new Vector3(nowPanel.localPosition.x, nowPanel.sizeDelta.y), 0.5f).SetEase(Ease.OutQuint);

        nowPanel = panel;
        panel.DOLocalMove(new Vector3(panel.localPosition.x, 0), 0.5f).SetEase(Ease.OutQuint);
    }

    // Event Function
    private void OnPlayerEnterLobbyScene(IEvent @event)
    {
        if (isTestGame)
        {
            var msgBuilder = new MessageBuilder();
            msgBuilder.AddMsg(((int)LobbyPlayerTestGameRequest.GameType), (int)GameType.KayKitBrawl, NetMsgFieldType.Int);
            NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerTestGameRequest, msgBuilder);
            Debug.Log("LobbyPlayerTestGameRequest");
        }
    }
    private void OnPlayerEnterLobbyRoomEvent(IEvent @event)
    {
        ActivePanel(LobbyRoomPanel);
    }
    private void OnPlayerLeaveLobbyRoomEvent(IEvent @event)
    {
        ActivePanel(GamePanel);
    }
    private void OnGameRoomEnterToLobbyRoom(IEvent @event)
    {
        ActivePanel(LobbyRoomPanel);
    }
}