using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GamePlayerPrefab : MonoBehaviour
{
    // CinemachineVirtualCamera virtualCamera;
    public GamePlayerRoomInfo GamePlayerRoomInfo;

    public Transform cameraFollowPoint;

    // 是否為控制玩家
    public bool IsGamePlayer = false;

    public bool gameFinish = false;
    private Vector2 movementInput;

    [Header("PlayerStatus")]
    public float maxSpeed = 6f;
    public float accelerationSpeed = 2f;
    public float speedNow;
    public float jumpForce = 6f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public bool onGround;

    [Header("Player Weapon")]
    [SerializeField] private GameObject _weapon;
    [SerializeField] private Transform weaponParent;

    //  bool
    public bool isPlayerDie;


    // Game State
    private GameRoomState _gameRoomState = GameRoomState.WaitingEnter;

    // Component
    private Rigidbody rb;
    private Collider coll;
    public Animator Animator;
    public GamePlayerInput GamePlayerInput;
    private GameBaseController GameController;
    public CameraController CameraController;
    private GunPrefab Gun;
    private PlayerUIController playerUIController;
    private GamePlayerAnimEvent playerAnimEvent;

    private void Awake()
    {
        //Input event Callback
        GamePlayerInput = new GamePlayerInput();
        GamePlayerInput.PlayerControls.Enable();

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        Animator = GetComponentInChildren<Animator>();
        CameraController = GetComponentInChildren<CameraController>();
        Gun = GetComponentInChildren<GunPrefab>();
        playerUIController = GetComponent<PlayerUIController>();
        playerAnimEvent = GetComponentInChildren<GamePlayerAnimEvent>();
        GameController = FindObjectOfType<GameBaseController>();

        Application.runInBackground = true;
    }
    private void Update()
    {
        // if (_gameRoomState != GameRoomState.GameStart)
        //     return;

        NetworkButtonPressedSeting();
        NetworkPlayerSyncRequest();

        // 更換顯示相機
        // ChangeMainCamera()
    }
    private void FixedUpdate()
    {
        if (gameFinish)
            return;

        PhysicsCheck();
        Movement();
        PlayerRotate();
        CameraController.SwitchFollowToAimCamera(GamePlayerInput.PlayerControls.Aim.ReadValue<float>() > 0);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Coin")
        {
            var coinPrefab = other.gameObject.GetComponent<GameCoinPrefab>();
            if (coinPrefab != null)
                OnPlayerTakeCoin(coinPrefab);
        }
    }
    public void PlayerWinnerGame(Vector3 position, Vector3 localEulerAngles)
    {
        transform.position = position;
        transform.localEulerAngles = localEulerAngles;
        Animator.Play("Dance");
    }
    public void GameFinish()
    {
        gameFinish = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    #region Network function 
    public void NetworkSyncInit(GamePlayerRoomInfo gamePlayerRoomInfo)
    {
        GamePlayerRoomInfo = gamePlayerRoomInfo;

        IsGamePlayer = ClientData.Instance.LobbyPlayerInfo.AccountId == gamePlayerRoomInfo.AccountId;
        transform.position = gamePlayerRoomInfo.PlayerPosition;
        // transform.localScale = gamePlayerRoomInfo.PlayerLocalScale;

        if (!IsGamePlayer)
        {
            CameraController.gameObject.SetActive(false);
        }
    }
    public void NetworkSyncUpdate(GamePlayerRoomInfo gamePlayerRoomInfo)
    {
        GamePlayerRoomInfo = gamePlayerRoomInfo;

        if (gamePlayerRoomInfo.IsPlayerDie && !isPlayerDie)
        {
            PlayerDie();
        }

        if (!gamePlayerRoomInfo.IsPlayerDie && isPlayerDie)
        {
            PlayerRevive();
        }

        if (!IsGamePlayer)
        {
            transform.position = gamePlayerRoomInfo.PlayerPosition;
            transform.localEulerAngles = gamePlayerRoomInfo.PlayerLocalEulerAngles;

            if (gamePlayerRoomInfo.IsPlayerShoot)
                PlayerShoot(gamePlayerRoomInfo.gunAimDirection);
        }
    }
    public void NetworkInputUpdate(PlayerNetworkInput playerNetworkInput)
    {
        if (!IsGamePlayer || isPlayerDie || gameFinish)
            return;

        movementInput = playerNetworkInput.movementInput;

        if (playerNetworkInput.GetNetworkButtonInputData(NetworkInputButtons.JUMP))
            Jump();

        if (playerNetworkInput.GetNetworkButtonInputData(NetworkInputButtons.FIRE))
            PlayerShoot(playerNetworkInput.gunAimDirection);

        if (playerNetworkInput.GetNetworkButtonInputData(NetworkInputButtons.ChangeCamera))
        {
            var gameplayer = GameController.GetAnotherAlivePlayerPrefab();
            if (gameplayer)
                CameraController.SwitchFollowToAnotherPlayerCamera(gameplayer);
        }
    }
    #endregion

    private void OnPlayerTakeCoin(GameCoinPrefab coinPrefab)
    {
        MessageBuilder msgBuilder = new MessageBuilder();
        Dictionary<PlayerSyncNetworkMessage, object> playerNetworkMessageTable = new Dictionary<PlayerSyncNetworkMessage, object>();
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerAccountId, GamePlayerRoomInfo.AccountId);
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerTakeCoin, coinPrefab.CoinInfo.CoinID);
        msgBuilder.AddMsg((int)GamePlayerSyncRequest.PlayerSyncMessage, playerNetworkMessageTable, NetMsgFieldType.Object);
        NetworkHandler.Instance.Send(RemoteConnetionType.Game, ClientHandlerMessage.GamePlayerSyncRequest, msgBuilder);
    }
    public void OnPlayerGetBullet(BulletPrefab bullet)
    {
        Debug.Log("OnPlayerGetBullet");
        MessageBuilder msgBuilder = new MessageBuilder();
        Dictionary<PlayerSyncNetworkMessage, object> playerNetworkMessageTable = new Dictionary<PlayerSyncNetworkMessage, object>();
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerAccountId, GamePlayerRoomInfo.AccountId);
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerGetBullet, bullet.SerializeObject().ToArray());
        msgBuilder.AddMsg((int)GamePlayerSyncRequest.PlayerSyncMessage, playerNetworkMessageTable, NetMsgFieldType.Object);
        NetworkHandler.Instance.Send(RemoteConnetionType.Game, ClientHandlerMessage.GamePlayerSyncRequest, msgBuilder);
    }

    private void NetworkButtonPressedSeting()
    {
        if (!IsGamePlayer)
            return;

        // Button input
        var isJumpButtonPressed = GamePlayerInput.PlayerControls.Jump.triggered;
        var isFireButtonPressed = GamePlayerInput.PlayerControls.Fire.triggered;
        var isChangeCameraPressed = GamePlayerInput.PlayerControls.Fire.triggered;

        // network move Input
        PlayerNetworkInput PlayerNetworkInput = new PlayerNetworkInput();
        PlayerNetworkInput.movementInput = GamePlayerInput.PlayerControls.Movement.ReadValue<Vector2>();
        PlayerNetworkInput.mousePointInput = GamePlayerInput.PlayerControls.Look.ReadValue<Vector2>();

        // Gun direction
        PlayerNetworkInput.gunAimDirection = Gun.GetDirection(Camera.main.transform.forward);

        PlayerNetworkInput.SetNetworkButtonInputData(NetworkInputButtons.JUMP, isJumpButtonPressed);
        PlayerNetworkInput.SetNetworkButtonInputData(NetworkInputButtons.FIRE, isFireButtonPressed);

        if (isPlayerDie)
            PlayerNetworkInput.SetNetworkButtonInputData(NetworkInputButtons.ChangeCamera, isChangeCameraPressed);

        PlayerNetworkInput.SendNetworkInputData();
    }

    private void NetworkPlayerSyncRequest()
    {
        if (!IsGamePlayer)
            return;

        MessageBuilder msgBuilder = new MessageBuilder();
        Dictionary<PlayerSyncNetworkMessage, object> playerNetworkMessageTable = new Dictionary<PlayerSyncNetworkMessage, object>();
        float[] playerPosition = transform.position.ToFloatArray();
        float[] playerlocalEulerAngles = transform.localEulerAngles.ToFloatArray();
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerAccountId, GamePlayerRoomInfo.AccountId);
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerPosition, playerPosition);
        playerNetworkMessageTable.Add(PlayerSyncNetworkMessage.PlayerLocalEulerAngles, playerlocalEulerAngles);
        msgBuilder.AddMsg((int)GamePlayerSyncRequest.PlayerSyncMessage, playerNetworkMessageTable, NetMsgFieldType.Object);
        NetworkHandler.Instance.Send(RemoteConnetionType.Game, ClientHandlerMessage.GamePlayerSyncRequest, msgBuilder);
    }
    private void PhysicsCheck()
    {
        //Ground Check
        onGround = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        Color color = onGround ? Color.red : Color.green;

        Debug.DrawRay(transform.position, Vector3.down * 0.3f, color);
    }

    //Move  
    private void Movement()
    {
        if (movementInput.x != 0 || movementInput.y != 0)
        {
            Vector3 moveDirection = transform.forward * movementInput.y + transform.right * movementInput.x;
            rb.AddForce(moveDirection.normalized * maxSpeed * accelerationSpeed * Time.fixedDeltaTime);

            MovementControler();
        }

        //Caculate speedNow
        Vector3 speedVectorNow = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        speedNow = speedVectorNow.magnitude;
        // Debug.Log(speedNow);
    }
    private void MovementControler()
    {
        Vector3 speedVectorNow = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (speedVectorNow.magnitude > maxSpeed)
        {
            Vector3 limitedSpeed = speedVectorNow.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedSpeed.x, rb.velocity.y, limitedSpeed.z);
        }
    }

    //Jump
    private void Jump()
    {
        if (onGround)
        {
            rb.AddForce(Vector3.up * jumpForce * Time.fixedDeltaTime, ForceMode.Impulse);
            onGround = false;
        }
    }

    //Player Rotate
    private void PlayerRotate()
    {
        if (!IsGamePlayer)
            return;

        Vector3 angle = Camera.main.transform.localEulerAngles;
        angle.z = 0;
        angle.x = 0;
        transform.localEulerAngles = angle;
    }
    //Shoot
    private void PlayerShoot(Vector3 aimForwardVertor)
    {
        Gun?.Shoot(aimForwardVertor * 1.5f);
    }

    // Player Death
    private void PlayerDie()
    {
        isPlayerDie = true;
        coll.enabled = false;

        Animator.Play("Death");
    }
    public void PlayerDieFinish()
    {
        playerAnimEvent.gameObject.SetActive(false);
        playerUIController.EnablePlayerUIStatus(false);

        var gameplayer = GameController.GetAnotherAlivePlayerPrefab();
        if (gameplayer)
            CameraController.SwitchFollowToAnotherPlayerCamera(gameplayer);
    }
    private void PlayerRevive()
    {
        isPlayerDie = false;
        coll.enabled = true;
        // rb.simulated = true;

        playerAnimEvent.gameObject.SetActive(true);
        playerUIController.EnablePlayerUIStatus(true);
        Animator.Play("Idle");
        transform.position = GamePlayerRoomInfo.PlayerPosition;
    }

    private void UseWeapon(GameObject weapon)
    {
        Destroy(_weapon);
        _weapon = Instantiate(weapon, weaponParent);
    }
}
