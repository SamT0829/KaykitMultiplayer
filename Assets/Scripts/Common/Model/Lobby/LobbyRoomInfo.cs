using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class LobbyRoomInfo
{
    private enum LobbyRoomInfoMessageType
    {
        RoomType,
        RoomId,
        RoomName,
        RoomPassword,
        MaxPlayer,
        HostPlayer,
        LobbyRoomStatus,
        LobbyPlayerRoomInfos,
    }
    public int RoomType { get; private set; }
    public int RoomId { get; private set; }
    public string RoomName { get; private set; }
    public string RoomPassword { get; private set; }
    public int MaxPlayer { get; private set; }
    public string HostPlayer { get; private set; }
    public LobbyRoomState LobbyRoomState { get; private set; }
    private List<LobbyPlayerRoomInfo> lobbyPlayerRoomInfosList;
    public List<LobbyPlayerRoomInfo> LobbyPlayerRoomInfos { get { return lobbyPlayerRoomInfosList; } }

    public void DeserializeObject(Dictionary<int, object> roomData)
    {
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.RoomType, out int roomType)) { RoomType = roomType; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.RoomId, out int roomId)) { RoomId = roomId; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.RoomName, out string roomName)) { RoomName = roomName; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.RoomPassword, out string roomPassword)) { RoomPassword = roomPassword; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.MaxPlayer, out int maxPlayer)) { MaxPlayer = maxPlayer; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.HostPlayer, out string hostPlayer)) { HostPlayer = hostPlayer; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.LobbyRoomStatus, out LobbyRoomState lobbyRoomState)) { LobbyRoomState = lobbyRoomState; }
        if (roomData.RetrieveMessageItem(LobbyRoomInfoMessageType.LobbyPlayerRoomInfos, out string lobbyPlayerRoomInfos))
        { lobbyPlayerRoomInfosList = JsonConvert.DeserializeObject<List<LobbyPlayerRoomInfo>>(lobbyPlayerRoomInfos); }
    }
}