using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Prefab")]
    [SerializeField] MessageTipsPrefab MessageTipsPrefab;

    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            _instance = this;

        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
    }

    public void CreateMessageTipsUI(string messageTitleText, string messageTipsText, string messageButtonText, Action messageCallBack, Action clickButtonCallBack)
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                var messageTips = Instantiate(MessageTipsPrefab, canvas.transform);
                messageTips.BuildGameMessage(messageTitleText, messageTipsText, messageButtonText, messageCallBack, clickButtonCallBack);
                return;
            }
        }
    }
}
