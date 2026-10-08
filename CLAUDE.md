# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Danmaku (탄막 게임) — a 2D bullet-hell game built with **Unity 6000.3.23f1**, URP 2D renderer, and the new Input System. Commit messages are usually written in Korean.

## Build / Run

- There is no CLI build script; open the project in the Unity Editor (matching version) and play `Assets/01.Scenes/InGame.unity` (the only scene in Build Settings).
- CI (`.github/workflows/build.yml`) builds **WebGL** via `game-ci/unity-builder` on every push to `main`, and the bot commits the output into `docs/` (served by GitHub Pages) with `[skip ci]`. Do not hand-edit `docs/`; it is overwritten on each build.
- Unity Test Framework is installed but there are no tests yet.

## Architecture

Scripts live in `Assets/02.Scripts/` (asset folders are number-prefixed: `01.Scenes`, `02.Scripts`, `03.Textures`). No asmdefs — everything compiles into `Assembly-CSharp`.

- **`SingletonBehaviour<T>`** — base for scene singletons (`GameManager`, `InputHandler`). Duplicates destroy themselves; override `IsPersistent` to enable `DontDestroyOnLoad`. Subclasses overriding `Awake` must call `base.Awake()`.
- **Fixed-step timing** — `GameManager` locks `Application.targetFrameRate` to `FPS = 60` and exposes `RATE = 1/FPS`. Movement uses `GameManager.RATE` instead of `Time.deltaTime` (frame-based, classic danmaku style), so gameplay speed is tied to frame count.
- **Input flow** — `PlayerInput` (Behavior: *Invoke Unity Events*, actions in `Assets/Settings/InputSystem_Actions.inputactions`: Move / LowSpeed / Bomb) is wired **in the scene** to `InputHandler` methods (`InvokeOnMove`, `ChangeSlowMoveState`, `InvokeOnUseBomb`). On-screen touch controls (`HoldButton`, `TapButton`) call the same `InputHandler` methods via serialized `UnityEvent`s. `InputHandler` re-exposes C# events (`OnMove`, `OnUseBomb`, `OnEscape`) and the `IsSlowMoving` state that gameplay code (e.g. `Player`) subscribes to. Renaming these methods breaks scene bindings silently — update the scene wiring too.
- **Play area** — `Player` clamps position between two scene Transforms (`deadzoneMinTr` / `deadzoneMaxTr`). The game camera renders into `Assets/03.Textures/InGame.renderTexture` (the gameplay view is a render texture, not the screen directly).

## Branches

The sections above describe `main`, which is behind. Active development happens on two branches; check which branch is checked out before trusting names above.

- **`art`** — art/asset work (bullet sprites, character sprites + animation, atlas, shader, test scenes). Fully merged into `code`.
- **`code`** — gameplay code; contains everything in `art` plus the systems below. Several `main` names are renamed/replaced here:
  - `InputHandler` → `InGameInputHandler` (adds `OnSwipe` via `InGameSwipe`; no `OnEscape`). Scene bindings use the same method names.
  - `Player` → `PlayerController`; `PlayerAnimation` drives the Animator `Right` int param (-1/0/1) from `OnMove`.
  - `SingletonBehaviour` moved to `Core/`.
  - **Timing** — `GameManager.FPS/RATE` and `targetFrameRate` are gone. All gameplay runs in `FixedUpdate` (Fixed Timestep = 1/60 in TimeManager) using `Time.fixedDeltaTime`; durations are expressed in **fixed frames** and waited with `yield return new WaitForFixedFrame(n)` (`Core/WaitForFixedFrame.cs`). `GameManager.WaitForFixedUpdate` is a cached yield instruction.

### `code` branch architecture

Danmaku content is **ScriptableObject data**, composed top-down (`Assets/Datas/`, `CreateAssetMenu` entries under Stage/Enemy/Spell/Bullet):

```
StageData → PhaseData[] → EnemyChain[] → EnemyPattern (EnemyData + spawn pos)
  EnemyData → SpellCard[] (HP, time limit) → BurstData[] → BulletPattern[]
    BulletPattern = BulletData (sprite) + InitialBullet (color/pos/vel/torque) + BulletSequenceBase + wait frames
```

- `StageRunner` walks phases/chains as a coroutine and calls `EnemyManager.SpawnEnemy` (`StageRunEditor` adds a play-mode "시작" button). `EnemyManager` pools common enemies (`05.Prefabs/EnemyPrefab`) and instantiates bosses (`BossPrefab`, `EnemyBoss`). `Enemy` runs each `SpellCard`, repeating `BulletManager.BurstChain` until its HP/time limit ends.
- **Bullets** — `Bullet` is a plain C# class (not a GameObject), pooled by `BulletManager` (`ObjectPool`, max 1024). `BulletManager.FixedUpdate` updates bullets and removes ones outside the deadzone `Rect` with in-place compaction, preserving spawn order (render order depends on it).
- **Sequences** — `BulletSequenceBase` (SO) creates an `ISequenceRunner` that moves a bullet each frame: `DefaultSequence` (linear + torque), `AccelateSequence` (curve to target speed, then acceleration), `TargettingSequence` (aims at player on start), `SequenceBridge` (chains sequences by frame counts). Stateless runners share a singleton instance; stateful ones must be created per bullet.
- **Rendering** — `BulletRenderer` draws `BulletManager.ActiveBullets` with `Graphics.DrawMeshInstanced` (no SpriteRenderers), batched per sprite in chunks of 1023, using a runtime copy of `04.Materials/Bullet01.mat` (shader `Custom/URP/Sprite Invert Multiply`). It only reads bullet data.
- **Status/UI** — `StatusManager` holds HP, power (`decimal`), score, high score (`DataSaveLoad` → PlayerPrefs) and exposes `UnityEvent`s wired to `UIManager` in the scene. A hit is applied after 18 frames unless a bomb (`UsePower`/`UseSemiPower`) cancels it.
- **Editor tools** (`02.Scripts/Editor/`) — `BulletDataEditor`, `UIManagerEditor`, `DeadzoneGizmos` (scene-view rects for player/bullet deadzones).
- Extra scenes `BulletTest`, `EnemyTest`, `art-test` are for testing only; `InGame` is still the only scene in Build Settings.
- Not implemented yet: collisions (bullet↔player, shot↔enemy), player shots/bomb effect, boss phase usage (`PhaseData.Boss`), tests.

### `art` branch asset layout

- `03.Textures/BulletSprites/Bullet_*.png` (Big/Default/Plasma/Prism/Small/Thin) packed into `03.Textures/AtlasSet.spriteatlasv2` (`BulletRenderer` builds meshes from sprite vertices/UVs, so atlas/tight packing renders correctly).
- `03.Textures/Status/` (heart/bomb UI icons), `03.Textures/Shaders/SpriteInvertMultiply.shader` (URP sprite shader used by bullet material).
- Player character "Baekyeon": sprites + atlas in `03.Textures/Mobs/Baekyeon/`, clips and Animator Controller in `Assets/Animate/Baekyoun/` (folder spelled *Baekyoun*), Main/Left/Right states switched by int param `Right`.

## Third-party

Suhgung font (OFL 1.1) in `Assets/ThirdParty/Suhgung-ttf`; project is MIT licensed. Add new third-party assets to the README's Third-Party table.
