# Art assets (Meshy)

## Models (`Models/Cells/*.glb`)

| File | Role |
| --- | --- |
| `player_core.glb` | Player eukaryote |
| `prey_rod.glb` | Prey rod bacteria |
| `predator_spiky.glb` | Spiky predator |
| `predator_worm.glb` | Segmented predator |
| `ally_probe.glb` | Ally |
| `food_pellet.glb` | Nutrient orb |

Runtime copies live in `StreamingAssets/Models/` (synced by **MicroEvolution → Sync Art GLBs to StreamingAssets**).

Requires package `com.unity.cloud.gltfast`. Loader: `ArtModelLibrary`.

## Sprites

- `Sprites/UI/` — `icon_dna`, `icon_boost`, `icon_chem`, `icon_evo`, `icon_radar`
- `Sprites/Cells/` — thumbs + `sprite_player_ref`

Loader: `ArtSpriteLibrary` (HUD buttons use boost/chem/evo icons when present).

## Regenerating with Meshy

Use Meshy REST API or MCP (`MESHY_API_KEY`). Do **not** commit API keys.
