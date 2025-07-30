using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List

public class ChessGamePrepare : MonoBehaviour
{
    [Header("Game Components")]
    public ChessBoardManager boardManager;
    public Camera mainCamera;

    [Header("UI Elements")]
    public GameObject mainMenuPanel;
    public GameObject boardSetupPanel;
    public GameObject playerConfigPanel;

    [Header("Player Configuration")]
    public TMP_InputField player1NameInput;
    public TMP_InputField player2NameInput;
    public Toggle player1ColorToggle;
    public Toggle player2ColorToggle;

    [Header("Board Setup")]
    public Dropdown boardSizeDropdown;
    public Toggle customBoardToggle;
    public GameObject customBoardConfigPanel;

    [Header("Game Modes")]
    public Toggle standardChessToggle;
    public Toggle customRulesToggle;

    [Header("Audio")]
    public AudioSource uiSoundSource;
    public AudioClip selectSound;
    public AudioClip confirmSound;

    [Header("Player Data")]
    public PlayerData player1;
    public PlayerData player2;

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public Color pieceColor;
        public bool isHuman = true;
        public int difficulty; // For AI
    }

    private void Start()
    {
        // Initial setup
        InitializeUI();
        SetupDefaultConfiguration();
    }

    private void InitializeUI()
    {
        // Show main menu, hide other panels
        mainMenuPanel.SetActive(true);
        boardSetupPanel.SetActive(false);
        playerConfigPanel.SetActive(false);
        customBoardConfigPanel.SetActive(false);

        // Setup dropdown for board size
        boardSizeDropdown.ClearOptions();
        boardSizeDropdown.AddOptions(new List<string> { 
            "Standard 8x8", 
            "Small 6x6", 
            "Large 10x10" 
        });

        // Setup toggles
        standardChessToggle.isOn = true;
        customRulesToggle.isOn = false;
        customBoardToggle.isOn = false;
    }

    private void SetupDefaultConfiguration()
    {
        // Default player setup
        player1 = new PlayerData 
        { 
            playerName = "Player 1", 
            pieceColor = Color.white,
            isHuman = true
        };

        player2 = new PlayerData 
        { 
            playerName = "Player 2", 
            pieceColor = Color.black,
            isHuman = true
        };
    }

    public void OnStartGameButtonClicked()
    {
        PlaySound(confirmSound);
        mainMenuPanel.SetActive(false);
        playerConfigPanel.SetActive(true);
    }

    public void OnPlayerConfigNextButtonClicked()
    {
        // Validate and save player names
        player1.playerName = string.IsNullOrEmpty(player1NameInput.text) 
            ? "Player 1" 
            : player1NameInput.text;
        
        player2.playerName = string.IsNullOrEmpty(player2NameInput.text) 
            ? "Player 2" 
            : player2NameInput.text;

        // Set piece colors based on toggles
        player1.pieceColor = player1ColorToggle.isOn ? Color.white : Color.black;
        player2.pieceColor = player1.pieceColor == Color.white ? Color.black : Color.white;

        PlaySound(confirmSound);
        playerConfigPanel.SetActive(false);
        boardSetupPanel.SetActive(true);
    }

    public void OnBoardSizeChanged()
    {
        PlaySound(selectSound);
        int selectedIndex = boardSizeDropdown.value;
        switch (selectedIndex)
        {
            case 0: // Standard 8x8
                boardManager.boardSize = 8;
                break;
            case 1: // Small 6x6
                boardManager.boardSize = 6;
                break;
            case 2: // Large 10x10
                boardManager.boardSize = 10;
                break;
        }

        // Reinitialize board with new size
        boardManager.InitializeBoard();
    }

    public void OnCustomBoardToggleChanged()
    {
        PlaySound(selectSound);
        customBoardConfigPanel.SetActive(customBoardToggle.isOn);
    }

    public void OnGameModeToggleChanged()
    {
        PlaySound(selectSound);
        // Enable/disable custom rules configuration
    }

    public void OnPrepareGameButtonClicked()
    {
        PlaySound(confirmSound);
        
        // Final board configuration
        if (customBoardToggle.isOn)
        {
            // Apply custom board setup
            ConfigureCustomBoard();
        }
        else
        {
            // Use standard board setup
            boardManager.SetupDefaultPieces();
        }

        // Transition to game scene or enable game components
        StartGame();
    }

    private void ConfigureCustomBoard()
    {
        // Clear existing custom setup
        boardManager.customBoardSetup.Clear();

        // Example of how you might add custom pieces
        // This would typically be done through UI interactions
        ChessPieceData customPiece = ScriptableObject.CreateInstance<ChessPieceData>();
        customPiece.pieceType = ChessPieceData.PieceType.Knight;
        
        boardManager.customBoardSetup.Add(new ChessBoardManager.PiecePosition 
        { 
            piece = customPiece, 
            chessNotation = "E4", 
            pieceColor = player1.pieceColor 
        });
    }

    private void StartGame()
    {
        // Hide setup panels
        mainMenuPanel.SetActive(false);
        playerConfigPanel.SetActive(false);
        boardSetupPanel.SetActive(false);

        // Position camera
        PositionCamera();

        // Additional game start logic
        // Could include enabling game controller, starting turn system, etc.
    }

    private void PositionCamera()
    {
        // Position camera to view the entire board
        float boardWidth = boardManager.boardSize * boardManager.tileSizeInUnits;
        mainCamera.transform.position = new Vector3(
            boardWidth / 2, 
            boardWidth, 
            boardWidth / 2
        );
        mainCamera.transform.LookAt(new Vector3(boardWidth / 2, 0, boardWidth / 2));
    }

    private void PlaySound(AudioClip clip)
    {
        if (uiSoundSource && clip)
        {
            uiSoundSource.PlayOneShot(clip);
        }
    }

    // Utility method to reset entire preparation
    public void OnBackButtonClicked()
    {
        PlaySound(selectSound);
        mainMenuPanel.SetActive(true);
        playerConfigPanel.SetActive(false);
        boardSetupPanel.SetActive(false);
        customBoardConfigPanel.SetActive(false);
    }
} 