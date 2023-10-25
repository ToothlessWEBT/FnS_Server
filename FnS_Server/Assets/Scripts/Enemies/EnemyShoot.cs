using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    public bool canShoot = true;

    [SerializeField] private Enemy enemy;

    [SerializeField] private GameObject bullet;

    [SerializeField] private ushort bulletType = 1;

    [SerializeField] private float maxShootDistance, spread, fireSpeed, firefireSpeed, damage;

    private void FixedUpdate()
    {
        if(canShoot) AttemptToFire();
    }

    private void AttemptToFire()
    {
        Transform cPlayer = enemy.GetClosestP();

        if(cPlayer == null) return;

        Vector2 dir = cPlayer.position - transform.position;

        float distance = (dir).magnitude;

        if(distance <= maxShootDistance)
        {
            GameObject currentBullet = Instantiate(bullet, transform.position, Quaternion.identity);


            Rigidbody2D rb = currentBullet.GetComponent<Rigidbody2D>();

            rb.velocity = dir * fireSpeed;

            float ySpread = Random.Range(-spread, spread);

            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + ySpread * 3);

            EBullet eBullet = currentBullet.GetComponent<EBullet>();

            ushort bulletId = BulletManager.AddBullet(currentBullet);

            eBullet.CreateBullet(bulletId, damage);

            BulletManager.SendBulletToAll(bulletId, currentBullet.transform.position, bulletType);

            canShoot = false;

            Invoke(nameof(ResetShot), firefireSpeed);

        }
    }

    private void ResetShot() => canShoot = true;
}
