using UnityEngine;
using System.Collections.Generic;
using Bidak.Data;
using Bidak.Manager;

namespace Bidak.Card
{
    /// <summary>
    /// Handles the processing and application of card effects to pieces and tiles
    /// </summary>
    public class CardEffectProcessor : MonoBehaviour
    {
        [Header("Effect Processing")]
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject effectApplicationPrefab;
        [SerializeField] private float effectDuration = 2f;
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color failureColor = Color.red;
        
        // References
        private GameManagerChess gameManager;
        private ChessCardManager cardManager;
        private TileManager tileManager;
        
        // Active effects tracking
        private Dictionary<PieceController, List<ICardEffect>> activePieceEffects = new Dictionary<PieceController, List<ICardEffect>>();
        
        // Track which cards are applied to which pieces (for visual indicators)
        private Dictionary<PieceController, ChessCardData> pieceToCardMapping = new Dictionary<PieceController, ChessCardData>();
        
        private void Start()
        {
            // Initialize references
            gameManager = GameManagerChess.Instance;
            cardManager = FindObjectOfType<ChessCardManager>();
            tileManager = FindObjectOfType<TileManager>();
            
            // Subscribe to game events
            if (gameManager != null)
            {
                GameManagerChess.OnPlayerTurnChanged += OnPlayerTurnChanged;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                GameManagerChess.OnPlayerTurnChanged -= OnPlayerTurnChanged;
            }
        }
        
        /// <summary>
        /// Apply a card's effects to a target piece
        /// </summary>
        public bool ApplyCardEffectToPiece(ChessCardData cardData, PieceController targetPiece, int playerIndex)
        {
            if (cardData == null || targetPiece == null)
            {
                LogError("ApplyCardEffectToPiece: Invalid card data or target piece");
                return false;
            }
            
            LogDebug($"Applying card {cardData.cardName} to piece {targetPiece.name} by player {playerIndex}");
            
            bool anyEffectApplied = false;
            
            // Process each effect in the card
            foreach (var effectData in cardData.cardEffects)
            {
                if (effectData != null && effectData.isEnabled)
                {
                    bool success = ApplyIndividualEffect(effectData, targetPiece, playerIndex);
                    if (success)
                    {
                        anyEffectApplied = true;
                        LogDebug($"Successfully applied effect {effectData.effectType} to {targetPiece.name}");
                    }
                    else
                    {
                        LogDebug($"Failed to apply effect {effectData.effectType} to {targetPiece.name}");
                    }
                }
            }
            
            if (anyEffectApplied)
            {
                // Show visual feedback
                ShowEffectApplicationFeedback(targetPiece.transform.position, true);
                
                // Track the effects on this piece
                TrackPieceEffects(targetPiece, cardData);
                
                // Notify card manager
                if (cardManager != null)
                {
                    cardManager.ActivateCard(playerIndex, cardData);
                }
            }
            else
            {
                // Show failure feedback
                ShowEffectApplicationFeedback(targetPiece.transform.position, false);
            }
            
            return anyEffectApplied;
        }
        
