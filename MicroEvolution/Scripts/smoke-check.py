#!/usr/bin/env python3
"""Headless smoke checks for MicroEvolution (no Unity required)."""
from __future__ import annotations

import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors: list[str] = []
warns: list[str] = []
oks: list[str] = []


def ok(msg: str) -> None:
    oks.append(msg)
    print(f"OK  {msg}")


def warn(msg: str) -> None:
    warns.append(msg)
    print(f"WARN {msg}")


def err(msg: str) -> None:
    errors.append(msg)
    print(f"FAIL {msg}")


def main() -> int:
    ver = (ROOT / "ProjectSettings/ProjectVersion.txt").read_text()
    if "2022.3.52f1" in ver:
        ok("ProjectVersion is 2022.3.52f1")
    else:
        err(f"Unexpected ProjectVersion: {ver.strip()}")

    manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
    deps = manifest.get("dependencies", {})
    if "com.unity.cloud.gltfast" in deps:
        ok(f"glTFast package declared ({deps['com.unity.cloud.gltfast']})")
    else:
        err("com.unity.cloud.gltfast missing from Packages/manifest.json")

    scene = ROOT / "Assets/Scenes/Main.unity"
    if scene.exists() and scene.stat().st_size > 500:
        ok(f"Main.unity present ({scene.stat().st_size} bytes)")
    else:
        err("Assets/Scenes/Main.unity missing/empty")

    required_scripts = [
        "Assets/Scripts/Core/GameBootstrap.cs",
        "Assets/Scripts/Core/AutoBootstrap.cs",
        "Assets/Scripts/Visuals/CellAppearance.cs",
        "Assets/Scripts/Visuals/ArtModelLibrary.cs",
        "Assets/Scripts/Visuals/ArtSpriteLibrary.cs",
        "Assets/Scripts/UI/CinematicHud.cs",
        "Assets/Editor/AndroidBuilder.cs",
        "Assets/Editor/ArtStreamingSync.cs",
    ]
    for rel in required_scripts:
        p = ROOT / rel
        if p.exists():
            ok(f"script {rel}")
        else:
            err(f"missing script {rel}")

    glbs = {
        "player_core.glb",
        "prey_rod.glb",
        "predator_spiky.glb",
        "predator_worm.glb",
        "ally_probe.glb",
        "food_pellet.glb",
    }
    cell_dir = ROOT / "Assets/Art/Models/Cells"
    for name in sorted(glbs):
        p = cell_dir / name
        if p.exists() and p.stat().st_size > 100_000:
            magic = p.read_bytes()[:4]
            if magic == b"glTF":
                ok(f"GLB {name} ({p.stat().st_size // 1024} KB, magic ok)")
            else:
                err(f"GLB {name} bad magic {magic!r}")
        else:
            err(f"GLB missing/too small: {name}")

    icons = ["icon_dna.png", "icon_boost.png", "icon_chem.png", "icon_evo.png", "icon_radar.png"]
    for name in icons:
        p = ROOT / "Assets/Art/Sprites/UI" / name
        if p.exists() and p.read_bytes()[:8] == b"\x89PNG\r\n\x1a\n":
            ok(f"PNG {name}")
        else:
            err(f"PNG missing/invalid: {name}")

    art_lib = (ROOT / "Assets/Scripts/Visuals/ArtModelLibrary.cs").read_text()
    for token in ["player_core", "prey_rod", "predator_spiky", "predator_worm", "ally_probe", "GltfImport"]:
        if token in art_lib:
            ok(f"ArtModelLibrary references {token}")
        else:
            err(f"ArtModelLibrary missing {token}")

    cell = (ROOT / "Assets/Scripts/Visuals/CellAppearance.cs").read_text()
    if "ArtModelLibrary.TryAttach" in cell:
        ok("CellAppearance uses ArtModelLibrary.TryAttach")
    else:
        err("CellAppearance does not call ArtModelLibrary.TryAttach")

    boot = (ROOT / "Assets/Scripts/Core/GameBootstrap.cs").read_text()
    if "ArtModelLibrary.WarmupAsync" in boot:
        ok("GameBootstrap warms art models")
    else:
        warn("GameBootstrap does not warmup ArtModelLibrary")

    hud = (ROOT / "Assets/Scripts/UI/CinematicHud.cs").read_text()
    if "ArtSpriteLibrary.GetUi" in hud:
        ok("CinematicHud uses ArtSpriteLibrary icons")
    else:
        warn("CinematicHud does not use ArtSpriteLibrary")

    print()
    print(f"Summary: {len(oks)} ok, {len(warns)} warn, {len(errors)} fail")
    if errors:
        print("Play Mode still blocked: needs Unity Editor + license on this machine.")
        return 1
    print("Static smoke checks passed. Play Mode still needs Unity Editor.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
