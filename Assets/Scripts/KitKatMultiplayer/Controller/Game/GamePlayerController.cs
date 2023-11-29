using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayerController : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public bool onGround;

    [Header("PlayerStatus")]
    public float maxSpeed = 6f;
    public float accelerationSpeed = 2f;
    public float speedNow;
    public float jumpForce = 6f;

    [Header("Game Player control input")]
    private Vector2 movementInput;
    private bool isJumpPressed;
    private bool isFirePressed;

    // Component
    private Rigidbody rb;
    public GamePlayerInput GamePlayerInput;
    private PlayerUIController playerUIController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerUIController = GetComponentInChildren<PlayerUIController>();

        //Input event Callback
        GamePlayerInput = new GamePlayerInput();
        GamePlayerInput.PlayerControls.Enable();
    }
    private void FixedUpdate()
    {
        PhysicsCheck();
        Movement();
        Jump();
    }

    public void NetworkInputUpdate(PlayerNetworkInput playerNetworkInput)
    {
        // if (isPlayerDie)
        // {
        //     xVelocity = 0;
        //     yVelocity = 0;
        //     isJumpPressed = false;
        //     isFirePressed = false;
        //     return;
        // }

        movementInput = new Vector2(playerNetworkInput.movementInput.x, playerNetworkInput.movementInput.y);

        isJumpPressed = playerNetworkInput.GetNetworkButtonInputData(NetworkInputButtons.JUMP);
        isFirePressed = playerNetworkInput.GetNetworkButtonInputData(NetworkInputButtons.FIRE);
    }
    private void PhysicsCheck()
    {
        //Ground Check
        onGround = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        Color color = onGround ? Color.red : Color.green;

        Debug.DrawRay(transform.position, Vector3.down * 1.1f, color);
    }

    //Move  
    private void Movement()
    {
        if (movementInput.x != 0 || movementInput.y != 0)
        {
            // transform.rotation = Quaternion.Euler(0, cameraController.cameraFollowPoint.transform.localEulerAngles.y, 0);
            Vector3 moveDirection = transform.forward * movementInput.y + transform.transform.right * movementInput.x;
            rb.AddForce(moveDirection.normalized * maxSpeed * accelerationSpeed);

            MovementControler();
        }

        //Caculate speedNow
        // Vector3 speedVectorNow = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        // speedNow = speedVectorNow.magnitude;
        // Debug.Log(speedNow);
    }
    private void MovementControler()
    {
        Vector3 speedVectorNow = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (speedVectorNow.magnitude > maxSpeed)
        {
            Vector3 limitedSpeed = speedVectorNow.normalized * maxSpeed;
            rb.velocity = new Vector3(limitedSpeed.x, rb.velocity.y, limitedSpeed.z);
        }
    }
    //Jump
    private void Jump()
    {
        if (onGround && isJumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            onGround = false;
        }
    }
}
