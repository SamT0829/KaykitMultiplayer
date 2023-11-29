using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayerAnimEvent : MonoBehaviour
{
    private GamePlayerPrefab gamePlayer;

    private void Awake()
    {
        gamePlayer = GetComponentInParent<GamePlayerPrefab>();
    }

    private void OnDeathFinish()
    {
        gamePlayer.PlayerDieFinish();
    }
}
