using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChatMessagePrefab : MonoBehaviour
{
    private string text;
    Text messageText;

    private void Awake()
    {
        if (messageText == null)
            messageText = GetComponent<Text>();
    }

    public void BuildChatMessage(List<object> chatData)
    {
        LobbyRoomMessage lobbyRoomMessage = (LobbyRoomMessage)Convert.ToInt32(chatData[0]);
        var name = chatData[1].ToString();
        var chatMessage = chatData[2].ToString();

        switch (lobbyRoomMessage)
        {
            case LobbyRoomMessage.InfoMessage:
                messageText.color = Color.blue;
                text = string.Format("{0} {1}", name, chatMessage);
                break;
            case LobbyRoomMessage.PlayerMessage:
                messageText.color = Color.black;
                text = string.Format("{0} : {1}", name, chatMessage);
                break;
            case LobbyRoomMessage.WarningMessage:
                messageText.color = Color.red;
                text = string.Format("{0} {1}", name, chatMessage);
                break;
        }

        messageText.text = text;
    }
}
