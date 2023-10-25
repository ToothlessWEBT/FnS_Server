using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private ushort Id;

    private float damage;

    private void Start()
    {
        Invoke(nameof(DestroySelf), 12f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //print("Hit" + other.gameObject.name);
        if(other.gameObject.tag == "Enemy")
        {
            //print("Take THAT");
            other.GetComponent<Enemy>().ReduceHealth(damage);

            DestroySelf();
        }
    }

    private void FixedUpdate()
    {
        SendPosition();
    }

    private void SendPosition()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.bulletMove);

        message.AddUShort(Id);

        message.AddVector2(transform.position);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void CreateBullet(ushort newId, float newdamage)
    {
        Id = newId;

        damage = newdamage;
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
