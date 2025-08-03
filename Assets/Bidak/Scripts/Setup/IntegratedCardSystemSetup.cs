using UnityEngine;
using UnityEngine.EventSystems;
using Bidak.Manager;

namespace Bidak.Setup
{
    /// <summary>
    /// Helper script to automatically setup the integrated card system
    /// Attach this to any GameObject and run Setup() to configure the scene
    /// </summary>
    public class IntegratedCardSystemSetup : MonoBehaviour
    {
        [Header("Auto Setup Options")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool createMissingComponents = true;
        [SerializeField] private bool validateExistingSetup = true;

        [Header("Camera References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera player1Camera;
        [SerializeField] private Camera player2Camera;

        [Header("Setup Results")]
        [SerializeField] private bool setupComplete = false;
        [SerializeField] private string[] setupMessages;

        private void Start()
        {
            if (setupOnStart)
            {
                SetupIntegratedCardSystem();
            }
        }

        [ContextMenu("Setup Integrated Card System")]
        public void SetupIntegratedCardSystem()
        {
            Debug.Log("=== Setting up Integrated Card System ===");
            
            var messages = new System.Collections.Generic.List<string>();
            
            try
            {
                // 1. Setup Event System
                SetupEventSystem(messages);
                
                // 2. Setup Cameras
                SetupCameras(messages);
                
                // 3. Setup Card Manager
                SetupCardManager(messages);
                
                // 4. Setup Targeting System
                SetupTargetingSystem(messages);
                
                // 5. Setup Camera Switch
                SetupCameraSwitch(messages);
                
                // 6. Setup Card Effect Manager
                SetupCardEffectManager(messages);
                
                // 7. Validate Setup
                if (validateExistingSetup)
                {
                    ValidateSetup(messages);
                }
                
                setupComplete = true;
                setupMessages = messages.ToArray();
                
                Debug.Log("=== Integrated Card System Setup Complete ===");
                foreach (var message in messages)
                {
                    Debug.Log($"✓ {message}");
                }
            }
            catch (System.Exception e)
            {
                setupComplete = false;
                messages.Add($"Setup failed: {e.Message}");
                setupMessages = messages.ToArray();
                Debug.LogError($"Setup failed: {e.Message}");
            }
        }

        private void SetupEventSystem(System.Collections.Generic.List<string> messages)
        {
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null && createMissingComponents)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystem = eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
                messages.Add("Created EventSystem");
            }
            else if (eventSystem != null)
            {
                messages.Add("EventSystem found");
            }
            else
            {
                messages.Add("EventSystem missing - manual setup required");
            }
        }

        private void SetupCameras(System.Collections.Generic.List<string> messages)
        {
            // Find main cameras (multiple main cameras for each player)
            GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
            
            // Find card cameras  
            if (player1Camera == null)
            {
                GameObject player1CameraObj = GameObject.FindGameObjectWithTag("Player1Camera");
                if (player1CameraObj != null)
                {
                    player1Camera = player1CameraObj.GetComponent<Camera>();
                }
            }

            if (player2Camera == null)
            {
                GameObject player2CameraObj = GameObject.FindGameObjectWithTag("Player2Camera");
                if (player2CameraObj != null)
                {
                    player2Camera = player2CameraObj.GetComponent<Camera>();
                }
            }

            // Add Physics Raycasters to all main cameras
            foreach (GameObject mainCameraObj in mainCameras)
            {
                Camera cam = mainCameraObj.GetComponent<Camera>();
                if (cam != null && cam.GetComponent<PhysicsRaycaster>() == null)
                {
                    cam.gameObject.AddComponent<PhysicsRaycaster>();
                    messages.Add($"Added PhysicsRaycaster to Main Camera: {mainCameraObj.name}");
                }
            }

            // Add Physics Raycasters to card cameras
            if (player1Camera != null)
            {
                if (player1Camera.GetComponent<PhysicsRaycaster>() == null)
                {
                    player1Camera.gameObject.AddComponent<PhysicsRaycaster>();
                    messages.Add("Added PhysicsRaycaster to Player1 Card Camera");
                }
            }

            if (player2Camera != null)
            {
                if (player2Camera.GetComponent<PhysicsRaycaster>() == null)
                {
                    player2Camera.gameObject.AddComponent<PhysicsRaycaster>();
                    messages.Add("Added PhysicsRaycaster to Player2 Card Camera");
                }
            }

            messages.Add($"Camera setup: MainCameras={mainCameras.Length}, P1Card={player1Camera != null}, P2Card={player2Camera != null}");
        }

        private void SetupCardManager(System.Collections.Generic.List<string> messages)
        {
            ChessCardManager cardManager = FindObjectOfType<ChessCardManager>();
            if (cardManager == null && createMissingComponents)
            {
                GameObject cardManagerObj = new GameObject("ChessCardManager");
                cardManager = cardManagerObj.AddComponent<ChessCardManager>();
                messages.Add("Created ChessCardManager");
            }
            else if (cardManager != null)
            {
                messages.Add("ChessCardManager found");
            }
            else
            {
                messages.Add("ChessCardManager missing - manual setup required");
            }
        }

        private void SetupTargetingSystem(System.Collections.Generic.List<string> messages)
        {
            CardTargetingSystem targetingSystem = FindObjectOfType<CardTargetingSystem>();
            if (targetingSystem == null && createMissingComponents)
            {
                GameObject targetingObj = new GameObject("CardTargetingSystem");
                targetingSystem = targetingObj.AddComponent<CardTargetingSystem>();
                messages.Add("Created CardTargetingSystem");
            }
            else if (targetingSystem != null)
            {
                messages.Add("CardTargetingSystem found");
            }
            else
            {
                messages.Add("CardTargetingSystem missing - manual setup required");
            }
        }

        private void SetupCameraSwitch(System.Collections.Generic.List<string> messages)
        {
            CameraSwitch cameraSwitch = FindObjectOfType<CameraSwitch>();
            if (cameraSwitch == null && createMissingComponents)
            {
                GameObject cameraSwitchObj = new GameObject("CameraSwitch");
                cameraSwitch = cameraSwitchObj.AddComponent<CameraSwitch>();
                
                // Configure camera switch if cameras are available
                if (mainCamera != null && player1Camera != null)
                {
                    // This would need manual configuration as CameraSwitch has specific setup requirements
                    messages.Add("Created CameraSwitch - manual configuration required");
                }
                else
                {
                    messages.Add("Created CameraSwitch");
                }
            }
            else if (cameraSwitch != null)
            {
                messages.Add("CameraSwitch found");
            }
            else
            {
                messages.Add("CameraSwitch missing - manual setup required");
            }
        }

        private void SetupCardEffectManager(System.Collections.Generic.List<string> messages)
        {
            CardEffectManager effectManager = FindObjectOfType<CardEffectManager>();
            if (effectManager == null && createMissingComponents)
            {
                GameObject effectManagerObj = new GameObject("CardEffectManager");
                effectManager = effectManagerObj.AddComponent<CardEffectManager>();
                messages.Add("Created CardEffectManager");
            }
            else if (effectManager != null)
            {
                messages.Add("CardEffectManager found");
            }
            else
            {
                messages.Add("CardEffectManager missing - manual setup required");
            }
        }

        private void ValidateSetup(System.Collections.Generic.List<string> messages)
        {
            messages.Add("--- Validation Results ---");
            
            // Check required components
            bool hasEventSystem = FindObjectOfType<EventSystem>() != null;
            bool hasCardManager = FindObjectOfType<ChessCardManager>() != null;
            bool hasTargetingSystem = FindObjectOfType<CardTargetingSystem>() != null;
            bool hasCameraSwitch = FindObjectOfType<CameraSwitch>() != null;
            bool hasEffectManager = FindObjectOfType<CardEffectManager>() != null;
            
            messages.Add($"EventSystem: {(hasEventSystem ? "✓" : "✗")}");
            messages.Add($"ChessCardManager: {(hasCardManager ? "✓" : "✗")}");
            messages.Add($"CardTargetingSystem: {(hasTargetingSystem ? "✓" : "✗")}");
            messages.Add($"CameraSwitch: {(hasCameraSwitch ? "✓" : "✗")}");
            messages.Add($"CardEffectManager: {(hasEffectManager ? "✓" : "✗")}");
            
            // Check camera setup
            GameObject[] mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
            bool hasMainCameras = mainCameras.Length >= 2; // Need at least 2 main cameras
            bool hasPlayer1Camera = player1Camera != null;
            bool hasPlayer2Camera = player2Camera != null;
            
            messages.Add($"Main Cameras: {(hasMainCameras ? "✓" : "✗")} (Found: {mainCameras.Length}/2)");
            messages.Add($"Player1 Card Camera: {(hasPlayer1Camera ? "✓" : "✗")}");
            messages.Add($"Player2 Card Camera: {(hasPlayer2Camera ? "✓" : "✗")}");
            
            // Overall validation
            bool setupValid = hasEventSystem && hasCardManager && hasTargetingSystem && 
                            hasCameraSwitch && hasEffectManager && hasMainCameras;
            
            messages.Add($"Overall Setup: {(setupValid ? "✓ VALID" : "✗ INCOMPLETE")}");
        }

        [ContextMenu("Validate Current Setup")]
        public void ValidateCurrentSetup()
        {
            var messages = new System.Collections.Generic.List<string>();
            ValidateSetup(messages);
            setupMessages = messages.ToArray();
            
            foreach (var message in messages)
            {
                Debug.Log(message);
            }
        }

        [ContextMenu("Clear Setup")]
        public void ClearSetup()
        {
            setupComplete = false;
            setupMessages = new string[0];
            Debug.Log("Setup status cleared");
        }

        // GUI for editor inspection
        private void OnValidate()
        {
            if (setupMessages == null)
                setupMessages = new string[0];
        }
    }
}