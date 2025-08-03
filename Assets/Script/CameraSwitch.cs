using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bidak.Manager;

public class CameraSwitch : MonoBehaviour
{
    public GameObject[] mainCameras;
    public GameObject[] cardCameras;
    public Camera camera1;
    public Camera camera2;
    public Camera currentCamera;
    
    // Player index is now managed by GameManagerChess
    private int currentPlayerIndex
    {
        get
        {
            if (GameManagerChess.Instance != null)
                return GameManagerChess.Instance.currentPlayerIndex;
            return 0; // Default to player 0
        }
    }
    
    [Header("Card Integration")]
    public bool isInCardMode = false;
    public CardTargetingSystem targetingSystem;
    public ChessCardHoverEffect selectedCard;
    // Start is called before the first frame update
    void Start()
    {
        mainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
        cardCameras = new GameObject[2];
        cardCameras[0] = GameObject.FindGameObjectWithTag("Player1Camera");
        cardCameras[1] = GameObject.FindGameObjectWithTag("Player2Camera");
        
        // Initialize cameras based on current player index from GameManagerChess
        InitializeCameras();
        
        // Find targeting system
        targetingSystem = FindObjectOfType<CardTargetingSystem>();
    }
    
    private void InitializeCameras()
    {
        // Player index 0: use main camera index 0 and card camera index 0
        // Player index 1: use main camera index 1 and card camera index 1
        if(currentPlayerIndex == 0)
        {
            camera1 = mainCameras[0].GetComponent<Camera>();
            camera2 = cardCameras[0].GetComponent<Camera>();
            
            // Disable other player's cameras
            if (mainCameras.Length > 1) mainCameras[1].GetComponent<Camera>().enabled = false;
            if (cardCameras.Length > 1) cardCameras[1].GetComponent<Camera>().enabled = false;
        }
        else
        {
            camera1 = mainCameras[1].GetComponent<Camera>();
            camera2 = cardCameras[1].GetComponent<Camera>();
            
            // Disable other player's cameras
            mainCameras[0].GetComponent<Camera>().enabled = false;
            cardCameras[0].GetComponent<Camera>().enabled = false;
        }

        camera1.enabled = true;
        currentCamera = camera1;
        camera2.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Handle manual camera switching
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(0))
        {
            SwitchToMainCamera();
        }
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(1))
        {
            SwitchToCardCamera();
        }
        
        // Handle automatic switching based on card selection
        HandleCardModeSwitching();
    }
    
    private void SwitchToMainCamera()
    {
        camera1.enabled = true;
        camera2.enabled = false;
        currentCamera = camera1;
        isInCardMode = false;
        
        // End targeting if switching away from card mode
        if (targetingSystem != null && targetingSystem.IsCurrentlyTargeting())
        {
            targetingSystem.EndTargeting();
        }
    }
    
    private void SwitchToCardCamera()
    {
        camera1.enabled = false;
        camera2.enabled = true;
        currentCamera = camera2;
        isInCardMode = true;
    }
    
    private void HandleCardModeSwitching()
    {
        // Check if any card is selected and requires targeting
        if (targetingSystem != null && targetingSystem.IsCurrentlyTargeting())
        {
            // Switch to main camera for piece targeting
            if (isInCardMode)
            {
                SwitchToMainCamera();
            }
        }
        else
        {
            // Check if we should return to card camera after targeting
            if (!isInCardMode && selectedCard != null && selectedCard.isSelected)
            {
                // Stay in main camera for targeting, but track the selected card
            }
        }
    }
    
    // Public methods for external control
    public void SetSelectedCard(ChessCardHoverEffect card)
    {
        selectedCard = card;
        
        // If a card is selected, switch to main camera for targeting
        if (card != null && card.isSelected)
        {
            SwitchToMainCamera();
        }
    }
    
    public void ClearSelectedCard()
    {
        selectedCard = null;
        
        // Optionally switch back to card camera
        // SwitchToCardCamera();
    }
    
    public bool IsCurrentPlayersTurn()
    {
        // Always return true since this camera switch is automatically managed by GameManagerChess
        return true;
    }
    
    /// <summary>
    /// Set the current player and update camera accordingly
    /// Called by GameManagerChess when turns change
    /// </summary>
    public void SetCurrentPlayer(int newPlayerIndex)
    {
        // Reinitialize cameras for the new current player
        InitializeCameras();
    }
    

}
