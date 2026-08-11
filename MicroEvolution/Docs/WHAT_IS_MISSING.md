# What’s Missing

Honest gap list vs a shippable cinematic Cell Stage (mockups + Spore-like feel).  
**Current visual fidelity: ~84%** — see [`FIDELITY_SCORE.md`](FIDELITY_SCORE.md).

---

## Blockers to a real APK

| Item | Status | Notes |
| --- | --- | --- |
| Unity Editor license in CI/cloud | Missing | Pipeline scripts exist; APK must be built on a machine with Unity 2022.3 + Android modules |
| Built `MicroEvolution.apk` | Not produced here | Use menu **MicroEvolution → Build Android APK** or `Scripts/build-android-apk.sh` |
| Release keystore / Play signing | Missing | Debug signing only unless you add a release keystore |
| Store listing assets | Missing | Icon, feature graphic, screenshots, privacy policy, content rating |

---

## Art & presentation (largest fidelity gap)

| Item | Status | Why it matters |
| --- | --- | --- |
| Authored 3D cell kits (FBX/glTF) | **Partial** | Meshy GLBs in `Assets/Art/Models/Cells` + `StreamingAssets/Models` via `ArtModelLibrary` (glTFast). First spawn may still use procedural until warmup finishes. |
| Hand-painted / AI biome plates as textures | Partial | Runtime noise-painted plates only — not final art assets on disk |
| Custom UI fonts | Missing | Default Unity fonts; mockups use distinctive display type |
| Custom HUD icons / radar glyphs | **Partial** | Meshy icons for boost/chem/evo/dna/radar under `Art/Sprites/UI`; wired on action buttons when present |
| Animator-authored cilia / flagella | Partial | Bone-chain procedural sway only |
| Photographed / high-end SSS & refraction | Missing | SoftCell fresnel/iridescence, not real subsurface or water IOR |
| URP/HDRP final lighting pass | Missing | Built-in / custom post; no production DoF bokeh / volume stack |
| Music bed (composed) | Missing | Procedural ambient tones only |
| Polished SFX library | Partial | Procedural blips; need eat/hurt/evolve/UI packs |
| Trailer / store video | Missing | Optional; Higgsfield MCP can help once authenticated |

---

## Gameplay & content gaps

| Item | Status | Notes |
| --- | --- | --- |
| Creature Stage / later Spore stages | Out of scope | Cell Stage only by design |
| Environmental hazards | Thin | Biomes change color/threat mix; currents, toxin clouds, darkness need more bite |
| Dynamic per-biome objectives | Thin | Win is fixed: pop 150 + Oscillator + Thermal Vent |
| Meta unlocks depth | Basic | Starting part / ATP bonus style progress — not a full tech tree |
| Balance / difficulty curves | Untested at scale | Needs playtest passes on device |
| Localization | Missing | English strings only |
| Accessibility | Minimal | No colorblind modes, remaps, or text scaling beyond canvas scale |
| Online / multiplayer | Missing | Single-player local only |
| Analytics / crash reporting | Missing | No Firebase/Sentry/etc. |

---

## Engineering / production gaps

| Item | Status | Notes |
| --- | --- | --- |
| Unit / playmode tests | Missing | No automated test suite |
| CI that builds APK | Missing | Scripts ready; needs licensed Unity runner |
| Addressables / asset bundles | Missing | Everything loaded with scene bootstrap |
| Input System package polish | Partial | Custom `GameInput` + touch; not full new Input System UX |
| Performance profiling on mid devices | Not done | Mobile caps exist; need real device FPS pass |
| Higgsfield MCP | Optional | Still needs desktop OAuth; Meshy REST used instead for this art pack |
| `Assets/Art/` authored folder | **In use** | Meshy GLBs + UI sprites committed; sync to StreamingAssets for builds |

---

## Suggested next slices (priority)

1. **Local APK** — build + install on a phone; fix touch/safe-area issues found in device play.
2. **Art pack** — 6–12 cell meshes, 3 biome plates, UI icon set, short music loop + SFX (Higgsfield 2D + image-to-3D or a small art pass).
3. **Wire art into loaders** — replace procedural meshes/textures when assets exist under `Assets/Art/`.
4. **Hazard + objective pass** — make Midwater / Vent feel meaningfully different.
5. **Store readiness** — icon, screenshots, release keystore, listing copy.

---

## What *is* already in the zip

Playable Cell Stage loop, cinematic HUD, evolution shop, 9 parts, 3 biomes, save/meta, touch controls, shaders/VFX stack, Android build scripts, and docs. Enough to open in Unity, play in Editor, and produce an APK locally — not enough to look like the final mockups or ship to Play Store without the gaps above.
