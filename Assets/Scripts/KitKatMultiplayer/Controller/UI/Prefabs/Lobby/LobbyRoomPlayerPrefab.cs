using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LobbyRoomPlayerPrefab : MonoBehaviour
{
    public LobbyPlayerRoomInfo LobbyPlayerRoomInfo;

    [SerializeField] Text PlayerName;
    [SerializeField] Image ReadyImage;
    [SerializeField] Image HostImage;
    [SerializeField] Image BackgroundImage;


    private void Awake()
    {
        PlayerName = GetComponentInChildren<Text>();
        HostImage.gameObject.SetActive(false);
    }

    public void SettingGameRoomPlayerPrefab(LobbyPlayerRoomInfo lobbyPlayerRoomInfo)
    {
        LobbyPlayerRoomInfo = lobbyPlayerRoomInfo;
        PlayerName.text = lobbyPlayerRoomInfo.NickName;
    }

    //Network Update
    public void UpdateNetworkSetting(LobbyRoomInfo lobbyRoomInfo, LobbyPlayerRoomInfo lobbyPlayerRoomInfo)
    {
        LobbyPlayerRoomInfo = lobbyPlayerRoomInfo;
        SetReady(lobbyPlayerRoomInfo.isReady);
        SetBackgroundColor(lobbyPlayerRoomInfo.Team);
        HostImageSetActive(lobbyPlayerRoomInfo.NickName == lobbyRoomInfo.HostPlayer);
    }

    private void SetReady(bool ready)
    {
        if (ready)
            ReadyImage.gameObject.SetActive(true);
        else
            ReadyImage.gameObject.SetActive(false);
    }

    private void SetBackgroundColor(Team team)
    {
        switch (team)
        {
            case Team.RedTeam:
                BackgroundImage.color = Color.red;
                break;
            case Team.BlueTeam:
                BackgroundImage.color = Color.blue;
                break;
            default:
                break;
        }
    }

    private void HostImageSetActive(bool value)
    {
        HostImage.gameObject.SetActive(value);
    }
}
