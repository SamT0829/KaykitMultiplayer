using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyRoomPrefab : MonoBehaviour
{
    public LobbyRoomInfo Info;
    [SerializeField]
    Button LobbyRoomButton;
    [SerializeField]
    Text LobbyRoomId;
    [SerializeField]
    Text LobbyRoomName;
    [SerializeField]
    Text LobbyRoomPlayerCount;

    private void Awake()
    {
        LobbyRoomButton.onClick.AddListener(JoinRoom);
    }

    public void SettingRoomPrefab(LobbyRoomInfo lobbyRoomInfo)
    {
        Info = lobbyRoomInfo;

        LobbyRoomId.text = lobbyRoomInfo.RoomId.ToString();
        LobbyRoomName.text = lobbyRoomInfo.RoomName;
        LobbyRoomPlayerCount.text = string.Format("Player {0}/{1}", lobbyRoomInfo.LobbyPlayerRoomInfos.Count, lobbyRoomInfo.MaxPlayer);
    }

    public void SettingRoomPrefab(string roomIdText, string gameNameText, string maxPlayerText)
    {
        // RoomId = Convert.ToInt32(roomIdText);
        LobbyRoomId.text = roomIdText;
        LobbyRoomName.text = gameNameText;
        LobbyRoomPlayerCount.text = maxPlayerText;
    }

    private void JoinRoom()
    {
        //Send Network Message
        MessageBuilder msgBuilder = new MessageBuilder();
        msgBuilder.AddMsg((int)LobbyPlayerJoinLobbyRoomRequest.RoomId, Info.RoomId, NetMsgFieldType.Int);
        msgBuilder.AddMsg((int)LobbyPlayerJoinLobbyRoomRequest.RoomType, Info.RoomType, NetMsgFieldType.Int);
        msgBuilder.AddMsg((int)LobbyPlayerJoinLobbyRoomRequest.RoomName, Info.RoomName, NetMsgFieldType.String);
        msgBuilder.AddMsg((int)LobbyPlayerJoinLobbyRoomRequest.RoomPassword, Info.RoomPassword, NetMsgFieldType.String);
        // msgBuilder.AddMsg(((int)JoinGameRomeRequest.PlayerData), GameManager.Instance.PlayerStatus, NetMsgFieldType.String);
        NetworkHandler.Instance.Send(RemoteConnetionType.Lobby, ClientHandlerMessage.LobbyPlayerJoinLobbyRoomRequest, msgBuilder);
        Debug.Log("send message");
    }
}
