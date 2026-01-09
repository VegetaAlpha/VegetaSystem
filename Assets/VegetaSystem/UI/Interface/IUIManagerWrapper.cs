namespace VegetaSystem.UI
{
    public interface IUIManagerWrapper
    {
        // ===== Screen =====
        T GetScreen<T>() where T : BaseScreen;
        T ShowScreen<T>(object data = null, bool forceShowData = false) where T : BaseScreen;
        void HideScreen<T>() where T : BaseScreen;
        void HideAllScreens();
        bool IsScreenActive<T>() where T : BaseScreen;
        bool HasScreenActiveExcept<T>() where T : BaseScreen;


        // ===== Popup =====
        T GetPopup<T>() where T : BasePopup;
        T ShowPopup<T>(object data = null, bool forceShowData = false) where T : BasePopup;
        void HidePopup<T>() where T : BasePopup;
        void HideAllPopups();
        bool IsPopupActive<T>() where T : BasePopup;
        bool HasPopupActiveExcept<T>() where T : BasePopup;


        // ===== Notify =====
        T GetNotify<T>() where T : BaseNotify;
        T ShowNotify<T>(object data = null, bool forceShowData = false) where T : BaseNotify;
        void HideNotify<T>() where T : BaseNotify;
        void HideAllNotifies();
        bool IsNotifyActive<T>() where T : BaseNotify;
        bool HasNotifyActiveExcept<T>() where T : BaseNotify;


        // ===== Overlap =====
        T GetOverlap<T>() where T : BaseOverlap;
        T ShowOverlap<T>(object data = null, bool forceShowData = false) where T : BaseOverlap;
        void HideOverlap<T>() where T : BaseOverlap;
        void HideAllOverlaps();
        bool IsOverlapActive<T>() where T : BaseOverlap;
        bool HasOverlapActiveExcept<T>() where T : BaseOverlap;

    }
}
