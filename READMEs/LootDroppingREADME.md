# Files changed
- Assets/Prefabs/Combat/Regen/HealthRegen.prefab
- Assets/Prefabs/Combat/Regen/AmmoRegen.prefab
- Assets/Scripts/Misc/Dropper.cs
- Assets/Scripts/Combat/Regen/SmallHealthPickup.cs
- Assets/Scripts/Combat/Regen/SmallAmmoPickup.cs
- Assets/Scenes/Showcase/MP/Dropping.unity
- Assets/Prefabs/Enemies/Chimp.prefab (Modified)
- Assets/Prefabs/Enemies/Gorilla.prefab (Modified)

# Description:
Enemies now drop health or ammo pickups when slain. Chance to drop a random pickup can be modified in the Dropper script using the Drop Chance property.

Pickups have a lifetime and will de-spawn when the lifetime expires.

Changes can be tested in the vertical slice scene or in Showcase/MP/Dropping scene, where the drop chance is set to always drop.