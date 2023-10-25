using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();


    public ushort Id {get; private set;}
    public string Username {get; private set;}

    public PlayerMovement Movement => movement;

    public Transform holderTrans;

    public GunHolder GunHolder => gunHolder;

    public Hotbar HotBar => hotbar;

    [SerializeField] private PlayerMovement movement;

    [SerializeField] private GunHolder gunHolder;

    [SerializeField] private Hotbar hotbar;

    [SerializeField] private Rigidbody2D rb;

    public bool isAlive = true;

    private static int allDead = 0;

    private void Start()
    {
        gunHolder.SetId(Id);
    }

    private void Update()
    {
        Vector2 vel = rb.velocity.normalized;

        ushort g = 5;

        float UD = Vector2.Dot(Vector2.up, vel);

        float LR = Vector2.Dot(Vector2.right, vel);

        if(UD < 0) g = 0;
        if(UD > 0) g = 1;
        if(LR < 0) g = 2;
        if(LR > 0) g = 3;

        if(g != 5)
        {
            Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerFacingDir);

            message.AddUShort(Id);

            message.AddUShort(g);

            NetworkManager.Singleton.Server.SendToAll(message);
        }
    }


    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach(Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? $"Guest {id}" : username;

        player.HotBar.SetGunHolder(player.GunHolder);
    
        player.SendSpawned();
        list.Add(id, player);
    }

    public void KillSelf()
    {
        isAlive = false;

        allDead += 1;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.killPlayer);

        message.AddUShort(Id);

        NetworkManager.Singleton.Server.SendToAll(message);

        if(allDead >= list.Count) //GameOver
        {
            GameManager.Singleton.EndGame();
        }
    }

    public static void ResetDead()
    {
        foreach (Player p in list.Values)
        {
            if(!p.isAlive)
            {
                p.isAlive = true;

                p.transform.position = new Vector2(0, 2);

                Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.respawnPlayer);

                message.AddUShort(p.Id);

                NetworkManager.Singleton.Server.SendToAll(message);
            }
        }

        allDead = 0;
    }

    #region Messages
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message) //How cool is this
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector2(transform.position);
        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.moveInput)]
    private static void Input(ushort fromClientId, Message message)
    {
        if(list.TryGetValue(fromClientId, out Player player))
        {
            player.Movement.SetInputs(message.GetBools(4));
        }
    }

    [MessageHandler((ushort)ClientToServerId.dashing)]
    private static void Dash(ushort fromClientId, Message message)
    {
        if(list.TryGetValue(fromClientId, out Player player))
        {
            player.Movement.AllowDash();
        }
    }

    [MessageHandler((ushort)ClientToServerId.playerAttemptPickUp)]
    private static void AttemptWeaponPickup(ushort fromClientId, Message message)
    {
        if(list.TryGetValue(fromClientId, out Player player))
        {
            if(message.GetBool())
            {
                player.GunHolder.PlayerAttemptPickUp();
            }
        }   
    }

    [MessageHandler((ushort)ClientToServerId.playerRotate)]
    private static void RotateGunHolder(ushort fromClientId, Message message)
    {
        if(list.TryGetValue(fromClientId, out Player player))
        {
            player.holderTrans.rotation = Quaternion.Euler(new Vector3(0f,0f,message.GetFloat()));
        }

    }
    #endregion
}
