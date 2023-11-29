using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageTipsPrefab : MonoBehaviour
{
    [SerializeField] Text MessageTitleText;
    [SerializeField] Text MessageTipsText;
    [SerializeField] Button MessageButton;
    [SerializeField] Text MessageButtonText;

    public void BuildGameMessage(string messageTitleText, string messageTipsText, string messageButtonText, Action messageCallBack, Action clickButtonCallBack)
    {
        MessageTitleText.text = messageTitleText;
        MessageTipsText.text = messageTipsText;
        MessageButtonText.text = messageButtonText;

        if (messageCallBack != null)
            messageCallBack.Invoke();

        MessageButton.onClick.AddListener(() =>
        {
            if (clickButtonCallBack != null)
                clickButtonCallBack.Invoke();

            DestroyGameMessage();
        });
    }

    private void ResetGameMessage()
    {
        MessageTipsText.text = string.Empty;
        MessageButtonText.text = string.Empty;

        MessageButton.onClick.RemoveAllListeners();
    }
    private void DestroyGameMessage()
    {
        ResetGameMessage();
        Destroy(gameObject);
    }
}
