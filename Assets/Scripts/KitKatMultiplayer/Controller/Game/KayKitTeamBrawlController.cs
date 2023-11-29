using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class KayKitTeamBrawlController : GameBaseController
{
    private enum KayKitTeamBrawlState
    {
        None,
        Prepare,
        Game,
        Finish,
        Ending,
    }
    private enum KayKitTeamBrawlGameData
    {
        GameMessage,
    }

    private enum KayKitTeamBrawlFinishData
    {
        TeamWinner,
    }

    [Header("Game Panel")]
    [SerializeField] private Text gameTimerText;
    [SerializeField] private Text nowGameRoundText;

    [SerializeField] private Text redTeamScoreText;
    [SerializeField] private Text blueTeamScoreText;

    private KayKitTeamBrawlState _gameState;

    private KayKitTeamBrawlModel GameModel;

    //bool
    private bool waittingNextRound;
    private bool waittingGameResult;

    private void Awake()
    {
        GameModel = GetComponent<KayKitTeamBrawlModel>();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void RetrieveGameStaticInfo(GameStaticInfo gameStaticInfo)
    {
        base.RetrieveGameStaticInfo(gameStaticInfo);
        GameModel.ParseStaticInfo(gameStaticInfo);

        nowGameRoundText.text = GameModel.GameRound.ToString();
        redTeamScoreText.text = GameModel.RedTeamScore.ToString();
        blueTeamScoreText.text = GameModel.BlueTeamScore.ToString();
    }
    protected override void RetrivieGameDynamicInfo(GameDynamicInfo gameDynamicInfo)
    {
        base.RetrivieGameDynamicInfo(gameDynamicInfo);
        GameModel.ParseDynamicInfo(gameDynamicInfo);

        _gameState = (KayKitTeamBrawlState)GameModel.GameState;
        GameState();
        gameTimerText.text = string.Format("Timer : {0:0.00}", GameModel.GameTimer);
    }
    protected override void RetrivieGameResultInfo(GameResultInfo gameResultInfo)
    {
        if (waittingGameResult)
            return;

        waittingGameResult = true;
        GameFinish();
        ShowWinnerPlayer(gameResultInfo);
    }
    private void GameState()
    {
        switch (_gameState)
        {
            case KayKitTeamBrawlState.Prepare:
                waittingNextRound = false;
                break;
            case KayKitTeamBrawlState.Game:
                break;
            case KayKitTeamBrawlState.Finish:
                NextRoundSetting();
                break;
        }
    }
    // private void GameMessage(Dictionary<int, object> gameStateData)
    // {
    //     if (gameStateData.RetrieveMessageItem(KayKitTeamBrawlGameData.GameMessage, out string gameMessage, false))
    //     {
    //         Debug.Log(gameMessage);
    //     }
    // }
    private void NextRoundSetting()
    {
        if (waittingNextRound)
            return;

        waittingNextRound = true;
        GamePlayerAction(ResetGamePlayerPosition);
    }
    private void ResetGamePlayerPosition(GamePlayerPrefab gamePlayer)
    {
        gamePlayer.transform.position = gamePlayer.GamePlayerRoomInfo.PlayerPosition;
    }
}
