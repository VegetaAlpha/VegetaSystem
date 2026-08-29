using System;
using UnityEngine;

namespace VegetaSystem
{
    public abstract class ObjPoolable : MonoBehaviour
    {
        private string keyTypeName;
        private string keySubKey;
        private bool isRelease;

        internal Action<ObjPoolable> OnDestroyedExternally;

        public virtual void Init()
        {
            this.gameObject.SetActive(false);
        }

        public abstract void Get();
        public abstract void Release();



        #region  Internal
        internal void In_Init(string typeName, string subKey)
        {
            this.keyTypeName = typeName;
            this.keySubKey = subKey;
            this.isRelease = true;
            Init();
        }

        internal string In_GetKeyTypeName()
        {
            return keyTypeName;
        }

        internal string In_GetSubKeyPool()
        {
            return keySubKey;
        }

        internal void In_Get()
        {
            isRelease = false;
            Get();
        }

        internal void In_Release()
        {
            isRelease = true;
            Release();
        }

        internal bool In_GetRelease()
        {
            return isRelease;
        }

        #endregion

        /// <summary>
        /// Notifies the owning pool so it can drop this instance from its bookkeeping
        /// when the object is destroyed directly instead of going through PoolSystem.ReleaseObj.
        /// </summary>
        protected virtual void OnDestroy()
        {
            OnDestroyedExternally?.Invoke(this);
        }
    }
}
