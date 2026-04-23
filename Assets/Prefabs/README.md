# Prefabs

Place Unity Prefab (`.prefab`) files here.

## Required prefabs for the Ecosystem feature

| Prefab name               | Description                                      |
|---------------------------|--------------------------------------------------|
| `GlowingGrass.prefab`     | Low-Poly fluorescent grass patch (Level 1)       |
| `GlowingTree.prefab`      | Low-Poly glowing tree (Level 2)                  |
| `AncientRelic.prefab`     | Stone relic monument spawned after 3-day streak  |
| `Planet.prefab`           | Root prefab for the planet scene object          |

Each plant prefab should:
1. Contain a `MeshRenderer` with an emissive/unlit material for the glow effect.
2. Optionally contain a `ParticleSystem` for particle-based glow auras.
3. Have its pivot at the base so `positionOffset` in PlantData places it correctly on the surface.
