using UnityEngine;
using Bidak.Manager;
using Bidak.Data;

namespace Bidak.Card
{
    /// <summary>
    /// Coordinates integration between card system, tile system, and piece movement
    /// Ensures all systems communicate properly and effects are applied correctly
    /// </summary>
    public class CardIntegrationManager : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private GameManagerChess gameManager;
        [SerializeField] private ChessCardManager cardManager;
        [SerializeField] private CardTargetingSystem targetingSystem;
        [SerializeField] private CardEffectProcessor effectProcessor;
        [SerializeField] private TileManager tileManager;
        
        [Header("Integration Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool autoFindReferences = true;
        
        // Integration state
        private bool isInitialized = false;
        
        private void Start()
        {
            InitializeIntegration();
        }
        
        /// <summary>
        /// Initialize all system references and set up integration
        /// </summary>
        private void InitializeIntegration()
        {
            if (autoFindReferences)
            {
                FindSystemReferences();
            }
            
            ValidateSystemReferences();
            SetupEventSubscriptions();
            
            isInitialized = true;
            LogDebug("Card Integration Manager initialized successfully");
        }
        
        /// <summary>
        /// Automatically find all required system references
        /// </summary>
        private void FindSystemReferences()
        {
            if (gameManager == null)
                gameManager = GameManagerChess.Instance;
                
            if (cardManager == null)
                cardManager = FindObjectOfType<ChessCardManager>();
                
            if (targetingSystem == null)
                targetingSystem = FindObjectOfType<CardTargetingSystem>();
                
            if (effectProcessor == null)
                effectProcessor = FindObjectOfType<CardEffectProcessor>();
                
            if (tileManager == null)
                tileManager = FindObjectOfType<TileManager>();
        }
        
        /// <summary>
        /// Validate that all required references are found
        /// </summary>
        private void ValidateSystemReferences()
        {
            bool allValid = true;
            
            if (gameManager == null)
            {
                LogError("GameManagerChess not found!");
                allValid = false;
            }
            
            if (cardManager == null)
            {
                LogError("ChessCardManager not found!");
                allValid = false;
            }
            
            if (targetingSystem == null)
            {
                LogError("CardTargetingSystem not found!");
                allValid = false;
            }
            
            if (effectProcessor == null)
            {
                LogError("CardEffectProcessor not found!");
                allValid = false;
            }
            
            if (tileManager == null)
            {
                LogError("TileManager not found!");
                allValid = false;
            }
            
            if (!allValid)
            {
                LogError("Card integration cannot proceed - missing required components!");
            }
            else
            {
                LogDebug("All system references validated successfully");
            }
        }
        
        /// <summary>
        /// Set up event subscriptions between systems
        /// </summary>
        private void SetupEventSubscriptions()
        {
            if (gameManager != null)
            {
                GameManagerChess.OnPlayerTurnChanged += OnPlayerTurnChanged;
                LogDebug("Subscribed to player turn change events");
            }
            
            if (cardManager != null)
            {
                cardManager.OnCardActivated += OnCardActivated;
                cardManager.OnCardDeactivated += OnCardDeactivated;
                cardManager.OnCardModeChanged += OnCardModeChanged;
                LogDebug("Subscribed to card manager events");
            }
        }
        
        /// <summary>
        /// Clean up event subscriptions
        /// </summary>
        private void OnDestroy()
        {
            if (gameManager != null)
            {
                GameManagerChess.OnPlayerTurnChanged -= OnPlayerTurnChanged;
            }
            
            if (cardManager != null)
            {
                cardManager.OnCardActivated -= OnCardActivated;
                cardManager.OnCardDeactivated -= OnCardDeactivated;
                cardManager.OnCardModeChanged -= OnCardModeChanged;
            }
        }
        
        #region Event Handlers
        
        /// <summary>
        /// Handle player turn changes
        /// </summary>
        private void OnPlayerTurnChanged(int newPlayerIndex)
        {
            LogDebug($"Player turn changed to: {newPlayerIndex}");
            
            // Update card effects duration
            if (effectProcessor != null)
            {
                // Effect processor handles this internally via its own subscription
                LogDebug("Card effects will be updated by CardEffectProcessor");
            }
            
            // Exit card mode when turn changes to prevent cross-player card usage
            if (cardManager != null && cardManager.IsCardModeActive())
            {
                cardManager.ExitCardMode();
                LogDebug("Exited card mode due to turn change");
            }
        }
        
        /// <summary>
        /// Handle card activation
        /// </summary>
        private void OnCardActivated(int playerIndex, ChessCardData cardData)
        {
            LogDebug($"Card activated: {cardData.cardName} by player {playerIndex}");
            
            // Additional integration logic can be added here
            // For example, updating UI, playing sounds, etc.
        }
        
        /// <summary>
        /// Handle card deactivation
        /// </summary>
        private void OnCardDeactivated(int playerIndex, ChessCardData cardData)
        {
            LogDebug($"Card deactivated: {cardData.cardName} for player {playerIndex}");
            
            // Additional cleanup logic can be added here
        }
        
        /// <summary>
        /// Handle card mode changes
        /// </summary>
        private void OnCardModeChanged(bool isCardModeActive)
        {
            LogDebug($"Card mode changed to: {(isCardModeActive ? "ACTIVE" : "INACTIVE")}");
            
            // Additional mode change logic can be added here
            // For example, updating UI states, changing input handling, etc.
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Check if all systems are properly integrated
        /// </summary>
        public bool IsIntegrationComplete()
        {
            return isInitialized && 
                   gameManager != null && 
                   cardManager != null && 
                   targetingSystem != null && 
                   effectProcessor != null && 
                   tileManager != null;
        }
        
        /// <summary>
        /// Get integration status report
        /// </summary>
        public string GetIntegrationStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine("=== CARD INTEGRATION STATUS ===");
            status.AppendLine($"Initialized: {isInitialized}");
            status.AppendLine($"GameManager: {(gameManager != null ? "✓" : "✗")}");
            status.AppendLine($"CardManager: {(cardManager != null ? "✓" : "✗")}");
            status.AppendLine($"TargetingSystem: {(targetingSystem != null ? "✓" : "✗")}");
            status.AppendLine($"EffectProcessor: {(effectProcessor != null ? "✓" : "✗")}");
            status.AppendLine($"TileManager: {(tileManager != null ? "✓" : "✗")}");
            status.AppendLine($"Overall Status: {(IsIntegrationComplete() ? "READY" : "INCOMPLETE")}");
            status.AppendLine("================================");
            
            return status.ToString();
        }
        
        /// <summary>
        /// Force re-initialization of the integration
        /// </summary>
        public void RefreshIntegration()
        {
            LogDebug("Refreshing card integration...");
            InitializeIntegration();
        }
        
        #endregion
        
        #region Debug Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[CardIntegrationManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[CardIntegrationManager] {message}");
        }
        
        #endregion
        
        #region Context Menu Debug
        
        #if UNITY_EDITOR
        [ContextMenu("Show Integration Status")]
        private void ShowIntegrationStatus()
        {
            Debug.Log(GetIntegrationStatus());
        }
        
        [ContextMenu("Refresh Integration")]
        private void DebugRefreshIntegration()
        {
            RefreshIntegration();
        }
        
        [ContextMenu("Test Card Integration")]
        private void TestCardIntegration()
        {
            Debug.Log("=== TESTING CARD INTEGRATION ===");
            
            if (!IsIntegrationComplete())
            {
                Debug.LogError("Integration not complete - cannot run test");
                return;
            }
            
            // Test basic functionality
            Debug.Log("✓ All systems found");
            Debug.Log("✓ Event subscriptions active");
            
            if (cardManager != null)
            {
                Debug.Log($"✓ Card Manager - Player 1 active cards: {cardManager.GetActiveCards(1).Count}");
                Debug.Log($"✓ Card Manager - Player 2 active cards: {cardManager.GetActiveCards(2).Count}");
            }
            
            if (effectProcessor != null)
            {
                var piecesWithEffects = effectProcessor.GetPiecesWithActiveEffects();
                Debug.Log($"✓ Effect Processor - Pieces with active effects: {piecesWithEffects.Count}");
            }
            
            Debug.Log("=== INTEGRATION TEST COMPLETE ===");
        }
        #endif
        
        #endregion
    }
}