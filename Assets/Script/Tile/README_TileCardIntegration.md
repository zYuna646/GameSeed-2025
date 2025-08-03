# Tile Card Integration Documentation

## 🎯 **Overview**

Tile.cs telah diintegrasikan dengan card targeting system untuk memberikan visual feedback dan handling ketika card effects diterapkan ke tiles dan pieces.

## ✨ **New Features**

### **1. Visual Feedback System**
- **Valid Target**: Tile berwarna hijau dan scale up
- **Invalid Target**: Tile berwarna merah  
- **Card Hover**: Real-time highlighting saat mouse hover
- **Effect Application**: Flash animation saat card effect diterapkan

### **2. Floating Card Display**
- **Card Prefab**: Floating card instance di atas tile/piece
- **Position Logic**: Float di atas piece jika ada, atau di atas tile jika kosong
- **Animation**: Smooth floating motion dengan rotation
- **Card Data**: Menampilkan nama dan icon card yang sedang digunakan

### **3. Smart Targeting Validation**
- **Player Check**: Validasi apakah piece milik player yang sama dengan card
- **Effect Type**: Berbeda validation untuk allied vs enemy effects
- **Empty Tile**: Support untuk effects yang bisa target empty tiles
- **Real-time**: Validation update langsung saat hover

## 🎮 **User Experience Flow**

### **Card Targeting Workflow:**
```
1. Player selects card → Targeting mode activated
2. Mouse hover tile → Visual feedback (green/red)
3. Valid tile → Floating card appears above
4. Click tile → Effect applied + visual confirmation
5. Targeting ends → UI restored to normal
```

### **Visual States:**
```
Normal State:
- Original tile color
- Normal scale
- No floating elements

Targeting State (Valid):
- Green color (cardTargetValidColor)
- Scaled up
- Floating card above tile/piece
- Smooth animations

Targeting State (Invalid):
- Red color (cardTargetInvalidColor)
- Normal scale
- No floating card

Effect Applied:
- White flash animation (3x)
- Return to normal state
- Effect applied to piece
```

## ⚙️ **Configuration**

### **Inspector Settings:**
```csharp
[Header("Card Integration")]
public Color cardTargetValidColor = Color.green;     // Valid target color
public Color cardTargetInvalidColor = Color.red;     // Invalid target color  
public Color cardHoverColor = Color.yellow;          // Hover state color
public float cardFloatHeight = 2f;                   // Height above tile/piece
public float cardFloatSpeed = 2f;                    // Animation speed
public GameObject floatingCardPrefab;                // Card prefab to instantiate
```

### **Floating Card Prefab Requirements:**
```
Card Prefab Structure:
├── Root GameObject
├── UI Elements (optional):
│   ├── Text component (name contains "Name" or "Title")
│   └── Image component (name contains "Icon" or "Image")
└── Visual Elements:
    ├── Card background/frame
    └── Effects/animations
```

## 🔧 **Key Methods**

### **Public Methods:**
```csharp
public void ApplyCardEffectToTile()
// Manually trigger card effect application
// Called automatically on mouse click during targeting
```

### **Core Integration Methods:**
```csharp
void CheckCardTargeting()
// Continuous check for targeting state
// Called every Update()

void StartCardTargeting() 
// Begin targeting on this tile
// Shows visuals and floating card

void EndCardTargeting()
// End targeting on this tile  
// Restore original state

bool ValidateCardTarget()
// Check if current card can target this tile
// Returns true/false for visual feedback
```

### **Visual Methods:**
```csharp
void ShowFloatingCard()
// Instantiate and position floating card
// Start floating animation

void HideFloatingCard()
// Remove floating card instance
// Stop all card animations

IEnumerator FloatCardAnimation()
// Smooth floating motion
// Sine wave + rotation

IEnumerator ShowEffectApplication()
// Flash effect when card applied
// White flash 3 times
```

## 🎯 **Targeting Validation Logic**

