#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Bidak.Data;
using System.Collections.Generic;

namespace Bidak.Editor
{
    /// <summary>
    /// Editor script untuk generate ChessCardData berdasarkan Excel data
    /// </summary>
    public static class ExcelCardGenerator
    {
        private const string OUTPUT_PATH = "Assets/Bidak/data/Card/Generated/";

        [MenuItem("Bidak/Generate All Excel Cards")]
        public static void GenerateAllExcelCards()
        {
            Debug.Log("=== Generating Cards from Excel Data ===");
            
            // Ensure directory exists
            EnsureDirectoryExists();
            
            // Generate all cards
            var generatedCards = new List<string>();
            
            generatedCards.Add(GenerateQueenCollision());
            generatedCards.Add(GenerateConquerorLeap());
            generatedCards.Add(GenerateTemporaryFriend());
            generatedCards.Add(GenerateWhereIsMyRival());
            generatedCards.Add(GenerateNotToday());
            generatedCards.Add(GenerateIAmUnstoppable());
            generatedCards.Add(GenerateItsTimeToRetreat());
            generatedCards.Add(GeneratePowerfulThanAQueen());
            generatedCards.Add(GenerateNiceDayToWalk());
            generatedCards.Add(GenerateDanceLikeAQueen());
            generatedCards.Add(GenerateISeeTheFinishLine());
            generatedCards.Add(GenerateIGotYouBuddy());
            generatedCards.Add(GenerateBackFromUnderworld());
            generatedCards.Add(GenerateStoneToTheGold());
            generatedCards.Add(GenerateItsAMoveFreeDay());
            
            // Save and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Show results
            string results = "Generated Cards:\n" + string.Join("\n", generatedCards);
            Debug.Log(results);
            
            EditorUtility.DisplayDialog("Excel Card Generation Complete", 
                $"Generated {generatedCards.Count} cards successfully!\n\nCheck: {OUTPUT_PATH}", 
                "OK");
        }

        #region Individual Card Generators

        private static string GenerateQueenCollision()
        {
            var cardData = CreateBaseCard("Queen collision", ChessCardData.CardRank.Legendary, ChessCardData.PieceType.Bishop);
            cardData.description = "Saat gajah menyerang dan memakan bidak lawan, bidak lawan yang sejajar secara diagonal juga ikut dihancurkan (maksimal 2 bidak). Efek area ini hanya berlaku satu kali.";
            
            AddCardEffect(cardData, CardEffectType.QueenCollision, new CardEffectParameters
            {
                maxTargets = 2,
                affectsEnemyPieces = true,
                canAttackDiagonally = true,
                turnDuration = 1
            });
            
            return SaveCard(cardData, "QueenCollision");
        }

        private static string GenerateConquerorLeap()
        {
            var cardData = CreateBaseCard("Conqueror's Leap", ChessCardData.CardRank.Rare, ChessCardData.PieceType.Knight);
            cardData.description = "Kuda dapat melompat dua kali dalam satu giliran selama keduanya merupakan langkah legal. Jika kedua lompatan mengenai musuh, kedua bidak tersebut langsung dikalahkan.";
            
            AddCardEffect(cardData, CardEffectType.ConquerorLeap, new CardEffectParameters
            {
                moveCount = 2,
                turnDuration = 1,
                canAttackDiagonally = true,
                affectsEnemyPieces = true
            });
            
            return SaveCard(cardData, "ConquerorLeap");
        }

        private static string GenerateTemporaryFriend()
        {
            var cardData = CreateBaseCard("temporary friend", ChessCardData.CardRank.Common, ChessCardData.PieceType.Rook);
            cardData.description = "Benteng menciptakan \"blokade\" di satu garis lurus di depannya (maksimal 3 petak) yang tidak bisa dilalui lawan selama satu giliran.";
            
            AddCardEffect(cardData, CardEffectType.BlockadeMove, new CardEffectParameters
            {
                isBlockaded = true,
                turnDuration = 1,
                maxTargets = 3
            });
            
            return SaveCard(cardData, "TemporaryFriend");
        }

        private static string GenerateWhereIsMyRival()
        {
            var cardData = CreateBaseCard("Where is my rival", ChessCardData.CardRank.Common, ChessCardData.PieceType.None);
            cardData.description = "Jika bidak tidak bergerak selama 2 giliran, ia mendapatkan \"perlindungan ringan\" yang mana hanya bisa dimakan oleh bidak yang lebih kuat. Efek kartu ini bertahan selama 2 giliran.";
            
            AddCardEffect(cardData, CardEffectType.ProtectedRing, new CardEffectParameters
            {
                hasLightProtection = true,
                protectionDuration = 2,
                turnDuration = 2,
                turnsWithoutMoving = 2
            });
            
            return SaveCard(cardData, "WhereIsMyRival");
        }

