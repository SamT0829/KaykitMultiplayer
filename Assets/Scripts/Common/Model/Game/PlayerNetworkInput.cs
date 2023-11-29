using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public enum NetworkInputButtons
{
    JUMP,
    FIRE,
    ChangeCamera,
    ThrowGrenade,
    RocketLauncherFire,
}

public class PlayerNetworkInput
{
    private Dictionary<NetworkInputButtons, bool> buttonsTable = new Dictionary<NetworkInputButtons, bool>();
    public Vector2 movementInput;
    public Vector2 mousePointInput;
    public float gunRotationZ;
    public Vector3 gunAimDirection;

    public PlayerNetworkInput()
    {
    }

    public PlayerNetworkInput(float movementInputX, float movementInputY)
    {
        movementInput = new Vector2(movementInputX, movementInputY);
    }

    public bool OnClick()
    {
        bool onClick = buttonsTable.Values.FirstOrDefault(onButton => onButton == true);
        return onClick;
    }

    public void SetNetworkButtonInputData(NetworkInputButtons networkInputButtons, bool buttonState)
    {
        buttonsTable[networkInputButtons] = buttonState;
    }

    public bool GetNetworkButtonInputData(NetworkInputButtons networkInputButtons)
    {
        if (buttonsTable.TryGetValue(networkInputButtons, out bool _buttonState))
        {
            return _buttonState;
        }
        return false;
    }

    public void SendNetworkInputData()
    {
        MessageBuilder message = new MessageBuilder();
        message.AddMsg((int)GamePlayerNetworkInputRequest.xVelocity, movementInput.x, NetMsgFieldType.Float);
        message.AddMsg((int)GamePlayerNetworkInputRequest.yVelocity, movementInput.y, NetMsgFieldType.Float);
        message.AddMsg((int)GamePlayerNetworkInputRequest.xRotation, mousePointInput.x, NetMsgFieldType.Float);
        message.AddMsg((int)GamePlayerNetworkInputRequest.yRotation, mousePointInput.y, NetMsgFieldType.Float);
        message.AddMsg((int)GamePlayerNetworkInputRequest.NetworkButon, buttonsTable, NetMsgFieldType.Object);

        // // Weapon
        // message.AddMsg(((int)GamePlayerNetworkInputRequest.GunRotationZ), gunRotationZ, NetMsgFieldType.Float);
        message.AddMsg((int)GamePlayerNetworkInputRequest.GunAimDirection, gunAimDirection.ToFloatArray(), NetMsgFieldType.Object);

        NetworkHandler.Instance.Send(RemoteConnetionType.Game, ClientHandlerMessage.GamePlayerNetworkInputRequest, message);
    }
    public List<object> CreateSerializedObject()
    {
        List<object> retv = new List<object>();
        retv.Add(movementInput.ToFloatArray());
        retv.Add(mousePointInput.ToFloatArray());
        retv.Add(gunAimDirection.ToFloatArray());

        var buttons = buttonsTable.ToDictionary(x => (int)x.Key, x => (object)x.Value);
        retv.Add(buttons);
        return retv;
    }

    public void DeserializeObject(object[] retv)
    {
        movementInput = ((float[])retv[0]).ToVector2();
        mousePointInput = ((float[])retv[1]).ToVector2();
        gunAimDirection = ((float[])retv[2]).ToVector3();

        if (retv[3] is Dictionary<int, object>)
        {
            Dictionary<int, object> buttons = (Dictionary<int, object>)(retv[3]);
            buttonsTable = buttons.ToDictionary(x => (NetworkInputButtons)x.Key, x => (bool)x.Value);
        }
    }
}