using UnityEngine;

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
    }
}