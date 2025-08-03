using UnityEngine;
using System;
using System.Collections.Generic;
using Bidak.Data;
using Bidak.Manager;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManagerChess : MonoBehaviour
{
    [Header("Game State")]
    public int currentPlayerIndex = 0; // 0 for Player 1, 1 for Player 2
    
    [Header("Player Configuration")]
    public string player1Name = "Player 1";
    public string player2Name = "Player 2";
    
    [Header("Player Material Settings")]
    public int player1MaterialIndex = 0;
    public int player2MaterialIndex = 1;
    public Color player1Color = Color.white;
    public Color player2Color = Color.black;
    
    [Header("Camera Configuration")]
    public CameraSwitch cameraSwitch;
    
    [Header("Card Management")]
    public ChessCardManager cardManager;
    [SerializeField] private List<ChessCardData> allAvailableCards = new List<ChessCardData>();
    [SerializeField] private int activeCardsPerPlayer = 3;
    [SerializeField] private int cardRedistributionInterval = 5; // Every 5 turns
    private int lastCardRedistributionTurn = 0;
    
    [Header("Turn Management")]
    public int turnCount = 1;
    public float turnTimer = 0f;
    public float maxTurnTime = 30f; // Maximum time per turn (0 = unlimited)
    
    [Header("Game Rules")]
    public bool enableTurnTimer = false;
    public bool autoSwitchTurns = true;
    
    // Events for turn changes
    public static event Action<int> OnPlayerTurnChanged;
    public static event Action<int, string> OnPlayerTurnChangedWithName;
    public static event Action OnGameStarted;
    public static event Action<int> OnGameEnded;
    
    // Singleton pattern
    public static GameManagerChess Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        LoadAllAvailableCards();
        StartGame();
    }
    
    private void Update()
    {
        // Handle turn timer if enabled
        if (enableTurnTimer && maxTurnTime > 0)
        {
            turnTimer += Time.deltaTime;
            
            if (turnTimer >= maxTurnTime)
            {
                // Auto-switch turn when time expires
                SwitchTurn();
            }
        }
    }
    
    /// <summary>
    /// Start a new game
    /// </summary>
    public void StartGame()
    {
        currentPlayerIndex = 0; // Start with Player 1
        turnCount = 1;
        turnTimer = 0f;
        
        Debug.Log($"GAME STARTED! Player {currentPlayerIndex} ({GetCurrentPlayerName()}) starts (currentPlayerIndex = {currentPlayerIndex})");
        
        // Update camera systems for starting player
        UpdateCameraForCurrentPlayer();
        
        // Ensure cards are distributed at game start
        if (allAvailableCards.Count > 0 && cardManager != null)
        {
            DistributeCardsToPlayers();
        }
        
        // Notify listeners
        OnGameStarted?.Invoke();
        OnPlayerTurnChanged?.Invoke(currentPlayerIndex);
        OnPlayerTurnChangedWithName?.Invoke(currentPlayerIndex, GetCurrentPlayerName());
    }
    
    /// <summary>
    /// Switch to the next player's turn
    /// </summary>
    public void SwitchTurn()
    {
        int previousPlayerIndex = currentPlayerIndex;
        
        // Switch player index (0 -> 1, 1 -> 0)
        currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        
        // Increment turn count only when it's Player 1's turn again
        if (currentPlayerIndex == 0)
        {
            turnCount++;
        }
        
        // Reset turn timer
        turnTimer = 0f;
        
        Debug.Log($"TURN SWITCH: Player {previousPlayerIndex} ({GetPlayerName(previousPlayerIndex)}) → Player {currentPlayerIndex} ({GetCurrentPlayerName()})");
        Debug.Log($"Turn {turnCount}: {GetCurrentPlayerName()}'s turn (currentPlayerIndex = {currentPlayerIndex})");
        
        // Update camera systems for current player
        UpdateCameraForCurrentPlayer();
        
        // Check for card redistribution every 5 turns
        CheckCardRedistribution();
        
        // Notify listeners
        OnPlayerTurnChanged?.Invoke(currentPlayerIndex);
        OnPlayerTurnChangedWithName?.Invoke(currentPlayerIndex, GetCurrentPlayerName());
    }
    
    /// <summary>
    /// Get player name by index
    /// </summary>
    public string GetPlayerName(int playerIndex)
    {
        return playerIndex == 0 ? player1Name : player2Name;
    }
    
    /// <summary>
    /// End the game with a winner
    /// </summary>
    public void EndGame(int winnerPlayerIndex)
    {
        string winnerName = winnerPlayerIndex == 0 ? player1Name : player2Name;
        Debug.Log($"Game Ended! {winnerName} wins!");
        
        // Notify listeners
        OnGameEnded?.Invoke(winnerPlayerIndex);
    }
    
    /// <summary>
    /// Get the current player's name
    /// </summary>
    public string GetCurrentPlayerName()
    {
        return currentPlayerIndex == 0 ? player1Name : player2Name;
    }
    
    /// <summary>
    /// Get the other player's name
    /// </summary>
    public string GetOtherPlayerName()
    {
        return currentPlayerIndex == 0 ? player2Name : player1Name;
    }
    
    /// <summary>
    /// Get the other player's index
    /// </summary>
    public int GetOtherPlayerIndex()
    {
        return (currentPlayerIndex + 1) % 2;
    }
    
    /// <summary>
    /// Check if it's a specific player's turn
    /// </summary>
    public bool IsPlayerTurn(int playerIndex)
    {
        return currentPlayerIndex == playerIndex;
    }
    
    /// <summary>
    /// Check if it's Player 1's turn
    /// </summary>
    public bool IsPlayer1Turn()
    {
        return currentPlayerIndex == 0;
    }
    
    /// <summary>
    /// Check if it's Player 2's turn
    /// </summary>
    public bool IsPlayer2Turn()
    {
        return currentPlayerIndex == 1;
    }
    
    /// <summary>
    /// Force set the current player (useful for testing or special rules)
    /// </summary>
    public void SetCurrentPlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 1)
        {
            Debug.LogWarning("Invalid player index. Must be 0 or 1.");
            return;
        }
        
        int previousPlayerIndex = currentPlayerIndex;
        currentPlayerIndex = playerIndex;
        turnTimer = 0f;
        
        Debug.Log($"MANUAL PLAYER SWITCH: Player {previousPlayerIndex} ({GetPlayerName(previousPlayerIndex)}) → Player {currentPlayerIndex} ({GetCurrentPlayerName()})");
        Debug.Log($"Current player index is now: {currentPlayerIndex}");
        
        // Update camera systems for current player
        UpdateCameraForCurrentPlayer();
        
        // Notify listeners
        OnPlayerTurnChanged?.Invoke(currentPlayerIndex);
        OnPlayerTurnChangedWithName?.Invoke(currentPlayerIndex, GetCurrentPlayerName());
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        StartGame();
    }
    
    /// <summary>
    /// Get remaining turn time (if timer is enabled)
    /// </summary>
    public float GetRemainingTurnTime()
    {
        if (!enableTurnTimer || maxTurnTime <= 0)
            return float.MaxValue;
            
        return Mathf.Max(0, maxTurnTime - turnTimer);
    }
    
    /// <summary>
    /// Get turn progress (0 to 1, if timer is enabled)
    /// </summary>
    public float GetTurnProgress()
    {
        if (!enableTurnTimer || maxTurnTime <= 0)
            return 0f;
            
        return Mathf.Clamp01(turnTimer / maxTurnTime);
    }
    
    /// <summary>
    /// Get material index for current player
    /// </summary>
    public int GetCurrentPlayerMaterialIndex()
    {
        return currentPlayerIndex == 0 ? player1MaterialIndex : player2MaterialIndex;
    }
    
    /// <summary>
    /// Get material index for specific player
    /// </summary>
    public int GetPlayerMaterialIndex(int playerIndex)
    {
        return playerIndex == 0 ? player1MaterialIndex : player2MaterialIndex;
    }
    
    /// <summary>
    /// Get color for current player
    /// </summary>
    public Color GetCurrentPlayerColor()
    {
        return currentPlayerIndex == 0 ? player1Color : player2Color;
    }
    
    /// <summary>
    /// Get color for specific player
    /// </summary>
    public Color GetPlayerColor(int playerIndex)
    {
        return playerIndex == 0 ? player1Color : player2Color;
    }
    
    /// <summary>
    /// Update camera systems for the current player
    /// </summary>
    private void UpdateCameraForCurrentPlayer()
    {
        if (cameraSwitch != null)
        {
            cameraSwitch.SetCurrentPlayer(currentPlayerIndex);
        }
    }
    
    /// <summary>
    /// Check if a piece belongs to the current player
    /// </summary>
    public bool IsPieceOwnedByCurrentPlayer(ChessPieceData pieceData)
    {
        if (pieceData == null) return false;
        
        Color currentColor = GetCurrentPlayerColor();
        return pieceData.playerType == currentColor;
    }
    
    /// <summary>
    /// Check if a piece belongs to a specific player
    /// </summary>
    public bool IsPieceOwnedByPlayer(ChessPieceData pieceData, int playerIndex)
    {
        if (pieceData == null) return false;
        
        Color playerColor = GetPlayerColor(playerIndex);
        return pieceData.playerType == playerColor;
    }
    
    #region Card Management System
    
    /// <summary>
    /// Load all available ChessCardData assets from project assets
    /// </summary>
    private void LoadAllAvailableCards()
    {
        allAvailableCards.Clear();
        
        #if UNITY_EDITOR
        // In editor, use AssetDatabase to find all ChessCardData assets
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ChessCardData");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ChessCardData card = UnityEditor.AssetDatabase.LoadAssetAtPath<ChessCardData>(path);
            if (card != null && !allAvailableCards.Contains(card))
            {
                allAvailableCards.Add(card);
            }
        }
        #else
        // In build, try to load from Resources (if cards are moved there)
        ChessCardData[] resourceCards = Resources.LoadAll<ChessCardData>("");
        allAvailableCards.AddRange(resourceCards);
        #endif
        
        Debug.Log($"CARD SYSTEM: Loaded {allAvailableCards.Count} cards from project");
        foreach (var card in allAvailableCards)
        {
            Debug.Log($"  - {card.cardName} ({card.rank})");
        }
        
        // Initial card distribution
        if (allAvailableCards.Count > 0)
        {
            DistributeCardsToPlayers();
        }
        else
        {
            Debug.LogWarning("CARD SYSTEM: No cards found! Please ensure ChessCardData assets exist in the project.");
        }
    }
    
    /// <summary>
    /// Distribute cards randomly to both players
    /// </summary>
    private void DistributeCardsToPlayers()
    {
        if (cardManager == null)
        {
            Debug.LogWarning("CARD SYSTEM: ChessCardManager is null - cannot distribute cards");
            return;
        }
        
        if (allAvailableCards.Count < activeCardsPerPlayer * 2)
        {
            Debug.LogWarning($"CARD SYSTEM: Not enough cards ({allAvailableCards.Count}) for distribution. Need at least {activeCardsPerPlayer * 2}");
            return;
        }
        
        // Clear existing cards
        cardManager.ClearPlayerCards(1);
        cardManager.ClearPlayerCards(2);
        
        // Create a shuffled copy of all cards
        List<ChessCardData> shuffledCards = new List<ChessCardData>(allAvailableCards);
        ShuffleCards(shuffledCards);
        
        // Distribute active cards
        for (int i = 0; i < activeCardsPerPlayer; i++)
        {
            // Player 1 active cards
            cardManager.ActivateCard(1, shuffledCards[i]);
            
            // Player 2 active cards
            cardManager.ActivateCard(2, shuffledCards[i + activeCardsPerPlayer]);
        }
        
        // Put remaining cards in storage (distributed alternately)
        for (int i = activeCardsPerPlayer * 2; i < shuffledCards.Count; i++)
        {
            int playerIndex = (i % 2) + 1; // Alternate between player 1 and 2
            cardManager.AddCardToStorage(playerIndex, shuffledCards[i]);
        }
        
        lastCardRedistributionTurn = turnCount;
        
        Debug.Log($"CARD SYSTEM: Distributed {activeCardsPerPlayer} active cards to each player");
        Debug.Log($"CARD SYSTEM: Player 1 active: {cardManager.GetActiveCards(1).Count}, storage: {cardManager.GetStorageCards(1).Count}");
        Debug.Log($"CARD SYSTEM: Player 2 active: {cardManager.GetActiveCards(2).Count}, storage: {cardManager.GetStorageCards(2).Count}");
    }
    
    /// <summary>
    /// Shuffle a list of cards using Fisher-Yates algorithm
    /// </summary>
    private void ShuffleCards(List<ChessCardData> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            ChessCardData temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// Check if cards should be redistributed based on turn count
    /// </summary>
    private void CheckCardRedistribution()
    {
        if (turnCount - lastCardRedistributionTurn >= cardRedistributionInterval)
        {
            Debug.Log($"CARD SYSTEM: Turn {turnCount} - Redistributing cards (every {cardRedistributionInterval} turns)");
            DistributeCardsToPlayers();
        }
    }
    
    /// <summary>
    /// Manually redistribute cards (for testing or special events)
    /// </summary>
    public void RedistributeCards()
    {
        Debug.Log("CARD SYSTEM: Manual card redistribution requested");
        DistributeCardsToPlayers();
    }
    
    /// <summary>
    /// Get all available cards in the system
    /// </summary>
    public List<ChessCardData> GetAllAvailableCards()
    {
        return new List<ChessCardData>(allAvailableCards);
    }
    
    /// <summary>
    /// Add a new card to the available pool
    /// </summary>
    public void AddCardToPool(ChessCardData card)
    {
        if (card != null && !allAvailableCards.Contains(card))
        {
            allAvailableCards.Add(card);
            Debug.Log($"CARD SYSTEM: Added {card.cardName} to card pool");
        }
    }
    
    /// <summary>
    /// Remove a card from the available pool
    /// </summary>
    public void RemoveCardFromPool(ChessCardData card)
    {
        if (card != null && allAvailableCards.Contains(card))
        {
            allAvailableCards.Remove(card);
            Debug.Log($"CARD SYSTEM: Removed {card.cardName} from card pool");
        }
    }
    
    #endregion
    
    // Debug methods for testing
    #if UNITY_EDITOR
    [ContextMenu("Switch Turn (Debug)")]
    private void DebugSwitchTurn()
    {
        Debug.Log("=== DEBUG: Manual Turn Switch Requested ===");
        SwitchTurn();
    }
    
    [ContextMenu("Set Player 1 Turn (Debug)")]
    private void DebugSetPlayer1Turn()
    {
        Debug.Log("=== DEBUG: Setting Player 1 Turn ===");
        SetCurrentPlayer(0);
    }
    
    [ContextMenu("Set Player 2 Turn (Debug)")]
    private void DebugSetPlayer2Turn()
    {
        Debug.Log("=== DEBUG: Setting Player 2 Turn ===");
        SetCurrentPlayer(1);
    }
    
    [ContextMenu("Show Current Turn Info (Debug)")]
    private void DebugShowTurnInfo()
    {
        Debug.Log("=== CURRENT TURN INFO ===");
        Debug.Log($"Current Player Index: {currentPlayerIndex}");
        Debug.Log($"Current Player Name: {GetCurrentPlayerName()}");
        Debug.Log($"Turn Count: {turnCount}");
        Debug.Log($"Is Player 1 Turn: {IsPlayer1Turn()}");
        Debug.Log($"Is Player 2 Turn: {IsPlayer2Turn()}");
        Debug.Log("========================");
    }
    
    [ContextMenu("Restart Game (Debug)")]
    private void DebugRestartGame()
    {
        Debug.Log("=== DEBUG: Restarting Game ===");
        RestartGame();
    }
    
    [ContextMenu("End Game - Player 1 Wins (Debug)")]
    private void DebugPlayer1Wins()
    {
        EndGame(0);
    }
    
    [ContextMenu("End Game - Player 2 Wins (Debug)")]
    private void DebugPlayer2Wins()
    {
        EndGame(1);
    }
    
    [ContextMenu("Redistribute Cards (Debug)")]
    private void DebugRedistributeCards()
    {
        Debug.Log("=== DEBUG: Manual Card Redistribution ===");
        RedistributeCards();
    }
    
    [ContextMenu("Show Card Distribution (Debug)")]
    private void DebugShowCardDistribution()
    {
        Debug.Log("=== CARD DISTRIBUTION INFO ===");
        Debug.Log($"Total Available Cards: {allAvailableCards.Count}");
        Debug.Log($"Cards per Player: {activeCardsPerPlayer} active");
        Debug.Log($"Redistribution Interval: Every {cardRedistributionInterval} turns");
        Debug.Log($"Last Redistribution: Turn {lastCardRedistributionTurn}");
        Debug.Log($"Next Redistribution: Turn {lastCardRedistributionTurn + cardRedistributionInterval}");
        
        if (cardManager != null)
        {
            var p1Active = cardManager.GetActiveCards(1);
            var p1Storage = cardManager.GetStorageCards(1);
            var p2Active = cardManager.GetActiveCards(2);
            var p2Storage = cardManager.GetStorageCards(2);
            
            Debug.Log($"Player 1 - Active: {p1Active.Count}, Storage: {p1Storage.Count}");
            foreach (var card in p1Active)
            {
                Debug.Log($"  Active: {card.cardName} ({card.rank})");
            }
            
            Debug.Log($"Player 2 - Active: {p2Active.Count}, Storage: {p2Storage.Count}");
            foreach (var card in p2Active)
            {
                Debug.Log($"  Active: {card.cardName} ({card.rank})");
            }
        }
        Debug.Log("===============================");
    }
    
    [ContextMenu("Reload All Cards (Debug)")]
    private void DebugReloadAllCards()
    {
        Debug.Log("=== DEBUG: Reloading All Cards ===");
        LoadAllAvailableCards();
    }
    
    [ContextMenu("Test Card Shuffle (Debug)")]
    private void DebugTestCardShuffle()
    {
        Debug.Log("=== DEBUG: Testing Card Shuffle ===");
        List<ChessCardData> testCards = new List<ChessCardData>(allAvailableCards);
        Debug.Log("Before shuffle:");
        for (int i = 0; i < Mathf.Min(5, testCards.Count); i++)
        {
            Debug.Log($"  {i}: {testCards[i].cardName}");
        }
        
        ShuffleCards(testCards);
        Debug.Log("After shuffle:");
        for (int i = 0; i < Mathf.Min(5, testCards.Count); i++)
        {
            Debug.Log($"  {i}: {testCards[i].cardName}");
        }
    }
    #endif
}