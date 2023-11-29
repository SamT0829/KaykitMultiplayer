using System.Collections.Generic;
using System;
using Newtonsoft.Json;

public class LobbyPlayerInfo
{
    private enum LobbyPlayerData
    {
        AccountId,
        GameType,
        NickName,
        Status,
        TotalPlayCount,
        TotalLoseCount,
        TotalWinCount,
    }

    public long AccountId { get; set; }
    public GameType GameType { get; set; }
    public string NickName { get; set; }
    public LobbyPlayerStatus Status { get; set; }
    public int TotalPlayCount { get; set; }
    public int TotalLoseCount { get; set; }
    public int TotalWinCount { get; set; }

    public string JsonSerializeObject()
    {
        return JsonConvert.SerializeObject(this);
    }

    public Dictionary<int, object> DictionarySerializeObject()
    {
        Dictionary<int, object> retv = new Dictionary<int, object>();
        retv.Add(LobbyPlayerData.AccountId.GetHashCode(), AccountId);
        retv.Add(LobbyPlayerData.GameType.GetHashCode(), GameType);
        retv.Add(LobbyPlayerData.NickName.GetHashCode(), NickName);
        retv.Add(LobbyPlayerData.Status.GetHashCode(), Status);
        retv.Add(LobbyPlayerData.TotalPlayCount.GetHashCode(), TotalPlayCount);
        retv.Add(LobbyPlayerData.TotalLoseCount.GetHashCode(), TotalLoseCount);
        retv.Add(LobbyPlayerData.TotalWinCount.GetHashCode(), TotalWinCount);

        return retv;
    }

    public List<object> SerializeObject()
    {
        List<object> retv = new List<object>();
        retv.Add(AccountId);                    //0
        retv.Add(NickName);                     //1
        retv.Add(Status);                       //2
        retv.Add(GameType);                     //3
        retv.Add(TotalPlayCount);               //4
        retv.Add(TotalLoseCount);               //5
        retv.Add(TotalWinCount);                //6

        return retv;
    }
   
    public void DeserializeObject(object[] retv)
    {
        AccountId = Convert.ToInt64(retv[0]);
        NickName = retv[1].ToString();
        Status = (LobbyPlayerStatus)Convert.ToInt32(retv[2]);
        GameType = (GameType)Convert.ToInt32(retv[3]);
        TotalPlayCount = Convert.ToInt32(retv[4]);
        TotalLoseCount = Convert.ToInt32(retv[5]);
        TotalWinCount = Convert.ToInt32(retv[6]);
    }
}