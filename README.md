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
scene, no component to add anywhere. It self-creates a hidden `DontDestroyOnLoad` container the
first time a pool actually needs one. This module has no dependencies and can be installed on its
own — see ["Only need the pool?"](#only-need-the-pool) above.

#### `SinglePoolable` vs `MultiPoolable` — which one do I inherit?

Every poolable object derives (indirectly) from `ObjPoolable`, but you never inherit that one
directly — you pick one of its two subclasses depending on the shape of what you're pooling:

- **`SinglePoolable`** — for a type that only ever needs **one pool total**. One class, one pool.
  Example: a `Bullet` class — every bullet in the game is interchangeable, there's exactly one pool
  of them.

    ```csharp
    public class Bullet : SinglePoolable
    {
        public override void Get()     { gameObject.SetActive(true); }
        public override void Release() { gameObject.SetActive(false); }
    }
    ```

- **`MultiPoolable`** — for a type where **the same C# class represents several interchangeable
  variants**, and each variant needs its *own* separate pool (its own set of pre-warmed instances,
  independently destroyable). Example: a `Bubble` class with a `BubbleColor` field — Red bubbles and
  Blue bubbles are the same script, but you don't want a `GetObj<Bubble>()` call to hand you a Red
  instance when you actually needed a Blue one. `MultiPoolable` adds one extra required override,
  `GetSubKeyPool()`, which returns a string identifying *which* variant this particular prefab is —
  that string becomes the pool's key alongside the type name. Using an enum's `.ToString()` here is
  recommended over a hand-typed string literal (typos in a raw string silently create a brand new,
  separate pool instead of erroring).

    ```csharp
    public class Bubble : MultiPoolable
    {
        [SerializeField] private BubbleColor color;

        public override void Get()     { gameObject.SetActive(true); }
        public override void Release() { gameObject.SetActive(false); }
        public override string GetSubKeyPool() => color.ToString();
    }
    ```

Internally, every pool is keyed by `(type name, sub-key)`. `SinglePoolable` always uses an empty
sub-key (there's only ever one pool for that type); `MultiPoolable` uses whatever
`GetSubKeyPool()` returns. This is also why the two `GetObj<T>()`/`DestroyPool<T>()` overloads below
are constrained the way they are — the compiler stops you from asking a single-variant type for a
sub-key it doesn't have, or a multi-variant type for a pool without specifying which variant.

#### Setting up the config

1. **Create → Pool → PoolData** for each prefab (or group of variant prefabs). Its custom inspector
   shows a **Single / Multiple** mode dropdown:
   - **Single** — assign one `Prefab` (must be a `SinglePoolable`) and an `InitAmount`.
   - **Multiple** — fill in the `PoolItems` list, one entry per variant (each a `MultiPoolable`
     prefab) with its own `InitAmount`.
2. **Create → Pool → AllPoolData** and drag every `PoolData` asset you made into its `configs` list.
3. Register it once at startup — this only reads the config into a lookup table, it doesn't
   instantiate anything yet:

    ```csharp
    PoolSystem.Instance.Init(soAllPoolData);
    ```

#### Fetching and releasing

```csharp
var bullet = PoolSystem.Instance.GetObj<Bullet>();                  // SinglePoolable
var bubble = PoolSystem.Instance.GetObj<Bubble>(BubbleColor.Red.ToString());  // MultiPoolable, needs the sub-key

PoolSystem.Instance.ReleaseObj(bullet);
```

Pools are created **lazily** — the first `GetObj<T>()` call for a given type/sub-key is what
actually builds that pool and pre-warms it according to the config's `InitAmount` (`0` means create
instances strictly on demand, with no pre-warming). Requesting a type/sub-key that was never
registered via `Init()` logs a warning and returns `null` rather than throwing.

`ReleaseObj` takes two optional parameters you'll rarely need to touch:
`ReleaseObj(obj, ignoreParentPool: false, worldPosStay: true)` — by default, releasing an object
re-parents it back under the pool's own container transform (`worldPosStay` controls whether its
world position is preserved through that re-parent); pass `ignoreParentPool: true` to leave it
wherever it currently is in the hierarchy instead. Calling `ReleaseObj` twice on the same instance,
or on one that's already idle, is a harmless no-op (logged, not thrown).

You don't have to manage destruction carefully — the pool tracks every instance it created (both
idle and currently in use) directly, not by walking the scene hierarchy. If something else destroys
a pooled instance directly (`Destroy(obj.gameObject)`) instead of going through `ReleaseObj` — while
it's active *or* while it's sitting idle in the pool — the pool notices and drops it from its
bookkeeping instead of handing back a dead reference later.

#### Tearing a pool set down

```csharp
PoolSystem.Instance.DestroyPool<Bullet>();                             // SinglePoolable, or every sub-key of a MultiPoolable type
PoolSystem.Instance.DestroyPool<Bubble>(BubbleColor.Red.ToString());   // MultiPoolable only — one sub-key
PoolSystem.Instance.DestroyAllPools();                                 // everything, every type
```

Each of these destroys every instance under it — both idle and currently active/in-use — not just
the idle ones sitting in the pool. `Init()` never clears or destroys pools that already exist on its
own; call `DestroyPool<T>()` / `DestroyAllPools()` explicitly first if a scene or level transition
should also tear down the previous pool set (e.g. swapping out a level's bullet types for a
different level's).

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
