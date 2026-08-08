# MicroEvolution

Spore-inspired **Cell Stage** game for **Unity 2022.3 LTS**, built to ship as an **Android APK**.

## Visual target

Designed to match cinematic mobile Cell Stage concepts: bioluminescent microbes, DNA/evolution HUD, joystick + ability cluster, evolution shop, and cell color customize. See `Docs/DEVELOPMENT_PLAN.md` → Visual target.

## Quick start (Editor)

1. Install Unity Hub + **Unity 2022.3.52f1** (Android Build Support + OpenJDK + SDK/NDK).
2. Open this `MicroEvolution` folder in Unity Hub.
3. Open `Assets/Scenes/Main.unity` → **Play**.
4. Main menu → **START RUN**.

## Controls

| Desktop | Android / touch | Action |
| --- | --- | --- |
| WASD / Arrows | Left virtual stick | Swim |
| Right Mouse | Stick direction | Steer |
| Q | Q button | Speed boost (ATP) |
| E | E button | Chemosynthesis |
| Tab | EVO | Evolution shop |
| 1–9 | Shop buttons | Buy parts |
| P / Esc | II | Pause |
| R | E (when dead) | Respawn |

## Objectives (win the Cell Stage)

1. Reach population **150**
2. Evolve **Oscillator**
3. Reach the **Thermal Vent** biome (swim outward)

## What’s missing

See [`Docs/WHAT_IS_MISSING.md`](Docs/WHAT_IS_MISSING.md) — APK/store blockers, art gaps, gameplay/content holes, and suggested next slices. Visual score: [`Docs/FIDELITY_SCORE.md`](Docs/FIDELITY_SCORE.md).

## Features by phase

See [`Docs/DEVELOPMENT_PLAN.md`](Docs/DEVELOPMENT_PLAN.md).

- Main menu, pause, victory / game-over
- Save meta progress (best population, biome access, settings)
- 9 evolution parts, 3 biomes, species variants
- Procedural audio + VFX, tutorial coach marks
- Touch controls + mobile entity caps
- Android build menu + CLI script

## Build Android APK

### Option A — Unity menu

1. `MicroEvolution → Configure Android Player Settings`
2. `MicroEvolution → Build Android APK`
3. Output: `Builds/Android/MicroEvolution.apk`

### Option B — CLI

```bash
# Install Unity 2022.3.52f1 with Android modules first
UNITY_EDITOR=/path/to/Editor/Unity ./Scripts/build-android-apk.sh
```

### Player settings (auto-applied by build script)

- Package: `com.gen.microevolution`
- Min SDK 24 / Target SDK 34
- ARM64 + IL2CPP
- Landscape

Install on device:

```bash
adb install -r Builds/Android/MicroEvolution.apk
```

## Project layout

```
Assets/Scripts/
  Core/ Input/ Player/ AI/ World/ Evolution/
  UI/ Audio/ Visuals/ Mobile/
Assets/Editor/AndroidBuilder.cs
Scripts/build-android-apk.sh
Docs/DEVELOPMENT_PLAN.md
Docs/WHAT_IS_MISSING.md
Docs/FIDELITY_SCORE.md
```
