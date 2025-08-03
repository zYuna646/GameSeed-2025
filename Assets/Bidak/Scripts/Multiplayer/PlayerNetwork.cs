using Unity.Netcode;
using UnityEngine;

namespace Bidak.Multiplayer
{
    public class PlayerNetwork : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 5f;
        [SerializeField] private LayerMask interactableLayer;

        // Network synchronized variables
        private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
        private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();
        private NetworkVariable<int> playerIndex = new NetworkVariable<int>();

        // Camera for raycasting
        private Camera playerCamera;

        private void Start()
        {
            // Get the player's camera
            playerCamera = Camera.main;
        }

        private void Update()
        {
            // Only allow input for the local player
            if (!IsOwner) return;

            // Handle movement
            HandleMovement();

            // Handle interactions
            HandleMouseInteractions();
        }

        private void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            
            // WASD Movement
            if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
            if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
            if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

            // Move the player
            if (moveDirection != Vector3.zero)
            {
                transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
                
                // Rotate towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Sync position and rotation to server
                UpdateTransformServerRpc(transform.position, transform.rotation);
            }
        }

        private void HandleMouseInteractions()
        {
            // Right-click interactions
            if (Input.GetMouseButtonDown(1))
            {
                // Check for Ctrl modifier
                bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                
                // Perform right-click interaction
                PerformRightClickInteractionServerRpc(isCtrlHeld);
            }

            // Left-click interactions
            if (Input.GetMouseButtonDown(0))
            {
                // Check for Ctrl modifier
                bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                
                // Perform left-click interaction
                PerformLeftClickInteractionServerRpc(isCtrlHeld);
            }
        }

        [ServerRpc]
        private void UpdateTransformServerRpc(Vector3 newPosition, Quaternion newRotation)
        {
            // Update network variables
            networkPosition.Value = newPosition;
            networkRotation.Value = newRotation;
        }

        [ServerRpc]
        private void PerformRightClickInteractionServerRpc(bool isCtrlHeld)
        {
            // Create a ray from the camera through the mouse position
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform raycast
            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                // Get the NetworkObject's ID for serialization
                ulong networkObjectId = hit.collider.gameObject.GetComponent<NetworkObject>()?.NetworkObjectId ?? 0;

                // Handle right-click interaction
                if (isCtrlHeld)
                {
                    // Ctrl + Right Click logic
                    HandleCtrlRightClickInteractionClientRpc(networkObjectId);
                }
                else
                {
                    // Regular Right Click logic
                    HandleRightClickInteractionClientRpc(networkObjectId);
                }
            }
        }

        [ServerRpc]
        private void PerformLeftClickInteractionServerRpc(bool isCtrlHeld)
        {
            // Create a ray from the camera through the mouse position
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform raycast
            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                // Get the NetworkObject's ID for serialization
                ulong networkObjectId = hit.collider.gameObject.GetComponent<NetworkObject>()?.NetworkObjectId ?? 0;

                // Handle left-click interaction
                if (isCtrlHeld)
                {
                    // Ctrl + Left Click logic
                    HandleCtrlLeftClickInteractionClientRpc(networkObjectId);
                }
                else
                {
                    // Regular Left Click logic
                    HandleLeftClickInteractionClientRpc(networkObjectId);
                }
            }
        }

        [ClientRpc]
        private void HandleRightClickInteractionClientRpc(ulong targetObjectId)
        {
            // Retrieve the NetworkObject using the ID
            NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject obj) ? obj : null;

            // Logic for standard right-click interaction
            Debug.Log($"Player {playerIndex.Value} performed right-click on {targetObject?.name ?? "null object"}");
        }

        [ClientRpc]
        private void HandleCtrlRightClickInteractionClientRpc(ulong targetObjectId)
        {
            // Retrieve the NetworkObject using the ID
            NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject obj) ? obj : null;

            // Logic for Ctrl + right-click interaction
            Debug.Log($"Player {playerIndex.Value} performed Ctrl + right-click on {targetObject?.name ?? "null object"}");
        }

        [ClientRpc]
        private void HandleLeftClickInteractionClientRpc(ulong targetObjectId)
        {
            // Retrieve the NetworkObject using the ID
            NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject obj) ? obj : null;

            // Logic for standard left-click interaction
            Debug.Log($"Player {playerIndex.Value} performed left-click on {targetObject?.name ?? "null object"}");
        }

        [ClientRpc]
        private void HandleCtrlLeftClickInteractionClientRpc(ulong targetObjectId)
        {
            // Retrieve the NetworkObject using the ID
            NetworkObject targetObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject obj) ? obj : null;

            // Logic for Ctrl + left-click interaction
            Debug.Log($"Player {playerIndex.Value} performed Ctrl + left-click on {targetObject?.name ?? "null object"}");
        }

        // Override for player spawn
        public override void OnNetworkSpawn()
        {
            // Set player index based on network object ID
            playerIndex.Value = (int)NetworkObjectId;

            // Log player connection
            if (IsOwner)
            {
                Debug.Log($"Player {playerIndex.Value} connected");
            }
        }

        // Method to get player index
        public int GetPlayerIndex()
        {
            return playerIndex.Value;
        }
    }
}