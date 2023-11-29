using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResultInfo
{
    public Team WinnerTeam = Team.None;
    public List<GamePlayerRoomInfo> winnerInfo = new List<GamePlayerRoomInfo>();

    public List<object> SerializeObject()
    {
        List<object> retv = new List<object>();

        List<object> playerData = new List<object>();
        winnerInfo.ForEach(playerInfo => playerData.Add(playerInfo.SerializedObject()));
        retv.Add(playerData);

        return retv;
    }

    public void DeserializeObject(object[] retv)
    {
        object[] playerData = (object[])(retv[0]);
        Array.ForEach(playerData, playerInfo =>
        {
            GamePlayerRoomInfo gamePlayerRoomInfo = new GamePlayerRoomInfo();
            gamePlayerRoomInfo.DeserializeObject((object[])playerInfo);
            winnerInfo.Add(gamePlayerRoomInfo);
        });
    }
}
