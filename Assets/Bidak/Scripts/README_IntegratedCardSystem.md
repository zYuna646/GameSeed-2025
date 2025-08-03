# Integrated Card Effect System Documentation

## Overview

Sistem kartu terintegrasi yang menggabungkan card effect system dengan camera switching dan piece targeting. Sistem ini memungkinkan pemain untuk:

1. **Memilih kartu** dari deck mereka
2. **Menargetkan bidak** untuk menerapkan efek
3. **Mengaktifkan kartu** untuk menerapkan efek ke bidak target
4. **Automatic camera switching** untuk transisi smooth antara card view dan main game view

## Komponen Sistem

### 1. Core Integration Components

#### `ChessCardHoverEffect` (Enhanced)
- **Card Selection**: Single click untuk select kartu
- **Card Activation**: Double click atau click saat selected untuk activate
- **Visual States**: 
  - Normal (warna asli)
  - Hover (highlight)
  - Selected (hijau)
  - Activated (merah - tidak bisa digunakan lagi)
- **Integration**: Otomatis integrate dengan camera switching dan targeting

#### `CardTargetingSystem` (New)
- **Piece Targeting**: Raycast-based targeting system
- **Visual Feedback**: Highlight bidak yang bisa ditarget (hijau) atau tidak valid (merah)
- **Target Validation**: Validasi berdasarkan efek kartu dan ownership bidak
- **Interactive Controls**: 
  - Left Click: Select target
  - Right Click/Escape: Cancel targeting

#### `CameraSwitch` (Enhanced)
- **Automatic Switching**: Switch ke main camera saat card selected untuk targeting
- **Manual Controls**: Ctrl + Left/Right mouse untuk manual switching
- **Integration Tracking**: Track selected cards dan targeting state

### 2. Enhanced Existing Components

#### `ChessCardDisplay` (Updated)
- Setup card data dan player index untuk setiap card instance
- Support untuk click events dan integration

#### `ChessCardManager` (Existing)
- Manage active dan storage cards untuk setiap player
- Events untuk card activation/deactivation

## Workflow Penggunaan

### 1. **Card Selection Flow**
```
1. Player in Card Camera view
2. Click pada kartu → Card becomes Selected (hijau)
3. Camera automatically switches ke Main Camera
4. Targeting mode activated
```

### 2. **Piece Targeting Flow**
```
1. Mouse hover pada bidak → Visual highlight (hijau/merah)
2. Click pada valid target → Target selected
3. Card automatically activated pada target
4. Card becomes deactivated (merah)
5. Effect applied ke target piece
```

### 3. **Effect Application Flow**
```
1. Card effect parameters diterapkan ke target piece
2. PieceController updated dengan capabilities baru
3. CardEffectManager tracks active effects
4. Turn management updates effect durations
```

## Controls dan Interactions

### **Card Controls**
- **Single Click**: Select card (change to green)
- **Double Click** (atau click saat selected): Activate card
- **Visual States**:
  - **Normal**: Default appearance
  - **Hover**: Slightly scaled dan brightened
  - **Selected**: Green tint
  - **Activated**: Red tint (disabled)

### **Camera Controls**
- **Ctrl + Left Mouse**: Switch ke Main Camera (manual)
- **Ctrl + Right Mouse**: Switch ke Card Camera (manual)
- **Automatic**: Switch ke Main Camera saat card selected

### **Targeting Controls**
- **Mouse Hover**: Highlight potential targets
- **Left Click**: Select target piece
- **Right Click/Escape**: Cancel targeting
- **Visual Feedback**:
  - **Green Highlight**: Valid target
  - **Red Highlight**: Invalid target

## Setup Instructions

### 1. **Scene Setup**
```
Required GameObjects:
- ChessCardManager (dengan card data)
- CardTargetingSystem
- CameraSwitch (dengan camera references)
- CardEffectManager (singleton)
- Main Camera (tagged "MainCamera")
- Player1Camera (tagged "Player1Camera") 
- Player2Camera (tagged "Player2Camera")
```

