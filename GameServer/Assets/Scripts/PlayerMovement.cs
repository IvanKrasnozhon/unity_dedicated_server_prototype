using Riptide;
using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Player player;

    [SerializeField]
    private CharacterController characterController;

    [SerializeField]
    private Transform camProxy;

    [SerializeField]
    private float gravity;

    [SerializeField]
    private float movementSpeed;

    [SerializeField]
    private float jumpHeight;

    private float gravityAcceleration;
    private float moveSpeed;
    private float jumpSpeed;

    private bool[] inputs;
    private float yVelocity;
    private bool isTeleported = false; //Assign this value to true if player should be teleported. This will disable interpolation between old position and new position for the client

    private void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (player == null)
            player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();
        inputs = new bool[6];
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;
        if (inputs[0])//w
            inputDirection.y += 1;
        if (inputs[2])//s
            inputDirection.y -= 1;
        if (inputs[1])//a
            inputDirection.x -= 1;
        if (inputs[3])//d
            inputDirection.x += 1;

        Move(inputDirection, inputs[5], inputs[4]);
    }

    private void Initialize()
    {
        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed = movementSpeed * Time.fixedDeltaTime;
        jumpSpeed = Mathf.Sqrt(jumpHeight * -2f * gravityAcceleration);
    }

    private void Move(Vector2 inputDirection, bool jump, bool sprint)
    {
        Vector3 moveDirection = Vector3.Normalize(camProxy.right * inputDirection.x + Vector3.Normalize(FlattenVector3(camProxy.forward)) * inputDirection.y);
        moveDirection *= moveSpeed;

        if (sprint)
            moveDirection *= 2f;

        if(characterController.isGrounded)
        {
            yVelocity = 0;
            if (jump)
                yVelocity = jumpSpeed;
        }

        yVelocity += gravityAcceleration;
        moveDirection.y = yVelocity;
        characterController.Move(moveDirection);

        SendMovement();
    }


    private Vector3 FlattenVector3(Vector3 inputDirection)
    {
        inputDirection.y = 0;
        return inputDirection;
    }
    private void SendMovement()
    {
        if (NetworkManager.Singleton.CurrentTick % 2 != 0) //Makes position updates become half the tickrate(default is 64)
            return;

        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUInt(NetworkManager.Singleton.CurrentTick);
        message.AddBool(isTeleported); 
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        NetworkManager.Singleton.Server.SendToAll(message); 

        isTeleported = false;
    }

    public void SetInput(bool[] inputs, Vector3 forward)
    {
        this.inputs = inputs;
        camProxy.forward = forward;
    }
}