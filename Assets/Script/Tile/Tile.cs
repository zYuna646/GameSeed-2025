using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Bidak.Manager;
using Bidak.Data;
using Bidak.Card;

public class Tile : MonoBehaviour
{
    public Color originColor;
    public Color highlightColor;
    public MeshRenderer MeshRenderer;
    public TileManager tileManager;
    private TileController tileController;
    public bool inSight = false;
    public bool hover = false;
    public bool rised = false;
    public bool contained = false;
    public bool canHover = true;
    public float normalHeight;
    public Vector3 normalScale;
    [SerializeField] LayerMask test;
    [SerializeField] LayerMask test2;
    
    [Header("Card Integration")]
    public Color cardTargetValidColor = Color.green;
    public Color cardTargetInvalidColor = Color.red;
    public Color cardHoverColor = Color.yellow;
    public float cardFloatHeight = 2f;
    public float cardFloatSpeed = 2f;
    public GameObject floatingCardPrefab;
    
    private CardTargetingSystem cardTargetingSystem;
    private CardEffectProcessor cardEffectProcessor;
    private bool isCardTarget = false;
    private bool isValidCardTarget = false;
    private GameObject floatingCardInstance;
    private Coroutine cardFloatCoroutine;

    [SerializeField] public int row;
    [SerializeField] public int collumn;
    // Start is called before the first frame update
    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        MeshRenderer = GetTileSelectRenderer();
        tileController = GetComponent<TileController>();
        cardTargetingSystem = FindObjectOfType<CardTargetingSystem>();
        cardEffectProcessor = FindObjectOfType<CardEffectProcessor>();
        
        originColor = MeshRenderer.material.color;
        
        normalHeight = transform.position.y;
        normalScale = transform.localScale;
        
        row = tileController.tileData.row + 1;
        collumn = tileController.tileData.column + 1;
        
