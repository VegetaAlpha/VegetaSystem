using UnityEngine;

namespace VegetaSystem.UI
{
    [RequireComponent(typeof(UIManagerCore))]
    public class UIManagerPersist : PersistSingleton<UIManagerPersist>, IUIManagerWrapper
    {
        private UIManagerCore _impl;

        internal UIManagerCore Impl
        {
            get
            {
                if (_impl == null)
                {
                    _impl = GetComponent<UIManagerCore>();

                    if (_impl == null)
                    {
                        Debug.LogError(
                            $"{nameof(UIManagerCore)} is missing on {name}", this);
                    }
                }
                return _impl;
            }
        }


#region  SCREEN
        public T GetScreen<T>() where T : BaseScreen => Impl.GetScreen<T>();
        
        public T ShowScreen<T>(object data = null, bool forceShowData = false)
            where T : BaseScreen
            => Impl.ShowScreen<T>(data, forceShowData);

        public void HideScreen<T>() where T : BaseScreen
            => Impl.HideScreen<T>();

        public void HideAllScreens()
            => Impl.HideAllScreens();

        public bool IsScreenActive<T>() where T : BaseScreen
            => Impl.IsScreenActive<T>();

        public bool HasScreenActiveExcept<T>() where T : BaseScreen
            => Impl.HasScreenActiveExcept<T>();
#endregion
#region  POPUP
        public T GetPopup<T>() where T : BasePopup => Impl.GetPopup<T>();

        public T ShowPopup<T>(object data = null, bool forceShowData = false)
            where T : BasePopup
            => Impl.ShowPopup<T>(data, forceShowData);

        public void HidePopup<T>() where T : BasePopup
            => Impl.HidePopup<T>();

        public void HideAllPopups()
            => Impl.HideAllPopups();

        public bool IsPopupActive<T>() where T : BasePopup
            => Impl.IsPopupActive<T>();
        
        public bool HasPopupActiveExcept<T>() where T : BasePopup
            => Impl.HasPopupActiveExcept<T>();
#endregion
#region  NOTIFY
        public T GetNotify<T>() where T : BaseNotify => Impl.GetNotify<T>();

        public T ShowNotify<T>(object data = null, bool forceShowData = false)
            where T : BaseNotify
            => Impl.ShowNotify<T>(data, forceShowData);

        public void HideNotify<T>() where T : BaseNotify
            => Impl.HideNotify<T>();

        public void HideAllNotifies()
            => Impl.HideAllNotifies();
        
        public bool IsNotifyActive<T>() where T : BaseNotify
            => Impl.IsNotifyActive<T>();

        public bool HasNotifyActiveExcept<T>() where T : BaseNotify
            => Impl.HasNotifyActiveExcept<T>();    
#endregion
#region  OVERLAP
        public T GetOverlap<T>() where T : BaseOverlap => Impl.GetOverlap<T>();

        public T ShowOverlap<T>(object data = null, bool forceShowData = false)
            where T : BaseOverlap
            => Impl.ShowOverlap<T>(data, forceShowData);

        public void HideOverlap<T>() where T : BaseOverlap
            => Impl.HideOverlap<T>();

        public void HideAllOverlaps()
            => Impl.HideAllOverlaps();


        public bool IsOverlapActive<T>() where T : BaseOverlap
            => Impl.IsOverlapActive<T>();

        public bool HasOverlapActiveExcept<T>() where T : BaseOverlap
            => Impl.HasOverlapActiveExcept<T>(); 
#endregion           
    }
}