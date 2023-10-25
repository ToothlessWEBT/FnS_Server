using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private static WeaponManager _singleton;

    public static WeaponManager Singleton
    {
        get => _singleton;

        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if(_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists");
                Destroy(value);
            }
        }
    }

    public List<GameObject> allWeapons = new List<GameObject>();
    public static Dictionary<ushort, GameObject> activeWeaponList = new Dictionary<ushort, GameObject>();

    private ushort added = 0;

    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O)) //Testing weapon spawning
            Invoke(nameof(test), 2f);
    }
    void test()
    {
        SpawnWeapon(0, Vector3.zero);

        Invoke(nameof(test2), 2f);
    }

    void test2()
    {
        SpawnWeapon(1, Vector3.up * 3);
    }

    private void AddWeapon(GameObject newWeapon)
    {
        activeWeaponList.Add(added, newWeapon);
    }

    public void SpawnWeapon(ushort weaponId, Vector3 position)
    {
        GameObject currentWeapon = Instantiate(allWeapons[weaponId], position, Quaternion.identity);

        currentWeapon.GetComponent<Weapon>().Id = added;

        AddWeapon(currentWeapon);

        NetworkManager.Singleton.Server.SendToAll(AddWeaponData(Message.Create(MessageSendMode.reliable, ServerToClientId.weaponSpawn), weaponId, position));

        added ++;
    }

    private Message AddWeaponData(Message message, ushort weaponSId, Vector3 position) //How cool is this
    {
        message.AddUShort(weaponSId);
        message.AddVector3(position);
        message.AddUShort(added);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.switchWeaponSlots)]
    private static void TrySwitchSlot(ushort fromClientId, Message message)
    {
        if(Player.list.TryGetValue(fromClientId, out Player player))
        {
            ushort index = message.GetUShort();
            if(player.GunHolder.activeSlot != index)
            {
                player.GunHolder.SwitchSlots(index);
            }

        //print($"Client{fromClientId} is swiching to {index}");
            Message newMessage = Message.Create(MessageSendMode.reliable, ServerToClientId.swichedSlots);
            newMessage.AddUShort(fromClientId);
            newMessage.AddUShort(index);

            NetworkManager.Singleton.Server.SendToAll(newMessage);
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerShoot)]
    private static void TryShoot(ushort fromClientId, Message message)
    {

        if(Player.list.TryGetValue(fromClientId, out Player player))
        {
            if(message.GetBool())
            {
                //print($"{fromClientId} is trying to fire");

                Player shooter = Player.list[fromClientId];

                shooter.GunHolder.Attack();
            }
        }
    }
}