        /// <summary>
        /// Apply an individual card effect
        /// </summary>
        private bool ApplyIndividualEffect(CardEffectData effectData, PieceController targetPiece, int playerIndex)
        {
            if (targetPiece.pieceData == null)
                return false;
            
            switch (effectData.effectType)
            {
                case CardEffectType.DoubleMove:
                    return ApplyMovementEffect(targetPiece, 2, effectData.parameters.turnDuration);
                    
                case CardEffectType.TripleMove:
                    return ApplyMovementEffect(targetPiece, 3, effectData.parameters.turnDuration);
                    
                case CardEffectType.StraightMove:
                    return ApplyStraightMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.DiagonalAttack:
                    return ApplyDiagonalAttackEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.QueenCollision:
                    return ApplyQueenCollisionEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.RoyalCommand:
                    return ApplyRoyalCommandEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.WhereIsMyDefense:
                    return ApplyDefenseEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.NotToday:
                    return ApplyNotTodayEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.UnstoppableForce:
                    return ApplyUnstoppableForceEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.DanceLikeQueen:
                    return ApplyDanceLikeQueenEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.DanceLikeElephant:
                    return ApplyDanceLikeElephantEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.BackFromDead:
                    return ApplyBackFromDeadEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.StoneTomorrow:
                    return ApplyStoneTomorrowEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.BlockadeMove:
                    return ApplyBlockadeMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.ForwardTwoMoves:
                    return ApplyForwardTwoMovesEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.TwoDirectionMove:
                    return ApplyTwoDirectionMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.PowerfulMove:
                    return ApplyPowerfulMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.NiceDay:
                    return ApplyNiceDayEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.BackMove:
                    return ApplyBackMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.LeapMove:
                    return ApplyLeapMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.RestoreMove:
                    return ApplyRestoreMoveEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.TimeFrozen:
                    return ApplyTimeFrozenEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.ProtectedRing:
                    return ApplyProtectedRingEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.IGotYou:
                    return ApplyIGotYouEffect(targetPiece, effectData.parameters);
                    
                case CardEffectType.SpecialMove:
                    return ApplySpecialMoveEffect(targetPiece, effectData.parameters);
                    
                default:
                    LogError($"Unknown effect type: {effectData.effectType}");
                    return false;
            }
        }
        
        #region Effect Implementations
        
        private bool ApplyMovementEffect(PieceController piece, int moveCount, int duration)
        {
            // Add movement bonus to piece data
            if (piece.pieceData != null)
            {
                // Store original move count if not already stored
                if (!piece.pieceData.cardEffectMoveCountBonus.HasValue)
                {
                    piece.pieceData.cardEffectMoveCountBonus = 0;
                }
                
                piece.pieceData.cardEffectMoveCountBonus += moveCount;
                piece.pieceData.cardEffectDuration = duration;
                
                LogDebug($"Applied {moveCount} extra moves to {piece.name} for {duration} turns");
                return true;
            }
            return false;
        }
        
