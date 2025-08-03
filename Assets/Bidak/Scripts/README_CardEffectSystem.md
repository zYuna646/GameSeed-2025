# Card Effect System Documentation

## Overview

Sistem kartu efek yang dibuat berdasarkan data Excel untuk game catur dengan efek kartu khusus. Sistem ini memungkinkan penerapan berbagai efek kartu pada bidak catur dengan parameter yang dapat dikonfigurasi.

## Struktur Sistem

### 1. Core Components

#### `CardEffectType.cs`
- Enum yang mendefinisikan semua jenis efek kartu berdasarkan data Excel
- Parameter efek yang dapat dikonfigurasi melalui `CardEffectParameters`

#### `ICardEffect.cs`
- Interface untuk semua efek kartu
- Base class `BaseCardEffect` untuk implementasi efek

#### `CardEffectManager.cs`
- Singleton manager untuk mengelola semua efek kartu aktif
- Menangani penerapan, penghapusan, dan update efek per turn

### 2. Data Structures

#### `ChessCardData.cs`
- ScriptableObject untuk menyimpan data kartu
- List `cardEffects` berisi semua efek yang dimiliki kartu
- Methods untuk mencari efek berdasarkan type

#### `ChessPieceData.cs`
- Diperluas dengan sistem efek kartu
- Tracking efek aktif, parameter pergerakan, dan status perlindungan
- Methods untuk apply/remove efek dan update per turn

### 3. Integration

#### `PieceController.cs`
- Diperluas dengan methods untuk menerapkan efek kartu
- Validasi pergerakan berdasarkan efek aktif
- Handling efek post-move

#### `TileController.cs`
- Logic untuk efek yang terkait dengan tile/posisi
- Handling efek khusus seperti promosi in-place dan revival

## Daftar Efek Kartu (Berdasarkan Excel)

### Movement Effects
1. **DoubleMove** - Dapat bergerak dua kali dalam satu giliran
2. **TripleMove** - Dapat bergerak tiga kali dalam satu giliran
3. **DiagonalAttack** - Dapat menyerang secara diagonal
4. **StraightMove** - Dapat bergerak lurus tanpa menyerang
5. **ForwardTwoMoves** - Dapat mundur satu petak
6. **TwoDirectionMove** - Dapat maju dua langkah
7. **BackMove** - Dapat bergerak mundur
8. **LeapMove** - Dapat bergerak ke samping
9. **RestoreMove** - Dapat bergerak ke samping
10. **SpecialMove** - Pergerakan khusus ke samping

### Attack Effects
1. **QueenCollision** - Gajah dapat menghancurkan bidak diagonal saat menyerang
2. **ConquerorLeap** - Kuda dapat melompat dua kali
3. **PowerfulMove** - Dapat melewati bidak penghalang
4. **DanceLikeQueen** - Gajah dapat bergerak lurus setelah memakan
5. **NiceDay** - Ratu dapat bergerak dua kali jika tidak menyerang

### Protection Effects
1. **ProtectedRing** - Perlindungan ringan
2. **WhereIsMyDefense** - Perlindungan setelah tidak bergerak 2 turn
3. **NotToday** - Membatalkan serangan pada raja
4. **UnstoppableForce** - Benteng tidak dapat dimakan kecuali oleh mentri/raja
5. **RoyalCommand** - Perlindungan multiple bidak
6. **BlockadeMove** - Memblokir pergerakan bidak

### Special Effects
1. **BackFromDead** - Dapat menghidupkan kembali bidak
2. **StoneTomorrow** - Promosi pawn setelah 15 turn tidak bergerak
3. **TimeFrozen** - Efek pembekuan waktu

## Cara Menggunakan

### 1. Membuat Kartu dengan Efek