        private static string GenerateNotToday()
        {
            var cardData = CreateBaseCard("Not Today", ChessCardData.CardRank.Legendary, ChessCardData.PieceType.King);
            cardData.description = "Saat raja diserang, setiap serangan yang diarahkan ke raja akan dibatalkan dan penyerang mundur satu langkah ke posisi sebelumnya. Hanya berlaku satu kali.";
            
            AddCardEffect(cardData, CardEffectType.NotToday, new CardEffectParameters
            {
                hasFullProtection = true,
                protectionDuration = 1,
                canSwapPositions = true
            });
            
            return SaveCard(cardData, "NotToday");
        }

        private static string GenerateIAmUnstoppable()
        {
            var cardData = CreateBaseCard("I am unstoppable", ChessCardData.CardRank.Rare, ChessCardData.PieceType.Rook);
            cardData.description = "Benteng tidak bisa dimakan selama 2 giliran kecuali oleh mentri atau raja. Memberi waktu bagi pemain untuk menyusun ulang pertahanan di belakangnya.";
            
            AddCardEffect(cardData, CardEffectType.UnstoppableForce, new CardEffectParameters
            {
                hasLightProtection = true,
                protectionDuration = 2,
                turnDuration = 2
            });
            
            return SaveCard(cardData, "IAmUnstoppable");
        }

        private static string GenerateItsTimeToRetreat()
        {
            var cardData = CreateBaseCard("it's time to retreat", ChessCardData.CardRank.Common, ChessCardData.PieceType.Pawn);
            cardData.description = "Pion dapat mundur satu petak ke belakang dalam posisi tidak menyerang. Bisa digunakan untuk bertahan dari tekanan lawan atau membuka jalur bidak lain.";
            
            AddCardEffect(cardData, CardEffectType.BackMove, new CardEffectParameters
            {
                canMoveBackward = true,
                requiresNoAttack = true,
                stepsBackward = 1
            });
            
            return SaveCard(cardData, "ItsTimeToRetreat");
        }

        private static string GeneratePowerfulThanAQueen()
        {
            var cardData = CreateBaseCard("powerful than a queen", ChessCardData.CardRank.Common, ChessCardData.PieceType.Bishop);
            cardData.description = "Gajah akan melewati 1 bidak yang menghalangi jalur diagonal gajah (hanya bisa melewati bidak sekutu).";
            
            AddCardEffect(cardData, CardEffectType.PowerfulMove, new CardEffectParameters
            {
                canAttackDiagonally = true,
                ignoreBlocking = true,
                affectsAlliedPieces = true
            });
            
            return SaveCard(cardData, "PowerfulThanAQueen");
        }

        private static string GenerateNiceDayToWalk()
        {
            var cardData = CreateBaseCard("a nice day to walk", ChessCardData.CardRank.Legendary, ChessCardData.PieceType.Queen);
            cardData.description = "Ratu dapat bergerak dua kali dalam satu giliran jika tidak menyerang di langkah pertama. Cocok untuk positioning atau mengecoh lawan. Kartu ini dapat digunakan 2 kali.";
            
            AddCardEffect(cardData, CardEffectType.NiceDay, new CardEffectParameters
            {
                moveCount = 2,
                requiresNoAttack = true,
                turnDuration = 2
            });
            
            return SaveCard(cardData, "NiceDayToWalk");
        }

        private static string GenerateDanceLikeAQueen()
        {
            var cardData = CreateBaseCard("Dance like a Queen", ChessCardData.CardRank.Rare, ChessCardData.PieceType.Bishop);
            cardData.description = "Setelah Gajah memakan bidak, langkah berikutnya bisa dilakukan dalam arah lurus dalam satu langkah seperti bidak ratu. Berlaku satu kali.";
            
            AddCardEffect(cardData, CardEffectType.DanceLikeQueen, new CardEffectParameters
            {
                canAttackStraight = true,
                canAttackDiagonally = true,
                turnDuration = 1
            });
            
            return SaveCard(cardData, "DanceLikeAQueen");
        }

        private static string GenerateISeeTheFinishLine()
        {
            var cardData = CreateBaseCard("I see, the finish line", ChessCardData.CardRank.Common, ChessCardData.PieceType.Pawn);
            cardData.description = "Pion mendapatkan kekuatan untuk maju dua langkah sebelum 2 langkah lagi melakukan promosi (jika tidak ada yang menghalangi), bukan hanya di awal.";
            
            AddCardEffect(cardData, CardEffectType.ForwardTwoMoves, new CardEffectParameters
            {
                stepsForward = 2,
                canPromoteInPlace = true
            });
            
            return SaveCard(cardData, "ISeeTheFinishLine");
        }

        private static string GenerateIGotYouBuddy()
        {
            var cardData = CreateBaseCard("i got you buddy", ChessCardData.CardRank.Common, ChessCardData.PieceType.King);
            cardData.description = "Setelah raja berpindah, semua pion di sekitarnya jadi kebal dari serangan selama giliran lawan berikutnya. Efek bertahan 1 giliran.";
            
            AddCardEffect(cardData, CardEffectType.IGotYou, new CardEffectParameters
            {
                affectsMultiplePieces = true,
                affectsAlliedPieces = true,
                hasLightProtection = true,
                protectionDuration = 1
            });
            
            return SaveCard(cardData, "IGotYouBuddy");
        }

