using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _singleton;

    public static EnemyManager Singleton
    {
        get => _singleton;

        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if(_singleton != value)
            {
                Debug.Log($"{nameof(EnemyManager)} instance already exists");
                Destroy(value);
            }
        }
    }

    public GameObject[] allEnemies;

    public GameObject[] allBosses;

    private Dictionary<ushort, GameObject> activeEnemies = new Dictionary<ushort, GameObject>();

    private ushort nextEnemyIndex = 0;



    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.H)) //Used for testing SpawnFunction
        {
            SpawnEnemy(0, new Vector2(5,7));
        }
    }


    public void SpawnEnemy(ushort type, Vector2 position)
    {
        GameObject newEnemy = Instantiate(allEnemies[type], position, Quaternion.identity);

        activeEnemies.Add(nextEnemyIndex, newEnemy);

        newEnemy.GetComponent<Enemy>().SetId(nextEnemyIndex);


        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.enemySpawned);
        message.AddUShort(0);
        message.AddUShort(type);
        message.AddVector2(position);
        message.AddUShort(nextEnemyIndex);
        

        NetworkManager.Singleton.Server.SendToAll(message);

        nextEnemyIndex++;
    }
    public void SpawnBoss(ushort type, Vector2 position)
    {
        GameObject newEnemy = Instantiate(allBosses[type], position, Quaternion.identity);

        activeEnemies.Add(nextEnemyIndex, newEnemy);

        newEnemy.GetComponent<Enemy>().SetId(nextEnemyIndex);


        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.enemySpawned);
        message.AddUShort(1);
        message.AddUShort(type);
        message.AddVector2(position);
        message.AddUShort(nextEnemyIndex);

        NetworkManager.Singleton.Server.SendToAll(message);

        nextEnemyIndex++;
    }

    public void KillEnemy(ushort id) => activeEnemies.Remove(id);

    public bool NoEnemiesLeft() => activeEnemies.Count <= 0;

}
