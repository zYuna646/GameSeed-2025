# Card Generator dari Excel Data

## 📋 **Overview**

Script generator untuk membuat ChessCardData assets berdasarkan data Excel yang telah disediakan. Generator ini akan create 14 kartu sesuai dengan spesifikasi Excel dengan semua effect dan parameter yang tepat.

## 🎯 **Cards yang Akan Di-Generate**

### **Legendary Cards (3)**
1. **Queen Collision** - Kartu Gaja dengan area damage diagonal
2. **Not Today** - Kartu Raja dengan protection dan position swap
3. **A Nice Day** - Kartu Queen dengan double move tanpa attack

### **Rare Cards (5)**
4. **Conqueror's Leap** - Kartu Knight dengan double leap attack
5. **Unstoppable Force** - Kartu Rook dengan light protection
6. **Dance Like Elephant** - Kartu Bishop dengan enhanced attack
7. **Back from Dead** - Kartu Universal untuk revive pieces
8. **Stone Tomorrow** - Kartu Pawn dengan in-place promotion

### **Common Cards (6)**
9. **Temporary Friend** - Kartu Pawn dengan blockade creation
10. **Where is my Horse** - Kartu Universal dengan protection
11. **It's Time to Go** - Kartu Pawn dengan backward + forward moves
12. **I Powerful** - Kartu Bishop dengan jump ability
13. **I Got You** - Kartu King dengan area protection
14. **It's a Move** - Kartu Universal dengan sideways movement

## 🚀 **Cara Menggunakan Generator**

### **Method 1: Unity Menu (Recommended)**
```
1. Di Unity Editor, klik menu "Bidak"
2. Pilih "Generate All Excel Cards"
3. Cards akan auto-generated ke folder "Assets/Bidak/data/Card/Generated/"
4. Dialog akan confirm berapa cards yang created
```

### **Method 2: Manual Script Execution**
```csharp
// Dalam script atau console
Bidak.Editor.ExcelCardGenerator.GenerateAllExcelCards();
```

### **Method 3: Individual Card Generation**
```csharp
// Generate single card for testing
var card = Bidak.Editor.ExcelCardGenerator.GenerateQueenCollision();
```

## 📁 **Output Structure**

Cards akan disave di:
```
Assets/Bidak/data/Card/Generated/
├── QueenCollision.asset
├── ConquerorLeap.asset
├── TemporaryFriend.asset
├── WhereIsMyHorse.asset
├── NotToday.asset
├── UnstoppableForce.asset
├── ItsTimeToGo.asset
├── IPowerful.asset
├── NiceDay.asset
├── DanceLikeElephant.asset
├── IGotYou.asset
├── BackFromDead.asset
├── StoneTomorrow.asset
└── ItsAMove.asset
```

## 🔧 **Generator Features**

### **Automatic Card Data Setup**
- ✅ **Card Name** dari Excel
- ✅ **Card Rarity** (Legendary/Rare/Common)
- ✅ **Card Type** (Pawn/Knight/Bishop/Rook/Queen/King/Universal)
- ✅ **Description** lengkap dalam Bahasa Indonesia
- ✅ **Card Effects** dengan parameters yang tepat

### **Effect Parameter Mapping**
Setiap card effect dilengkapi dengan parameters yang sesuai:
- **Movement**: moveCount, turnDuration, stepsForward
- **Attack**: canAttackDiagonally, canAttackStraight, affectsEnemyPieces
- **Protection**: hasLightProtection, hasFullProtection, protectionDuration
- **Special**: canRevive, canPromoteInPlace, canSwapPositions

### **Validation & Safety**
- ✅ Directory auto-creation jika belum ada
- ✅ Asset overwrite protection (bisa di-configure)
- ✅ Error handling untuk missing data
- ✅ Console logging untuk track progress

## 🎮 **Testing Generated Cards**

### **1. Validate Generation**
```
Menu: Bidak → Validate Card Data
Akan check semua generated cards dan log details
```

### **2. Manual Inspection**
```
1. Navigate ke Assets/Bidak/data/Card/Generated/
2. Select card asset
3. Check Inspector untuk details:
   - Card Name & Description
   - Rank & Type
   - Card Effects list
   - Effect Parameters
```

### **3. Runtime Testing**
```csharp
// Load dan test card
ChessCardData card = Resources.Load<ChessCardData>("QueenCollision");
Debug.Log($"Card: {card.cardName}");
Debug.Log($"Effects: {card.cardEffects.Count}");
```

## 🔄 **Management Tools**

### **Clear Generated Cards**
```
Menu: Bidak → Clear Generated Cards
Menghapus semua generated cards (dengan confirmation)
```

### **Open Cards Folder**
```
Menu: Bidak → Open Generated Cards Folder
Membuka folder di File Explorer/Finder
```

### **Regenerate Cards**
```
1. Clear existing cards (optional)
2. Run "Generate All Excel Cards" lagi
3. Fresh cards akan di-create
```

## 📊 **Generated Card Examples**

### **Queen Collision (Legendary)**
```yaml
Name: "Queen Collision"
Type: Bishop
Effect: QueenCollision
Parameters:
  - maxTargets: 2
  - affectsEnemyPieces: true
  - canAttackDiagonally: true
  - turnDuration: 1
Description: "Kartu Gaja. Saat gajah menyerang dan memakan bidak lawan..."
```

### **Conqueror's Leap (Rare)**
```yaml
Name: "Conqueror's Leap" 
Type: Knight
Effect: ConquerorLeap
Parameters:
  - moveCount: 2
  - turnDuration: 1
  - canAttackDiagonally: true
  - affectsEnemyPieces: true
Description: "Kartu Kuda. Kuda dapat melompat dua kali dalam satu giliran..."
```

## ⚙️ **Customization**

### **Modify Card Data**
```csharp
// Edit ExcelCardGenerator.cs untuk customize:
private static string GenerateCustomCard()
{
    var cardData = CreateBaseCard("Custom Card", ChessCardData.CardRank.Common, ChessCardData.CardType.Universal);
    cardData.description = "Custom description here";
    
    AddCardEffect(cardData, CardEffectType.DoubleMove, new CardEffectParameters
    {
        moveCount = 2,
        turnDuration = 1
    });
    
    return SaveCard(cardData, "CustomCard");
}
```

### **Add New Cards**
```csharp
// Add method ke ExcelCardGenerator
// Call dari GenerateAllExcelCards()
generatedCards.Add(GenerateCustomCard());
```

### **Modify Output Path**
```csharp
// Change OUTPUT_PATH constant
private const string OUTPUT_PATH = "Assets/YourCustomPath/";
```

## 🐛 **Troubleshooting**

### **Cards Not Generated**
- Check console untuk error messages
- Verify Excel data mapping di generator
- Ensure CardEffectType enum has all required effects

### **Missing Effects**
- Check CardEffectType.cs untuk required enums
- Update generator mapping jika ada new effects
- Verify CardEffectParameters has required fields

### **Asset Creation Failed**
- Check folder permissions
- Verify output path exists dan writable
- Clear existing assets yang corrupt

## 📈 **Next Steps**

Setelah generate cards:
1. **Test di Game**: Load cards ke ChessCardManager
2. **Assign Icons**: Add visual assets untuk cards
3. **Balance Testing**: Adjust parameters jika needed
4. **Localization**: Add multi-language support
5. **Animation**: Add card activation animations

Generator ini provides foundation untuk complete card system berdasarkan Excel specifications!