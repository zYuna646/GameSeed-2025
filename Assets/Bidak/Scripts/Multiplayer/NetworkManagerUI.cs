using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Bidak.Multiplayer
{
    public class NetworkManagerUI : MonoBehaviour
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button serverButton;

        private void Awake()
        {
            // Assign button click listeners
            hostButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartHost();
            });

            clientButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartClient();
            });

            serverButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartServer();
            });
        }

        private void OnEnable()
        {
            // Ensure buttons are only interactable if NetworkManager exists
            if (NetworkManager.Singleton == null)
            {
                hostButton.interactable = false;
                clientButton.interactable = false;
                serverButton.interactable = false;
                Debug.LogWarning("NetworkManager not found in the scene!");
            }
        }
    }
}