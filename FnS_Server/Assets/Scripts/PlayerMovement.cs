using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

//RequireComponent(typeof())
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed;

    private bool[] inputs;

    private void Start()
    {
        inputs = new bool[4];

        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;

        if(inputs[0])
            inputDirection.y += 1;
        
        if(inputs[1])
            inputDirection.y -= 1;
        
        if(inputs[2])
            inputDirection.x += 1;

        if(inputs[3])
            inputDirection.x -= 1;

        MovePlayer(inputDirection);
    }

    private void MovePlayer(Vector2 inputDir)
    {
        rb.velocity = Vector2.Lerp(rb.velocity, inputDir.normalized * moveSpeed * Time.fixedDeltaTime, 0.8f);
    
        SendMovement();
    }

    public void SetInputs(bool[] clientInputs)
    {
        inputs = clientInputs;
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddVector2(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

}
