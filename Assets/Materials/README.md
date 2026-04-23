# Materials

Place Unity Material (`.mat`) files here.

## Suggested materials

| Material name              | Shader              | Notes                                               |
|----------------------------|---------------------|-----------------------------------------------------|
| `Planet_Idle.mat`          | URP/Lit             | Dark grey base colour for the dormant planet state  |
| `Planet_Active.mat`        | URP/Lit             | Vibrant green/teal for the active focus state       |
| `GlowingGrass.mat`         | URP/Unlit           | Emissive cyan-green for grass blades                |
| `GlowingTree.mat`          | URP/Unlit           | Emissive blue-white for the tree                    |
| `Relic.mat`                | URP/Lit             | Stone texture with subtle emissive runes            |

All emissive materials should have **HDR Emission** enabled to allow bloom via
the URP Post-Processing stack.
