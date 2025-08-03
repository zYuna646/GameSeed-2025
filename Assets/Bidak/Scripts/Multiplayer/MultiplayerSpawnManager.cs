using Unity.Netcode;
using UnityEngine;

namespace Bidak.Multiplayer
{
    public class MultiplayerSpawnManager : NetworkBehaviour
    {
        [Header("Player Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform[] playerSpawnPoints;

        [Header("Game Setup")]
        [SerializeField] private int maxPlayers = 2;

        // Singleton-like instance for easy access
        public static MultiplayerSpawnManager Instance { get; private set; }

        private void Awake()
        {
            // Ensure only one instance exists
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            // Only the server should handle player spawning
            if (!IsServer) return;

            // Subscribe to player connection events
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayerForClient;
        }

        private void SpawnPlayerForClient(ulong clientId)
        {
            // Prevent spawning more players than max allowed
            if (NetworkManager.Singleton.ConnectedClients.Count > maxPlayers)
            {
                Debug.LogWarning($"Max players ({maxPlayers}) reached. Rejecting connection.");
                NetworkManager.Singleton.DisconnectClient(clientId);
                return;
            }

            // Select spawn point (round-robin)
            Transform spawnPoint = playerSpawnPoints[NetworkManager.Singleton.ConnectedClients.Count % playerSpawnPoints.Length];

            // Spawn player prefab
            GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Spawn the player network object
            NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            Debug.Log($"Spawned player for client {clientId}");
        }

        public override void OnNetworkDespawn()
        {
            // Unsubscribe from events to prevent memory leaks
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayerForClient;
            }
        }

        // Method to get spawn point for a specific player index
        public Transform GetSpawnPoint(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= playerSpawnPoints.Length)
            {
                Debug.LogWarning($"Invalid player index {playerIndex}. Using default spawn point.");
                return playerSpawnPoints[0];
            }
            return playerSpawnPoints[playerIndex];
        }
    }
}