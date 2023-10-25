using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private static BulletManager _singleton;

    public static BulletManager Singleton
    {
        get => _singleton;

        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if(_singleton != value)
            {
                Debug.Log($"{nameof(BulletManager)} instance already exists");
                Destroy(value);
            }
        }
    }

    private static ushort currentB = 0;

    public static Dictionary<ushort, GameObject> activeBullets = new Dictionary<ushort, GameObject>();

    private void Awake()
    {
        Singleton = this;
    }

    public static ushort AddBullet(GameObject newBullet)
    {
        activeBullets.Add(currentB, newBullet);

        ushort retuenV = currentB;

        currentB ++;

        return retuenV;
    }

    public void RemoveBullet(ushort id)
    {
        activeBullets.Remove(id);
    }

    public static void SendBulletToAll(ushort Id, Vector2 position, ushort bulletType)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.bulletSpawn);

        message.AddUShort(Id);

        message.AddUShort(bulletType);

        message.AddVector2(position);

        NetworkManager.Singleton.Server.SendToAll(message);
    }


}
