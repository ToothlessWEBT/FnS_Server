using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

//RequireComponent(typeof())
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    private Rigidbody2D rb;

    [SerializeField] private float moveSpeed, dashSpeed, dashLength, dashCooldown;

    private bool canDash = false, allowedToDash = true, dashing = false;

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
        if(!player.isAlive) return;

        if(canDash && allowedToDash)
        {
            allowedToDash = false;
            dashing = true;
            rb.velocity = Vector2.Lerp(rb.velocity, inputDir.normalized * dashSpeed * Time.fixedDeltaTime, 0.8f);

            Invoke(nameof(StopDashing), dashLength);
        }
        if(!dashing) rb.velocity = Vector2.Lerp(rb.velocity, inputDir.normalized * moveSpeed * Time.fixedDeltaTime, 0.8f);
    
        SendMovement();
    }

    public void SetInputs(bool[] clientInputs)
    {
        inputs = clientInputs;
    }

    public void AllowDash()
    {
        canDash = true;

        Invoke(nameof(StopDash), 0.3f);
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddVector2(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void StopDash() => canDash = false;

    private void AllowDashing() => allowedToDash = true;

    private void StopDashing()
    {
        dashing = false;

        Invoke(nameof(AllowDashing), dashCooldown);
    }
}
