using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KayKitBrawlControler : GameBaseController
{
    private enum KayKitBrawlDynamicInfo
    {
        GameTimer,
        GameCoinInfo,
    }

    [Header("Game Panel")]
    [SerializeField] private Text gameTimerText;

    [Header("CoinPrefab")]
    public GameCoinPrefab gameCoinPrefab;

    private Dictionary<int, GameCoinPrefab> coinIdGameCoinTable = new Dictionary<int, GameCoinPrefab>();

    protected override void RetrieveGameStaticInfo(GameStaticInfo gameStaticInfo)
    {
        base.RetrieveGameStaticInfo(gameStaticInfo);
    }
    protected override void RetrivieGameDynamicInfo(GameDynamicInfo gameDynamicInfo)
    {
        if (gameDynamicInfo.RetrieveDynamicData(KayKitBrawlDynamicInfo.GameTimer, out Dictionary<string, double> timerData))
        {
            if (timerData.TryGetValue("GameTimer", out double gameTimer))
            {
                gameTimerText.text = string.Format("Timer : {0:0.00}", gameTimer);
            }
        }

        if (gameDynamicInfo.RetrieveDynamicData(KayKitBrawlDynamicInfo.GameCoinInfo, out Dictionary<int, object> coinSpawnerData))
        {
            foreach (var coinData in coinSpawnerData.Values)
            {
                CoinInfo coinInfo = new CoinInfo();
                coinInfo.DeserializeObject((object[])coinData);

                if (!coinIdGameCoinTable.TryGetValue(coinInfo.CoinID, out GameCoinPrefab coinPrefab))
                {
                    coinPrefab = Instantiate(gameCoinPrefab, coinInfo.Position, Quaternion.identity);
                    coinPrefab.Init(coinInfo);
                    coinIdGameCoinTable.Add(coinInfo.CoinID, coinPrefab);
                }
                else
                {
                    coinPrefab.NetworkUpdate(coinInfo);
                }
            }
        }
    }

    protected override void RetrivieGameResultInfo(GameResultInfo gameResultInfo)
    {
        GameFinish();
        ShowWinnerPlayer(gameResultInfo);
    }
}
