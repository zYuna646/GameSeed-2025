using UnityEngine;
using Bidak.Data;
using Bidak.Manager;

namespace Bidak.Examples
{
    /// <summary>
    /// Example script showing how to use the card effect system
    /// This demonstrates how to create and apply card effects based on the Excel data
    /// </summary>
    public class CardEffectExample : MonoBehaviour
    {
        [Header("Test Components")]
        public ChessCardData testCard;
        public PieceController testPiece;
        
        [Header("Effect Testing")]
        public bool testEffectsOnStart = false;
        
        private void Start()
        {
            if (testEffectsOnStart)
            {
                StartCoroutine(TestCardEffects());
            }
        }
        
        private System.Collections.IEnumerator TestCardEffects()
        {
            yield return new WaitForSeconds(1f);
            
            Debug.Log("=== Testing Card Effect System ===");
            
            // Test 1: Create and apply a Double Move effect
            TestDoubleMove();
            yield return new WaitForSeconds(2f);
            
            // Test 2: Create and apply Queen Collision effect
            TestQueenCollision();
            yield return new WaitForSeconds(2f);
            
            // Test 3: Create and apply Protection effect
            TestProtectionEffect();
            yield return new WaitForSeconds(2f);
            
            // Test 4: Test turn progression
            TestTurnProgression();
            
            Debug.Log("=== Card Effect Testing Complete ===");
        }
        
        /// <summary>
        /// Test the Double Move effect (Dapat bergerak dua kali dalam satu giliran)
        /// </summary>
        private void TestDoubleMove()
        {
            Debug.Log("--- Testing Double Move Effect ---");
            
            // Create Double Move effect data
            var doubleMoveEffect = new CardEffectData
            {
                effectType = CardEffectType.DoubleMove,
                isEnabled = true,
                effectDescription = "Dapat bergerak dua kali dalam satu giliran",
                parameters = new CardEffectParameters
                {
                    moveCount = 2,
                    turnDuration = 3 // Effect lasts 3 turns
                }
            };
            
            // Apply to test piece
            if (testPiece != null && testPiece.pieceData != null)
            {
                bool applied = testPiece.ApplyEffect(doubleMoveEffect);
                Debug.Log($"Double Move effect applied: {applied}");
                Debug.Log($"Piece can move multiple times: {testPiece.pieceData.canMoveMultipleTimes}");
                Debug.Log($"Remaining moves: {testPiece.pieceData.remainingMoves}");
            }
        }
        
        /// <summary>
        /// Test the Queen Collision effect (Saat gajah menyerang dan memakan bidak lawan, bidak lawan yang sejajar secara diagonal juga ikut dihancurkan)
        /// </summary>
        private void TestQueenCollision()
        {
            Debug.Log("--- Testing Queen Collision Effect ---");
            
            // Create Queen Collision effect data
            var queenCollisionEffect = new CardEffectData
            {
                effectType = CardEffectType.QueenCollision,
                isEnabled = true,
                effectDescription = "Saat gajah menyerang dan memakan bidak lawan, bidak lawan yang sejajar secara diagonal juga ikut dihancurkan",
                parameters = new CardEffectParameters
                {
                    affectsMultiplePieces = true,
                    affectsEnemyPieces = true,
                    turnDuration = 0 // Permanent effect
                }
            };
            
            // Apply to test piece (should be a bishop)
            if (testPiece != null && testPiece.pieceData != null)
            {
                bool applied = testPiece.ApplyEffect(queenCollisionEffect);
                Debug.Log($"Queen Collision effect applied: {applied}");
                Debug.Log($"Piece has Queen Collision effect: {testPiece.HasCardEffect(CardEffectType.QueenCollision)}");
            }
        }
        
        /// <summary>
        /// Test protection effects (Mendapatkan perlindungan ringan yang mana hanya bisa dimakan oleh bidak yang lebih kuat)
        /// </summary>
        private void TestProtectionEffect()
        {
            Debug.Log("--- Testing Protection Effect ---");
            
            // Create Protection effect data
            var protectionEffect = new CardEffectData
            {
                effectType = CardEffectType.ProtectedRing,
                isEnabled = true,
                effectDescription = "Mendapatkan perlindungan ringan yang mana hanya bisa dimakan oleh bidak yang lebih kuat",
                parameters = new CardEffectParameters
                {
                    hasLightProtection = true,
                    protectionDuration = 5,
                    turnDuration = 5
                }
            };
            
            // Apply to test piece
            if (testPiece != null && testPiece.pieceData != null)
            {
                bool applied = testPiece.ApplyEffect(protectionEffect);
                Debug.Log($"Protection effect applied: {applied}");
                Debug.Log($"Piece has light protection: {testPiece.pieceData.hasLightProtection}");
                Debug.Log($"Protection turns remaining: {testPiece.pieceData.protectionTurnsRemaining}");
            }
        }
        
