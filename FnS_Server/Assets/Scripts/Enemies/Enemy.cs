using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private ushort Id;

    [SerializeField] private float health, damage;

    [SerializeField] private float moveSpeed, findRadius;

    private Rigidbody2D rb;

    private bool following;

    private bool canHit = true;

    private EnemyShoot enemyShoot;

    public void SetId(ushort newId) => Id = newId;

    [SerializeField]
    private LayerMask whatArePlayers;

    [SerializeField] private bool canFlip = false;

    private bool facingLeft = true;

    private Transform closestP;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void FixedUpdate()
    {
        if(!following)
        {
            FindCP();
        }

        SendEnemyPos();

        MoveToPlayer();

        if(!canFlip) return;

        if(Vector2.Dot(Vector2.left, rb.velocity.normalized) > 0 && !facingLeft)
        {
            facingLeft = true;

            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.flipEnemy);

            message.AddUShort(Id);

            NetworkManager.Singleton.Server.SendToAll(message);
        }
        else if(Vector2.Dot(Vector2.left, rb.velocity.normalized) < 0 && facingLeft)
        {
            facingLeft = false;

            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.flipEnemy);

            message.AddUShort(Id);

            NetworkManager.Singleton.Server.SendToAll(message);
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        //print(other.gameObject.tag);
        if(other.gameObject.tag == "Player" && canHit)
        {
            other.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);

            canHit = false;

            Invoke(nameof(ResetHit), 1f);
        }
    }

    

    private void SendEnemyPos()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.enemyMove);

        message.AddUShort(Id);

        message.AddVector2(transform.position);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void MoveToPlayer()
    {
        if(closestP == null) return;

        if(!following)
            {
                following = true;

                Invoke(nameof(FindCP), 3f);
            }

        Vector2 moveDir = closestP.position - transform.position;

        rb.velocity = moveDir.normalized * moveSpeed * Time.fixedDeltaTime;

    }

    private Transform FindClosestPlayer(Collider2D[] all)
    {
        if(all.Length == 0) return null;

        Transform closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (Collider2D w in all)
        {
            Vector3 diff = w.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance && w.GetComponent<Player>().isAlive)
            {
                closest = w.transform;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void FindCP()
    {
        Collider2D[] all = Physics2D.OverlapCircleAll(transform.position, findRadius, whatArePlayers);

            //print(all.Length);

            closestP = FindClosestPlayer(all);

        if(following) Invoke(nameof(FindCP), 3f);
    }

    public void ReduceHealth(float damage)
    {
        health -= damage;

        if(health <= 0)
        {
            //Kill enemey
            EnemyManager.Singleton.KillEnemy(Id);

            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.enemyKill);
            message.AddUShort(Id);

            NetworkManager.Singleton.Server.SendToAll(message);

            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position, findRadius);
    }

    public Transform GetClosestP() => closestP;

    private void ResetHit() => canHit = true;
}
