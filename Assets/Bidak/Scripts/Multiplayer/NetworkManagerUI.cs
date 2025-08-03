using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Linq;

namespace Bidak.Multiplayer
{
    public class NetworkManagerUI : MonoBehaviour
    {
        [Header("Connection UI Elements")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;

        [Header("IP and Connection Details")]
        [SerializeField] private TMP_InputField ipAddressInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private GameObject joinPanel;
        [SerializeField] private TextMeshProUGUI hostIPDisplayText;

        [Header("Connection Status")]
        [SerializeField] private TextMeshProUGUI connectionStatusText;

        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private UnityTransport transport;

        private void Awake()
        {
            // Ensure NetworkManager and Transport are set up
            SetupNetworkManager();

            // Set default values
            if (portInput != null)
                portInput.text = "7777";

            // Setup initial UI state
            SetupUIState();
        }

        private void SetupUIState()
        {
            // Initially hide both panels
            if (hostPanel != null) hostPanel.SetActive(false);
            if (joinPanel != null) joinPanel.SetActive(false);

            // Setup button listeners
            if (hostButton != null)
                hostButton.onClick.AddListener(ShowHostPanel);

            if (joinButton != null)
                joinButton.onClick.AddListener(ShowJoinPanel);
        }

        private void SetupNetworkManager()
        {
            // Find or setup NetworkManager
            if (networkManager == null)
            {
                GameObject networkManagerObject = GameObject.FindGameObjectWithTag("NetworkManager");
                if (networkManagerObject != null)
                {
                    networkManager = networkManagerObject.GetComponent<NetworkManager>();
                }
            }

            // Fallback to Singleton
            if (networkManager == null)
                networkManager = NetworkManager.Singleton;

            // Find or add UnityTransport
            if (transport == null && networkManager != null)
            {
                transport = networkManager.GetComponent<UnityTransport>();
            }

            // Add transport if missing
            if (transport == null && networkManager != null)
            {
                transport = networkManager.gameObject.AddComponent<UnityTransport>();
            }

            // Ensure transport is set for NetworkManager
            if (networkManager != null && transport != null)
            {
                networkManager.NetworkConfig.NetworkTransport = transport;
            }
            else
            {
                Debug.LogError("Failed to set up NetworkManager or Transport!");
            }
        }

        private void ShowHostPanel()
        {
            // Show host panel, hide join panel
            if (hostPanel != null) hostPanel.SetActive(true);
            if (joinPanel != null) joinPanel.SetActive(false);

            // Setup host button
            Button confirmHostButton = hostPanel.transform.Find("ConfirmHostButton")?.GetComponent<Button>();
            if (confirmHostButton != null)
            {
                confirmHostButton.onClick.RemoveAllListeners();
                confirmHostButton.onClick.AddListener(StartHost);
            }
        }

        private void ShowJoinPanel()
        {
            // Show join panel, hide host panel
            if (hostPanel != null) hostPanel.SetActive(false);
            if (joinPanel != null) joinPanel.SetActive(true);

            // Setup join button
            Button confirmJoinButton = joinPanel.transform.Find("ConfirmJoinButton")?.GetComponent<Button>();
            if (confirmJoinButton != null)
            {
                confirmJoinButton.onClick.RemoveAllListeners();
                confirmJoinButton.onClick.AddListener(StartClient);
            }
        }

        public void StartHost()
        {
            try
            {
                // Ensure NetworkManager is set up
                SetupNetworkManager();

                // Get local IP address
                string localIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ?.ToString() ?? "127.0.0.1";
                
                // Update transport with local settings
                UpdateConnectionSettings(localIP);

                // Start host
                bool success = networkManager.StartHost();
                
                if (success)
                {
                    // Display host IP for other players to join
                    UpdateConnectionStatus($"Host Started. Join IP: {localIP}");
                    
                    // Update IP display text if available
                    if (hostIPDisplayText != null)
                        hostIPDisplayText.text = $"Join IP: {localIP}";
                }
                else
                {
                    UpdateConnectionStatus("Failed to Start Host");
                }
            }
            catch (System.Exception e)
            {
                UpdateConnectionStatus($"Host Start Error: {e.Message}");
            }
        }

        public void StartClient()
        {
            try
            {
                // Ensure NetworkManager is set up
                SetupNetworkManager();

                // Get IP from input field
                string ipAddress = ipAddressInput != null ? ipAddressInput.text : "127.0.0.1";
                
                // Update transport with input IP
                UpdateConnectionSettings(ipAddress);

                // Start client
                bool success = networkManager.StartClient();
                
                if (success)
                {
                    UpdateConnectionStatus($"Connecting to {ipAddress}...");
                }
                else
                {
                    UpdateConnectionStatus("Failed to Connect");
                }
            }
            catch (System.Exception e)
            {
                UpdateConnectionStatus($"Client Connection Error: {e.Message}");
            }
        }

        private void UpdateConnectionSettings(string ipAddress)
        {
            // Ensure transport is available
            if (transport == null)
            {
                Debug.LogWarning("No transport available to update connection settings!");
                return;
            }

            // Get port from input or use default
            ushort port = 7777;
            if (portInput != null && !string.IsNullOrEmpty(portInput.text))
                ushort.TryParse(portInput.text, out port);

            // Set connection data
            transport.SetConnectionData(
                ipAddress,   // IP Address
                port,        // Port
                "0.0.0.0"    // Listen Address
            );

            Debug.Log($"Connection Settings: {ipAddress}:{port}");
        }

        private void UpdateConnectionStatus(string status)
        {
            Debug.Log(status);
            if (connectionStatusText != null)
                connectionStatusText.text = status;
        }
    }
}