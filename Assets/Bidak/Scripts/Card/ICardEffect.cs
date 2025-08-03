using UnityEngine;
using Bidak.Data;

namespace Bidak.Card
{
    /// <summary>
    /// Interface for all card effects
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// The type of this card effect
        /// </summary>
        CardEffectType EffectType { get; }
        
        /// <summary>
        /// Parameters for this effect
        /// </summary>
        CardEffectParameters Parameters { get; }
        
        /// <summary>
        /// Whether this effect can be applied to the given piece
        /// </summary>
        bool CanApplyTo(ChessPieceData pieceData);
        
        /// <summary>
        /// Apply the effect to a piece
        /// </summary>
        /// <param name="pieceData">The piece to apply the effect to</param>
        /// <param name="pieceController">The piece controller</param>
        /// <param name="currentTile">The current tile of the piece</param>
        /// <returns>True if effect was successfully applied</returns>
        bool ApplyEffect(ChessPieceData pieceData, PieceController pieceController, TileController currentTile);
        
        /// <summary>
        /// Remove the effect from a piece
        /// </summary>
        /// <param name="pieceData">The piece to remove the effect from</param>
        /// <param name="pieceController">The piece controller</param>
        /// <param name="currentTile">The current tile of the piece</param>
        void RemoveEffect(ChessPieceData pieceData, PieceController pieceController, TileController currentTile);
        
        /// <summary>
        /// Update the effect (called each turn)
        /// </summary>
        /// <param name="pieceData">The piece with this effect</param>
        /// <param name="pieceController">The piece controller</param>
        /// <param name="currentTile">The current tile of the piece</param>
        void UpdateEffect(ChessPieceData pieceData, PieceController pieceController, TileController currentTile);
        
        /// <summary>
        /// Check if the effect is still active
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Get remaining duration of the effect
        /// </summary>
        int RemainingDuration { get; }
    }
    
    /// <summary>
    /// Abstract base class for card effects
    /// </summary>
    public abstract class BaseCardEffect : ICardEffect
    {
        public abstract CardEffectType EffectType { get; }
        public CardEffectParameters Parameters { get; protected set; }
        public bool IsActive { get; protected set; }
        public int RemainingDuration { get; protected set; }
        
        protected BaseCardEffect(CardEffectParameters parameters)
        {
            Parameters = parameters ?? new CardEffectParameters();
            RemainingDuration = Parameters.turnDuration;
            IsActive = true;
        }
        
        public virtual bool CanApplyTo(ChessPieceData pieceData)
        {
            return pieceData != null && !pieceData.isDead;
        }
        
        public abstract bool ApplyEffect(ChessPieceData pieceData, PieceController pieceController, TileController currentTile);
        
        public virtual void RemoveEffect(ChessPieceData pieceData, PieceController pieceController, TileController currentTile)
        {
            IsActive = false;
        }
        
        public virtual void UpdateEffect(ChessPieceData pieceData, PieceController pieceController, TileController currentTile)
        {
            if (Parameters.turnDuration > 0)
            {
                RemainingDuration--;
                if (RemainingDuration <= 0)
                {
                    IsActive = false;
                }
            }
        }
    }
}