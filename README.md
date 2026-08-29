# VegetaSystem

[![GitHub Release](https://img.shields.io/github/v/release/VegetaAlpha/VegetaSystem?label=Latest%20Release)](https://github.com/VegetaAlpha/VegetaSystem/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/VegetaAlpha/VegetaSystem/total?color=brightgreen)](https://github.com/VegetaAlpha/VegetaSystem/releases)

A lightweight, reusable Unity framework providing object pooling, a layered UI manager, scene
loading helpers, and generic singleton bases — meant to be dropped into different Unity game
projects.

## Installation

This package requires [UniTask](https://github.com/Cysharp/UniTask). Unity's Package Manager does
not support git-URL dependencies being auto-resolved from *inside* another package's own
`package.json` (only plain project manifests can reference a git URL directly), so it has to be
added as its own step:

1. In Unity's Package Manager, choose **Add package from git URL** and add UniTask first:

    ```
    https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
    ```

2. Then add VegetaSystem the same way:

    ```
    https://github.com/VegetaAlpha/VegetaSystem.git?path=VegetaSystemPackage
    ```

### Only need the pool?

Object pooling has no dependency on UniTask or on any other module in this repo — it can be
installed on its own, without step 1 above and without pulling in the UI manager or scene loading
helpers:

```
https://github.com/VegetaAlpha/VegetaSystem.git?path=VegetaSystemPackage/Pool
```

Don't install this at the same time as the full `VegetaSystemPackage` path above — both would claim
the same `Pool/` folder and conflict. Pick one or the other.

## Requirements

- Unity 6000.2 or newer for the full package (`Singleton/` uses `FindFirstObjectByType`,
  introduced in Unity 2023.1)
- Unity 2022.3 or newer for the standalone Pool package — it doesn't touch that API

## Modules

### Singletons

Two generic MonoBehaviour singleton base classes:

- `PersistSingleton<T>` — calls `DontDestroyOnLoad` on the first instance; survives scene loads.
- `DestroyableSingleton<T>` — scene-scoped, no persistence.

Both expose a static `Instance` (lazily found via `FindFirstObjectByType`) and `HasInstance`.

### Object Pooling

`PoolSystem` is a plain C# singleton (`PoolSystem.Instance`) — there is nothing to drag into a
scene. This module has no dependencies and can be installed on its own — see
["Only need the pool?"](#only-need-the-pool) above.

1. Create pool config assets: **Create → Pool → PoolData** for each prefab (or group of variant
   prefabs), then **Create → Pool → AllPoolData** to collect them into one list.
2. For an object with a single variant (e.g. a bullet), inherit `SinglePoolable` and override
   `Get()` / `Release()`.
3. For an object with multiple variants sharing one class (e.g. colored bubbles), inherit
   `MultiPoolable` and additionally override `GetSubKeyPool()`, returning a string that identifies
   the variant (an enum's `.ToString()` is recommended over a raw string literal).
4. Register the config once at startup:

    ```csharp
    PoolSystem.Instance.Init(soAllPoolData);
    ```

5. Fetch and release instances:

    ```csharp
    var bullet = PoolSystem.Instance.GetObj<Bullet>();
    var bubble = PoolSystem.Instance.GetObj<Bubble>(BubbleColor.Red.ToString());

    PoolSystem.Instance.ReleaseObj(bullet);
    ```

Pools are created **lazily** — the first `GetObj<T>()` call for a given type/sub-key builds that
pool and pre-warms it according to the config's `InitAmount` (set to `0` to create instances
strictly on demand, with no pre-warming).

Tearing a pool set down:

```csharp
PoolSystem.Instance.DestroyPool<Bullet>();                             // every sub-key of this type
PoolSystem.Instance.DestroyPool<Bubble>(BubbleColor.Red.ToString());   // one sub-key only (multi-variant types)
PoolSystem.Instance.DestroyAllPools();                                 // everything
```

`Init()` only registers config for later lazy creation — it never clears or destroys pools that
already exist. Call `DestroyPool<T>()` / `DestroyAllPools()` explicitly first if a scene or level
transition should also tear down the previous pool set.

### UI Manager

Four UI layers, each with its own base class deriving from `BaseUIElement`: `BaseScreen`,
`BasePopup`, `BaseNotify`, `BaseOverlap`.

1. Subclass the matching base class for each UI element you build.
2. Create a UI manager subclassing either `UIManagerPersist` (survives scene loads — typically your
   game's main UI root) or `UIManagerTransient` (scene-local, no persistence).
3. Prefabs are auto-resolved by convention from `Resources/Prefabs/UI/<Type>/<ClassName>.prefab` if
   an instance isn't already present as a child of the manager's root Transform for that layer.

```csharp
UIManager.Instance.GetScreen<MainMenuScreen>();     // fetch (auto-instantiates from Resources if missing)
UIManager.Instance.ShowScreen<MainMenuScreen>();    // show (auto-instantiates if missing)
UIManager.Instance.HideScreen<MainMenuScreen>();
UIManager.Instance.HideAllScreens();
UIManager.Instance.IsScreenActive<MainMenuScreen>();
```

The same pattern applies to Popup / Notify / Overlap — just swap the method suffix. Hiding a UI
element does not destroy or deactivate its GameObject; it sets the element's `CanvasGroup.alpha` to
`0` and disables raycasts, so its state is preserved across hide/show cycles.

### Scene Loading

`LoadSceneSystem` (a static, UniTask-based class) wraps `SceneManager`'s async load/unload with
lifecycle hooks and an optional fake progress bar:

```csharp
var config = new ConfigLoadScene(
    sceneName: "Gameplay",
    onBeforeLoad: () => loadingScreen.Show(),
    onProgress: value => loadingScreen.SetProgress(value),
    onAfterLoad: () => loadingScreen.Hide()
);

LoadSceneSystem.LoadNewScene(config);
```

Flow: `OnBeforeLoad` → optional `DelayBeforeLoad` → scene load begins, `OnProgress` reports progress
(combined with an optional parallel `OnLoadAPI` call, if provided) → optional `DelayCompleted` →
`OnAfterLoad`. Pass `force: true` to `LoadNewScene` to reload a scene that's already active (the
default behavior refuses and logs an error instead).

> **Note:** scene loading uses UniTask under the hood. If you're not familiar with UniTask's
> execution model, be careful when composing your own async calls around it — some of its APIs
> resume continuations off the main thread, which can cause issues if used without understanding
> Unity's threading constraints.

## Samples

A working demo of the pooling system and UI manager together is available via **Package Manager →
VegetaSystem → Samples → Import** (only available on the full `VegetaSystemPackage` install, not
the standalone Pool-only one).
