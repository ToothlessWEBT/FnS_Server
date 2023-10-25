using System.Collections;
using RiptideNetworking;
using UnityEngine;

public class EBullet : MonoBehaviour
{
    private float damage;
    private ushort Id;

    private void Start()
    {
        Invoke(nameof(DestroySelf), 12f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        print("Hit" + other.gameObject.name);
        if(other.gameObject.tag == "Player")
        {
            //print("Take THAT PLAYER");
            other.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }

    private void FixedUpdate()
    {
        SendPosition();
    }

    public void CreateBullet(ushort iD, float newdamage)
    {
        damage = newdamage;

        Id = iD;
    }

    private void SendPosition()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.bulletMove);

        message.AddUShort(Id);

        message.AddVector2(transform.position);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void DestroySelf()
    {
        BulletManager.Singleton.RemoveBullet(Id);

        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.bulletKill);

        message.AddUShort(Id);

        NetworkManager.Singleton.Server.SendToAll(message);

        Destroy(gameObject);
    }
}
