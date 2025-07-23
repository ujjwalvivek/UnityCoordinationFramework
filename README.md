# Unity Coordination Framework

A modular Unity toolkit that demonstrates:
- **Deterministic Input-Based Simulation**: Fake multiplayer coordination without a network  
- **Advanced Weapon Inventory System**: Data-driven, extensible FPS weapon framework  

## Overview

This repository extracts two educational systems from a single-player prototype:

1. **CoordinationSystem**  
   - Minimal-byte state synchronization via input lockstep. 
   - Deterministic simulation allowing multiple instances to stay in sync.
2. **InventoryFramework & WeaponSystem**  
   - ScriptableObject-driven weapon definitions.  
   - Abstract base classes for modular weapon behavior.  
   - HUD integration, recoil, and camera-shake feedback.  

## Repository Structure

```bash
UnityCoordinationFramework/                             ← Unity 2020.3+ project root
├── Assets/                                             
│   ├── Controller/                                     ← First-person controller assets
│   ├── Fonts/                                          ← TextMesh Pro fonts
│   ├── Icons/                                          ← UI icon textures
│   ├── Interior/                                       ← Demo scene interiors
│   ├── Items/                                          ← ScriptableObject assets for weapons
│   ├── Materials/                                      ← Custom materials
│   ├── Particles/                                      ← Particle effects (muzzle flashes)
│   ├── Presets/                                        ← Post-processing presets
│   ├── Resources/                                      ← Runtime resource folder
│   ├── Scenes/                                         ← Well, Scenes
│   ├── Scripts/                                        
│   │   ├── Main/                                       ← Scene-navigation scripts (ReturnToMain, SwitchScene)
│   │   ├── Task A/                                     ← `Local` & `Remote` Player Coordination
│   │   └── Task B/                                     ← Weapon/Inventory:
│   │       ├── CamShake/                               ← Camera shake coroutines
│   │       ├── Controller/                             ← FirstPersonController logic (equip, recoil)
│   │       ├── UI/                                     ← Crosshair & ammo UI handlers
│   │       └── Weapon Architecture/  
│   │           ├── Abstract Item & WeaponData  
│   │           ├── SingleShotWeapons, HeavyWeapons  
│   │           └── WeaponHandler, WeaponVariables  
│   ├── Settings/                                       ← Unity project settings assets
│   ├── Skybox/                                         ← Skybox materials
│   ├── Sprites/                                        ← UI sprites
│   ├── TextMesh Pro/                                   ← TMPro resources
│   ├── Volume/                                         ← Post-processing volume profiles
│   └── Weapon Models/                                  ← 3D weapon models & prefabs
├── Packages/                                           ← Unity Package Manager manifest
├── ProjectSettings/                                    ← ProjectSettings folder
├── UnityCoordinationFramework.sln                      ← Visual Studio solution
├── Assembly-CSharp.csproj                              ← Assembly project
├── Assembly-CSharp-Editor.csproj
├── .gitignore
└── .gitattributes
```

## Quick Start

```bash
# Clone & Open the Repository
git clone https://github.com/ujjwalvivek/UnityCoordinationFramework.git
cd UnityCoordinationFramework

#Open the UnityCoordinationFramework.sln in Visual Studio or the project folder in Unity 2020.3 LTS or later.
```

## Explore Scenes

* **Main.unity**: Scene selector for Task A/B demos
* **Task A.unity**: Deterministic coordination showcase
* **Task B.unity**: FPS weapon & inventory demo

### Tunable Parameters

* **In Task A**: adjust `FixedUpdate timestep` and `input thresholds` in LocalPlayerMovement.
* **In Task B**: configure damage, fireRate, magazineSize, recoil, and camera shake via `WeaponVariables` and `ScriptableObject` assets in Assets/Items/.

### Extending Weapon System

1. Create a new ScriptableObject under Assets/Items/ by duplicating an existing WeaponData.
2. Implement a new subclass of Item or extend SingleShotWeapons/HeavyWeapons.
3. Link UI icons in Assets/Icons/ and assign prefabs in Assets/Weapon Models/.

---

Run & Exlore around :)

## License 

MIT License.