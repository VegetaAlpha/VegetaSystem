using UnityEngine;
namespace VegetaSystem
{

    public class PersistSingleton<T> : MonoBehaviour where T : PersistSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

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
                DontDestroyOnLoad(this);
                return true;
            }
            else if (instance == this)
            {
                return true;
            }

            Destroy(this.gameObject);
            return false;
        }
    }
}
