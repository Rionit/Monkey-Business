# Enemy spawning, input lock, main menu & death screen
This update documents:
- the new enemy spawning method in `GameManager.CombatPhase()`,
- releasing/locking player input via `CanReceiveInput` in `Player` and `EquipmentManager`,
- and the Main Menu + Death Screen scene flow.

**<span style="color:red"> ! THIS BRANCH DIRECTLY MODIFIES THE VERTICAL SLICE SCENE AND MAIN MENU SCENE, KEEP THIS IN MIND DURING REVIEW !  </span>**

Following files are added or modified in an important way:
```
.
├── Scripts
│   ├── Managers
│   │   ├── GameManager.cs
│   │   └── EquipmentManager.cs
│   ├── Player
│   │   └── Player.cs
│   └── UI
│       ├── MainMenuManager.cs
│       └── DeathScreenManager.cs
└── Scenes
		├── MainMenu
		└── Arena scene (default: VerticalSlice)
```

## New enemy spawning in `CombatPhase()`
Enemy waves are now defined by explicit per-wave composition (`gorillas`, `chimps`, `enemiesPerSpawn`, `enemiesAtOnce`) through `_waveDefinitions`.

When `CombatPhase()` starts:
- It picks wave config by index (`_currentWave`) and clamps to the last entry if waves exceed definition count.
- It fills `_typesToSpawn` dictionary for prefab type counters (`gorillaPrefab`, `chimpPrefab`).
- It computes `_enemiesRemaining = gorillas + chimps` and emits `OnEnemyCountChanged`.

Spawning loop behavior:
- Spawning continues while alive enemies (`_enemies.Count`) are below total enemies for the wave.
- Per batch size is limited by:
	- `waveInfo.enemiesPerSpawn`,
	- current alive-cap (`waveInfo.enemiesAtOnce - _enemies.Count`),
	- and `_enemiesPerWave` safety cap.
- For each enemy in the batch, enemy type is selected randomly but weighted by remaining counts in `_typesToSpawn`, so final composition always matches wave definition.
- Spawn points are rotated with `i % _enemiesSpawnedAtOnce`.
- There is a short `0.2s` delay between individual enemies and `_enemySpawnDelay` between spawn batches.

When `OnEnemyDestroyed` reduces `_enemiesRemaining` to `0`, wave is considered defeated:
- `_currentWave` increments,
- `OnWaveDefeated` is invoked,
- and `PreparationPhase()` starts again.

## Input lock/release with `CanReceiveInput`
Input gating is centralized in `GameManager` and consumed by:
- `Player.CanReceiveInput`
- `EquipmentManager.CanReceiveInput`

### In `PreparationPhase()`
- Input is disabled before perk selection UI (`CanReceiveInput = false` on both systems).
- Cursor is unlocked/confined for selection.
- After perk confirmation (`_perkSelected`), HUD returns and both input flags are set back to `true`.

### In `Player`
- If `CanReceiveInput == true`, regular movement/look/jump/crouch input is processed.
- If `CanReceiveInput == false`, the controller receives zeroed movement/jump input, effectively releasing movement control.

### In `EquipmentManager`
- Shooting in `Update()` only runs when `CanReceiveInput && _isShooting`.
- This prevents held-fire usage while input is locked by game state transitions.

### On player death
`GameManager.OnPlayerDeath()` does all of the following:
- `Time.timeScale = 0f` (freeze gameplay),
- hide HUD and show death screen,
- set `Player.CanReceiveInput = false` and `EquipmentManager.CanReceiveInput = false`,
- confine cursor for UI interaction.

## Main Menu & Death Screen flow
### Main menu (`MainMenuManager`)
- `Awake()` resets `Time.timeScale` to `1f`.
- `StartGame()` loads arena scene by serialized `ArenaSceneName` (default `VerticalSlice`).
- `QuitGame()` exits the application.

### Death screen (`DeathScreenManager`)
- `PlayAgain()` reloads current active scene.
- `GoToMainMenu()` loads serialized `MainMenuSceneName` (default `MainMenu`).

In practice, the loop is:
`MainMenu -> Arena -> DeathScreen (on death) -> PlayAgain OR MainMenu`.
