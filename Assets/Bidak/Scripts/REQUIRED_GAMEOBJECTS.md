# Required GameObjects untuk Card Effect System

## 🎯 **GameObjects yang Wajib Ada**

### **1. CardEffectManager**
```
GameObject Name: CardEffectManager
Component: CardEffectManager.cs
Fungsi: Mengelola lifecycle semua card effects
Location: Assets/Bidak/Scripts/Manager/CardEffectManager.cs
```

### **2. CardTargetingSystem**
```
GameObject Name: CardTargetingSystem  
Component: CardTargetingSystem.cs
Fungsi: Handle targeting pieces untuk card effects
Location: Assets/Bidak/Scripts/Card/CardTargetingSystem.cs
```

### **3. CameraSwitch**
```
GameObject Name: CameraSwitch
Component: CameraSwitch.cs (dari Assets/Script/)
Fungsi: Handle switching antara card camera dan main camera
Location: Assets/Script/CameraSwitch.cs
```

### **4. EventSystem**
```
GameObject Name: EventSystem
Components: EventSystem, StandaloneInputModule
Fungsi: Handle UI interactions (click, hover)
Note: Unity akan create otomatis jika tidak ada
```

## 🎮 **Camera Setup yang Diperlukan**

### **Main Cameras (untuk menggerakkan pieces)**
```
Player1 Main Camera:
- Tag: "MainCamera" 
- Component: Camera, PhysicsRaycaster
- Fungsi: Main game view untuk Player 1 (menggerakkan pieces)

Player2 Main Camera:
- Tag: "MainCamera"
- Component: Camera, PhysicsRaycaster  
- Fungsi: Main game view untuk Player 2 (menggerakkan pieces)
```

### **Card Cameras (untuk memilih cards)**
```
Player1 Card Camera:
- Tag: "Player1Camera"  
- Component: Camera, PhysicsRaycaster
- Fungsi: Card view untuk Player 1 (memilih cards)

Player2 Card Camera:
- Tag: "Player2Camera"
- Component: Camera, PhysicsRaycaster  
- Fungsi: Card view untuk Player 2 (memilih cards)
```

### **Struktur Camera System:**
```
Player 1 Setup:
- Main Camera: mainCameras[1] (untuk chess pieces)
- Card Camera: Player1Camera (untuk cards)

Player 2 Setup:  
- Main Camera: mainCameras[0] (untuk chess pieces)
- Card Camera: Player2Camera (untuk cards)
```

## ⚙️ **Setup Step-by-Step**

### **Manual Setup:**

1. **Create CardEffectManager:**
   ```
   1. Create Empty GameObject
   2. Rename ke "CardEffectManager"
   3. Add Component: CardEffectManager
   ```

2. **Create CardTargetingSystem:**
   ```
   1. Create Empty GameObject
   2. Rename ke "CardTargetingSystem"
   3. Add Component: CardTargetingSystem
   ```

3. **Setup CameraSwitch:**
   ```
   1. Create Empty GameObject (atau use existing)
   2. Rename ke "CameraSwitch"
   3. Add Component: CameraSwitch
   4. Assign camera references di inspector
   ```

4. **Setup Cameras:**
   ```
   Main Cameras (untuk chess pieces):
   - 2 cameras dengan tag "MainCamera"
   - Add PhysicsRaycaster ke keduanya
   - Position untuk Player1 dan Player2 chess view
   
   Card Cameras (untuk cards):
   - 1 camera dengan tag "Player1Camera" 
   - 1 camera dengan tag "Player2Camera"
   - Add PhysicsRaycaster ke keduanya
   - Position sesuai card view masing-masing player
   ```

## 🚀 **Auto Setup (Recommended)**

### **Gunakan IntegratedCardSystemSetup:**

1. **Create Setup GameObject:**
   ```
   1. Create Empty GameObject
   2. Rename ke "SystemSetup"
   3. Add Component: IntegratedCardSystemSetup
   ```

2. **Configure Setup:**
   ```
   - Set setupOnStart = true
   - Set createMissingComponents = true
   - Assign camera references jika sudah ada
   ```

3. **Run Setup:**
   ```
   - Play scene (auto setup on start)
   - Atau Right-click component → "Setup Integrated Card System"
   ```

## 🔍 **Validation Checklist**

Setelah setup, pastikan ada:
- [x] EventSystem (untuk UI interactions)
- [x] CardEffectManager (untuk effect management)
- [x] CardTargetingSystem (untuk piece targeting)
- [x] CameraSwitch (untuk camera switching)
- [x] Main Camera dengan PhysicsRaycaster
- [x] Player cameras dengan proper tags
- [x] ChessCardManager (sudah ada)
- [x] ChessCardDisplay (sudah ada)

## 🎮 **Testing Setup**

1. **Test Card Selection:**
   - Click kartu → should turn green
   - Camera should auto-switch ke main camera

2. **Test Piece Targeting:**
   - Hover pieces → should highlight green/red
   - Click valid piece → effect should apply

3. **Test Camera Switching:**
   - Ctrl + Right Mouse → Card camera
   - Ctrl + Left Mouse → Main camera

## ⚠️ **Common Issues**

1. **Cards tidak clickable:**
   - Check EventSystem ada
   - Check card punya Collider
   - Check PhysicsRaycaster di camera

2. **Targeting tidak work:**
   - Check pieces punya tag "Piece"
   - Check LayerMask settings
   - Check CardTargetingSystem active

3. **Camera tidak switch:**
   - Check camera tags benar
   - Check CameraSwitch references assigned
   - Check camera aktif

## 📁 **File Locations**

```
Required Scripts:
Assets/Bidak/Scripts/Manager/CardEffectManager.cs
Assets/Bidak/Scripts/Card/CardTargetingSystem.cs
Assets/Script/CameraSwitch.cs
Assets/Bidak/Scripts/Setup/IntegratedCardSystemSetup.cs (optional auto-setup)
```