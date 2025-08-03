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

        [Header("Card Details")]
        public string cardName;
        public Sprite cardImage;
        public int points;
        public CardType type;

        [Header("Additional Card Properties")]
        [TextArea(3, 10)]
        public string description;
    }
}