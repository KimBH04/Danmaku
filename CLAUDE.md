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

## Third-party

Suhgung font (OFL 1.1) in `Assets/ThirdParty/Suhgung-ttf`; project is MIT licensed. Add new third-party assets to the README's Third-Party table.