### 2. **Card Prefab Setup**
```
Card Prefab Requirements:
- Collider untuk interaction
- Renderer untuk visual feedback
- CardHeader child dengan "CardHeader" tag
- Proper materials assigned
```

### 3. **Piece Setup**
```
Piece Requirements:
- PieceController component
- ChessPieceData dengan card effect support
- Collider untuk targeting
- "Piece" tag
```

## Code Examples

### **Creating a Card with Effects**
```csharp
// Create card data
var cardData = ScriptableObject.CreateInstance<ChessCardData>();
cardData.cardName = "Double Move";
cardData.rank = ChessCardData.CardRank.Common;

// Add effect
var effect = new CardEffectData
{
    effectType = CardEffectType.DoubleMove,
    isEnabled = true,
    parameters = new CardEffectParameters
    {
        moveCount = 2,
        turnDuration = 3
    }
};
cardData.cardEffects.Add(effect);

// Add to player
cardManager.AddCardToStorage(1, cardData);
cardManager.ActivateCard(1, cardData);
```

### **Programmatic Card Activation**
```csharp
// Find card hover effect
ChessCardHoverEffect cardEffect = cardObject.GetComponent<ChessCardHoverEffect>();

// Set data
cardEffect.SetCardData(cardData, playerIndex);

// Programmatically select and activate
cardEffect.OnPointerClick(new PointerEventData(EventSystem.current));
```

### **Manual Targeting**
```csharp
// Start targeting for specific card
CardTargetingSystem targeting = FindObjectOfType<CardTargetingSystem>();
ChessCardHoverEffect card = GetSelectedCard();
targeting.StartTargeting(card);

// Check if valid target selected
if (targeting.HasValidTarget())
{
    PieceController target = targeting.GetTargetPiece();
    // Apply effects
}
```

## Advanced Features

### **Card Effect Validation**
Sistem otomatis validasi apakah card effect bisa diterapkan ke target:
- **Own Pieces**: Kebanyakan effect hanya bisa diterapkan ke bidak sendiri
- **Enemy Pieces**: Beberapa attack effect bisa target bidak lawan
- **Piece Type Restrictions**: Beberapa effect hanya untuk piece type tertentu

### **Turn Management Integration**
```csharp
// Advance turn dan update effects
CardEffectManager.Instance.NextTurn();

// Reset piece movements
foreach(var piece in allPieces)
{
    piece.StartNewTurn();
}
```

### **Camera State Tracking**
```csharp
// Check current camera mode
if (cameraSwitch.isInCardMode)
{
    // Player in card selection mode
}
else
{
    // Player in main game mode
}
```

## Demo dan Testing

### **CardSystemIntegrationDemo**
Script untuk testing complete integration:
- **F1**: Add test cards ke players
- **Space**: Advance turn
- **R**: Reset demo
- **Runtime GUI**: Shows system state

### **Card Effect Examples**
Lihat `CardEffectExample.cs` untuk implementasi semua Excel card effects.

## Troubleshooting

### **Common Issues**

1. **Cards tidak clickable**
   - Check Collider ada dan configured
   - Check EventSystem ada di scene
   - Check PhysicsRaycaster ada di camera

2. **Targeting tidak work**
   - Check piece punya Collider
   - Check piece tagged sebagai "Piece"
   - Check LayerMask settings

3. **Camera tidak switch**
   - Check camera references di CameraSwitch
   - Check camera tags correct
   - Check CameraSwitch.enabled = true

4. **Effects tidak applied**
   - Check CardEffectManager active
   - Check piece punya PieceController
   - Check effect validation logic

## Performance Notes

- **Object Pooling**: Consider pooling card instances untuk better performance
- **Effect Cleanup**: Effects otomatis cleaned up saat duration habis
- **Memory Management**: Card instances proper destroyed saat deactivated

## Extension Points

Sistem designed untuk extensibility:
- **Custom Effects**: Add new CardEffectType enum values
- **Custom Validation**: Override targeting validation logic
- **Custom UI**: Replace hover effects dengan custom UI elements
- **Network Support**: Add networking untuk multiplayer

Sistem ini provides complete integration antara card selection, effect application, dan camera management untuk smooth player experience!