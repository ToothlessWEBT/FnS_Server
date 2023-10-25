using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    List<GameObject> playersInArea = new List<GameObject>();

    [SerializeField] private GameObject walls;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag != "Player")return;
        playersInArea.Add(other.gameObject);

        if(playersInArea.Count == Player.list.Count)
        {
            Invoke(nameof(StartG), 3f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag != "Player")return;

        playersInArea.Remove(other.gameObject);

        CancelInvoke(nameof(StartG));
    }

    private void StartG()
    {
        if(playersInArea.Count != Player.list.Count) return;

        walls.SetActive(false);

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.startGame);

        NetworkManager.Singleton.Server.SendToAll(message);

        print("Game Starting");

        GameManager.Singleton.StartGame();

        gameObject.SetActive(false);
    }
}
