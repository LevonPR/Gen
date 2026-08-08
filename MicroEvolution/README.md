# MicroEvolution

A Spore-inspired **Cell Stage** prototype built for **Unity 2022.3 LTS**.

Swim as a microorganism, eat biomass, manage ATP, grow your population, and spend evolution points on new parts.

## Open & play

1. Install [Unity Hub](https://unity.com/download) and **Unity 2022.3 LTS** (other 2022.3.x versions are fine).
2. In Unity Hub: **Open →** select this `MicroEvolution` folder.
3. Open `Assets/Scenes/Main.unity` (or any scene).
4. Press **Play**.

The game auto-boots via `AutoBootstrap` — you do not need to place objects in the scene.

## Controls

| Input | Action |
| --- | --- |
| **WASD** / Arrows | Swim |
| **Right Mouse** (hold) | Swim toward cursor |
| **Q** | Speed boost (costs ATP) |
| **E** | Chemosynthesis (restore ATP + heal) |
| **1** | Evolve Oscillator (12 evo) — objective part |
| **2** | Evolve Spikes (10 evo) |
| **3** | Evolve Membrane (10 evo) |
| **4** | Evolve Chemosynthesis (14 evo) |
| **R** | Respawn after death |

## Objectives

1. Reach **population 150**
2. Evolve the **Oscillator**

Complete both to finish the Cell Stage milestone.

## Gameplay systems

- **ATP** — energy for movement/boost; regenerates over time
- **Biomass** — gained from food and prey; feeds population growth
- **Evolution points** — spent on parts in the HUD or with keys 1–4
- **Population** — grows from allies, feeding, and evolutions; shrinks under predator pressure
- **Ecology** — food pellets, prey, colony allies, and spiky predators with simple AI
- **Minimap / HUD** — ATP, evo points, objectives, health, abilities

## Project layout

```
Assets/Scripts/
  Core/         Game state, bootstrap, motor, living cells
  Player/       Player controller
  AI/           Prey / predator / ally behavior
  World/        Spawning, food, camera, ambience
  Evolution/    Part unlock shop
  UI/           Runtime HUD
  Visuals/      Procedural cell sprites
```

All art is **procedural** (no external sprite pack required).

## Notes

- Target editor: Unity **2022.3.52f1** (any 2022.3 LTS should import cleanly).
- 2D physics, orthographic camera, runtime world generation.
- This is an MVP slice of Spore’s cell stage — not the full multi-stage Spore pipeline.
