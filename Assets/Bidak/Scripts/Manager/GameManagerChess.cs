using UnityEngine;
using System;

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
        
        Debug.Log($"Game Started! {GetCurrentPlayerName()}'s turn");
        
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
        // Switch player index (0 -> 1, 1 -> 0)
        currentPlayerIndex = (currentPlayerIndex + 1) % 2;
        
        // Increment turn count only when it's Player 1's turn again
        if (currentPlayerIndex == 0)
        {
            turnCount++;
        }
        
        // Reset turn timer
        turnTimer = 0f;
        
        Debug.Log($"Turn {turnCount}: {GetCurrentPlayerName()}'s turn");
        
        // Update camera systems for current player
        UpdateCameraForCurrentPlayer();
        
        // Notify listeners
        OnPlayerTurnChanged?.Invoke(currentPlayerIndex);
        OnPlayerTurnChangedWithName?.Invoke(currentPlayerIndex, GetCurrentPlayerName());
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
        
        currentPlayerIndex = playerIndex;
        turnTimer = 0f;
        
        Debug.Log($"Player switched to: {GetCurrentPlayerName()}");
        
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
    
    // Debug methods for testing
    #if UNITY_EDITOR
    [ContextMenu("Switch Turn (Debug)")]
    private void DebugSwitchTurn()
    {
        SwitchTurn();
    }
    
    [ContextMenu("Restart Game (Debug)")]
    private void DebugRestartGame()
    {
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
    #endif
}