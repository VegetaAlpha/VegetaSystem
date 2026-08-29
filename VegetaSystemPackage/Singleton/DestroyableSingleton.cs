using UnityEngine;

namespace VegetaSystem
{
    public class DestroyableSingleton<T> : MonoBehaviour
        where T : DestroyableSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        Debug.LogError($"No {typeof(T).Name} Singleton Instance.");
                    }
                }

                return instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return (instance != null);
            }
        }

        protected virtual void Awake()
        {
            CheckInstance();
        }

        protected bool CheckInstance()
        {
            if (instance == null)
            {
                instance = (T)this;
                return true;
            }

            if (instance == this)
                return true;

            Destroy(this.gameObject);
            return false;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
