using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject followCamera;
    [SerializeField] private GameObject aimCamera;
    [SerializeField] private GameObject lookAnotherPlayerCamera;
    [SerializeField] public Transform followPoint;
    [SerializeField] public Transform lookAnotherPlayerPoint;

    [SerializeField] private float aimRotationPower;
    [SerializeField] private float followRotationPower;


    private float rotationPower;

    //Rotation
    float cameraRotationX;
    float cameraRotationY;

    private GamePlayerPrefab GamePlayer;
    private GamePlayerInput GamePlayerInput;
    private bool isAim;

    private void Awake()
    {
        GamePlayer = GetComponentInParent<GamePlayerPrefab>();
        GamePlayerInput = GamePlayer.GamePlayerInput;
    }

    private void Start()
    {
        transform.parent = null;
    }

    private void Update()
    {
        CameraControl();
    }

    private void CameraControl()
    {
        if (GamePlayerInput == null || followPoint == null)
            return;

        Vector2 mouseInput = GamePlayerInput.PlayerControls.Look.ReadValue<Vector2>();

        //Calculate rotation
        cameraRotationX += mouseInput.y * Time.deltaTime * rotationPower;
        if (isAim)
            cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);
        else
            cameraRotationX = Mathf.Clamp(cameraRotationX, -50, 50);

        cameraRotationY += mouseInput.x * Time.deltaTime * rotationPower;

        followPoint.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        followPoint.position = GamePlayer.cameraFollowPoint.position;
    }

    public void SwitchFollowToAimCamera(bool isAim)
    {
        this.isAim = isAim;
        if (isAim)
        {
            aimCamera.SetActive(true);
            followCamera.SetActive(false);
            rotationPower = aimRotationPower;
        }
        else
        {
            aimCamera.SetActive(false);
            followCamera.SetActive(true);
            rotationPower = followRotationPower;
        }
    }

    public void SwitchFollowToAnotherPlayerCamera(GamePlayerPrefab gamePlayer)
    {
        if (!GamePlayer.isPlayerDie)
            return;

        lookAnotherPlayerCamera.SetActive(true);
        aimCamera.SetActive(false);
        followCamera.SetActive(false);
        rotationPower = followRotationPower;
        lookAnotherPlayerPoint = gamePlayer.CameraController.followPoint;
    }
}