### **Player Validation:**
```csharp
bool IsSamePlayer(PieceController piece, int cardPlayerIndex)
// Player 1 = White pieces (Color.white)
// Player 2 = Black pieces (Color.black)
// Cards check piece ownership before allowing targeting
```

### **Effect-Specific Targeting:**
```csharp
// Allied Effects (affect own pieces):
- Most card effects default to this
- Protection effects
- Enhancement effects

// Enemy Effects (affect opponent pieces):
- Attack effects
- Debuff effects  
- Specified with affectsEnemyPieces = true

// Empty Tile Effects:
- BackFromDead (revive pieces)
- StoneTomorrow (promotion effects)
- LeapMove (movement effects)
```

## 🎨 **Visual Customization**

### **Color Scheme:**
```csharp
// Default Colors (customizable in Inspector):
cardTargetValidColor = Color.green;      // Valid targets
cardTargetInvalidColor = Color.red;      // Invalid targets  
cardHoverColor = Color.yellow;           // Hover state
originColor = (current tile color);      // Normal state
```

### **Animation Parameters:**
```csharp
cardFloatHeight = 2f;     // Height above tile (units)
cardFloatSpeed = 2f;      // Animation speed multiplier
// Sine wave amplitude: 0.2f units
// Rotation speed: 30 degrees/second
```

### **Scale Effects:**
```csharp
// Valid targets: Scale up using tileManager.ScaleUp()
// Invalid targets: Normal scale
// Flash effect: Color flashing (no scale change)
```

## 🔌 **Integration Points**

### **Dependencies:**
```csharp
using Bidak.Manager;      // CardTargetingSystem
using Bidak.Data;         // ChessCardData, CardEffectType
```

### **Required Components:**
```csharp
CardTargetingSystem cardTargetingSystem;  // Found via FindObjectOfType
TileManager tileManager;                  // Existing dependency
TileController tileController;            // Existing dependency
PieceController (on child objects);       // For piece targeting
```

### **Events & Callbacks:**
```csharp
// Mouse Events:
OnMouseDown() → ApplyCardEffectToTile()

// Update Events:  
Update() → CheckCardTargeting()

// State Changes:
StartCardTargeting() → Visual feedback starts
EndCardTargeting() → Visual feedback ends
```

## 🚀 **Advanced Features**

### **Multi-Target Support:**
```csharp
// Current: Single target per card
// Extensible: maxTargets parameter in CardEffectParameters
// Future: Area effects, multiple piece targeting
```

### **Effect Preview:**
```csharp
// Current: Floating card shows selected card
// Future: Preview effect results
// Future: Range indicators, movement paths
```

### **Animation Extensions:**
```csharp
// Current: Simple float + rotation
// Extensible: Custom animations per card type
// Extensible: Particle effects, trails
```

## 🐛 **Troubleshooting**

### **Targeting Not Working:**
```
1. Check CardTargetingSystem exists in scene
2. Verify Tile has proper colliders
3. Ensure Camera.main is assigned
4. Check card selection state
```

### **Visual Issues:**
```
1. Verify MeshRenderer component exists
2. Check TileSelect child object with proper tag
3. Ensure floatingCardPrefab is assigned
4. Verify color values are not identical
```

### **Performance Issues:**
```
1. Limit floating card complexity
2. Use object pooling for frequent cards
3. Optimize raycast frequency
4. Cache component references
```

## 📈 **Performance Notes**

### **Optimizations:**
- **Component Caching**: FindObjectOfType called only in Start()
- **Conditional Updates**: Card targeting only when needed
- **Coroutine Management**: Proper cleanup of floating animations
- **Raycast Efficiency**: Single raycast per Update() during targeting

### **Memory Management:**
- **Instance Cleanup**: Floating cards destroyed when targeting ends
- **Coroutine Cleanup**: Stopped when objects destroyed
- **Event Unsubscription**: Proper cleanup in OnDestroy()

The tile integration provides a complete visual feedback system for card targeting yang smooth dan responsive!