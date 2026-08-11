# Connect MicroEvolution to Unity (via Cursor)

This repo is a **Unity 2022.3 LTS** project. Cursor edits the code; Unity runs Play Mode / builds the APK. They share the same folder on disk.

## 1. Get the project

**Option A — Git (best)**  
Branch: `cursor/micro-evolution-unity-134a`  
```bash
git clone https://github.com/LevonPR/Gen.git
cd Gen
git checkout cursor/micro-evolution-unity-134a
```

**Option B — Zip**  
- Artifact: `MicroEvolution-unity.zip` (from the Cloud Agent)  
- Or: https://github.com/LevonPR/Gen/archive/refs/heads/cursor/micro-evolution-unity-134a.zip  

Unzip and use the inner **`MicroEvolution/`** folder as the Unity project root (the folder that contains `Assets/`, `Packages/`, `ProjectSettings/`).

## 2. Install Unity

1. Install [Unity Hub](https://unity.com/download)
2. Install editor **2022.3.52f1** (LTS) with modules:
   - **Android Build Support** (if you want APK)
   - OpenJDK + Android SDK & NDK (Hub usually offers these with Android module)

Exact version is in `ProjectSettings/ProjectVersion.txt`.

## 3. Open in Unity Hub

1. Unity Hub → **Projects** → **Open**
2. Select the `MicroEvolution` folder
3. Wait for package resolve (`com.unity.cloud.gltfast` will download)
4. Open scene: `Assets/Scenes/Main.unity`
5. Press **Play** → **START RUN**

Optional menu: **MicroEvolution → Sync Art GLBs to StreamingAssets** (copies Meshy models for builds).

## 4. Connect Cursor to the same project

Cursor does **not** replace the Unity Editor. Use both side-by-side:

1. In **Cursor Desktop**: **File → Open Folder** → choose the repo root (`Gen/`) or `MicroEvolution/`
2. Keep **Unity** open on the same `MicroEvolution/` path
3. Edit scripts in Cursor → Unity recompiles automatically (watch the bottom-right spinner)
4. Test in Unity Play Mode after each change

Tips:
- Prefer opening the **git repo root** in Cursor so PR/branch tools work
- Don’t commit `Library/`, `Temp/`, `Logs/` (already gitignored)
- If Unity shows script errors after pull: **Assets → Reimport All** or delete `Library/` and reopen

### Cursor Cloud Agents vs local Unity

| | Cloud Agent (cursor.com) | Cursor Desktop + local Unity |
| --- | --- | --- |
| Edit C# / commit / PR | Yes | Yes |
| Play Mode / Game view | No (no licensed Editor here) | Yes |
| Build APK | Needs Unity secrets + Editor | Yes (`MicroEvolution → Build Android APK`) |
| Meshy / MCP tools | Via API key / MCP | Same if configured |

To let a Cloud Agent run Unity itself you’d need `UNITY_EMAIL` / `UNITY_PASSWORD` (+ optional `UNITY_SERIAL`) as secrets — still separate from “connecting Cursor to Unity” locally.

## 5. What you should see in Play Mode

- Meshy GLB bodies for player / prey / predators / ally (after art warmup; procedural fallback first frames)
- HUD icons for Boost / Chem / Evolve from `Assets/Art/Sprites/UI/`
- Touch stick + cinematic underwater look

## 6. Android APK (local)

In Unity: **MicroEvolution → Build Android APK**  
Output: `Builds/Android/MicroEvolution.apk`

## Smoke check (no Unity)

```bash
python3 MicroEvolution/Scripts/smoke-check.py
```

Validates GLBs, PNGs, and art wiring without opening the Editor.
