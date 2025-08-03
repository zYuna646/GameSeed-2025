# Camera Setup Guide untuk Card Effect System

## 🎮 **Struktur Camera yang Anda Miliki**

### **Berdasarkan CameraSwitch.cs Anda:**

```
Player 1:
├── Main Camera: mainCameras[1] (Tag: "MainCamera")
│   └── Fungsi: Menggerakkan chess pieces Player 1
└── Card Camera: Player1Camera (Tag: "Player1Camera") 
    └── Fungsi: Memilih cards Player 1

Player 2:
├── Main Camera: mainCameras[0] (Tag: "MainCamera")
│   └── Fungsi: Menggerakkan chess pieces Player 2  
└── Card Camera: Player2Camera (Tag: "Player2Camera")
    └── Fungsi: Memilih cards Player 2
```

## ⚙️ **Yang Perlu Anda Setup di Scene**

### **1. Main Cameras (untuk Chess Pieces)**
Anda membutuhkan **2 Main Cameras** dengan:
```
Camera 1:
- Name: bebas (contoh: "Player1MainCamera" atau "ChessCamera1")
- Tag: "MainCamera" 
- Component: Camera + PhysicsRaycaster
- Position: View untuk Player 1 menggerakkan pieces

Camera 2:  
- Name: bebas (contoh: "Player2MainCamera" atau "ChessCamera2")
- Tag: "MainCamera"
- Component: Camera + PhysicsRaycaster  
- Position: View untuk Player 2 menggerakkan pieces
```

### **2. Card Cameras (untuk Cards)**
Anda membutuhkan **2 Card Cameras** dengan:
```
Player1 Card Camera:
- Name: bebas (contoh: "Player1CardCamera")
- Tag: "Player1Camera"
- Component: Camera + PhysicsRaycaster
- Position: View untuk Player 1 memilih cards

Player2 Card Camera:
- Name: bebas (contoh: "Player2CardCamera")  
- Tag: "Player2Camera"
- Component: Camera + PhysicsRaycaster
- Position: View untuk Player 2 memilih cards
```

## 🔧 **Setup Instructions**

### **Manual Setup:**

1. **Check Camera Tags:**
   ```
   ✓ 2 cameras dengan tag "MainCamera" 
   ✓ 1 camera dengan tag "Player1Camera"
   ✓ 1 camera dengan tag "Player2Camera"
   ```

2. **Add PhysicsRaycaster ke semua cameras:**
   ```
   Untuk setiap camera:
   1. Select camera GameObject
   2. Add Component → Event → Physics Raycaster
   ```

3. **Setup CameraSwitch GameObject:**
   ```
   1. Pastikan ada GameObject dengan CameraSwitch component
   2. Set player field (0 atau 1) sesuai dengan player
   ```

### **Auto Setup (Recommended):**

1. **Create Setup GameObject:**
   ```
   1. Create Empty GameObject
   2. Rename: "SystemSetup"
   3. Add Component: IntegratedCardSystemSetup
   4. Set setupOnStart = true
   5. Play scene → auto setup!
   ```

## 🎯 **Cara Kerja Camera Switching**

### **Flow untuk Player 1 (player = 0):**
```
Start:
├── camera1 = mainCameras[1] (chess view)  ← Active
├── camera2 = Player1Camera (card view)    ← Inactive
├── Disable: mainCameras[0] dan Player2Camera

Card Selection:
├── Ctrl + Right Mouse → Switch ke camera2 (cards)
├── Click card → Auto switch ke camera1 (chess)
├── Target piece → Effect applied
├── Ctrl + Left Mouse → Manual switch ke camera1
```

### **Flow untuk Player 2 (player = 1):**
```
Start:
├── camera1 = mainCameras[0] (chess view)  ← Active  
├── camera2 = Player2Camera (card view)    ← Inactive
├── Disable: mainCameras[1] dan Player1Camera

Card Selection:
├── Ctrl + Right Mouse → Switch ke camera2 (cards)
├── Click card → Auto switch ke camera1 (chess)
├── Target piece → Effect applied
├── Ctrl + Left Mouse → Manual switch ke camera1
```

## 🎮 **Controls Summary**

### **Manual Camera Controls:**
- **Ctrl + Left Mouse**: Switch ke Main Camera (chess pieces)
- **Ctrl + Right Mouse**: Switch ke Card Camera (cards)

### **Automatic Switching:**
- **Card Selected**: Auto switch ke Main Camera untuk targeting
- **Targeting Done**: Stay di Main Camera

### **Card Interaction:**
- **Single Click Card**: Select card (hijau) + auto switch ke chess view
- **Click Chess Piece**: Apply effect + deactivate card (merah)
- **Right Click/Escape**: Cancel targeting

## ✅ **Validation Checklist**

Pastikan ada di scene Anda:
- [ ] **2 cameras** dengan tag "MainCamera"
- [ ] **1 camera** dengan tag "Player1Camera"  
- [ ] **1 camera** dengan tag "Player2Camera"
- [ ] **PhysicsRaycaster** di semua 4 cameras
- [ ] **CameraSwitch** GameObject dengan player value set
- [ ] **EventSystem** (auto-created)
- [ ] **CardEffectManager** GameObject
- [ ] **CardTargetingSystem** GameObject

## 🚨 **Troubleshooting**

### **Cards tidak clickable:**
- Check semua card cameras punya PhysicsRaycaster
- Check EventSystem ada di scene
- Check card prefabs punya Collider

### **Camera tidak switch:**
- Check camera tags benar ("MainCamera", "Player1Camera", "Player2Camera")
- Check CameraSwitch player value correct (0 atau 1)
- Check semua cameras referenced properly

### **Targeting tidak work:**
- Check main cameras punya PhysicsRaycaster  
- Check pieces punya tag "Piece"
- Check CardTargetingSystem active

## 🎯 **Next Steps**

Setelah camera setup:
1. **Test manual switching**: Ctrl + Mouse buttons
2. **Test card selection**: Click cards di card camera
3. **Test piece targeting**: Click pieces di main camera
4. **Test effect application**: Verify effects apply to pieces

Camera system Anda sudah bagus - tinggal pastikan tags dan PhysicsRaycaster setup correct!