        private bool ApplyStraightMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                // Enable straight-line movement only
                piece.pieceData.cardEffectCanMoveStraight = true;
                piece.pieceData.cardEffectCanMoveDiagonally = false;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied straight move restriction to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyDiagonalAttackEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanAttackDiagonally = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied diagonal attack ability to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyQueenCollisionEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                // Queen-like movement and attack
                piece.pieceData.cardEffectCanMoveStraight = true;
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectCanAttackDiagonally = true;
                piece.pieceData.cardEffectCanAttackStraight = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Queen Collision effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyRoyalCommandEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                // King-like movement with protection
                piece.pieceData.cardEffectHasLightProtection = true;
                piece.pieceData.cardEffectCanMoveStraight = true;
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Royal Command effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyDefenseEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectHasLightProtection = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied defense effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyNotTodayEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectHasFullProtection = true;
                piece.pieceData.cardEffectCanSwapPositions = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Not Today effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyUnstoppableForceEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanAttackStraight = true;
                piece.pieceData.cardEffectHasLightProtection = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Unstoppable Force effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyDanceLikeQueenEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                // Queen-like movement and attack with double move
                piece.pieceData.cardEffectCanMoveStraight = true;
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectCanAttackDiagonally = true;
                piece.pieceData.cardEffectCanAttackStraight = true;
                piece.pieceData.cardEffectMoveCountBonus = (piece.pieceData.cardEffectMoveCountBonus ?? 0) + 2;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Dance Like Queen effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyDanceLikeElephantEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                // Bishop-like movement with enhanced attack
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectCanAttackDiagonally = true;
                piece.pieceData.cardEffectMoveCountBonus = (piece.pieceData.cardEffectMoveCountBonus ?? 0) + 1;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Dance Like Elephant effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyBackFromDeadEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null && piece.pieceData.isDead)
            {
                piece.pieceData.isDead = false;
                piece.pieceData.cardEffectCanRevive = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                // Re-enable the piece GameObject if it was disabled
                piece.gameObject.SetActive(true);
                
                LogDebug($"Applied Back From Dead effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyStoneTomorrowEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null && piece.pieceData.pieceType == ChessPieceData.PieceType.Pawn)
            {
                piece.pieceData.cardEffectCanPromoteInPlace = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Stone Tomorrow effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyBlockadeMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanCreateBlockade = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Blockade Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyForwardTwoMovesEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectStepsForward = parameters.stepsForward;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Forward Two Moves effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyTwoDirectionMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanMoveStraight = true;
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Two Direction Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyPowerfulMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanJump = true;
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Powerful Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyNiceDayEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectMoveCountBonus = (piece.pieceData.cardEffectMoveCountBonus ?? 0) + 2;
                piece.pieceData.cardEffectCanAttackDiagonally = false;
                piece.pieceData.cardEffectCanAttackStraight = false;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Nice Day effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyBackMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanMoveBackward = true;
                piece.pieceData.cardEffectStepsForward = parameters.stepsForward;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Back Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyLeapMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanJump = true;
                piece.pieceData.cardEffectMoveCountBonus = (piece.pieceData.cardEffectMoveCountBonus ?? 0) + 2;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Leap Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyRestoreMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                // Reset piece to its default state and add restoration bonus
                piece.pieceData.isFirstMove = true;
                piece.pieceData.cardEffectMoveCountBonus = (piece.pieceData.cardEffectMoveCountBonus ?? 0) + 1;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Restore Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyTimeFrozenEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectHasFullProtection = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                // Temporarily freeze the piece
                piece.pieceData.cardEffectIsFrozen = true;
                
                LogDebug($"Applied Time Frozen effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyProtectedRingEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectHasLightProtection = true;
                piece.pieceData.cardEffectProtectionDuration = parameters.protectionDuration;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Protected Ring effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplyIGotYouEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectHasLightProtection = true;
                piece.pieceData.cardEffectCanMoveStraight = true;
                piece.pieceData.cardEffectCanMoveDiagonally = true;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied I Got You effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        private bool ApplySpecialMoveEffect(PieceController piece, CardEffectParameters parameters)
        {
            if (piece.pieceData != null)
            {
                piece.pieceData.cardEffectCanMoveSideways = true;
                piece.pieceData.cardEffectMoveCountBonus = (piece.pieceData.cardEffectMoveCountBonus ?? 0) + 1;
                piece.pieceData.cardEffectDuration = parameters.turnDuration;
                
                LogDebug($"Applied Special Move effect to {piece.name}");
                return true;
            }
            return false;
        }
        
        #endregion
        
        /// <summary>
        /// Track effects applied to a piece
        /// </summary>
        private void TrackPieceEffects(PieceController piece, ChessCardData cardData)
        {
            if (!activePieceEffects.ContainsKey(piece))
            {
                activePieceEffects[piece] = new List<ICardEffect>();
            }
            
            // Store the card data reference for visual indicators
            pieceToCardMapping[piece] = cardData;
            
            // Store the card data reference in the piece for later use by indicators
            if (piece.pieceData != null)
            {
                // Ensure the piece has an active effect duration
                if (piece.pieceData.cardEffectDuration <= 0)
                {
                    piece.pieceData.cardEffectDuration = GetDefaultEffectDuration(cardData);
                }
            }
            
            LogDebug($"Tracking {cardData.cardEffects.Count} effects on piece {piece.name} with duration {piece.pieceData?.cardEffectDuration}");
            LogDebug($"Stored card mapping: {piece.name} -> {cardData.cardName}");
        }
        
        /// <summary>
        /// Get default effect duration for a card
        /// </summary>
        private int GetDefaultEffectDuration(ChessCardData cardData)
        {
            if (cardData?.cardEffects != null && cardData.cardEffects.Count > 0)
            {
                foreach (var effect in cardData.cardEffects)
                {
                    if (effect.parameters.turnDuration > 0)
                    {
                        return effect.parameters.turnDuration;
                    }
                }
            }
            
            // Default duration if not specified
            return 3;
        }
        
        /// <summary>
        /// Update all active effects each turn
        /// </summary>
        private void OnPlayerTurnChanged(int newPlayerIndex)
        {
            UpdateAllActiveEffects();
        }
        
        /// <summary>
        /// Update duration of all active effects
        /// </summary>
        private void UpdateAllActiveEffects()
        {
            var piecesToRemove = new List<PieceController>();
            
            foreach (var kvp in activePieceEffects)
            {
                PieceController piece = kvp.Key;
                
                if (piece == null || piece.pieceData == null)
                {
                    piecesToRemove.Add(piece);
                    continue;
                }
                
                // Update effect duration
                if (piece.pieceData.cardEffectDuration > 0)
                {
                    piece.pieceData.cardEffectDuration--;
                    
                    if (piece.pieceData.cardEffectDuration <= 0)
                    {
                        // Remove all card effects from piece
                        RemoveAllEffectsFromPiece(piece);
                        piecesToRemove.Add(piece);
                    }
                }
            }
            
            // Clean up finished effects
            foreach (var piece in piecesToRemove)
            {
                activePieceEffects.Remove(piece);
                pieceToCardMapping.Remove(piece); // Also remove from card mapping
            }
            
            LogDebug($"Updated {activePieceEffects.Count} pieces with active effects");
        }
        
        /// <summary>
        /// Remove all card effects from a piece
        /// </summary>
        private void RemoveAllEffectsFromPiece(PieceController piece)
        {
            if (piece.pieceData != null)
            {
                // Reset all card effect properties
                piece.pieceData.cardEffectMoveCountBonus = null;
                piece.pieceData.cardEffectCanMoveStraight = false;
                piece.pieceData.cardEffectCanMoveDiagonally = false;
                piece.pieceData.cardEffectCanAttackDiagonally = false;
                piece.pieceData.cardEffectCanAttackStraight = false;
                piece.pieceData.cardEffectHasLightProtection = false;
                piece.pieceData.cardEffectHasFullProtection = false;
                piece.pieceData.cardEffectCanSwapPositions = false;
                piece.pieceData.cardEffectCanRevive = false;
                piece.pieceData.cardEffectCanPromoteInPlace = false;
                piece.pieceData.cardEffectCanCreateBlockade = false;
                piece.pieceData.cardEffectCanJump = false;
                piece.pieceData.cardEffectCanMoveBackward = false;
                piece.pieceData.cardEffectCanMoveSideways = false;
                piece.pieceData.cardEffectIsFrozen = false;
                piece.pieceData.cardEffectStepsForward = 0;
                piece.pieceData.cardEffectProtectionDuration = 0;
                piece.pieceData.cardEffectDuration = 0;
                
                LogDebug($"Removed all card effects from {piece.name}");
            }
            
            // Also remove from card mapping
            pieceToCardMapping.Remove(piece);
        }
        
        /// <summary>
        /// Show visual feedback for effect application
        /// </summary>
        private void ShowEffectApplicationFeedback(Vector3 position, bool success)
        {
            if (effectApplicationPrefab != null)
            {
                GameObject feedback = Instantiate(effectApplicationPrefab, position + Vector3.up * 2f, Quaternion.identity);
                
                // Change color based on success
                Renderer renderer = feedback.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = success ? successColor : failureColor;
                }
                
                // Destroy after duration
                Destroy(feedback, effectDuration);
            }
        }
        
        /// <summary>
        /// Check if a piece can be targeted by a specific card
        /// </summary>
        public bool CanTargetPieceWithCard(PieceController targetPiece, ChessCardData cardData, int playerIndex)
        {
            if (targetPiece == null || cardData == null)
                return false;
            
            // Check if card effects can be applied to this piece
            foreach (var effect in cardData.cardEffects)
            {
                if (effect.isEnabled && CanApplyEffectToPiece(effect, targetPiece, playerIndex))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a specific effect can be applied to a piece
        /// </summary>
        private bool CanApplyEffectToPiece(CardEffectData effectData, PieceController piece, int playerIndex)
        {
            if (piece.pieceData == null)
                return false;
            
            // Check player ownership for certain effects
            bool isPieceOwnedByPlayer = gameManager != null && gameManager.IsPieceOwnedByPlayer(piece.pieceData, playerIndex - 1);
            
            // Check effect-specific requirements
            switch (effectData.effectType)
            {
                case CardEffectType.BackFromDead:
                    return piece.pieceData.isDead; // Can only revive dead pieces
                    
                case CardEffectType.StoneTomorrow:
                    return piece.pieceData.pieceType == ChessPieceData.PieceType.Pawn; // Only works on pawns
                    
                default:
                    // Most effects work on own pieces that are alive
                    return isPieceOwnedByPlayer && !piece.pieceData.isDead;
            }
        }
        
        /// <summary>
        /// Get all pieces with active card effects
        /// </summary>
        public List<PieceController> GetPiecesWithActiveEffects()
        {
            return new List<PieceController>(activePieceEffects.Keys);
        }
        
        /// <summary>
        /// Get the active card data for a piece (for visual indicators)
        /// </summary>
        public ChessCardData GetActiveCardForPiece(PieceController piece)
        {
            if (piece?.pieceData == null || !activePieceEffects.ContainsKey(piece))
                return null;
            
            // First, try to get the specific card from our mapping
            if (pieceToCardMapping.ContainsKey(piece))
            {
                LogDebug($"Found card mapping for {piece.name}: {pieceToCardMapping[piece].cardName}");
                return pieceToCardMapping[piece];
            }
            
            // Fallback: Get the card from the card manager
            if (cardManager != null && gameManager != null)
            {
                // Get the piece's player index (convert from GameManager 0-based to CardManager 1-based)
                int playerIndex = gameManager.IsPieceOwnedByPlayer(piece.pieceData, 0) ? 1 : 2;
                
                // Get active cards for this player
                var activeCards = cardManager.GetActiveCards(playerIndex);
                
                // Return the first active card
                if (activeCards != null && activeCards.Count > 0)
                {
                    LogDebug($"Using fallback card for {piece.name}: {activeCards[0].cardName}");
                    return activeCards[0];
                }
            }
            
            LogDebug($"No card found for piece {piece.name}");
            return null;
        }
        
        /// <summary>
        /// Get remaining duration for effects on a piece
        /// </summary>
        public int GetEffectDurationForPiece(PieceController piece)
        {
            if (piece?.pieceData != null)
            {
                return piece.pieceData.cardEffectDuration;
            }
            return 0;
        }
        
        #region Debug Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[CardEffectProcessor] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[CardEffectProcessor] {message}");
        }
        
        #endregion
        
        #region Context Menu Debug
        
        #if UNITY_EDITOR
        [ContextMenu("Debug Active Effects")]
        private void DebugActiveEffects()
        {
            Debug.Log("=== ACTIVE CARD EFFECTS ===");
            Debug.Log($"Total pieces with effects: {activePieceEffects.Count}");
            
            foreach (var kvp in activePieceEffects)
            {
                PieceController piece = kvp.Key;
                if (piece?.pieceData != null)
                {
                    Debug.Log($"Piece: {piece.name} - Duration: {piece.pieceData.cardEffectDuration} turns");
                    Debug.Log($"  Move bonus: {piece.pieceData.cardEffectMoveCountBonus}");
                    Debug.Log($"  Can move straight: {piece.pieceData.cardEffectCanMoveStraight}");
                    Debug.Log($"  Can move diagonally: {piece.pieceData.cardEffectCanMoveDiagonally}");
                    Debug.Log($"  Has protection: {piece.pieceData.cardEffectHasLightProtection || piece.pieceData.cardEffectHasFullProtection}");
                }
            }
            Debug.Log("===========================");
        }
        
        [ContextMenu("Clear All Effects")]
        private void DebugClearAllEffects()
        {
            foreach (var piece in activePieceEffects.Keys)
            {
                RemoveAllEffectsFromPiece(piece);
            }
            activePieceEffects.Clear();
            pieceToCardMapping.Clear();
            Debug.Log("Cleared all active card effects and mappings");
        }
        
        [ContextMenu("Debug: Show Card Mappings")]
        public void DebugShowCardMappings()
        {
            Debug.Log("=== CARD PROCESSOR MAPPINGS ===");
            Debug.Log($"Active piece effects: {activePieceEffects.Count}");
            Debug.Log($"Piece to card mappings: {pieceToCardMapping.Count}");
            
            foreach (var kvp in pieceToCardMapping)
            {
                Debug.Log($"  {kvp.Key?.name ?? "NULL"} -> {kvp.Value?.cardName ?? "NULL"}");
            }
            Debug.Log("==============================");
        }
        #endif
        
        #endregion
    }
}