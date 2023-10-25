using System.Linq;
using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class GunHolder : MonoBehaviour
{
    private List<WeaponSlot> allSlots = new List<WeaponSlot> {new RangedSlot(), new RangedSlot(), new MeleeSlot(), new ThrowableSlot()};

    [SerializeField] private Transform[] allSlotTrans;  

    public ushort activeSlot;
    private Transform closestWeapon;

    private bool pickupCooldown = false;

    [SerializeField] private float maxReach = 7f;

    [SerializeField] private LayerMask whatAreWeapons;

    public ushort ourId;

    private Camera playerCamera;

    private void Awake()
    {

       for (int i = 0; i < allSlotTrans.Length; i++) // Gives every slot a transform
       {
           allSlots[i].SetSlotTrans(allSlotTrans[i]);
       }
    }

    private void Start()
    {
        //playerCamera = CameraManager.Instance.GetCamera();   

        activeSlot = 0;
    }

    public void SwitchSlots(ushort newActiceSlot)
    {
        activeSlot = newActiceSlot;

        for (var i = 0; i < allSlots.Count; i++)
        {
            allSlotTrans[i].gameObject.SetActive(i == activeSlot);
        }
    }

    public void PlayerAttemptPickUp()
    {
        if(pickupCooldown || !Player.list[ourId].isAlive) return;
        print("Trying");
        closestWeapon = FindClosestWeapon();
            
        if(closestWeapon == null) return;
        
        ushort index = PickUp();

        if(index == 100) return;

        pickupCooldown = true;

        Invoke(nameof(ResetPickup), 0.5f);


        SendPickUpToAll(closestWeapon.GetComponent<Weapon>(), index);
    }

    private Transform FindClosestWeapon() //Finds all weapons in radius and then loops through to find closest. null returned if non is found
    {
        
        Collider2D[] weapons = Physics2D.OverlapCircleAll(transform.position, maxReach, whatAreWeapons);

        //print(weapons.Length);

        if(weapons.Length == 0) return null;

        Transform closest = null;
        float distance = maxReach;
        Vector3 position = transform.position;
        foreach (Collider2D w in weapons)
        {
            if(w.transform.tag != "Weapon") continue;

            Vector3 diff = w.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = w.transform;
                distance = curDistance;
            }
        }

        return closest;
    }

    private ushort PickUp()
    {
        print($"Picking up {closestWeapon.name}!");

        Weapon cWeapon = closestWeapon.GetComponent<Weapon>();

        ushort i = 0;

        foreach (WeaponSlot slot in allSlots) //loops through all slots and checks if weapon can be picked up. If it can then it is.
        {
            if(slot.GetSlotType() == cWeapon.GetWeaponType() && !slot.IsFull())
            {
                slot.Equipt(cWeapon);
                return i;
            }
            i++;
        }
        return 100;
    }

    public void Attack()
    {
        if(allSlots[activeSlot].IsFull())
            allSlots[activeSlot].GetWeapon().TryToAttack();
    }

    private void SendPickUpToAll(Weapon weapon, ushort pickUPIndex)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.weaponPickedUp);

        message.AddUShort(ourId);

        message.AddUShort(pickUPIndex);

        print(pickUPIndex);

        ushort iNdex = weapon.Id;

        print(iNdex);

        message.AddUShort(iNdex);

        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void ResetPickup() => pickupCooldown = false;

    public void SetId(ushort i) => ourId = i;
   
    public Transform[] GetAllSlots() => allSlotTrans;

    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawSphere(transform.position, maxReach);
    }
}