        /// <summary>
        /// Test turn progression and effect duration
        /// </summary>
        private void TestTurnProgression()
        {
            Debug.Log("--- Testing Turn Progression ---");
            
            // Simulate several turns
            for (int turn = 1; turn <= 5; turn++)
            {
                Debug.Log($"Turn {turn}:");
                CardEffectManager.Instance.NextTurn();
                
                if (testPiece != null && testPiece.pieceData != null)
                {
                    testPiece.StartNewTurn();
                    Debug.Log($"  Remaining moves: {testPiece.pieceData.remainingMoves}");
                    Debug.Log($"  Protection turns: {testPiece.pieceData.protectionTurnsRemaining}");
                    Debug.Log($"  Active effects: {testPiece.GetActiveCardEffects().Count}");
                }
            }
        }
        
        /// <summary>
        /// Create a sample card with multiple effects based on Excel data
        /// </summary>
        [ContextMenu("Create Sample Card")]
        public void CreateSampleCard()
        {
            // Create a new card data asset
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Conqueror's Leap";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Rare;
            cardData.pieceType = ChessCardData.PieceType.Knight;
            cardData.description = "Kuda dapat melompat dua kali dalam satu giliran selama keduanya merupakan langkah legal";
            
            // Add the Conqueror's Leap effect
            var conquerorLeapEffect = new CardEffectData
            {
                effectType = CardEffectType.ConquerorLeap,
                isEnabled = true,
                effectDescription = "Kuda dapat melompat dua kali dalam satu giliran selama keduanya merupakan langkah legal",
                parameters = new CardEffectParameters
                {
                    moveCount = 2,
                    turnDuration = 1 // Effect lasts for one turn
                }
            };
            
            cardData.cardEffects.Add(conquerorLeapEffect);
            
            testCard = cardData;
            Debug.Log("Sample card 'Conqueror's Leap' created with Conqueror's Leap effect");
        }
        
        /// <summary>
        /// Create all cards from Excel data
        /// </summary>
        [ContextMenu("Create All Excel Cards")]
        public void CreateAllExcelCards()
        {
            CreateQueenCollisionCard();
            CreateConquerorLeapCard();
            CreateRoyalCommandCard();
            CreateWhereIsMyDefenseCard();
            CreateNotTodayCard();
            CreateUnstoppableForceCard();
            CreateTimeFrozenCard();
            CreatePowerfulCard();
            CreateNiceDayCard();
            CreateDanceLikeQueenCard();
            CreateBackFromDeadCard();
            CreateStoneTomorrowCard();
            CreateSpecialMoveCard();
            
            Debug.Log("All Excel cards created successfully!");
        }
        
