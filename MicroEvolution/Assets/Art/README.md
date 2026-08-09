# Art drop folder

Put authored assets here. Chat cannot attach `.glb` / `.gltf` / `.zip` — use one of the methods below.

## Models (glTF)

| Path | Use for |
| --- | --- |
| `Models/Cells/` | Player, prey, predator, ally cell meshes (`.glb` preferred) |
| `Models/Biomes/` | Optional props / rock / vent pieces |

**Preferred format:** single-file **`.glb`** (embeds textures).  
If you use `.gltf` + `.bin` + textures, keep the whole set in one subfolder.

### Suggested names

```
Models/Cells/player_core.glb
Models/Cells/prey_soft.glb
Models/Cells/predator_spiky.glb
Models/Cells/ally_rod.glb
```

## How to get files into this repo

1. **GitHub web UI** — open this branch → upload into `MicroEvolution/Assets/Art/Models/Cells/` → commit.
2. **Local clone** — copy files into the folder above → `git add` → `git push` → tell the agent.
3. **Public/direct URL** — paste a download link to a `.glb` (not a Drive “view” page) in chat.

After files land on the branch, ask the agent to wire them into `CellAppearance` / loaders.
