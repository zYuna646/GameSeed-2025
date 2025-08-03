using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using Bidak.Manager;
using Bidak.Data;
using Bidak.Card;

public class TileManager : MonoBehaviour
{
    private GameManagerChess gameManagerChess;
    private CameraSwitch cameraSwitch;
    private ChessBoardTileSpawner chessBoardTileSpawner;
    
    [Header("Card Integration")]
    private CardTargetingSystem cardTargetingSystem;
    private ChessCardManager cardManager;
    private CardEffectProcessor cardEffectProcessor;
    [SerializeField] private GameObject cardPreviewPrefab;
    [SerializeField] private float cardPreviewHeight = 2f;
    private Dictionary<GameObject, GameObject> activeCardPreviews = new Dictionary<GameObject, GameObject>();
    private GameObject tempHoverCardPreview;
    
    [Header("Card Effect Indicators")]
    [SerializeField] private GameObject cardEffectIndicatorPrefab;
    [SerializeField] private float indicatorHeight = 1.5f;
    [SerializeField] private float indicatorRotationSpeed = 30f;
    [SerializeField] private float indicatorBobSpeed = 2f;
    [SerializeField] private float indicatorBobHeight = 0.3f;
    private Dictionary<PieceController, GameObject> activeEffectIndicators = new Dictionary<PieceController, GameObject>();
    
    [Header("Movement System")]
    [SerializeField]private TileController goTo;
    public GameObject kiri;
    public GameObject kanan;
    public Collider[] tileColliders;
    public Collider[] pieceColliders;
    [SerializeField] float cameraOffset;
    [SerializeField] public bool isPicked = false;
    [SerializeField] public bool isGoing = false;
    Vector3 cameraPoint;
    Vector3 mousePos;

    [SerializeField] public LayerMask tileMask;
    [SerializeField] public LayerMask pieceMask;
    [SerializeField] float hoverHeight;
    [SerializeField] Vector3 hoverScale;
    Collider piece;

    [SerializeField] public List<GameObject> tiles;

    [SerializeField] Transform test;
    void Start()
    {
        cameraSwitch = FindObjectOfType<CameraSwitch>();
        gameManagerChess = GameManagerChess.Instance;
        chessBoardTileSpawner = FindObjectOfType<ChessBoardTileSpawner>();
        
        // Initialize card systems
        cardTargetingSystem = FindObjectOfType<CardTargetingSystem>();
        cardManager = FindObjectOfType<ChessCardManager>();
        cardEffectProcessor = FindObjectOfType<CardEffectProcessor>();
        
        // Subscribe to card events
        if (cardManager != null)
        {
            cardManager.OnCardActivated += OnCardActivated;
            cardManager.OnCardDeactivated += OnCardDeactivated;
            cardManager.OnCardModeChanged += OnCardModeChanged;
        }
        
        // Subscribe to game manager events
        if (gameManagerChess != null)
        {
            GameManagerChess.OnPlayerTurnChanged += OnPlayerTurnChanged;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if(tiles.Count != 64)
        {
            tiles = GatherTiles(chessBoardTileSpawner);

            tiles.Sort((a, b) => a.name.CompareTo(b.name));
        }
        mousePos = Input.mousePosition;
        if (cameraSwitch != null) { transform.position = cameraSwitch.currentCamera.ScreenToWorldPoint(mousePos); }
        mousePos.z = cameraOffset;

        if (Input.GetKeyDown(KeyCode.S))
        {
            ScaleUp(test);
        }

        if(Input.GetMouseButtonDown(0))
        {
            // Check if we're actively targeting with a card
            bool isActivelyTargeting = cardTargetingSystem != null && cardTargetingSystem.isTargeting;
            
            if (isActivelyTargeting)
            {
                // Handle card targeting when actively targeting
                HandleCardTargeting();
            }
            else
            {
                // Normal piece movement logic (when NOT actively targeting)
                HandlePieceMovement();
            }
        }
        if (!isGoing)
        {
            Hover();
        }else if (isGoing && piece == null)
        {
            Highlight(goTo.gameObject.GetComponent<Tile>(), goTo.gameObject.GetComponent<Tile>().originColor);
            isGoing = false;
        }
        if(isGoing && Vector3.Distance(piece.transform.position, goTo.transform.position) > 0.01f)
        {
            Highlight(goTo.gameObject.GetComponent<Tile>(), goTo.gameObject.GetComponent<Tile>().highlightColor);
        }
        
        // Update card effect indicators
        UpdateCardEffectIndicators();
    }
    void Highlight(Tile a, Color color)
    {
        a.MeshRenderer.material.color = color;
        
    }
    void Hover()
    {
        cameraPoint = cameraSwitch.currentCamera.ScreenToWorldPoint(mousePos);
        tileColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, tileMask);
        pieceColliders = Physics.OverlapCapsule(transform.position, cameraPoint, 0.1f, pieceMask);
        if (!isPicked)
        {
            if (tileColliders.Length > 0 && pieceColliders.Length == 0)
            {
                Collider tile = tileColliders[tileColliders.Length - 1];
                tile.GetComponent<Tile>().inSight = true;
                if (!tile.GetComponent<Tile>().contained)
                {
                    if (tile.GetComponent<Tile>().hover && !tile.GetComponent<Tile>().rised && tile.GetComponent<Tile>().canHover)
                    {
                        ScaleUp(tile.transform);
                    }
                }
            }
            if (pieceColliders.Length > 0)
            {
                GameObject piece = pieceColliders[pieceColliders.Length - 1].gameObject;

                PieceHover(piece.GetComponent<PieceType>().parent.transform,piece.GetComponent<PieceType>());
            
            }
        }else if (tileColliders.Length != 0 && tileColliders[tileColliders.Length-1].GetComponent<Tile>().rised && isPicked)
        {
            goTo = tileColliders[tileColliders.Length - 1].GetComponent<TileController>();
        }
            
    }
    public void ScaleUp(Transform tile)
    {
        Vector3 rise = new Vector3(tile.position.x, hoverHeight, tile.position.z);
        tile.position = Vector3.MoveTowards(tile.position, rise, 100f * Time.deltaTime);
        //tile.localScale = Vector3.Lerp(tile.localScale, hoverScale, 200f * Time.deltaTime);
        tile.GetComponent<Tile>().rised = true;
        Debug.Log("Scale UP "+ tile.gameObject);
    }