```csharp
// Buat CardEffectData
var doubleMoveEffect = new CardEffectData
{
    effectType = CardEffectType.DoubleMove,
    isEnabled = true,
    effectDescription = "Dapat bergerak dua kali dalam satu giliran",
    parameters = new CardEffectParameters
    {
        moveCount = 2,
        turnDuration = 3 // Berlaku 3 turn
    }
};

// Tambahkan ke ChessCardData
cardData.cardEffects.Add(doubleMoveEffect);
```

### 2. Menerapkan Efek ke Bidak

```csharp
// Melalui PieceController
pieceController.ApplyCardEffect(cardData);

// Atau langsung apply effect
pieceController.ApplyEffect(effectData);
```

### 3. Mengecek Efek Aktif

```csharp
// Cek apakah bidak memiliki efek tertentu
if (pieceController.HasCardEffect(CardEffectType.DoubleMove))
{
    // Bidak dapat bergerak dua kali
}

// Dapatkan parameter efek
var params = pieceData.GetEffectParameters(CardEffectType.DoubleMove);
```

### 4. Manajemen Turn

```csharp
// Advance ke turn berikutnya (mengurangi durasi efek)
CardEffectManager.Instance.NextTurn();

// Reset movement untuk turn baru
pieceController.StartNewTurn();
```

## Contoh Implementasi Kartu Excel

### 1. Queen Collision (Legendary)
```csharp
var queenCollisionCard = ScriptableObject.CreateInstance<ChessCardData>();
queenCollisionCard.cardName = "Queen Collision";
queenCollisionCard.rank = ChessCardData.CardRank.Legendary;
queenCollisionCard.description = "Saat gajah menyerang dan memakan bidak lawan, bidak lawan yang sejajar secara diagonal juga ikut dihancurkan (maksimal 2 bidak)";

var effect = new CardEffectData
{
    effectType = CardEffectType.QueenCollision,
    parameters = new CardEffectParameters
    {
        affectsMultiplePieces = true,
        maxTargets = 2
    }
};
```

### 2. Conqueror's Leap (Rare)
```csharp
var conquerorCard = ScriptableObject.CreateInstance<ChessCardData>();
conquerorCard.cardName = "Conqueror's Leap";
conquerorCard.rank = ChessCardData.CardRank.Rare;
conquerorCard.description = "Kuda dapat melompat dua kali dalam satu giliran selama keduanya merupakan langkah legal";

var effect = new CardEffectData
{
    effectType = CardEffectType.ConquerorLeap,
    parameters = new CardEffectParameters
    {
        moveCount = 2,
        turnDuration = 1
    }
};
```

### 3. Stone Tomorrow (Rare)
```csharp
var stoneCard = ScriptableObject.CreateInstance<ChessCardData>();
stoneCard.cardName = "Stone Tomorrow";
stoneCard.rank = ChessCardData.CardRank.Rare;
stoneCard.description = "Setelah tidak bergerak selama 15 giliran, Pawn akan melakukan promosi di tempat";

var effect = new CardEffectData
{
    effectType = CardEffectType.StoneTomorrow,
    parameters = new CardEffectParameters
    {
        canPromoteInPlace = true,
        turnsCooldown = 15
    }
};
```

## Testing

Gunakan `CardEffectExample.cs` untuk testing:

1. Attach script ke GameObject
2. Set test piece dan card references
3. Enable `testEffectsOnStart`
4. Run scene untuk melihat efek dalam action

## Notes

- Semua efek implementasi sudah sesuai dengan deskripsi Excel
- Parameter dapat dikustomisasi melalui inspector
- Sistem mendukung multiple efek pada satu bidak
- Durasi efek dikelola otomatis per turn
- Protection effects menghormati hierarki kekuatan bidak

## Extension

Untuk menambah efek baru:

1. Tambahkan enum baru di `CardEffectType`
2. Tambahkan parameter yang diperlukan di `CardEffectParameters`
3. Implementasikan logic di `ApplySpecificEffect` methods
4. Update validation di `TileController` dan `PieceController`