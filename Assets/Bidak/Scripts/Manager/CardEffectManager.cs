using UnityEngine;
using System.Collections.Generic;
using Bidak.Data;
using Bidak.Card;

namespace Bidak.Manager
{
    /// <summary>
    /// Manages the application and execution of card effects on chess pieces
    /// </summary>
    public class CardEffectManager : MonoBehaviour
    {
        [Header("Effect Management")]
        [SerializeField] private List<ActiveCardEffect> activeEffects = new List<ActiveCardEffect>();
        
        [Header("Turn Management")]
        [SerializeField] private int currentTurn = 0;
        
        // Events
        public System.Action<ChessPieceData, CardEffectData> OnEffectApplied;
        public System.Action<ChessPieceData, CardEffectType> OnEffectRemoved;
        public System.Action<int> OnTurnChanged;
        
        private static CardEffectManager instance;
        public static CardEffectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CardEffectManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("CardEffectManager");
                        instance = go.AddComponent<CardEffectManager>();
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Apply a card effect from a card to a target piece
        /// </summary>
        public bool ApplyCardEffect(ChessCardData cardData, ChessPieceData targetPiece, PieceController pieceController)
        {
            if (cardData == null || targetPiece == null || cardData.cardEffects.Count == 0)
            {
                Debug.LogWarning("Cannot apply card effect: Invalid card or target piece");
                return false;
            }
            
            bool appliedAny = false;
            
            // Apply all effects from the card
            foreach (var effectData in cardData.cardEffects)
            {
                if (effectData.isEnabled && CanApplyEffectToTarget(effectData, targetPiece))
                {
                    ApplyEffect(effectData, targetPiece, pieceController);
                    appliedAny = true;
                }
            }
            
            return appliedAny;
        }
        
        /// <summary>
        /// Apply a specific effect to a target piece
        /// </summary>
        public bool ApplyEffect(CardEffectData effectData, ChessPieceData targetPiece, PieceController pieceController)
        {
            if (!CanApplyEffectToTarget(effectData, targetPiece))
                return false;
            
            // Create active effect tracking
            var activeEffect = new ActiveCardEffect
            {
                effectData = effectData.Clone(),
                targetPiece = targetPiece,
                pieceController = pieceController,
                startTurn = currentTurn,
                remainingDuration = effectData.parameters.turnDuration
            };
            
            activeEffects.Add(activeEffect);
            
            // Apply effect to piece data
            targetPiece.ApplyCardEffect(effectData);
            
            // Apply specific effect logic
            ApplySpecificEffect(effectData, targetPiece, pieceController);
            
            OnEffectApplied?.Invoke(targetPiece, effectData);
            
            Debug.Log($"Applied card effect {effectData.effectType} to {targetPiece.pieceName}");
            return true;
        }
        
        /// <summary>
        /// Remove a specific effect from a target piece
        /// </summary>
        public void RemoveEffect(CardEffectType effectType, ChessPieceData targetPiece)
        {
            // Remove from active effects
            activeEffects.RemoveAll(effect => 
                effect.effectData.effectType == effectType && 
                effect.targetPiece == targetPiece);
            
            // Remove from piece data
            targetPiece.RemoveCardEffect(effectType);
            
            OnEffectRemoved?.Invoke(targetPiece, effectType);
            
            Debug.Log($"Removed card effect {effectType} from {targetPiece.pieceName}");
        }
        
        /// <summary>
        /// Advance to the next turn and update all effects
        /// </summary>
        public void NextTurn()
        {
            currentTurn++;
            OnTurnChanged?.Invoke(currentTurn);
            
            // Update all active effects
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                
                if (effect.targetPiece == null)
                {
                    activeEffects.RemoveAt(i);
                    continue;
                }
                
                // Update effect duration
                if (effect.effectData.parameters.turnDuration > 0)
                {
                    effect.remainingDuration--;
                    if (effect.remainingDuration <= 0)
                    {
                        RemoveEffect(effect.effectData.effectType, effect.targetPiece);
                        continue;
                    }
                }
                
                // Update piece card effects
                effect.targetPiece.UpdateCardEffects(currentTurn);
                
                // Check for special turn-based effects
                UpdateSpecialEffects(effect);
            }
            
            Debug.Log($"Advanced to turn {currentTurn}. Active effects: {activeEffects.Count}");
        }
        
        /// <summary>
        /// Check if an effect can be applied to a target piece
        /// </summary>
        private bool CanApplyEffectToTarget(CardEffectData effectData, ChessPieceData targetPiece)
        {
            if (targetPiece == null || targetPiece.isDead)
                return false;
            
            // Check piece type restrictions (if any)
            // Add more validation as needed
            
            return true;
        }
        