    public void ScaleDown(Transform tile, float normalHeight, Vector3 normalScale)
    {
        Vector3 down = new Vector3(tile.position.x, normalHeight, tile.position.z);
        tile.position = Vector3.MoveTowards(tile.position, down, 100f * Time.deltaTime);
        tile.localScale = Vector3.Lerp(tile.localScale, normalScale, 200f * Time.deltaTime);
        tile.GetComponent<Tile>().rised = false;
        Debug.Log("Scale Down");    
    }

    //void PieceHover(PieceType piece)
    //{
    //    piece.RayCheck();
    //}
    void PieceHover(Transform tile, PieceType piece)
    {
        if (tile == null || piece == null || tiles == null) return;

        Tile currentTile = tile.GetComponent<Tile>();
        int currentRow = currentTile.row;
        int currentCol = currentTile.collumn;
        bool isWhite = piece.isWhite;
        
        // Show card preview if this piece has active card effects
        ShowHoverCardPreview(piece.gameObject);

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile targetTile = tiles[i].GetComponent<Tile>();
            if (targetTile == null || !targetTile.canHover) continue;

            int rowDiff = targetTile.row - currentRow;
            int colDiff = targetTile.collumn - currentCol;
            int absRowDiff = Mathf.Abs(rowDiff);
            int absColDiff = Mathf.Abs(colDiff);

            bool isValidMove = false;
            bool isBlocked = false;

            switch (piece.typePiece)
            {
                case type.ROOK:
                    isValidMove = (targetTile.row == currentRow) ^ (targetTile.collumn == currentCol);
                    if (isValidMove)
                        isBlocked = IsPathBlocked(currentTile, targetTile, false);
                    break;

                case type.PAWN:
                    int forwardDir = isWhite ? 1 : -1;
                    isValidMove = (rowDiff == forwardDir && colDiff == 0 && !targetTile.contained) ||
                                 (!piece.hasDoubleMoved && rowDiff == 2 * forwardDir && colDiff == 0 && !targetTile.contained) ||
                                 (absColDiff == 1 && rowDiff == forwardDir && targetTile.contained &&
                                  targetTile.GetComponentInChildren<PieceType>().isWhite != isWhite);
                    break;

                case type.BISHOP:
                    isValidMove = (absRowDiff == absColDiff && absRowDiff > 0);
                    if (isValidMove)
                        isBlocked = IsPathBlocked(currentTile, targetTile, true);
                    break;

                case type.KNIGHT:
                    isValidMove = (absRowDiff == 2 && absColDiff == 1) || (absRowDiff == 1 && absColDiff == 2);
                    break;

                case type.KING:
                    isValidMove = (absRowDiff <= 1 && absColDiff <= 1 && (absRowDiff + absColDiff) > 0 && !targetTile.contained);
                    // Castling logic remains unchanged
                    break;

                case type.QUEEN:
                    bool isStraight = (targetTile.row == currentRow) ^ (targetTile.collumn == currentCol);
                    bool isDiagonal = absRowDiff == absColDiff;
                    isValidMove = isStraight || isDiagonal;
                    if (isValidMove)
                        isBlocked = IsPathBlocked(currentTile, targetTile, isDiagonal);
                    break;
            }

            if (isValidMove && !isBlocked)
            {
                if (tiles[i].gameObject.GetComponentInChildren<PieceType>() != null)
                {
                    if (tiles[i].gameObject.GetComponentInChildren<PieceType>().isWhite != currentTile.gameObject.GetComponentInChildren<PieceType>().isWhite)
                    {
                        ScaleUp(tiles[i].transform);
                    }
                }
                else
                {
                    ScaleUp(tiles[i].transform);
                }
            }
        }
    }

    bool IsPathBlocked(Tile start, Tile end, bool isDiagonal)
    {
        int xDir = Mathf.Clamp(end.collumn - start.collumn, -1, 1);
        int yDir = Mathf.Clamp(end.row - start.row, -1, 1);

        int steps = isDiagonal ?
            Mathf.Abs(end.collumn - start.collumn) :
            Mathf.Max(Mathf.Abs(end.collumn - start.collumn), Mathf.Abs(end.row - start.row));

        // Check each tile along the path (excluding start and end)
        for (int i = 1; i < steps; i++)
        {
            int checkX = start.collumn + xDir * i;
            int checkY = start.row + yDir * i;

            Tile intermediateTile = tiles.Find(t =>
                t.GetComponent<Tile>().collumn == checkX &&
                t.GetComponent<Tile>().row == checkY)?.GetComponent<Tile>();

            if (intermediateTile != null && intermediateTile.contained)
                return true;
        }

        return false;
    }


    public List<GameObject> GatherTiles(ChessBoardTileSpawner gm)
    {
        // Create empty list if null
        if (gm == null || gm.transform == null) return new List<GameObject>();

        // Proper way to get all child GameObjects
        List<GameObject> childObjects = new List<GameObject>();

        foreach (Transform child in gm.transform)
        {
            childObjects.Add(child.gameObject);
        }

        return childObjects;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, cameraPoint);
    }
    
    #region Card Integration Methods
    
    /// <summary>
    /// Handle card targeting when a card is selected
    /// </summary>
    private void HandleCardTargeting()
    {
        if (pieceColliders.Length > 0)
        {
            Collider targetPieceCollider = pieceColliders[pieceColliders.Length - 1];
            PieceController targetPiece = targetPieceCollider.GetComponent<PieceController>();
            
            if (targetPiece != null && cardTargetingSystem.selectedCard != null)
            {
                // Check if this piece can be targeted by the selected card
                if (CanTargetPieceWithCard(targetPiece, cardTargetingSystem.selectedCard))
                {
                    ChessCardHoverEffect selectedCard = cardTargetingSystem.selectedCard;
                    ApplyCardEffect(targetPiece, selectedCard);
                    cardTargetingSystem.EndTargetingWithSound(true); // Success sound
                    
                    // Check if this card type should end the turn
                    bool shouldEndTurn = ShouldCardEndTurn(selectedCard?.cardData);
                    Debug.Log($"Card effect applied. Should end turn: {shouldEndTurn}");
                    
                    if (shouldEndTurn && gameManagerChess != null)
                    {
                        Debug.Log($"TILE MANAGER: Card effect completed - switching turn from {gameManagerChess.currentPlayerIndex} ({gameManagerChess.GetCurrentPlayerName()})");
                        gameManagerChess.SwitchTurn();
                    }
                    else if (gameManagerChess != null)
                    {
                        Debug.Log($"TILE MANAGER: Player {gameManagerChess.GetCurrentPlayerName()} keeps turn after using effect card");
                    }
                    else
                    {
                        Debug.LogWarning("TILE MANAGER: GameManagerChess is null - cannot manage turn");
                    }
                }
                else
                {
                    Debug.Log("Cannot target this piece with the selected card");
                    cardTargetingSystem.EndTargetingWithSound(false); // Failure sound
                }
            }
        }
    }
    
    /// <summary>
    /// Handle normal piece movement (original logic)
    /// </summary>
    private void HandlePieceMovement()
    {
        // Check if it's the current player's turn
        if (gameManagerChess != null && pieceColliders.Length > 0)
        {
            Collider selectedPieceCollider = pieceColliders[pieceColliders.Length - 1];
            PieceController selectedPiece = selectedPieceCollider.GetComponent<PieceController>();
            
            if (selectedPiece != null && selectedPiece.pieceData != null)
            {
                // Check if this piece belongs to the current player
                if (!gameManagerChess.IsPieceOwnedByCurrentPlayer(selectedPiece.pieceData))
                {
                    Debug.Log("It's not your turn or this piece doesn't belong to you!");
                    return;
                }
            }
        }
        
        // Original movement logic
        if(!isPicked && pieceColliders.Length != 0)
        {
            piece = pieceColliders[pieceColliders.Length - 1];
            Highlight(piece.GetComponentInParent<Tile>(), piece.GetComponentInParent<Tile>().highlightColor);
            isPicked = true;
        }
        else if(isPicked && tileColliders.Length == 0)
        {
            isPicked = false;
            isGoing = false;
        }
        else if (isPicked && !tileColliders[tileColliders.Length - 1].GetComponent<Tile>().rised)
        {
            isPicked = false;
            isGoing = false;
        }
        else if(isPicked && goTo != null) 
        {
            piece.GetComponent<PieceMovementController>().destinationTile = goTo;
            
            Highlight(piece.GetComponentInParent<Tile>(), piece.GetComponentInParent<Tile>().originColor);
            isPicked = false;
            isGoing = true;
            
            // Switch turns after a successful move
            if (gameManagerChess != null)
            {
                Debug.Log($"TILE MANAGER: Piece movement completed - switching turn from {gameManagerChess.currentPlayerIndex} ({gameManagerChess.GetCurrentPlayerName()})");
                gameManagerChess.SwitchTurn();
            }
            else
            {
                Debug.LogWarning("TILE MANAGER: GameManagerChess is null - cannot switch turn");
            }
        }
    }
    
    /// <summary>
    /// Check if a piece can be targeted with a specific card
    /// </summary>
    private bool CanTargetPieceWithCard(PieceController targetPiece, ChessCardHoverEffect card)
    {
        if (targetPiece == null || card == null || card.cardData == null)
            return false;
        
        // Check if it's the card owner's turn
        int cardPlayerIndex = card.playerIndex - 1; // Convert from 1-based to 0-based
        if (gameManagerChess != null && !gameManagerChess.IsPlayerTurn(cardPlayerIndex))
        {
            return false;
        }
        
        // Use CardEffectProcessor for more sophisticated targeting validation
        if (cardEffectProcessor != null)
        {
            return cardEffectProcessor.CanTargetPieceWithCard(targetPiece, card.cardData, card.playerIndex);
        }
        
        // Fallback: Allow targeting any piece
        return true;
    }
    
    /// <summary>
    /// Apply card effect to a piece
    /// </summary>
    private void ApplyCardEffect(PieceController targetPiece, ChessCardHoverEffect card)
    {
        if (targetPiece == null || card == null || card.cardData == null)
            return;
        
        Debug.Log($"Applying card effect: {card.cardData.cardName} to piece: {targetPiece.name}");
        
        // Use the CardEffectProcessor to apply effects
        if (cardEffectProcessor != null)
        {
            bool success = cardEffectProcessor.ApplyCardEffectToPiece(card.cardData, targetPiece, card.playerIndex);
            
            if (success)
            {
                // Show card preview above the piece
                ShowCardPreviewAbovePiece(targetPiece.gameObject, card.cardData);
                Debug.Log($"Successfully applied card effect {card.cardData.cardName} to {targetPiece.name}");
            }
            else
            {
                Debug.LogWarning($"Failed to apply card effect {card.cardData.cardName} to {targetPiece.name}");
            }
        }
        else
        {
            // Fallback: Apply card effects directly to the piece data
            if (targetPiece.pieceData != null && card.cardData.cardEffects != null)
            {
                foreach (var effect in card.cardData.cardEffects)
                {
                    if (effect != null && effect.isEnabled)
                    {
                        targetPiece.pieceData.ApplyCardEffect(effect);
                    }
                }
            }
            
            // Show card preview above the piece
            ShowCardPreviewAbovePiece(targetPiece.gameObject, card.cardData);
            
            // Activate the card in the card manager
            if (cardManager != null)
            {
                cardManager.ActivateCard(card.playerIndex, card.cardData);
            }
            
            Debug.LogWarning("CardEffectProcessor not found, using fallback method");
        }
    }
    
    /// <summary>
    /// Show card preview above a piece
    /// </summary>
    private void ShowCardPreviewAbovePiece(GameObject piece, ChessCardData cardData)
    {
        if (cardPreviewPrefab == null || piece == null || cardData == null)
            return;
        
        // Check if card mode is active or a card effect is being applied
        bool isCardModeOrEffectActive = cardManager != null && 
            (cardManager.IsCardModeActive() || cardManager.currentSelectedCard != null);
        
        Debug.Log($"Showing card preview: Card Mode Active = {isCardModeOrEffectActive}");
        
        // Remove existing card preview if any
        if (activeCardPreviews.ContainsKey(piece))
        {
            if (activeCardPreviews[piece] != null)
            {
                Destroy(activeCardPreviews[piece]);
            }
            activeCardPreviews.Remove(piece);
        }
        
        // Create new card preview
        Vector3 previewPosition = piece.transform.position + Vector3.up * cardPreviewHeight;
        GameObject cardPreview = Instantiate(cardPreviewPrefab, previewPosition, Quaternion.identity);
        
        // Configure the card preview
        ChessCardHoverEffect cardHoverEffect = cardPreview.GetComponent<ChessCardHoverEffect>();
        if (cardHoverEffect == null)
        {
            cardHoverEffect = cardPreview.AddComponent<ChessCardHoverEffect>();
        }
        
        if (cardHoverEffect != null)
        {
            // Get the correct player index for the piece
            PieceController pieceController = piece.GetComponent<PieceController>();
            int piecePlayerIndex = GetPiecePlayerIndex(pieceController);
            if (piecePlayerIndex == -1) piecePlayerIndex = 1; // Fallback to player 1
            
            // Setup card with data and audio from manager
            if (cardManager != null)
            {
                cardManager.SetupCardHoverEffect(cardHoverEffect, cardData, piecePlayerIndex);
            }
            else
            {
                // Fallback if no card manager
                cardHoverEffect.SetCardData(cardData, piecePlayerIndex);
            }
            
            // Disable interaction if not in card mode
            cardHoverEffect.canBeActivated = isCardModeOrEffectActive;
            
            Debug.Log($"Card preview setup: CanBeActivated = {cardHoverEffect.canBeActivated}");
        }
        
        // Make the card face the camera
        if (cameraSwitch != null && cameraSwitch.currentCamera != null)
        {
            cardPreview.transform.LookAt(cameraSwitch.currentCamera.transform);
            cardPreview.transform.Rotate(0, 180, 0); // Flip to face camera correctly
        }
        
        // Store reference
        activeCardPreviews[piece] = cardPreview;
        
        Debug.Log($"Created card preview for piece: {piece.name}, Card: {cardData.cardName}");
    }
    
    /// <summary>
    /// Called when a card is activated
    /// </summary>
    private void OnCardActivated(int playerIndex, ChessCardData cardData)
    {
        Debug.Log($"Card activated for player {playerIndex}: {cardData.cardName}");
    }
    
    /// <summary>
    /// Called when a card is deactivated
    /// </summary>
    private void OnCardDeactivated(int playerIndex, ChessCardData cardData)
    {
        Debug.Log($"Card deactivated for player {playerIndex}: {cardData.cardName}");
        
        // Remove any card previews associated with this card
        RemoveCardPreviewsOfType(cardData);
    }
    
    /// <summary>
    /// Called when player turn changes
    /// </summary>
    private void OnPlayerTurnChanged(int newPlayerIndex)
    {
        Debug.Log($"Turn changed to player {newPlayerIndex}");
        
        // Reset any temporary states
        isPicked = false;
        isGoing = false;
        
        // Clear any active targeting
        if (cardTargetingSystem != null && cardTargetingSystem.isTargeting)
        {
            cardTargetingSystem.EndTargetingWithSound(false);
        }
        
        // Exit card mode when turn changes
        if (cardManager != null && cardManager.IsCardModeActive())
        {
            cardManager.ExitCardMode();
        }
    }
    
    /// <summary>
    /// Called when card mode changes (enters/exits)
    /// </summary>
    private void OnCardModeChanged(bool isCardModeActive)
    {
        Debug.Log($"Card mode changed: {(isCardModeActive ? "ENTERED" : "EXITED")}");
        
        if (!isCardModeActive)
        {
            // When exiting card mode, reset movement states and clear card previews
            isPicked = false;
            isGoing = false;
            
            // Remove all existing card previews
            foreach (var preview in activeCardPreviews.Values)
            {
                if (preview != null)
                {
                    Destroy(preview);
                }
            }
            activeCardPreviews.Clear();
            
            Debug.Log("Reset movement states and cleared card previews after exiting card mode");
        }
        else
        {
            Debug.Log("Entered card mode - movement will be restricted during targeting");
        }
    }
    
    /// <summary>
    /// Remove card previews of a specific card type
    /// </summary>
    private void RemoveCardPreviewsOfType(ChessCardData cardData)
    {
        List<GameObject> toRemove = new List<GameObject>();
        
        foreach (var kvp in activeCardPreviews)
        {
            if (kvp.Value != null)
            {
                ChessCardHoverEffect cardHoverEffect = kvp.Value.GetComponent<ChessCardHoverEffect>();
                if (cardHoverEffect != null && cardHoverEffect.cardData == cardData)
                {
                    Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
        }
        
        foreach (var key in toRemove)
        {
            activeCardPreviews.Remove(key);
        }
    }
    
    /// <summary>
    /// Cleanup method
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (cardManager != null)
        {
            cardManager.OnCardActivated -= OnCardActivated;
            cardManager.OnCardDeactivated -= OnCardDeactivated;
            cardManager.OnCardModeChanged -= OnCardModeChanged;
        }
        
        GameManagerChess.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        
        // Clean up card previews
        foreach (var preview in activeCardPreviews.Values)
        {
            if (preview != null)
            {
                Destroy(preview);
            }
        }
        activeCardPreviews.Clear();
        
        // Clean up temp hover preview
        if (tempHoverCardPreview != null)
        {
            Destroy(tempHoverCardPreview);
        }
        
        // Clean up card effect indicators
        RemoveAllEffectIndicators();
    }
    
    /// <summary>
    /// Show temporary card preview when hovering over pieces with active cards
    /// </summary>
    private void ShowHoverCardPreview(GameObject piece)
    {
        if (cardPreviewPrefab == null || piece == null)
            return;
        
        // Check if this piece has active card effects
        PieceController pieceController = piece.GetComponent<PieceController>();
        if (pieceController == null || pieceController.pieceData == null)
            return;
        
        // Get active cards for the piece owner
        int piecePlayerIndex = GetPiecePlayerIndex(pieceController);
        if (piecePlayerIndex == -1 || cardManager == null)
            return;
        
        List<ChessCardData> activeCards = cardManager.GetActiveCards(piecePlayerIndex);
        if (activeCards.Count == 0)
            return;
        
        // Clear existing temp hover preview
        if (tempHoverCardPreview != null)
        {
            Destroy(tempHoverCardPreview);
            tempHoverCardPreview = null;
        }
        
        // Show preview of the first active card (or you could cycle through them)
        ChessCardData cardToShow = activeCards[0];
        Vector3 previewPosition = piece.transform.position + Vector3.up * (cardPreviewHeight + 0.5f);
        tempHoverCardPreview = Instantiate(cardPreviewPrefab, previewPosition, Quaternion.identity);
        
        // Configure the temp card preview
        ChessCardHoverEffect cardHoverEffect = tempHoverCardPreview.GetComponent<ChessCardHoverEffect>();
        if (cardHoverEffect == null)
        {
            cardHoverEffect = tempHoverCardPreview.AddComponent<ChessCardHoverEffect>();
        }
        
        if (cardHoverEffect != null)
        {
            // Setup card with data and audio from manager
            if (cardManager != null)
            {
                cardManager.SetupCardHoverEffect(cardHoverEffect, cardToShow, piecePlayerIndex);
            }
            else
            {
                // Fallback if no card manager
                cardHoverEffect.SetCardData(cardToShow, piecePlayerIndex);
            }
        }
        
        // Make the card face the camera and slightly transparent
        if (cameraSwitch != null && cameraSwitch.currentCamera != null)
        {
            tempHoverCardPreview.transform.LookAt(cameraSwitch.currentCamera.transform);
            tempHoverCardPreview.transform.Rotate(0, 180, 0);
        }
        
        // Make it semi-transparent to indicate it's a hover preview
        Renderer[] renderers = tempHoverCardPreview.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = 0.7f;
                    material.color = color;
                }
            }
        }
        
        // Auto-destroy after a short time
        Destroy(tempHoverCardPreview, 2f);
    }
    
    /// <summary>
    /// Get player index for a piece (1-based for card system)
    /// </summary>
    private int GetPiecePlayerIndex(PieceController pieceController)
    {
        if (pieceController == null || pieceController.pieceData == null || gameManagerChess == null)
            return -1;
        
        // Convert from GameManagerChess 0-based to card system 1-based
        if (gameManagerChess.IsPieceOwnedByPlayer(pieceController.pieceData, 0))
            return 1; // Player 1
        else if (gameManagerChess.IsPieceOwnedByPlayer(pieceController.pieceData, 1))
            return 2; // Player 2
        
        return -1;
    }
    
    /// <summary>
    /// Determine if card should end turn based on its type
    /// </summary>
    private bool ShouldCardEndTurn(ChessCardData card)
    {
        if (card == null || card.cardEffects == null || card.cardEffects.Count == 0)
            return true; // Default to ending turn
        
        foreach (var effect in card.cardEffects)
        {
            // Attack and Defense cards end the turn
            switch (effect.effectType)
            {
                // Attack effects - end turn
                case Bidak.Data.CardEffectType.QueenCollision:
                case Bidak.Data.CardEffectType.DiagonalAttack:
                case Bidak.Data.CardEffectType.UnstoppableForce:
                case Bidak.Data.CardEffectType.DanceLikeQueen:
                case Bidak.Data.CardEffectType.DanceLikeElephant:
                    Debug.Log($"Attack card {effect.effectType} - will end turn");
                    return true;
                
                // Defense effects - end turn
                case Bidak.Data.CardEffectType.RoyalCommand:
                case Bidak.Data.CardEffectType.WhereIsMyDefense:
                case Bidak.Data.CardEffectType.NotToday:
                case Bidak.Data.CardEffectType.TimeFrozen:
                case Bidak.Data.CardEffectType.ProtectedRing:
                case Bidak.Data.CardEffectType.IGotYou:
                    Debug.Log($"Defense card {effect.effectType} - will end turn");
                    return true;
                
                // Movement/Effect cards - don't end turn (player keeps their turn)
                case Bidak.Data.CardEffectType.DoubleMove:
                case Bidak.Data.CardEffectType.TripleMove:
                case Bidak.Data.CardEffectType.StraightMove:
                case Bidak.Data.CardEffectType.BlockadeMove:
                case Bidak.Data.CardEffectType.ForwardTwoMoves:
                case Bidak.Data.CardEffectType.TwoDirectionMove:
                case Bidak.Data.CardEffectType.PowerfulMove:
                case Bidak.Data.CardEffectType.NiceDay:
                case Bidak.Data.CardEffectType.BackMove:
                case Bidak.Data.CardEffectType.LeapMove:
                case Bidak.Data.CardEffectType.RestoreMove:
                case Bidak.Data.CardEffectType.BackFromDead:
                case Bidak.Data.CardEffectType.StoneTomorrow:
                case Bidak.Data.CardEffectType.SpecialMove:
                    Debug.Log($"Effect card {effect.effectType} - will NOT end turn");
                    return false;
            }
        }
        
        Debug.Log("Unknown card type - defaulting to end turn");
        return true; // Default to ending turn for unknown effects
    }
    
    /// <summary>
    /// Public method to manually show card above piece (for external scripts)
    /// </summary>
    public void ShowCardAbovePiece(GameObject piece, ChessCardData cardData)
    {
        ShowCardPreviewAbovePiece(piece, cardData);
    }
    
    /// <summary>
    /// Public method to hide card above piece (for external scripts)
    /// </summary>
    public void HideCardAbovePiece(GameObject piece)
    {
        if (activeCardPreviews.ContainsKey(piece))
        {
            if (activeCardPreviews[piece] != null)
            {
                Destroy(activeCardPreviews[piece]);
            }
            activeCardPreviews.Remove(piece);
        }
    }
    
    #endregion
    
    #region Card Effect Indicators
    
    /// <summary>
    /// Update visual indicators for all pieces with active card effects
    /// </summary>
    private void UpdateCardEffectIndicators()
    {
        if (cardEffectProcessor == null)
        {
            if (Time.frameCount % 60 == 0) // Only log once per second
                Debug.LogWarning("TILE MANAGER: CardEffectProcessor is null");
            return;
        }
        
        if (cardEffectIndicatorPrefab == null)
        {
            if (Time.frameCount % 60 == 0) // Only log once per second
                Debug.LogWarning("TILE MANAGER: CardEffectIndicatorPrefab is null");
            return;
        }
        
        // Get all pieces with active effects
        var piecesWithEffects = cardEffectProcessor.GetPiecesWithActiveEffects();
        
        // Debug logging
        if (Time.frameCount % 60 == 0) // Only log once per second
        {
            Debug.Log($"TILE MANAGER: Found {piecesWithEffects.Count} pieces with active effects");
        }
        
        // Remove indicators for pieces that no longer have effects
        var indicatorsToRemove = new List<PieceController>();
        foreach (var kvp in activeEffectIndicators)
        {
            if (!piecesWithEffects.Contains(kvp.Key) || kvp.Key == null)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                    Debug.Log($"TILE MANAGER: Removed effect indicator for piece: {kvp.Key?.name ?? "NULL"}");
                }
                indicatorsToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var piece in indicatorsToRemove)
        {
            activeEffectIndicators.Remove(piece);
        }
        
        // Add indicators for pieces that need them
        foreach (var piece in piecesWithEffects)
        {
            if (piece != null && !activeEffectIndicators.ContainsKey(piece))
            {
                CreateEffectIndicator(piece);
            }
        }
        
        // Update existing indicators
        UpdateIndicatorAnimations();
    }
    
    /// <summary>
    /// Create a visual indicator above a piece with active card effects
    /// </summary>
    private void CreateEffectIndicator(PieceController piece)
    {
        if (piece == null)
        {
            Debug.LogWarning("TILE MANAGER: Cannot create indicator - piece is null");
            return;
        }
        
        if (cardEffectIndicatorPrefab == null)
        {
            Debug.LogWarning("TILE MANAGER: Cannot create indicator - cardEffectIndicatorPrefab is null");
            return;
        }
        
        // Calculate position above the piece
        Vector3 indicatorPosition = piece.transform.position + Vector3.up * indicatorHeight;
        
        Debug.Log($"TILE MANAGER: Creating effect indicator for piece: {piece.name} at position {indicatorPosition}");
        
        // Create the indicator
        GameObject indicator = Instantiate(cardEffectIndicatorPrefab, indicatorPosition, Quaternion.identity);
        
        if (indicator == null)
        {
            Debug.LogError("TILE MANAGER: Failed to instantiate effect indicator");
            return;
        }
        
        // Set up the indicator with card data if available
        SetupEffectIndicator(indicator, piece);
        
        // Store reference
        activeEffectIndicators[piece] = indicator;
        
        Debug.Log($"TILE MANAGER: Successfully created card effect indicator for piece: {piece.name}");
    }
    
    /// <summary>
    /// Setup the effect indicator with appropriate card data and visuals
    /// </summary>
    private void SetupEffectIndicator(GameObject indicator, PieceController piece)
    {
        if (indicator == null)
        {
            Debug.LogWarning("TILE MANAGER: Cannot setup indicator - indicator is null");
            return;
        }
        
        if (piece?.pieceData == null)
        {
            Debug.LogWarning("TILE MANAGER: Cannot setup indicator - piece or piece data is null");
            return;
        }
        
        // Try to find an active card for this piece's effects
        ChessCardData activeCard = GetActiveCardForPiece(piece);
        
        Debug.Log($"TILE MANAGER: Setting up indicator for piece {piece.name}, active card: {activeCard?.cardName ?? "NULL"}");
        
        if (activeCard != null)
        {
            // Setup card hover effect component on the indicator
            ChessCardHoverEffect indicatorCardEffect = indicator.GetComponent<ChessCardHoverEffect>();
            if (indicatorCardEffect == null)
            {
                indicatorCardEffect = indicator.AddComponent<ChessCardHoverEffect>();
                Debug.Log($"TILE MANAGER: Added ChessCardHoverEffect component to indicator");
            }
            
            // Get the piece's player index
            int piecePlayerIndex = GetPiecePlayerIndex(piece);
            if (piecePlayerIndex == -1) piecePlayerIndex = 1; // Fallback
            
            Debug.Log($"TILE MANAGER: Piece player index: {piecePlayerIndex}");
            
            // Setup card with data and audio from manager
            if (cardManager != null)
            {
                cardManager.SetupCardHoverEffect(indicatorCardEffect, activeCard, piecePlayerIndex);
                Debug.Log($"TILE MANAGER: Setup card data via CardManager");
            }
            else
            {
                // Fallback setup
                indicatorCardEffect.SetCardData(activeCard, piecePlayerIndex);
                Debug.Log($"TILE MANAGER: Setup card data via fallback method");
            }
            
            // Disable interaction on indicators (they're just visual)
            indicatorCardEffect.canBeActivated = false;
            
            // Make the indicator face the camera
            if (cameraSwitch != null && cameraSwitch.currentCamera != null)
            {
                indicator.transform.LookAt(cameraSwitch.currentCamera.transform);
                indicator.transform.Rotate(0, 180, 0); // Flip to face camera correctly
            }
        }
        
        // Add bobbing animation component if not present
        CardEffectIndicatorAnimation animation = indicator.GetComponent<CardEffectIndicatorAnimation>();
        if (animation == null)
        {
            animation = indicator.AddComponent<CardEffectIndicatorAnimation>();
        }
        
        // Configure animation
        animation.Initialize(indicatorRotationSpeed, indicatorBobSpeed, indicatorBobHeight);
    }
    
    /// <summary>
    /// Update animations for all active indicators
    /// </summary>
    private void UpdateIndicatorAnimations()
    {
        foreach (var kvp in activeEffectIndicators)
        {
            if (kvp.Value != null && kvp.Key != null)
            {
                // Update position to follow piece
                Vector3 targetPosition = kvp.Key.transform.position + Vector3.up * indicatorHeight;
                kvp.Value.transform.position = Vector3.Lerp(kvp.Value.transform.position, targetPosition, Time.deltaTime * 5f);
                
                // Update camera facing
                if (cameraSwitch != null && cameraSwitch.currentCamera != null)
                {
                    kvp.Value.transform.LookAt(cameraSwitch.currentCamera.transform);
                    kvp.Value.transform.Rotate(0, 180, 0);
                }
            }
        }
    }
    
    /// <summary>
    /// Get the active card data that's affecting a piece
    /// </summary>
    private ChessCardData GetActiveCardForPiece(PieceController piece)
    {
        if (piece?.pieceData == null)
            return null;
        
        // Use CardEffectProcessor to get the active card for this piece
        if (cardEffectProcessor != null)
        {
            return cardEffectProcessor.GetActiveCardForPiece(piece);
        }
        
        // Fallback: get from card manager
        if (cardManager == null)
            return null;
        
        // Get the piece's player index
        int piecePlayerIndex = GetPiecePlayerIndex(piece);
        if (piecePlayerIndex == -1)
            return null;
        
        // Get active cards for this player
        var activeCards = cardManager.GetActiveCards(piecePlayerIndex);
        
        // Return the first active card
        if (activeCards.Count > 0)
        {
            return activeCards[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// Remove effect indicator for a specific piece
    /// </summary>
    public void RemoveEffectIndicator(PieceController piece)
    {
        if (piece != null && activeEffectIndicators.ContainsKey(piece))
        {
            if (activeEffectIndicators[piece] != null)
            {
                Destroy(activeEffectIndicators[piece]);
            }
            activeEffectIndicators.Remove(piece);
            
            Debug.Log($"Removed card effect indicator for piece: {piece.name}");
        }
    }
    
    /// <summary>
    /// Remove all effect indicators
    /// </summary>
    public void RemoveAllEffectIndicators()
    {
        foreach (var indicator in activeEffectIndicators.Values)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }
        activeEffectIndicators.Clear();
        
        Debug.Log("Removed all card effect indicators");
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Debug: Test Card Effect on First Piece")]
    private void DebugTestCardEffectOnFirstPiece()
    {
        // Find the first piece in the scene
        PieceController firstPiece = FindObjectOfType<PieceController>();
        if (firstPiece == null)
        {
            Debug.LogWarning("No pieces found in scene");
            return;
        }
        
        Debug.Log($"Testing card effect on piece: {firstPiece.name}");
        
        // Apply a dummy effect to test the visual indicator
        if (cardEffectProcessor != null && gameManagerChess != null)
        {
            // Get a test card from available cards
            var availableCards = gameManagerChess.GetAllAvailableCards();
            if (availableCards != null && availableCards.Count > 0)
            {
                ChessCardData testCard = availableCards[0];
                Debug.Log($"Applying test card: {testCard.cardName}");
                
                bool success = cardEffectProcessor.ApplyCardEffectToPiece(testCard, firstPiece, 1);
                Debug.Log($"Card effect application result: {success}");
            }
            else
            {
                Debug.LogWarning("No available cards found");
            }
        }
        else
        {
            Debug.LogWarning("CardEffectProcessor or GameManagerChess is null");
        }
    }
    
    [ContextMenu("Debug: Show Effect Indicator Status")]
    private void DebugShowEffectIndicatorStatus()
    {
        Debug.Log("=== EFFECT INDICATOR STATUS ===");
        Debug.Log($"CardEffectProcessor: {(cardEffectProcessor != null ? "OK" : "NULL")}");
        Debug.Log($"CardEffectIndicatorPrefab: {(cardEffectIndicatorPrefab != null ? "OK" : "NULL")}");
        Debug.Log($"Active Effect Indicators: {activeEffectIndicators.Count}");
        
        if (cardEffectProcessor != null)
        {
            var piecesWithEffects = cardEffectProcessor.GetPiecesWithActiveEffects();
            Debug.Log($"Pieces with active effects: {piecesWithEffects.Count}");
            
            foreach (var piece in piecesWithEffects)
            {
                if (piece != null)
                {
                    var activeCard = cardEffectProcessor.GetActiveCardForPiece(piece);
                    Debug.Log($"  - {piece.name}: {activeCard?.cardName ?? "No card data"}");
                }
            }
        }
        Debug.Log("==============================");
    }
    
    [ContextMenu("Debug: Force Create Indicators")]
    private void DebugForceCreateIndicators()
    {
        Debug.Log("Forcing update of card effect indicators...");
        UpdateCardEffectIndicators();
    }
    
    #endregion
}
