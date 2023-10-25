using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    protected float damage, attackCooldown, totalAttackCooldown;

    public ushort Id;

    protected Rigidbody2D rb;

    [SerializeField]
    protected bool canAttack;

    protected int weaponType; //Same as slot type. 0=Ranged 1=Melee 2=Throwable
    
    private void Awake()
    {
        gameObject.tag = "Weapon";

        gameObject.layer = 7; //Weapon Layer

        rb = GetComponent<Rigidbody2D>();
    }


    public void Equipt()
    {
        gameObject.tag = "EquiptWeapon";

        canAttack = true;

        //coll.enabled = false;

        //rb.isKinematic = true;
    }
    public void Dropped()
    {
        gameObject.tag = "Weapon";

        canAttack = false;

        rb.isKinematic = false;
    }

    public void SetId(ushort id) => Id = id;

    public void SwitchShooting(bool newPol) => canAttack = newPol;

    public int GetWeaponType()
    {
        return weaponType;
    }

    public abstract void Attack();

    public abstract void TryToAttack();
}
