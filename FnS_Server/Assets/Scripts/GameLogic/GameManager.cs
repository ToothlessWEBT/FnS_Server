using System.Collections;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _singleton;

    public static GameManager Singleton
    {
        get => _singleton;

        private set
        {
            if(_singleton == null)
                _singleton = value;
            else if(_singleton != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists");
                Destroy(value);
            }
        }
    }
    private bool gameOver = false;

    //private Random rand = new Random();

    [SerializeField] private int currentWave = -1;

    private bool gameStarted = false, stillSummoning = false;

    [SerializeField] private GameObject[] enemySpawnPos;

    public void StartGame()
    {
        Invoke(nameof(GiveRandWeaponAll), 3f);
    }

    private void GiveRandWeaponAll()
    {
        foreach (Player p in Player.list.Values)
        {
            ushort id = (ushort)Random.Range(0, WeaponManager.Singleton.allWeapons.Count);
            WeaponManager.Singleton.SpawnWeapon(id, p.transform.position);
        }
        Invoke(nameof(AllowSpawn), 2f);

        float rTime = Random.Range(10, 40f);

        Invoke(nameof(GiveRandWeapon), rTime);
    }


    private void GiveRandWeapon()
    {
        int pWanted = Random.Range(0, Player.list.Keys.Count);

        Player p = null;

        int i = 0;

        foreach (var f in Player.list.Values)
        {
            if(i == pWanted)
            {
                p = f;
                break;
            }
            i++;
        }

        ushort id = (ushort)Random.Range(0, WeaponManager.Singleton.allWeapons.Count);
        WeaponManager.Singleton.SpawnWeapon(id, p.transform.position);

        float rTime = Random.Range(15, 40f);

        Invoke(nameof(GiveRandWeapon), rTime);
    }

    private void AllowSpawn() =>  gameStarted = true;



    private void Awake()
    {
        Singleton = this;
    }

    private void Update()
    {
        if(!gameStarted || !EnemyManager.Singleton.NoEnemiesLeft() || stillSummoning || gameOver) return;

        currentWave ++;

        print($"Starting Round{currentWave + 1}");

        Player.ResetDead();

        stillSummoning = true;

        StartCoroutine(nameof(SpawnAllEnemies));

        if(currentWave == 2 || currentWave == 5 || currentWave >= 7) GiveRandWeapon();

    }

    private IEnumerator SpawnAllEnemies()
    {
        float maxEnemyToSpawn;

        int allNormEn = NumberOfEnemiesThisTurn();

        int allBosses = NumberOfBossesThisTurn();

        if(currentWave <= 2) maxEnemyToSpawn = 0;
        else if(currentWave <= 5) maxEnemyToSpawn = 2;
        else maxEnemyToSpawn = 3;

        //print("max:" + maxEnemyToSpawn.ToString());

        for (int i = 0; i < allNormEn; i++)
        {
            ushort currenteSID = (ushort)Mathf.FloorToInt(Random.Range(0, maxEnemyToSpawn));

            int spawnPosId = Random.Range(0, enemySpawnPos.Length-1);

            Vector2 spawnPosition = enemySpawnPos[spawnPosId].transform.position;

            EnemyManager.Singleton.SpawnEnemy(currenteSID, spawnPosition);

            yield return new WaitForSeconds(0.4f);
        }

        float maxBToSpawn;

        if(currentWave <= 4) maxBToSpawn = 0;
        else maxBToSpawn = 2;

        for (int i = 0; i < allBosses; i++)
        {
            ushort currenteSID = (ushort)Mathf.FloorToInt(Random.Range(0, maxBToSpawn));

            int spawnPosId = Random.Range(0, enemySpawnPos.Length-1);

            Vector2 spawnPosition = enemySpawnPos[spawnPosId].transform.position;

            EnemyManager.Singleton.SpawnBoss(currenteSID, spawnPosition);

            yield return new WaitForSeconds(0.4f);
        }

        stillSummoning = false;
    }

    private int NumberOfEnemiesThisTurn()
    {
        float y = Mathf.Pow(1.4f, currentWave) + 2;

        int yInt = Mathf.FloorToInt(y);

        return yInt < 28? yInt: 28;
    } 

    private int NumberOfBossesThisTurn()
    {
        if(currentWave <= 6.1f)
        {
            float xValue = 0.6f * currentWave - 1.8f;

            float Y = Mathf.Pow(xValue, 3) + 2.4f;

            int yInt = Mathf.FloorToInt(Y);

            return yInt;
        }

        float y = 0.4f * currentWave + 2.8f;

        return Mathf.FloorToInt(y);
    }

    public void EndGame()
    {
       Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.gameOver);

       NetworkManager.Singleton.Server.SendToAll(message);

       //NetworkManager.Singleton.StopServer();
    }
}
