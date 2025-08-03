using Unity.Netcode;
using UnityEngine;

namespace Bidak.Multiplayer
{
    [RequireComponent(typeof(NetworkManager))]
    public class MultiplayerTestHelper : MonoBehaviour
    {
        [Header("Multiplayer Test Settings")]
        [SerializeField] private bool startAsHost = false;
        [SerializeField] private bool startAsClient = false;
        [SerializeField] private bool startAsServer = false;

        [SerializeField] private float autoStartDelay = 0.5f;

        private NetworkManager networkManager;

        private void Awake()
        {
            // Ensure NetworkManager component is referenced
            networkManager = GetComponent<NetworkManager>();

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
            // Validate NetworkManager
            if (networkManager == null)
            {
                Debug.LogError("NetworkManager component is missing!");
                return;
            }

            // Invoke auto-start with a small delay to ensure everything is initialized
            if (startAsHost)
                Invoke(nameof(StartHost), autoStartDelay);
            else if (startAsClient)
                Invoke(nameof(StartClient), autoStartDelay);
            else if (startAsServer)
                Invoke(nameof(StartServer), autoStartDelay);
        }

        private void StartHost()
        {
            try
            {
                Debug.Log("Attempting to start as Host");
                
                // Use local NetworkManager to start host
                networkManager.StartHost();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start host: {e.Message}");
            }
        }

        private void StartClient()
        {
            try
            {
                Debug.Log("Attempting to start as Client");
                
                // Use local NetworkManager to start client
                networkManager.StartClient();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start client: {e.Message}");
            }
        }

        private void StartServer()
        {
            try
            {
                Debug.Log("Attempting to start as Server");
                
                // Use local NetworkManager to start server
                networkManager.StartServer();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to start server: {e.Message}");
            }
        }

        // Optional: Method to switch modes during runtime
        public void SwitchToHost()
        {
            if (networkManager != null && networkManager.IsClient)
                networkManager.StartHost();
        }

        public void SwitchToClient()
        {
            if (networkManager != null && networkManager.IsHost)
                networkManager.StartClient();
        }
    }
}