using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GameDynamicInfo
{
    private enum GameDynamicInfoKey
    {
        GameDynamicData,
    }

    private Dictionary<int, object> gameDynamicDataTable = new Dictionary<int, object>();

    public void AddDynamicData<TEnum>(TEnum gameDynamicDataKey, object data) where TEnum : Enum
    {
        if (!gameDynamicDataTable.TryGetValue(gameDynamicDataKey.GetHashCode(), out object gameDynamicDataValue))
        {
            gameDynamicDataTable.Add(gameDynamicDataKey.GetHashCode(), data);
        }
        else
        {
            gameDynamicDataTable[gameDynamicDataKey.GetHashCode()] = data;
        }
    }
    public bool RetrieveDynamicData<TEnum, D>(TEnum gameDynamicDataKey, out D gameDynamicDataValue, bool isDebugLogForNotExistValue = true) where TEnum : Enum
    {
        if (gameDynamicDataTable.RetrieveMessageItem(gameDynamicDataKey, out D gameDynamicData, isDebugLogForNotExistValue))
        {
            gameDynamicDataValue = gameDynamicData;
            return true;
        }

        Debug.Log(JsonConvert.SerializeObject(gameDynamicDataTable));
        gameDynamicDataValue = default;
        return false;
    }
    public Dictionary<int, object> SerializeObject()
    {
        Dictionary<int, object> data = new Dictionary<int, object>();
        var gameDynamicData = gameDynamicDataTable.ToDictionary(x => x.Key, x => x.Value);
        data.Add(GameDynamicInfoKey.GameDynamicData.GetHashCode(), gameDynamicData);

        return data;
    }

    public void DeserializeObject(Dictionary<int, object> data)
    {
        if (data.RetrieveMessageItem(GameDynamicInfoKey.GameDynamicData, out Dictionary<int, object> gameDynamicData))
        {
            gameDynamicDataTable = gameDynamicData.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
