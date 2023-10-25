using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth, currentHealth;

    [SerializeField] private Player player;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        print("Taken damage");
        currentHealth -= damage;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.damagePlayer);
        message.AddUShort(player.Id);
        message.AddInt((int) currentHealth);

        NetworkManager.Singleton.Server.SendToAll(message);

        if(currentHealth <= 0) //Die
        {
            player.KillSelf();
        }
    }
}
