using UnityEngine;
using System.Collections.Generic;

namespace Bidak.Data
{
    [CreateAssetMenu(fileName = "New Chess Card", menuName = "Bidak/Chess Card Data", order = 3)]
    public class ChessCardData : ScriptableObject
    {
        public enum CardType
        {
            Effects,
            Attack,
            Defend
        }

        public enum CardRank
        {
            Common,
            Rare,
            Legendary
        }
        
        public enum PieceType
        {
            Pawn,
            Rook,
            Knight,
            Bishop,
            Queen,
            King,
            None
        }

        [Header("Card Details")]
        public string cardName;
        public Sprite cardImage;
        public int points;
        public GameObject cardPrefab;
        public CardType type;
        public CardRank rank;
        public PieceType pieceType;

        [Header("Card Material")]
        public Material cardMaterial; // New field for custom card header material

        [Header("Additional Card Properties")]
        [TextArea(3, 10)]
        public string description;
        
        [Header("Card Effects")]
        public List<CardEffectData> cardEffects = new List<CardEffectData>();
        
        /// <summary>
        /// Get all card effects of a specific type
        /// </summary>
        public List<CardEffectData> GetEffectsOfType(CardEffectType effectType)
        {
            List<CardEffectData> result = new List<CardEffectData>();
            foreach (var effect in cardEffects)
            {
                if (effect.effectType == effectType)
                {
                    result.Add(effect);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Check if this card has a specific effect
        /// </summary>
        public bool HasEffect(CardEffectType effectType)
        {
            foreach (var effect in cardEffects)
            {
                if (effect.effectType == effectType)
                {
                    return true;
                }
            }
            return false;
        }
    }
    
    /// <summary>
    /// Data structure for individual card effects
    /// </summary>
    [System.Serializable]
    public class CardEffectData
    {
        [Header("Effect Configuration")]
        public CardEffectType effectType;
        public bool isEnabled = true;
        public CardEffectParameters parameters = new CardEffectParameters();
        
        [Header("Effect Details")]
        [TextArea(2, 5)]
        public string effectDescription;
        
        /// <summary>
        /// Create a copy of this effect data
        /// </summary>
        public CardEffectData Clone()
        {
            CardEffectData clone = new CardEffectData();
            clone.effectType = this.effectType;
            clone.isEnabled = this.isEnabled;
            clone.effectDescription = this.effectDescription;
            
            // Deep copy parameters
            clone.parameters = new CardEffectParameters();
            clone.parameters.moveCount = this.parameters.moveCount;
            clone.parameters.turnDuration = this.parameters.turnDuration;
            clone.parameters.stepsForward = this.parameters.stepsForward;
            clone.parameters.stepsBackward = this.parameters.stepsBackward;
            clone.parameters.turnsCooldown = this.parameters.turnsCooldown;
            clone.parameters.canAttackDiagonally = this.parameters.canAttackDiagonally;
            clone.parameters.canAttackStraight = this.parameters.canAttackStraight;
            clone.parameters.attackRange = this.parameters.attackRange;
            clone.parameters.ignoreBlocking = this.parameters.ignoreBlocking;
            clone.parameters.hasLightProtection = this.parameters.hasLightProtection;
            clone.parameters.hasFullProtection = this.parameters.hasFullProtection;
            clone.parameters.protectionDuration = this.parameters.protectionDuration;
            clone.parameters.canPromoteInPlace = this.parameters.canPromoteInPlace;
            clone.parameters.canRevive = this.parameters.canRevive;
            clone.parameters.canMoveBackward = this.parameters.canMoveBackward;
            clone.parameters.canMoveSideways = this.parameters.canMoveSideways;
            clone.parameters.requiresNoAttack = this.parameters.requiresNoAttack;
            clone.parameters.affectsMultiplePieces = this.parameters.affectsMultiplePieces;
            clone.parameters.affectsAlliedPieces = this.parameters.affectsAlliedPieces;
            clone.parameters.affectsEnemyPieces = this.parameters.affectsEnemyPieces;
            clone.parameters.maxTargets = this.parameters.maxTargets;
            
            return clone;
        }
    }
}