        // Subscribe to card targeting events
        if (cardTargetingSystem != null)
        {
            // We'll add event subscriptions here when CardTargetingSystem is updated
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for card targeting
        CheckCardTargeting();
        
        if (!tileManager.isPicked && !isCardTarget)
        {
            if (inSight)
            {
                CheckInArray();
            }
            if (!hover && rised && tileManager.pieceColliders.Length == 0)
            {
                tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                MeshRenderer.material.color = originColor;
            }
        }else if(tileManager.isPicked && !isCardTarget)
        {
            if (!tileManager.isPicked)
            {
                tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                MeshRenderer.material.color = originColor;
            }
        }
        if(rised && hover && tileManager.tileColliders.Length == 0 && !tileManager.isPicked && !isCardTarget)
        {
            tileManager.ScaleDown(this.transform, normalHeight, normalScale);
            MeshRenderer.material.color = originColor;
        }
        
        CheckContain();
    }
    MeshRenderer GetTileSelectRenderer()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("TileSelect"))
            {
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                    return renderer;
            }
        }
        return null;
    }

    void CheckInArray()
    {
        if (tileManager.tileColliders != null)
        {
            for (int i = 0; i < tileManager.tileColliders.Length; i++)
            {
                if (this.name == tileManager.tileColliders[i].name)
                {
                    hover = true;
                }
                else
                {
                    hover = false;
                    if (rised)
                    {
                        tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                        MeshRenderer.material.color = originColor;
                        inSight = false;
                    }
                }
            }
        }
        if (tileManager.tileColliders.Length == 0)
        {
            hover = false;
            if (rised)
            {
                tileManager.ScaleDown(this.transform, normalHeight, normalScale);
                MeshRenderer.material.color = originColor;
                inSight = false;
            }
        }
    }
    void CheckContain()
    {
        if (transform.GetComponentInChildren<PieceType>() != null)
        {
            contained = true;
        }
        else
        {
            contained = false;
        }
    }
    
    #region Card Integration Methods
    
    /// <summary>
    /// Check if this tile is being targeted by a card effect
    /// </summary>
    void CheckCardTargeting()
    {
        if (cardTargetingSystem == null || !cardTargetingSystem.IsCurrentlyTargeting())
        {
            if (isCardTarget)
            {
                EndCardTargeting();
            }
            return;
        }
        
        // Check if mouse is over this tile
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                if (!isCardTarget)
                {
                    StartCardTargeting();
                }
                
                // Check if this is a valid target
                bool wasValid = isValidCardTarget;
                isValidCardTarget = ValidateCardTarget();
                
                if (wasValid != isValidCardTarget)
                {
                    UpdateCardTargetVisuals();
                }
            }
            else if (isCardTarget)
            {
                EndCardTargeting();
            }
        }
        else if (isCardTarget)
        {
            EndCardTargeting();
        }
    }
    
    /// <summary>
    /// Start card targeting on this tile
    /// </summary>
    void StartCardTargeting()
    {
        isCardTarget = true;
        isValidCardTarget = ValidateCardTarget();
        
        // Show visual feedback
        UpdateCardTargetVisuals();
        
        // Show floating card
        ShowFloatingCard();
        
        Debug.Log($"Card targeting started on tile {name}");
    }
    
    /// <summary>
    /// End card targeting on this tile
    /// </summary>
    void EndCardTargeting()
    {
        isCardTarget = false;
        isValidCardTarget = false;
        
        // Restore original visuals
        if (MeshRenderer != null)
        {
            MeshRenderer.material.color = originColor;
        }
        
        // Hide floating card
        HideFloatingCard();
        
        Debug.Log($"Card targeting ended on tile {name}");
    }
    
    /// <summary>
    /// Validate if this tile can be targeted by the current card
    /// </summary>
    bool ValidateCardTarget()
    {
        if (cardTargetingSystem == null || cardTargetingSystem.selectedCard == null)
            return false;
            
        var cardData = cardTargetingSystem.selectedCard.cardData;
        if (cardData == null)
            return false;
        
        // Check if tile has a piece
        PieceController pieceOnTile = GetPieceOnTile();
        
        if (pieceOnTile == null)
        {
            // Some effects can target empty tiles
            return CanTargetEmptyTile(cardData);
        }
        
        // Check piece-specific targeting
        return CanTargetPiece(cardData, pieceOnTile);
    }
    
    /// <summary>
    /// Check if card can target empty tiles
    /// </summary>
    bool CanTargetEmptyTile(ChessCardData cardData)
    {
        foreach (var effect in cardData.cardEffects)
        {
            switch (effect.effectType)
            {
                case CardEffectType.BackFromDead:
                case CardEffectType.StoneTomorrow:
                case CardEffectType.LeapMove:
                    return true; // These effects can target empty tiles
            }
        }
        return false;
    }
    
    /// <summary>
    /// Check if card can target specific piece
    /// </summary>
    bool CanTargetPiece(ChessCardData cardData, PieceController piece)
    {
        if (piece == null || piece.pieceData == null)
            return false;
            
        // Get player index of the card
        int cardPlayerIndex = cardTargetingSystem.selectedCard.playerIndex;
        
        // Use CardEffectProcessor for sophisticated targeting validation
        if (cardEffectProcessor != null)
        {
            return cardEffectProcessor.CanTargetPieceWithCard(piece, cardData, cardPlayerIndex);
        }
        
        // Fallback to basic validation
        foreach (var effect in cardData.cardEffects)
        {
            // Check if effect affects allied or enemy pieces
            if (effect.parameters.affectsAlliedPieces)
            {
                // Check if piece belongs to same player
                if (IsSamePlayer(piece, cardPlayerIndex))
                    return true;
            }
            
            if (effect.parameters.affectsEnemyPieces)
            {
                // Check if piece belongs to different player
                if (!IsSamePlayer(piece, cardPlayerIndex))
                    return true;
            }
            
            // Default: most effects target own pieces
            if (!effect.parameters.affectsEnemyPieces && !effect.parameters.affectsAlliedPieces)
            {
                if (IsSamePlayer(piece, cardPlayerIndex))
                    return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if piece belongs to the same player as the card
    /// </summary>
    bool IsSamePlayer(PieceController piece, int cardPlayerIndex)
    {
        // This is a simplified check - you might need to implement proper player identification
        // For now, assume player 1 has white pieces, player 2 has black pieces
        bool isWhitePiece = piece.pieceData.playerType == Color.white;
        bool isPlayer1Card = cardPlayerIndex == 1;
        
        return (isWhitePiece && isPlayer1Card) || (!isWhitePiece && !isPlayer1Card);
    }
    
    /// <summary>
    /// Update visual feedback for card targeting
    /// </summary>
    void UpdateCardTargetVisuals()
    {
        if (MeshRenderer == null) return;
        
        if (isValidCardTarget)
        {
            MeshRenderer.material.color = cardTargetValidColor;
            
            // Scale up tile for valid targets
            if (!rised)
            {
                tileManager.ScaleUp(transform);
                rised = true;
            }
        }
        else
        {
            MeshRenderer.material.color = cardTargetInvalidColor;
        }
    }
    
    /// <summary>
    /// Show floating card above this tile
    /// </summary>
    void ShowFloatingCard()
    {
        if (floatingCardPrefab == null || floatingCardInstance != null)
            return;
            
        // Get card data from targeting system
        var cardData = cardTargetingSystem.selectedCard?.cardData;
        if (cardData == null) return;
        
        // Calculate position above tile (or above piece if there is one)
        Vector3 floatPosition = transform.position;
        PieceController piece = GetPieceOnTile();
        
        if (piece != null)
        {
            // Float above the piece
            floatPosition = piece.transform.position;
            floatPosition.y += cardFloatHeight;
        }
        else
        {
            // Float above the tile
            floatPosition.y += cardFloatHeight * 0.5f;
        }
        
        // Instantiate floating card
        floatingCardInstance = Instantiate(floatingCardPrefab, floatPosition, Quaternion.identity);
        
        // Optional: Set card visual data
        SetFloatingCardData(floatingCardInstance, cardData);
        
        // Start floating animation
        if (cardFloatCoroutine != null)
        {
            StopCoroutine(cardFloatCoroutine);
        }
        cardFloatCoroutine = StartCoroutine(FloatCardAnimation());
    }
    
    /// <summary>
    /// Hide floating card
    /// </summary>
    void HideFloatingCard()
    {
        if (floatingCardInstance != null)
        {
            if (cardFloatCoroutine != null)
            {
                StopCoroutine(cardFloatCoroutine);
                cardFloatCoroutine = null;
            }
            
            Destroy(floatingCardInstance);
            floatingCardInstance = null;
        }
    }
    
    /// <summary>
    /// Set visual data for floating card
    /// </summary>
    void SetFloatingCardData(GameObject cardInstance, ChessCardData cardData)
    {
        // Try to find and set card name/image
        var textComponents = cardInstance.GetComponentsInChildren<UnityEngine.UI.Text>();
        foreach (var text in textComponents)
        {
            if (text.name.Contains("Name") || text.name.Contains("Title"))
            {
                text.text = cardData.cardName;
                break;
            }
        }
        
        // Try to set card image
        var imageComponents = cardInstance.GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var image in imageComponents)
        {
            if (image.name.Contains("Icon") || image.name.Contains("Image"))
            {
                if (cardData.cardImage != null)
                {
                    image.sprite = cardData.cardImage;
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// Floating animation for card
    /// </summary>
    IEnumerator FloatCardAnimation()
    {
        if (floatingCardInstance == null) yield break;
        
        Vector3 basePosition = floatingCardInstance.transform.position;
        float time = 0f;
        
        while (floatingCardInstance != null)
        {
            time += Time.deltaTime * cardFloatSpeed;
            
            // Simple sine wave floating motion
            float yOffset = Mathf.Sin(time) * 0.2f;
            Vector3 newPosition = basePosition;
            newPosition.y += yOffset;
            
            floatingCardInstance.transform.position = newPosition;
            
            // Optional: slight rotation
            floatingCardInstance.transform.Rotate(0, Time.deltaTime * 30f, 0);
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Get piece controller on this tile
    /// </summary>
    PieceController GetPieceOnTile()
    {
        return GetComponentInChildren<PieceController>();
    }
    
    /// <summary>
    /// Apply card effect to piece on this tile
    /// </summary>
    public void ApplyCardEffectToTile()
    {
        if (!isValidCardTarget || cardTargetingSystem == null || cardTargetingSystem.selectedCard == null)
            return;
            
        var cardData = cardTargetingSystem.selectedCard.cardData;
        var selectedCard = cardTargetingSystem.selectedCard;
        PieceController piece = GetPieceOnTile();
        
        if (piece != null)
        {
            // Use CardEffectProcessor to apply effect
            bool success = false;
            
            if (cardEffectProcessor != null)
            {
                success = cardEffectProcessor.ApplyCardEffectToPiece(cardData, piece, selectedCard.playerIndex);
            }
            else
            {
                // Fallback: Apply effect directly to piece
                success = piece.ApplyCardEffect(cardData);
            }
            
            if (success)
            {
                Debug.Log($"Applied card effect {cardData.cardName} to piece {piece.name} on tile {name}");
                
                // Trigger visual effect
                StartCoroutine(ShowEffectApplication());
            }
            else
            {
                Debug.LogWarning($"Failed to apply card effect {cardData.cardName} to piece {piece.name} on tile {name}");
            }
        }
        else if (CanTargetEmptyTile(cardData))
        {
            // Handle effects that target empty tiles
            HandleEmptyTileEffect(cardData);
        }
        
        // End targeting
        EndCardTargeting();
    }
    
    /// <summary>
    /// Handle card effects that target empty tiles
    /// </summary>
    void HandleEmptyTileEffect(ChessCardData cardData)
    {
        foreach (var effect in cardData.cardEffects)
        {
            switch (effect.effectType)
            {
                case CardEffectType.BackFromDead:
                    // Could spawn a revived piece here
                    Debug.Log($"BackFromDead effect applied to empty tile {name}");
                    break;
                    
                case CardEffectType.StoneTomorrow:
                    // Could mark tile for promotion effect
                    Debug.Log($"StoneTomorrow effect applied to tile {name}");
                    break;
            }
        }
    }
    
    /// <summary>
    /// Show visual effect when card is applied
    /// </summary>
    IEnumerator ShowEffectApplication()
    {
        // Flash the tile
        Color originalColor = MeshRenderer.material.color;
        
        for (int i = 0; i < 3; i++)
        {
            MeshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            MeshRenderer.material.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Restore original color
        MeshRenderer.material.color = originColor;
    }
    
    /// <summary>
    /// Handle mouse click for card targeting
    /// </summary>
    void OnMouseDown()
    {
        if (isCardTarget && isValidCardTarget)
        {
            ApplyCardEffectToTile();
        }
    }
    
    #endregion
}
