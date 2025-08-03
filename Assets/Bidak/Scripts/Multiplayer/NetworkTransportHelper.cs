using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using UnityEngine;

namespace Bidak.Multiplayer
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkTransportHelper : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private string ipAddress = "127.0.0.1";
        [SerializeField] private ushort port = 7777;

        private UnityTransport transport;
        private NetworkManager networkManager;

        private void Awake()
        {
            // Get references
            networkManager = GetComponent<NetworkManager>();
            transport = GetComponent<UnityTransport>();

            // If transport is not set, add it
            if (transport == null)
            {
                transport = gameObject.AddComponent<UnityTransport>();
            }

            // Configure transport
            ConfigureTransport();

            // Set transport for NetworkManager
            if (networkManager != null)
            {
                networkManager.NetworkConfig.NetworkTransport = transport;
            }
            else
            {
                Debug.LogError("NetworkManager component not found on this GameObject!");
            }

            // Ensure this is the only NetworkManager
            if (FindObjectsOfType<NetworkManager>().Length > 1)
            {
                Debug.LogWarning("Multiple NetworkManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Subscribe to events safely
            SafelySubscribeToEvents();
        }

        private void SafelySubscribeToEvents()
        {
            // Unsubscribe first to prevent multiple subscriptions
            UnsubscribeFromEvents();

            // Subscribe to local NetworkManager events
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback += OnClientConnected;
                networkManager.OnServerStarted += OnServerStarted;
                networkManager.OnClientDisconnectCallback += OnClientDisconnected;
            }
            else
            {
                Debug.LogWarning("Cannot subscribe to NetworkManager events - local NetworkManager is null");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback -= OnClientConnected;
                networkManager.OnServerStarted -= OnServerStarted;
                networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnEnable()
        {
            SafelySubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void ConfigureTransport()
        {
            if (transport == null) return;

            // Configure connection parameters
            transport.SetConnectionData(
                ipAddress,   // IP Address
                port,        // Port
                "0.0.0.0"    // Listen Address (0.0.0.0 means listen on all network interfaces)
            );

            Debug.Log($"Network Transport Configured: {ipAddress}:{port}");
        }

        // Method to change connection settings at runtime
        public void SetConnectionData(string newIpAddress, ushort newPort)
        {
            ipAddress = newIpAddress;
            port = newPort;
            ConfigureTransport();
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client connected: {clientId}");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client disconnected: {clientId}");
        }

        private void OnServerStarted()
        {
            Debug.Log("Server started successfully");
        }
    }
}