        /// <summary>
        /// Apply specific effect logic based on effect type
        /// </summary>
        private void ApplySpecificEffect(CardEffectData effectData, ChessPieceData targetPiece, PieceController pieceController)
        {
            switch (effectData.effectType)
            {
                case CardEffectType.QueenCollision:
                    ApplyQueenCollisionEffect(targetPiece, pieceController);
                    break;
                    
                case CardEffectType.ConquerorLeap:
                    ApplyConquerorLeapEffect(targetPiece, pieceController);
                    break;
                    
                case CardEffectType.NiceDay:
                    ApplyNiceDayEffect(targetPiece, pieceController);
                    break;
                    
                case CardEffectType.BackFromDead:
                    ApplyBackFromDeadEffect(targetPiece, pieceController);
                    break;
                    
                case CardEffectType.StoneTomorrow:
                    ApplyStoneTomorrowEffect(targetPiece, pieceController);
                    break;
                    
                // Add more specific effects as needed
            }
        }
        
        /// <summary>
        /// Update special effects that have turn-based logic
        /// </summary>
        private void UpdateSpecialEffects(ActiveCardEffect activeEffect)
        {
            var effectData = activeEffect.effectData;
            var targetPiece = activeEffect.targetPiece;
            
            switch (effectData.effectType)
            {
                case CardEffectType.WhereIsMyDefense:
                    // Check if piece hasn't moved for 2 turns
                    if (targetPiece.turnsWithoutMoving >= 2)
                    {
                        targetPiece.hasLightProtection = true;
                        targetPiece.protectionTurnsRemaining = 1;
                    }
                    break;
                    
                case CardEffectType.StoneTomorrow:
                    // Check if pawn hasn't moved for 15 turns
                    if (targetPiece.pieceType == ChessPieceData.PieceType.Pawn && targetPiece.turnsWithoutMoving >= 15)
                    {
                        targetPiece.canPromoteInPlace = true;
                    }
                    break;
            }
        }
        
        // Specific effect implementations
        
        private void ApplyQueenCollisionEffect(ChessPieceData targetPiece, PieceController pieceController)
        {
            // When bishop attacks and captures, diagonal pieces are also destroyed
            // This would be implemented in the movement/attack logic
            Debug.Log("Queen Collision effect applied - diagonal attack bonus activated");
        }
        
        private void ApplyConquerorLeapEffect(ChessPieceData targetPiece, PieceController pieceController)
        {
            if (targetPiece.pieceType == ChessPieceData.PieceType.Knight)
            {
                targetPiece.canMoveMultipleTimes = true;
                targetPiece.remainingMoves = 2;
                Debug.Log("Conqueror's Leap effect applied - knight can jump twice");
            }
        }
        
        private void ApplyNiceDayEffect(ChessPieceData targetPiece, PieceController pieceController)
        {
            if (targetPiece.pieceType == ChessPieceData.PieceType.Queen)
            {
                targetPiece.canMoveMultipleTimes = true;
                targetPiece.remainingMoves = 2;
                Debug.Log("Nice Day effect applied - queen can move twice if no attack");
            }
        }
        
        private void ApplyBackFromDeadEffect(ChessPieceData targetPiece, PieceController pieceController)
        {
            targetPiece.canRevive = true;
            Debug.Log("Back from Dead effect applied - piece can be revived");
        }
        
        private void ApplyStoneTomorrowEffect(ChessPieceData targetPiece, PieceController pieceController)
        {
            if (targetPiece.pieceType == ChessPieceData.PieceType.Pawn)
            {
                Debug.Log("Stone Tomorrow effect applied - pawn can promote after 15 turns");
            }
        }
        
        /// <summary>
        /// Get all active effects for a specific piece
        /// </summary>
        public List<ActiveCardEffect> GetActiveEffectsForPiece(ChessPieceData targetPiece)
        {
            List<ActiveCardEffect> result = new List<ActiveCardEffect>();
            foreach (var effect in activeEffects)
            {
                if (effect.targetPiece == targetPiece)
                {
                    result.Add(effect);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Clear all effects (useful for game reset)
        /// </summary>
        public void ClearAllEffects()
        {
            activeEffects.Clear();
            currentTurn = 0;
        }
        
        /// <summary>
        /// Data structure for tracking active card effects
        /// </summary>
        [System.Serializable]
        public class ActiveCardEffect
        {
            public CardEffectData effectData;
            public ChessPieceData targetPiece;
            public PieceController pieceController;
            public int startTurn;
            public int remainingDuration;
        }
    }
}