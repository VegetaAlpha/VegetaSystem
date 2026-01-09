using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Cysharp.Threading.Tasks;

namespace VegetaSystem
{
    [Serializable]
    public class ConfigLoadScene
    {
        public string SceneName;
        public Action OnBeforeLoad;
        public Action OnAfterLoad;
        public Action<float> OnProgress;
        public Action<Action> OnLoadAPI;
        public LoadSceneMode Mode;
        public float DelayCompleted;
        public float DelayBeforeLoad;
        public bool IgnoreDisplayProgress; 

        public ConfigLoadScene(
                string sceneName = "",
                Action onBeforeLoad = null,
                Action onAfterLoad = null,
                Action<float> onProgress = null,
                Action<Action> onLoadAPI = null,
                LoadSceneMode mode = LoadSceneMode.Single,
                float delayCompleted = 0f,
                float delayBeforeLoad = 0f,
                bool ignoreDisplayProgress = false)
        {
            SceneName = sceneName;
            OnBeforeLoad = onBeforeLoad;
            OnAfterLoad = onAfterLoad;
            OnProgress = onProgress;
            OnLoadAPI = onLoadAPI;
            Mode = mode;
            DelayCompleted = delayCompleted;
            DelayBeforeLoad = delayBeforeLoad;
            IgnoreDisplayProgress = ignoreDisplayProgress;
        }

        /* Flow Config
        Excute : OnBeforeLoad (if not null)
        Wait : delayBeforeLoad
        Excute : OnLoadAPI (if not null excute same time with load scene)
        Excute : OnProgress (if not null excute same time with load scene)
        Param : IgnoreDisplayProgress (if false wait fake process done, if true change new scene immediately)
        Wait : DelayCompleted (After loadscene done or both loadscene and load api done , wait delaycomplete)
        Excute : OnAfterLoad (if not null)
        */

    }

    public struct ConfigUnloadScene
    {
        public string SceneName;
        public Action OnBeforeUnload;
        public Action OnAfterUnload;
        public float DelayAfterUnloaded;

        /// <summary>
        /// param: SceneName, OnBeforeUnload, OnAfterUnload, DelayAfterUnloaded
        /// </summary>
        public ConfigUnloadScene(string sceneName = "", Action onBeforeUnload = null, Action onAfterUnLoad = null, float delayAfterUnload = 0)
        {
            this.SceneName = sceneName;
            this.OnBeforeUnload = onBeforeUnload;
            this.OnAfterUnload = onAfterUnLoad;
            this.DelayAfterUnloaded = delayAfterUnload;
        }
    }

    public class LoadSceneSystem : PersistSingleton<LoadSceneSystem>
    {
        public virtual void LoadNewScene(ConfigLoadScene config, bool force = false)
        {
            LoadNewSceneAsync(config, force).Forget();
        }

        protected async virtual UniTask LoadNewSceneAsync(ConfigLoadScene config, bool force)
        {
            if (force == false && IsSceneActive(config.SceneName))
            {
                Debug.LogError("Ignore Load Scene " + config.SceneName);
                return;
            }

            var token = this.GetCancellationTokenOnDestroy();

            config.OnBeforeLoad?.Invoke();

            if (config.DelayBeforeLoad > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(config.DelayBeforeLoad), cancellationToken: token);
            }

            AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(config.SceneName, config.Mode);
            if (sceneAsync == null)
            {
                config.OnAfterLoad?.Invoke();
                Debug.LogError($"Failed to load scene: {config.SceneName} (not found in build settings?)");
                return;
            }

            sceneAsync.allowSceneActivation = false;
            UniTaskCompletionSource apiTcs = null;

            if (config.OnLoadAPI != null)
            {
                apiTcs = new UniTaskCompletionSource();
                config.OnLoadAPI(() => apiTcs.TrySetResult());
            }

            float displayProgress = 0f;
            float fakeSpeed = 0.7f;

            while (sceneAsync.progress < 0.9f || (apiTcs != null && !apiTcs.Task.Status.IsCompleted()))
            {
                float realProgress = Mathf.Clamp01(sceneAsync.progress / 0.9f);

                //Fake
                float target = Mathf.Min(realProgress, 0.9f);
                displayProgress = Mathf.MoveTowards(displayProgress, target, Time.deltaTime * fakeSpeed);

                config.OnProgress?.Invoke(displayProgress);
                await UniTask.Yield(token);
            }

            // After real progress done
            while (displayProgress < 1f && config.IgnoreDisplayProgress == false)
            {
                displayProgress = Mathf.MoveTowards(displayProgress, 1f, Time.deltaTime * fakeSpeed);
                config.OnProgress?.Invoke(displayProgress);
                await UniTask.Yield(token);
            }

            sceneAsync.allowSceneActivation = true;


            if (config.DelayCompleted > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(config.DelayCompleted), cancellationToken: token);
            }
            else
            {
                await UniTask.NextFrame(token);
            }
            config.OnAfterLoad?.Invoke();
        }

        protected virtual void SetSceneAdditive(string sceneName, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive)
            {
                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                if (loadedScene.IsValid() && loadedScene.isLoaded)
                {
                    SceneManager.SetActiveScene(loadedScene);
                }
                else
                {
                    Debug.LogError($"Cannot set active scene: {sceneName} is not loaded or invalid");
                }
            }
        }

        public virtual void UnloadScene(ConfigUnloadScene config)
        {
            UnloadSceneAsync(config).Forget();
        }

        protected virtual async UniTask UnloadSceneAsync(ConfigUnloadScene config)
        {
            var token = this.GetCancellationTokenOnDestroy();

            Scene scene = SceneManager.GetSceneByName(config.SceneName);
            if (IsSceneActive(config.SceneName) == false)
            {
                config.OnAfterUnload?.Invoke();
                return;
            }

            config.OnBeforeUnload?.Invoke();

            AsyncOperation op = SceneManager.UnloadSceneAsync(config.SceneName);

            if (op == null)
            {
                Debug.LogError($"Can not unload scene: {config.SceneName}");
                config.OnAfterUnload?.Invoke();
                return;
            }

            await UniTask.WaitUntil(() => op.isDone);

            if (config.DelayAfterUnloaded > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(config.DelayAfterUnloaded), cancellationToken: token);

            config.OnAfterUnload?.Invoke();
        }

        protected virtual bool IsSceneActive(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene != null && scene.IsValid() && scene.isLoaded;
        }
    }
}