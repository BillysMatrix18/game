using System.Collections.Generic;
using UnityEngine;

namespace StarboundSprint.Systems
{
    public class LocalMultiplayerManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField, Range(1, 4)] private int activePlayers = 1;

        private readonly List<GameObject> _players = new();

        private void Start()
        {
            SpawnPlayers(activePlayers);
        }

        public void SpawnPlayers(int count)
        {
            count = Mathf.Clamp(count, 1, 4);

            for (int i = 0; i < count; i++)
            {
                Transform spawn = spawnPoints[Mathf.Min(i, spawnPoints.Count - 1)];
                GameObject player = Instantiate(playerPrefab, spawn.position, Quaternion.identity);
                player.name = $"Player_{i + 1}";
                _players.Add(player);
            }
        }
    }
}