        private static string GenerateBackFromUnderworld()
        {
            var cardData = CreateBaseCard("back from underworld", ChessCardData.CardRank.Rare, ChessCardData.PieceType.None);
            cardData.description = "Setelah bidak dimakan dan musuh yang memakan bidak sudah pergi, maka bidak yang telah dimakan dapat dihidupkan kembali dan mendapatkan 1 giliran untuk bergerak. Efek ini hilang setelah 2 giliran.";
            
            AddCardEffect(cardData, CardEffectType.BackFromDead, new CardEffectParameters
            {
                canRevive = true,
                moveCount = 1,
                turnDuration = 2
            });
            
            return SaveCard(cardData, "BackFromUnderworld");
        }

        private static string GenerateStoneToTheGold()
        {
            var cardData = CreateBaseCard("stone to the Gold", ChessCardData.CardRank.Rare, ChessCardData.PieceType.Pawn);
            cardData.description = "Setelah tidak bergerak selama 15 giliran, Pion akan melakukan promosi di tempat dan menjadi bidak apapun (kuda/gajah/benteng). Efek hilang jika pion bergerak sebelum giliran ke-15.";
            
            AddCardEffect(cardData, CardEffectType.StoneTomorrow, new CardEffectParameters
            {
                canPromoteInPlace = true,
                turnDuration = 15,
                turnsWithoutMoving = 15
            });
            
            return SaveCard(cardData, "StoneToTheGold");
        }

        private static string GenerateItsAMoveFreeDay()
        {
            var cardData = CreateBaseCard("its a move free day", ChessCardData.CardRank.Common, ChessCardData.PieceType.None);
            cardData.description = "Bidak apa pun dapat bergerak satu langkah ke samping (kiri atau kanan), terlepas dari pola gerak normalnya, asal tidak melanggar peraturan catur standar.";
            
            AddCardEffect(cardData, CardEffectType.LeapMove, new CardEffectParameters
            {
                canMoveSideways = true,
                stepsForward = 1,
                turnDuration = 1
            });
            
            return SaveCard(cardData, "ItsAMoveFreeDay");
        }

        #endregion

        #region Helper Methods

        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Bidak/data/Card/Generated"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Bidak/data/Card"))
                {
                    AssetDatabase.CreateFolder("Assets/Bidak/data", "Card");
                }
                AssetDatabase.CreateFolder("Assets/Bidak/data/Card", "Generated");
            }
        }

        private static ChessCardData CreateBaseCard(string name, ChessCardData.CardRank rank, ChessCardData.PieceType pieceType)
        {
            var cardData = ScriptableObject.CreateInstance<ChessCardData>();
            cardData.cardName = name;
            cardData.rank = rank;
            cardData.pieceType = pieceType;
            cardData.type = ChessCardData.CardType.Effects; // Set default card type
            cardData.cardEffects = new List<CardEffectData>();
            return cardData;
        }

        private static void AddCardEffect(ChessCardData cardData, CardEffectType effectType, CardEffectParameters parameters)
        {
            var effect = new CardEffectData
            {
                effectType = effectType,
                isEnabled = true,
                parameters = parameters
            };
            cardData.cardEffects.Add(effect);
        }

        private static string SaveCard(ChessCardData cardData, string fileName)
        {
            string fullPath = OUTPUT_PATH + fileName + ".asset";
            AssetDatabase.CreateAsset(cardData, fullPath);
            return $"✓ {cardData.cardName} ({cardData.rank})";
        }

        #endregion

        #region Additional Menu Items

        [MenuItem("Bidak/Open Generated Cards Folder")]
        public static void OpenGeneratedCardsFolder()
        {
            EnsureDirectoryExists();
            EditorUtility.RevealInFinder(OUTPUT_PATH);
        }

        [MenuItem("Bidak/Clear Generated Cards")]
        public static void ClearGeneratedCards()
        {
            if (EditorUtility.DisplayDialog("Clear Generated Cards", 
                "Are you sure you want to delete all generated cards?", 
                "Yes", "Cancel"))
            {
                string[] guids = AssetDatabase.FindAssets("t:ChessCardData", new[] { OUTPUT_PATH });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.DeleteAsset(path);
                }
                
                AssetDatabase.Refresh();
                Debug.Log($"Cleared {guids.Length} generated cards");
            }
        }

        [MenuItem("Bidak/Validate Card Data")]
        public static void ValidateCardData()
        {
            string[] guids = AssetDatabase.FindAssets("t:ChessCardData", new[] { OUTPUT_PATH });
            
            Debug.Log($"=== Validating {guids.Length} Generated Cards ===");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ChessCardData card = AssetDatabase.LoadAssetAtPath<ChessCardData>(path);
                
                if (card != null)
                {
                    Debug.Log($"✓ {card.cardName} - {card.rank} - {card.cardEffects.Count} effects");
                    
                    foreach (var effect in card.cardEffects)
                    {
                        Debug.Log($"  └─ {effect.effectType} (Enabled: {effect.isEnabled})");
                    }
                }
            }
        }

        #endregion
    }
}
#endif