using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    None,
    RedTeam,
    BlueTeam,
}
public class LobbyPlayerRoomInfo
{
    public int RoomId { get; set; }
    public long AccountId { get; set; }
    public string NickName { get; set; }
    public GameType GameType { get; set; }
    public string PlayerDataStatus { get; set; }


    // Player in LobbyRoom Setting
    public bool isReady { get; set; }

    public Team Team { get; set; }

}
