using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] bool alwaysVisible;

    [Header("PlayerUIPrefab")]
    [SerializeField] GameObject PlayerUIPrefab;

    [Header("Point")]
    [SerializeField] Transform playerNamePoint;
    [SerializeField] Transform playerTeamPoint;
    [SerializeField] Transform coinCountPoint;
    [SerializeField] Transform healthPoint;
    [SerializeField] Transform energyPoint;

    GameObject PlayerStatus;

    // Component
    Text playerNameText;
    Text playerTeamText;
    Text coinCountText;
    Image playerHealth;
    Image playerHealthCurrent;
    Image playerEnergy;
    Image playerEnergyCurrent;

    GamePlayerPrefab gamePlayerPrefab;

    private void Awake()
    {
        gamePlayerPrefab = GetComponent<GamePlayerPrefab>();
    }

    private void Start()
    {
        if (gamePlayerPrefab.GamePlayerRoomInfo.Team != Team.None)
            InitPlayerTeamUIStatus();
        else
            playerTeamText.gameObject.SetActive(false);

        Debug.Log(gamePlayerPrefab.GamePlayerRoomInfo.Team.ToString());
        playerNameText.text = gamePlayerPrefab.GamePlayerRoomInfo.NickName;
        coinCountText.text = gamePlayerPrefab.GamePlayerRoomInfo.CointCount.ToString();
        float healthSize = (float)gamePlayerPrefab.GamePlayerRoomInfo.PlayerHealth / (float)gamePlayerPrefab.GamePlayerRoomInfo.PlayerMaxHealth;
        playerHealthCurrent.fillAmount = healthSize;
    }

    private void OnEnable()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                PlayerStatus = Instantiate(PlayerUIPrefab, canvas.transform);
                playerNameText = PlayerStatus.transform.Find("PlayerName").GetComponent<Text>();
                playerTeamText = PlayerStatus.transform.Find("playerTeamText").GetComponent<Text>();
                coinCountText = PlayerStatus.transform.Find("CoinCount").GetComponent<Text>();
                playerHealth = PlayerStatus.transform.Find("HealthBackground").GetComponent<Image>();
                playerHealthCurrent = PlayerStatus.transform.Find("HealthBackground").Find("HealthCurrent").GetComponent<Image>();
                playerEnergy = PlayerStatus.transform.Find("EnergyBackground").GetComponent<Image>();
                playerEnergyCurrent = PlayerStatus.transform.Find("EnergyBackground").Find("EnergyCurrent").GetComponent<Image>();
                PlayerStatus.SetActive(alwaysVisible);
            }
        }
    }

    private void LateUpdate()
    {
        var cam = Camera.main.transform;

        if (playerNameText != null)
        {
            playerNameText.transform.position = playerNamePoint.position;
            playerNameText.transform.forward = cam.forward;
        }

        if (playerTeamText != null)
        {
            playerTeamText.transform.position = playerTeamPoint.position;
            playerTeamText.transform.forward = cam.forward;
        }

        if (coinCountText != null)
        {
            coinCountText.text = gamePlayerPrefab.GamePlayerRoomInfo.CointCount.ToString();
            coinCountText.transform.position = coinCountPoint.position;
            coinCountText.transform.forward = cam.forward;
        }

        if (playerHealth != null)
        {
            float healthSize = (float)gamePlayerPrefab.GamePlayerRoomInfo.PlayerHealth / (float)gamePlayerPrefab.GamePlayerRoomInfo.PlayerMaxHealth;
            playerHealthCurrent.fillAmount = healthSize;
            playerHealth.transform.position = healthPoint.position;
            playerHealth.transform.forward = cam.forward;
        }

        // if (playerEnergy != null)
        // {
        //     float energySize = (float)gamePlayerPrefab.GamePlayerRoomInfo.PlayerEnergy / (float)gamePlayerPrefab.GamePlayerRoomInfo.PlayerMaxEnergy;
        //     playerEnergyCurrent.fillAmount = energySize;
        //     playerEnergy.transform.position = energyPoint.position;
        //     playerEnergy.transform.forward = cam.forward;
        // }
    }
    private void InitPlayerTeamUIStatus()
    {
        switch (gamePlayerPrefab.GamePlayerRoomInfo.Team)
        {
            case Team.RedTeam:
                playerTeamText.color = Color.red;
                break;
            case Team.BlueTeam:
                playerTeamText.color = Color.blue;
                break;
        }

        playerTeamText.text = gamePlayerPrefab.GamePlayerRoomInfo.Team.ToString();
    }
    public void InitPlayerUIStatus(string playerName, int coinCount)
    {
        playerNameText.text = playerName;
        coinCountText.text = coinCount.ToString();
    }

    public void EnablePlayerUIStatus(bool enabled)
    {
        PlayerStatus.SetActive(enabled);
    }

    public void RemovePlayerUIStatus()
    {
        Destroy(gameObject);
    }
}