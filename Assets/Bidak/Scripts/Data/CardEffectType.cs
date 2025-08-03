using System;
using UnityEngine;

namespace Bidak.Data
{
    /// <summary>
    /// Defines all possible card effects based on the Excel data
    /// </summary>
    [Serializable]
    public enum CardEffectType
    {
        // Movement Effects
        DoubleMove,                 // Dapat bergerak dua kali dalam satu giliran
        TripleMove,                 // Dapat bergerak tiga kali dalam satu giliran
        DiagonalAttack,             // Dapat menyerang secara diagonal
        StraightMove,               // Dapat bergerak lurus ke depan dalam posisi tidak dapat menyerang selama keduanya
        ProtectedRing,              // Mendapatkan perlindungan ringan yang mana hanya bisa dimakan oleh bidak yang lebih kuat
        BlockadeMove,               // Menggunakan bidak tidak bergerak selama 2 giliran kecuali oleh mentri atau raja
        ForwardTwoMoves,            // Dapat mundur satu petak ke belakang dalam posisi tidak dapat menyerang
        TwoDirectionMove,           // Dapat maju dua langkah sebelum (jika tidak ada yang menyerang di langkah pertama)
        PowerfulMove,               // Bidak lawan yang menyerang akan melewati 1 bidak yang menghalangi jalur diagonal gajah
        NiceDay,                    // Ratu dapat bergerak dua kali dalam satu giliran jika tidak menyerang di langkah pertama
        BackMove,                   // Dapat bergerak dan musuh yang memakan bidak sudah pergi, maka bidak yang telah dimakan dapat dihidupkan kembali
        LeapMove,                   // Pion dapat bergerak selama 15 giliran, Pion akan melakukan promosi di tempat dan menjadi bidak apapun
        RestoreMove,                // Bidak apa pun dapat bergerak satu langkah ke samping (kiri atau kanan), terlepas dari pola gerak normalnya

        // Attack Effects
        QueenCollision,             // Saat gajah menyerang dan memakan bidak lawan, bidak lawan yang sejajar secara diagonal juga ikut dihancurkan
        ConquerorLeap,              // Kuda dapat melompat dua kali dalam satu giliran selama keduanya merupakan langkah legal
        RoyalCommand,               // Bisa dari beberapa bidak tidak bisa dimakan oleh bidak lain pada satu putaran
        WhereIsMyDefense,           // Jika bidak tidak bergerak selama 2 giliran, ia mendapatkan "perlindungan ringan" yang mana hanya bisa dimakan oleh bidak yang lebih kuat
        NotToday,                   // Saat raja diserang, setiap serangan yang diarahkan ke raja akan dibatalkan dan penyerang mundur satu langkah ke posisi sebelumnya
        UnstoppableForce,           // Benteng tidak bisa dimakan selama 2 giliran kecuali oleh mentri atau raja
        TimeFrozen,                 // Bisa digunakan untuk bertahan dari lawan atau membuka jalur bidak lain
        DanceLikeQueen,             // Setelah Gajah memakan bidak, langkah berikutnya bisa dilakukan dalam arah lurus dalam satu langkah seperti bidak ratu
        BackFromDead,               // Setelah bidak dimakan dan musuh yang memakan bidak sudah pergi, maka bidak yang telah dimakan dapat dihidupkan kembali
        StoneTomorrow,              // Setelah tidak bergerak selama 15 giliran, Pion akan melakukan promosi di tempat dan menjadi bidak apapun
        SpecialMove                 // Bidak apa pun dapat bergerak satu langkah ke samping (kiri atau kanan), terlepas dari pola gerak normalnya
    }

    /// <summary>
    /// Parameters for card effects
    /// </summary>
    [Serializable]
    public class CardEffectParameters
    {
        [Header("Movement Parameters")]
        public int moveCount = 1;                   // Number of moves (for DoubleMove, TripleMove, etc.)
        public int turnDuration = 1;                // Duration in turns
        public int stepsForward = 1;                // Steps forward for movement effects
        public int stepsBackward = 1;               // Steps backward for movement effects
        public int turnsCooldown = 0;               // Cooldown in turns
        
        [Header("Attack Parameters")]
        public bool canAttackDiagonally = false;    // Can attack diagonally
        public bool canAttackStraight = false;      // Can attack straight
        public int attackRange = 1;                 // Attack range
        public bool ignoreBlocking = false;         // Ignore blocking pieces
        
        [Header("Protection Parameters")]
        public bool hasLightProtection = false;     // Light protection (only stronger pieces can capture)
        public bool hasFullProtection = false;      // Full protection (cannot be captured)
        public int protectionDuration = 1;          // Protection duration in turns
        
        [Header("Special Parameters")]
        public bool canPromoteInPlace = false;      // Can promote in place
        public bool canRevive = false;              // Can revive captured pieces
        public bool canMoveBackward = false;        // Can move backward
        public bool canMoveSideways = false;        // Can move sideways
        public bool requiresNoAttack = false;       // Effect only works if no attack was made
        
        [Header("Target Parameters")]
        public bool affectsMultiplePieces = false;  // Affects multiple pieces
        public bool affectsAlliedPieces = false;    // Affects allied pieces
        public bool affectsEnemyPieces = false;     // Affects enemy pieces
        public int maxTargets = 1;                  // Maximum number of targets
    }
}