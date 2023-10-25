using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedWeapon : Weapon
{
    [SerializeField]
    private GameObject bullet;

    [SerializeField]
    private Transform bulletSpawn;

    [SerializeField]
    private float numberOfshots, timeInbetweenBullets, spread, totalTravelTime, shootPower;

    private float curShots;
    [SerializeField]
    private bool shouldInvoke = true, finishedShooting = true;

    private void Awake()
    {
        weaponType = 0;


    }


    public override void Attack()
    {
        finishedShooting = false;

        curShots --;

        GameObject curBullet = Instantiate(bullet, bulletSpawn.position, bulletSpawn.rotation);

        float ySpread = Random.Range(-spread, spread);

        Rigidbody2D bulletRb = curBullet.GetComponent<Rigidbody2D>(); 

        bulletRb.velocity = bulletSpawn.right * shootPower;//Its right as forward makes it go into z direction

        bulletRb.velocity = new Vector2(bulletRb.velocity.x, bulletRb.velocity.y + ySpread * 3);

        Bullet newBullet = curBullet.GetComponent<Bullet>();

        ushort bulletId = BulletManager.AddBullet(curBullet);

        newBullet.CreateBullet(bulletId, damage);

        BulletManager.SendBulletToAll(bulletId, curBullet.transform.position, 0);

        attackCooldown = totalAttackCooldown;

        if(curShots > 0 && canAttack)
        {
            StartCoroutine(nameof(WaitForNextShot));
        }
        else
        {
            StopCoroutine(nameof(WaitForNextShot));
            finishedShooting = true;
        }

        if(shouldInvoke)
        {
            shouldInvoke = false;
            attackCooldown = totalAttackCooldown;
        }
    }

    private IEnumerator WaitForNextShot()
    {
        yield return new WaitForSeconds(timeInbetweenBullets);
        Attack();
    }
    public override void TryToAttack()
    {
        if(!canAttack) return;   //FIX THIS SOON.

        //print("Here");


        if(attackCooldown <= 0 && finishedShooting)
        {
            shouldInvoke = true;
            curShots = numberOfshots;

            Attack();
        }

        TickTimer();
    }

    private void OnDisable()
    {
        StopCoroutine(nameof(WaitForNextShot));
        finishedShooting = true;
    }
    private void TickTimer()
    {
        attackCooldown -= Time.fixedDeltaTime;
    }
}
