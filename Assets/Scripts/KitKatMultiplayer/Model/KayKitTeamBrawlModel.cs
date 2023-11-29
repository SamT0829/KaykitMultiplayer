using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KayKitTeamBrawlModel : MonoBehaviour
{
    private enum KayKitTeamBrawlStaticInfo
    {
        GameRound = 100,
        GameScore,
    }

    private enum KayKitTeamBrawlDynamicInfo
    {
        GameState = 100,
        GameStateData,
        GameTimer,
    }

    //Static Data
    public int GameRound;
    public int RedTeamScore;
    public int BlueTeamScore;

    //Dynamic Data
    public int GameState;
    public double PrepareTimer;
    public double GameTimer;
    public double BonusTimer;


    public void ParseStaticInfo(GameStaticInfo staticInfo)
    {
        if (staticInfo.RetrieveStaticData(KayKitTeamBrawlStaticInfo.GameRound, out int gameRound, false))
        {
            GameRound = gameRound;
        }

        if (staticInfo.RetrieveStaticData(KayKitTeamBrawlStaticInfo.GameScore, out object[] gameScoreData, false))
        {
            RedTeamScore = (int)gameScoreData[0];
            BlueTeamScore = (int)gameScoreData[1];
        }
    }

    public void ParseDynamicInfo(GameDynamicInfo dynamicInfo)
    {
        if (dynamicInfo.RetrieveDynamicData(KayKitTeamBrawlDynamicInfo.GameState, out int gameStateData))
        {
            GameState = gameStateData;
        }
        if (dynamicInfo.RetrieveDynamicData(KayKitTeamBrawlDynamicInfo.GameTimer, out object[] gameTimerData))
        {
            PrepareTimer = (double)gameTimerData[0];
            GameTimer = (double)gameTimerData[1];
            BonusTimer = (double)gameTimerData[2];
        }
    }
}