        private void CreateQueenCollisionCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Queen Collision";
            cardData.type = ChessCardData.CardType.Attack;
            cardData.rank = ChessCardData.CardRank.Legendary;
            cardData.description = "Saat gajah menyerang dan memakan bidak lawan, bidak lawan yang sejajar secara diagonal juga ikut dihancurkan (maksimal 2 bidak). Efek area ini hanya berlaku satu kali.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.QueenCollision,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    affectsMultiplePieces = true,
                    maxTargets = 2,
                    turnDuration = 0
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateConquerorLeapCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Conqueror's Leap";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Rare;
            cardData.description = "Kuda dapat melompat dua kali dalam satu giliran selama keduanya merupakan langkah legal. Jika kedua lompatan mengenai musuh, kuda bidak tersebut langsung dikalahkan.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.ConquerorLeap,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    moveCount = 2,
                    turnDuration = 1
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateRoyalCommandCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Royal Command";
            cardData.type = ChessCardData.CardType.Defend;
            cardData.rank = ChessCardData.CardRank.Common;
            cardData.description = "Bisa dari beberapa bidak tidak bisa dimakan oleh bidak lain pada satu putaran temporer untuk menggerakan \"blockade\" di satu garis lurus di depannya untuk lokasi atau tempat yang tidak ada promosi. Jika tidak ada yang melakukan, tidak bukan akan lain.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.RoyalCommand,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    hasFullProtection = true,
                    turnDuration = 1,
                    affectsMultiplePieces = true
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        // Continue with other cards...
        private void CreateWhereIsMyDefenseCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Where is my Defense";
            cardData.type = ChessCardData.CardType.Defend;
            cardData.rank = ChessCardData.CardRank.Common;
            cardData.description = "Jika bidak tidak bergerak selama 2 giliran, ia mendapatkan \"perlindungan ringan\" yang mana hanya bisa dimakan oleh bidak yang lebih kuat. Efek kartu ini bertahan selama 2 giliran.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.WhereIsMyDefense,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    hasLightProtection = true,
                    turnDuration = 2,
                    turnsCooldown = 2
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateNotTodayCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Not Today";
            cardData.type = ChessCardData.CardType.Defend;
            cardData.rank = ChessCardData.CardRank.Legendary;
            cardData.description = "Saat raja diserang, setiap serangan yang diarahkan ke raja akan dibatalkan dan penyerang mundur satu langkah ke posisi sebelumnya. Hanya berlaku satu kali.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.NotToday,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    hasFullProtection = true,
                    turnDuration = 1
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateUnstoppableForceCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Unstoppable Force";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Rare;
            cardData.description = "Benteng tidak bisa dimakan selama 2 giliran kecuali oleh mentri atau raja. Memberi waktu bagi pemain untuk menyusun strategi pertahanan di belakangnya.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.UnstoppableForce,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    hasFullProtection = true,
                    turnDuration = 2
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateTimeFrozenCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Time Frozen";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Common;
            cardData.description = "Bisa digunakan untuk bertahan dari lawan atau membuka jalur bidak lain.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.TimeFrozen,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    turnDuration = 1
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreatePowerfulCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Powerful";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Common;
            cardData.description = "Gajah akan melewati 1 bidak yang menghalangi jalur diagonal gajah (hanya bisa melewati bidak sekutu).";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.PowerfulMove,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    ignoreBlocking = true,
                    turnDuration = 0
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateNiceDayCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "A Nice Day";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Legendary;
            cardData.description = "Ratu dapat bergerak dua kali dalam satu giliran jika tidak menyerang di langkah pertama. Cocok untuk positioning atau mengecoh lawan. Kartu ini dapat digunakan 2 kali.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.NiceDay,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    moveCount = 2,
                    requiresNoAttack = true,
                    turnDuration = 1
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateDanceLikeQueenCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Dance Like Queen";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Rare;
            cardData.description = "Setelah Gajah memakan bidak, langkah berikutnya bisa dilakukan dalam arah lurus dalam satu langkah seperti bidak ratu. Berlaku satu kali.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.DanceLikeQueen,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    canAttackStraight = true,
                    turnDuration = 1
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateBackFromDeadCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Back from Dead";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Rare;
            cardData.description = "Setelah bidak dimakan dan musuh yang memakan bidak sudah pergi, maka bidak yang telah dimakan dapat dihidupkan kembali dan mendapatkan 1 giliran untuk bergerak. Efek ini hilang setelah 2 giliran.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.BackFromDead,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    canRevive = true,
                    turnDuration = 2
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateStoneTomorrowCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "Stone Tomorrow";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Rare;
            cardData.description = "Setelah tidak bergerak selama 15 giliran, Pion akan melakukan promosi di tempat dan menjadi bidak apapun (kuda/gajah/benteng). Efek hilang setelah pion bergerak sebelum giliran ke-15.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.StoneTomorrow,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    canPromoteInPlace = true,
                    turnsCooldown = 15,
                    turnDuration = 0
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
        
        private void CreateSpecialMoveCard()
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = "It's a Move";
            cardData.type = ChessCardData.CardType.Effects;
            cardData.rank = ChessCardData.CardRank.Common;
            cardData.description = "Bidak apa pun dapat bergerak satu langkah ke samping (kiri atau kanan), terlepas dari pola gerak normalnya, asal tidak melanggar peraturan catur standar.";
            
            var effect = new CardEffectData
            {
                effectType = CardEffectType.SpecialMove,
                isEnabled = true,
                effectDescription = cardData.description,
                parameters = new CardEffectParameters
                {
                    canMoveSideways = true,
                    stepsForward = 1,
                    turnDuration = 1
                }
            };
            
            cardData.cardEffects.Add(effect);
        }
    }
}