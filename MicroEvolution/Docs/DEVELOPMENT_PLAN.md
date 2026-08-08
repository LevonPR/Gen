# MicroEvolution — Full Development Plan (to Android APK)

Spore-inspired microorganism evolution game. Goal: a shippable **Android APK**.

## Vision

A top-down cell-stage survival/evolution game: swim, eat, fight, unlock organelles, grow a colony, and push into deeper biomes until the Cell Stage is complete.

## Phase overview

| Phase | Name | Outcome | Status |
| --- | --- | --- | --- |
| **0** | Prototype | Playable sandbox loop in Unity | ✅ |
| **1** | Core loop polish | Feel, feedback, pause, save, menus | ✅ |
| **2** | Content depth | Parts, biomes, species variety | ✅ |
| **3** | Progression | Stages, persistent unlocks, win flow | ✅ |
| **4** | Presentation | Audio, VFX, tutorial, mobile-ready UI | ✅ |
| **5** | Android adaptation | Touch controls, mobile caps, landscape | ✅ |
| **6** | Ship | Android player settings + APK pipeline | ✅ code ready; APK needs local Unity |

---

## Phase 0 — Prototype ✅

Already in repo:
- Player cell, ATP/biomass/evo/population
- Food, prey, allies, predators
- 4 evolution parts + objectives
- Procedural visuals + IMGUI HUD

---

## Phase 1 — Core loop polish

**Goals:** make the first 10 minutes addictive and stable.

- Input abstraction (keyboard + future touch)
- Hit feedback: flash, knockback, floating text
- Camera punch / light shake on damage
- Pause + main menu + game-over / victory screens
- Autosave (PlayerPrefs): unlocks, high population, settings
- Object pooling for food / VFX
- Tuning pass on ATP, damage, spawn rates

**Exit criteria:** start → play → die/respawn → pause → quit → resume unlocks works.

---

## Phase 2 — Content depth

**Goals:** enough variety that runs feel different.

- Organelle set expanded (Flagella, Eyes, Jaws, Toxin, Storage)
- 3 biomes inside one world: Tide Pool → Midwater → Thermal Vent
- Species variants per biome (stats + colors)
- Environmental hazards (current, toxin clouds, darkness without Eyes)
- Dynamic objectives per biome

**Exit criteria:** player can travel biomes and faces different threats/rewards.

---

## Phase 3 — Progression & meta

**Goals:** reason to keep playing beyond one session.

- Run → victory unlocks next difficulty / biome access
- Persistent meta unlocks (starting part, ATP bonus)
- Milestone rewards (population 50/100/150, first predator kill…)
- Simple “Cell Stage complete” ending + New Run

**Exit criteria:** two full runs show persistent progress.

---

## Phase 4 — Presentation & onboarding

**Goals:** readable, juicy, teachable on first launch.

- uGUI Canvas HUD (replace IMGUI for mobile)
- Tutorial coach marks (move, eat, evolve, boost)
- Procedural SFX (eat, hurt, evolve, UI) + ambient bed
- Stronger cell VFX (burst on eat/death, chem glow)
- Settings: mute, reduced particles

**Exit criteria:** new player understands controls without README.

---

## Phase 5 — Android adaptation

**Goals:** playable one-handed / thumbs on phone.

- Virtual joystick (left) + Boost / Chem buttons (right)
- Evolve panel as modal sheet
- Safe-area / notch padding, scalable canvas
- Mobile quality tier (fewer particles, smaller world entity caps)
- Landscape lock, 60 FPS target, sleep timeout disabled while playing
- Touch + keyboard both supported in editor

**Exit criteria:** full run completable with touch only (in editor simulation).

---

## Phase 6 — Android ship / APK

**Goals:** installable APK artifact.

- Player Settings: package `com.gen.microevolution`, min SDK 24, target SDK 34
- IL2CPP ARM64, orientation landscape
- Keystore (debug for CI; release notes for store)
- Editor build script + CLI batchmode instructions
- Produce `Builds/Android/MicroEvolution.apk`

**Exit criteria:** APK installs and launches on an Android device/emulator.

---

## Technical constraints

- Engine: Unity **2022.3 LTS**
- Rendering: Built-in 2D (orthographic)
- No paid assets required; procedural art/audio preferred
- Keep systems data-driven (`GameConfig`, part definitions)

## Visual target (end-product examples)

Match cinematic mobile concept art (Micronus / Evolve Cell Stage style):

- Translucent cells with rim light, organelles, cilia/flagella
- Dark aquatic atmosphere, god rays, marine snow, depth layers
- DNA / Generation / Score header, region + objective card
- Circular radar, virtual joystick, round action cluster, EVO button
- Evolution grid shop + Customize Cell color swatches
- Reproduction / generation victory flow

Implemented via runtime Canvas HUD (`CinematicHud`) + `UnderwaterAtmosphere` + upgraded `CellAppearance`.

## Non-goals (post-APK)

- Full Spore creature/tribal/civilization stages
- Online multiplayer
- Photoreal shaders / heavy 3D soft-body

## Build order (execution)

Implement **1 → 2 → 3 → 4 → 5 → 6** sequentially on branch, committing after each phase.
