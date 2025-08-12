<p align="center">
  <img src="Screenshots\Rhythm Walker.png" alt="Rhythm Runner Banner" width="40%"/>
</p>

# 🎵 Rhythm Walker

**A Cinematic Rhythm Experience for Music Lovers**  
*Built with Unity (HDRP) — Windows*

<p align="center">
  <img src="https://img.shields.io/badge/Engine-Unity%202022+-black?style=for-the-badge&logo=unity" />
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=c-sharp" />
  <img src="https://img.shields.io/badge/Render%20Pipeline-HDRP-blueviolet?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows" />
</p>

---

## 🌟 Overview

**Rhythm Runner** is a precision-based rhythm game where players use the mouse cursor to trace lyric words as they appear on screen (maximum 4 words visible at once). Correct and timely traces build streaks and multipliers; missed or out-of-order words reduce score. The game pairs tight gameplay with cinematic visuals — HDR lighting, dynamic cameras, and polished animations — to deliver an immersive, music-first experience.

---

## 🎯 Core Gameplay

* 🎼 **Sequential Word Tracing** — Trace lyrics in the exact order they appear.
* 🔁 **Streak & Multiplier System** — Chain accurate traces for higher multipliers.
* 🌌 **HDRP Visuals** — High-fidelity lighting, skyboxes and post-processing.
* 🎥 **Cinematic Direction** — Camera sequencing, character animations, and lighting are driven by data for director-style control.
* 🎶 **Multilingual Support** — Currently ships with 4 songs (2 English, 2 Hindi); any language is supported via lyric files.

---

## 📷 Media

<img src="Screenshots\Screenshot 2025-08-12 232439.png" style="margin-bottom:20px;" />

<img src="Screenshots\Screenshot 2025-08-12 232715.png" />

---

<details>
<summary>💻 Developer Deep-Dive (Click to expand)</summary>

### 🛠 Technical Highlights

* **Engine:** Unity (HDRP)
* **Platform:** Windows
* **Key scripts:** `SequenceController.cs`, lyric & song loaders, tracing detection
* **Design goal:** Scalability & replicability — level creation is data-driven, non-programmers can author content.

---

### 🔁 Content Pipeline (Add songs / levels without code)

Rhythm Runner separates *content* from *code* so creators can design levels purely by editing JSON and adding assets to `Assets/Resources`.

#### 1) Lyrics files — `Assets/Lyrics`

Lyrics are time-coded JSON arrays. Each object defines the window when a word appears.

```json
[
  { "start": "00:00:17.89", "end": "00:00:18.36", "text": "Arms" },
  { "start": "00:00:18.36", "end": "00:00:18.83", "text": "around" },
  { "start": "00:00:18.83", "end": "00:00:19.30", "text": "you" }
]
```

* **Creation:** Manually authoring JSON or converting `.lrc` files works.
* **Behavior:** Words are spawned at their `start` time and removed at `end` (or when traced). Order is enforced — untraced earlier words count as missed.

---

#### 2) Song configuration — `Assets/Resources/song_config.json`

This is the director’s control file. It maps cinematic assets and timing to a song and drives `SequenceController.cs`.

Example entry:

```json
{
  "title": "bbno$, y2k - lalala",
  "playerModelName": "MainCharacter",
  "skyCubemapNames": ["AmbienceExposure4k", "UnearthlyRed4k", "PlanetaryEarth4k", "sandsloot_4k"],
  "cameraPositions": [ { "x": 0, "y": 0, "z": -2 }, { "x": 0, "y": 0, "z": -4 }, { "x": 100, "y": 0, "z": 0 }, { "x": 0, "y": 0, "z": -4 } ],
  "rotateSpeed": [0, 0, 0, 0],
  "fadeDuration": [2, 2, 2, 2],
  "followDistances": [1.5, 1.5, 3.0, 1.5],
  "followHeights": [-1, -1, 1, -1],
  "transitionDurations": [7, 26, 71, 0],
  "animationClipNames": ["idle", "walk", "fall", "run"],
  "sunWorldPositions": [ { "x": 0, "y": 1, "z": -9 }, { "x": 0, "y": 1, "z": -9 }, { "x": 0, "y": 1, "z": -9 }, { "x": 0, "y": 1, "z": -9 } ],
  "attachSunToCamera": [false, false, false, false]
}
```

* **What creators can change:** scenes, camera paths, transition times, character model, animations, skyboxes, sun & lighting, and more.
* **Where assets live:** Place models, animation clips, and skybox cubemaps inside `Assets/Resources` and reference them by name in the JSON.

---

### ⚙️ Extending & Customizing

* Add songs by creating a new lyrics JSON and an entry in `song_config.json`.
* Small behavior changes and new features belong in `SequenceController.cs` and are intentionally modular.
* Want runtime editors for non‑coders? The data-driven approach makes building an in‑editor tool straightforward.

</details>

---

## 🔧 Developer Setup

1. Open project in Unity (use the same Unity version used to author the project).
2. `Assets`, `Packages`, and `ProjectSettings` are the only folders tracked in this repo (large assets are stored externally — see below).
3. To add a song:

   * Add your lyrics JSON to `Assets/Lyrics/`.
   * Add cinematic assets (models/skyboxes/animations) to `Assets/Resources/`.
   * Add a new entry in `Assets/Resources/song_config.json` and play.

---

## 📁 Large assets & distribution

Large binary assets (HDRI `.exr`, large `.fbx`, etc.) are intentionally **not** included in this repository. They are tracked separately to keep the repo lean.

* A file `LargeAssetsList.txt` (committed) contains file paths of ignored large files (if present).
* To upload large files, consider a shared cloud (Drive/Dropbox) and add download links to the README or `LargeAssetsList.txt`.

PowerShell snippet to regenerate the list locally:

```powershell
Get-ChildItem -Path "Assets" -Recurse |
  Where-Object { $_.Length -gt 50MB } |
  ForEach-Object { $_.FullName } |
  Set-Content "LargeAssetsList.txt"
```

---

## Contributing

* Found a bug or a perf issue? Open an issue with repro steps.
* Want to add a feature? Send a PR and describe the design in the PR description.
* If you add new resources, list them in `Assets/Resources` and reference them in `song_config.json`.

---

## License

This project is made for learning purposes. The assets are thanks to mixamo.org and other such sites.

---

## Contact

**Developer:** Aartem Singh
**Email:** \[[aartemsingh.uk@gmail.com](mailto:aartemsingh.uk@gmail.com)]

<p align="center">
  <i>Created in 2 weeks — focused on creativity, scalability, and player experience.</i>
</p>
