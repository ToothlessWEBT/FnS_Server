using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponSlot// : MonoBehaviour
{
    private bool full = false;

    private Weapon weapon;

    protected  static int slotType; //0=Ranged 1=Melee 2=Throwable

    private Transform slotTran;

    public void SetSlotTrans(Transform slotT)
    {
        slotTran = slotT;
    }

    public void Equipt(Weapon weaponNew)//theheheheheheheheheh2
    {
        full = true;
        weapon = weaponNew;  
        weapon.Equipt();

        weaponNew.transform.SetParent(slotTran);

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        weapon.Dropped();

        full = false;

        weapon.transform.SetParent(null);
    }
    
    public void SwitchShooting(bool newShoot)
    {
        if(weapon != null) weapon.SwitchShooting(newShoot);
    }

    public Weapon GetWeapon() => weapon;

    public bool IsFull()
    {
        return full;   
    }

    public int GetSlotType(){
        /* This returns the slot type. Look at slot type to see logic there */
        return slotType;
    }

}

public class RangedSlot : WeaponSlot
{
    private void Awake()
    {
        slotType = 0;
    }
}

public class MeleeSlot : WeaponSlot
{
    private void Awake()
    {
        slotType = 1;
    }
}

public class ThrowableSlot : WeaponSlot
{
    private void Awake()
    {
        slotType = 2;
    }
}