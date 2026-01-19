using System.Collections.Generic;
using UnityEngine;

namespace VegetaSystem.UI
{
    internal class UIManagerCore : MonoBehaviour, IUIManager
    {
        [Header("Roots")]
        public Transform  cScreen, cPopup, cNotify, cOverlap;
        private Dictionary<string, BaseScreen> screens = new Dictionary<string, BaseScreen>();
        private Dictionary<string, BasePopup> popups = new Dictionary<string, BasePopup>();
        private Dictionary<string, BaseNotify> notifies = new Dictionary<string, BaseNotify>();
        private Dictionary<string, BaseOverlap> overlaps = new Dictionary<string, BaseOverlap>();

        public Dictionary<string, BaseScreen> Screens => screens;
        public Dictionary<string, BasePopup> Popups => popups;
        public Dictionary<string, BaseNotify> Notifies => notifies;
        public Dictionary<string, BaseOverlap> Overlaps => overlaps;

        private BaseScreen curScreen;
        private BasePopup curPopup;
        private BaseNotify curNotify;
        private BaseOverlap curOverlap;

        public BaseScreen CurScreen => curScreen;
        public BasePopup CurPopup => curPopup;
        public BaseNotify CurNotify => curNotify;
        public BaseOverlap CurOverlap => curOverlap;

        private const string SCREEN_RESOURCES_PATH = "Prefabs/UI/Screen/";
        private const string POPUP_RESOURCES_PATH = "Prefabs/UI/Popup/";
        private const string NOTIFY_RESOURCES_PATH = "Prefabs/UI/Notify/";
        private const string OVERLAP_RESOURCES_PATH = "Prefabs/UI/Overlap/";

        #region Screen

        private BaseScreen GetNewScreen<T>() where T : BaseScreen
        {
            string nameScreen = typeof(T).Name;
            BaseScreen screenScr;
            if (cScreen != null)
            {
                T existingScreen = cScreen.GetComponentInChildren<T>(true);
                if (existingScreen != null)
                {
                    screenScr = existingScreen as BaseScreen;
#if UNITY_EDITOR
                    screenScr.gameObject.name = "SCREEN_" + nameScreen;
#endif
                    screenScr.Init();

                    return screenScr;
                }
            }


            GameObject pfScreen = GetUIPrefab(UIType.Screen, nameScreen);
            if (pfScreen == null || !pfScreen.GetComponent<BaseScreen>())
            {
                throw new MissingReferenceException("Can not found" + nameScreen + "screen. !!!");
            }
            GameObject ob = Instantiate(pfScreen) as GameObject;
            ob.transform.SetParent(this.cScreen.transform);
            SetUpAnchorOffsetFullScreen(ob);
#if UNITY_EDITOR
            ob.name = "SCREEN_" + nameScreen;
#endif
            screenScr = ob.GetComponent<BaseScreen>();
            screenScr.Init();
            return screenScr;
        }

        public void HideAllScreens()
        {
            BaseScreen screenScr = null;

            foreach (KeyValuePair<string, BaseScreen> item in screens)
            {
                screenScr = item.Value;
                if (screenScr == null || screenScr.IsHide)
                    continue;
                screenScr.Hide();

                if (screens.Count <= 0)
                    break;
            }
        }

        public void HideScreen<T>() where T : BaseScreen
        {
            string screenName = typeof(T).Name;
            if (screens.TryGetValue(screenName, out BaseScreen screen))
            {
                if (screen.IsActive)
                {
                    screen.Hide();
                }
            }
        }

        public T GetScreen<T>() where T : BaseScreen
        {
            string screenName = typeof(T).Name;

            if (screens.TryGetValue(screenName, out BaseScreen cached))
                return cached as T;

            BaseScreen newScreen = GetNewScreen<T>();
            if (newScreen == null)
            {
                return null;
            }
            T typedScreen = newScreen as T;
            screens[screenName] = typedScreen;

            return typedScreen;
        }

        /// <summary>
        /// forceShowData: 
        /// forces the Show(data) method to be called even if the screen is already active/visible.
        /// Useful for refreshing or updating the UI with new data without hiding and re-showing the screen.
        /// </summary>
        /// <returns></returns>
        public T ShowScreen<T>(object data = null, bool forceShowData = false) where T : BaseScreen
        {
            string screenName = typeof(T).Name;
            BaseScreen result = null;

            if (curScreen != null)
            {
                var curName = curScreen.GetType().Name;
                if (curName.Equals(screenName))
                {
                    result = curScreen;
                }
            }

            if (result == null)
            {
                if (!screens.ContainsKey(screenName))
                {
                    BaseScreen screenScr = GetNewScreen<T>();
                    if (screenScr != null)
                    {
                        screens.Add(screenName, screenScr);
                    }
                }

                if (screens.ContainsKey(screenName))
                {
                    result = screens[screenName];
                }
            }

            bool isShow = false;
            if (result != null)
            {
                if (forceShowData)
                {
                    isShow = true;
                }
                else
                {
                    if (result.IsHide)
                    {
                        isShow = true;
                    }
                }
            }

            if (isShow)
            {
                curScreen = result;
                result.transform.SetAsLastSibling();
                result.Show(data);
            }
            return result as T;
        }

        public bool IsScreenActive<T>() where T : BaseScreen
        {
            string screenName = typeof(T).Name;
            if (screens.TryGetValue(screenName, out BaseScreen screen))
            {
                return screen.IsActive;
            }
            return false;
        }

        #endregion

        #region Popup

        private BasePopup GetNewPopup<T>() where T : BasePopup
        {
            string namePopup = typeof(T).Name;
            BasePopup popupScr;
            if (cPopup != null)
            {
                T existingPopup = cPopup.GetComponentInChildren<T>(true);
                if (existingPopup != null)
                {
                    popupScr = existingPopup as BasePopup;
#if UNITY_EDITOR
                    popupScr.gameObject.name = "POPUP_" + namePopup;
#endif
                    popupScr.Init();

                    return popupScr;
                }
            }

            GameObject pfPopup = GetUIPrefab(UIType.Popup, namePopup);
            if (pfPopup == null || !pfPopup.GetComponent<BasePopup>())
            {
                throw new MissingReferenceException("Can not found" + namePopup + "popup. !!!");
            }
            GameObject ob = Instantiate(pfPopup) as GameObject;
            ob.transform.SetParent(this.cPopup.transform);
            SetUpAnchorOffsetFullScreen(ob);
#if UNITY_EDITOR
            ob.name = "POPUP_" + namePopup;
#endif
            popupScr = ob.GetComponent<BasePopup>();
            popupScr.Init();
            return popupScr;
        }

        public void HideAllPopups()
        {
            BasePopup popupScr = null;

            foreach (KeyValuePair<string, BasePopup> item in popups)
            {
                popupScr = item.Value;
                if (popupScr == null || popupScr.IsHide)
                    continue;
                popupScr.Hide();

                if (popups.Count <= 0)
                    break;
            }
        }

        public void HidePopup<T>() where T : BasePopup
        {
            string popupName = typeof(T).Name;
            if (popups.TryGetValue(popupName, out BasePopup popup))
            {
                if (popup.IsActive)
                {
                    popup.Hide();
                }
            }
        }

        public T GetPopup<T>() where T : BasePopup
        {
            string popupName = typeof(T).Name;

            if (popups.TryGetValue(popupName, out BasePopup cached))
                return cached as T;

            BasePopup newPopup = GetNewPopup<T>();
            if (newPopup == null)
            {
                return null;
            }
            T typedPopup = newPopup as T;
            popups[popupName] = typedPopup;

            return typedPopup;
        }

        public T ShowPopup<T>(object data = null, bool forceShowData = false) where T : BasePopup
        {
            string popupName = typeof(T).Name;
            BasePopup result = null;

            if (curPopup != null)
            {
                var curName = curPopup.GetType().Name;
                if (curName.Equals(popupName))
                {
                    result = curPopup;
                }
            }

            if (result == null)
            {
                if (!popups.ContainsKey(popupName))
                {
                    BasePopup popupScr = GetNewPopup<T>();
                    if (popupScr != null)
                    {
                        popups.Add(popupName, popupScr);
                    }
                }

                if (popups.ContainsKey(popupName))
                {
                    result = popups[popupName];
                }
            }

            bool isShow = false;
            if (result != null)
            {
                if (forceShowData)
                {
                    isShow = true;
                }
                else
                {
                    if (result.IsHide)
                    {
                        isShow = true;
                    }
                }
            }

            if (isShow)
            {
                curPopup = result;
                result.transform.SetAsLastSibling();
                result.Show(data);
            }
            return result as T;
        }

        public bool IsPopupActive<T>() where T : BasePopup
        {
            string popupName = typeof(T).Name;
            if (popups.TryGetValue(popupName, out BasePopup popup))
            {
                return popup.IsActive;
            }
            return false;
        }

        #endregion

        #region Notify

        private BaseNotify GetNewNotify<T>() where T : BaseNotify
        {
            string nameNotify = typeof(T).Name;
            BaseNotify notifyScr;
            if (cNotify != null)
            {
                T existingNotify = cNotify.GetComponentInChildren<T>(true);
                if (existingNotify != null)
                {
                    notifyScr = existingNotify as BaseNotify;
#if UNITY_EDITOR
                    notifyScr.gameObject.name = "NOTIFY_" + nameNotify;
#endif
                    notifyScr.Init();

                    return notifyScr;
                }
            }

            GameObject pfNotify = GetUIPrefab(UIType.Notify, nameNotify);
            if (pfNotify == null || !pfNotify.GetComponent<BaseNotify>())
            {
                throw new MissingReferenceException("Can not found" + nameNotify + "notify. !!!");
            }
            GameObject ob = Instantiate(pfNotify) as GameObject;
            ob.transform.SetParent(this.cNotify.transform);
            SetUpAnchorOffsetFullScreen(ob);
#if UNITY_EDITOR
            ob.name = "NOTIFY_" + nameNotify;
#endif
            notifyScr = ob.GetComponent<BaseNotify>();
            notifyScr.Init();
            return notifyScr;
        }

        public void HideAllNotifies()
        {
            BaseNotify notifyScr = null;

            foreach (KeyValuePair<string, BaseNotify> item in notifies)
            {
                notifyScr = item.Value;
                if (notifyScr == null || notifyScr.IsHide)
                    continue;
                notifyScr.Hide();

                if (notifies.Count <= 0)
                    break;
            }
        }

        public void HideNotify<T>() where T : BaseNotify
        {
            string notifyName = typeof(T).Name;
            if (notifies.TryGetValue(notifyName, out BaseNotify notify))
            {
                if (notify.IsActive)
                {
                    notify.Hide();
                }
            }
        }

        public T GetNotify<T>() where T : BaseNotify
        {
            string notifyName = typeof(T).Name;

            if (notifies.TryGetValue(notifyName, out BaseNotify cached))
                return cached as T;

            BaseNotify newNotify = GetNewNotify<T>();
            if (newNotify == null)
            {
                return null;
            }
            T typedNotify = newNotify as T;
            notifies[notifyName] = typedNotify;

            return typedNotify;
        }

        public T ShowNotify<T>(object data = null, bool forceShowData = false) where T : BaseNotify
        {
            string notifyName = typeof(T).Name;
            BaseNotify result = null;

            if (curNotify != null)
            {
                var curName = curNotify.GetType().Name;
                if (curName.Equals(notifyName))
                {
                    result = curNotify;
                }
            }

            if (result == null)
            {
                if (!notifies.ContainsKey(notifyName))
                {
                    BaseNotify notifyScr = GetNewNotify<T>();
                    if (notifyScr != null)
                    {
                        notifies.Add(notifyName, notifyScr);
                    }
                }

                if (notifies.ContainsKey(notifyName))
                {
                    result = notifies[notifyName];
                }
            }

            bool isShow = false;
            if (result != null)
            {
                if (forceShowData)
                {
                    isShow = true;
                }
                else
                {
                    if (result.IsHide)
                    {
                        isShow = true;
                    }
                }
            }

            if (isShow)
            {
                curNotify = result;
                result.transform.SetAsLastSibling();
                result.Show(data);
            }
            return result as T;
        }

        public bool IsNotifyActive<T>() where T : BaseNotify
        {
            string notifyName = typeof(T).Name;
            if (notifies.TryGetValue(notifyName, out BaseNotify notify))
            {
                return notify.IsActive;
            }
            return false;
        }

        #endregion

        #region Overlap

        private BaseOverlap GetNewOverLap<T>() where T : BaseOverlap
        {
            string nameOverlap = typeof(T).Name;
            BaseOverlap overlapScr;
            if (cOverlap != null)
            {
                T existingOverlap = cOverlap.GetComponentInChildren<T>(true);
                if (existingOverlap != null)
                {
                    overlapScr = existingOverlap as BaseOverlap;
#if UNITY_EDITOR
                    overlapScr.gameObject.name = "OVERLAP_" + nameOverlap;
#endif
                    overlapScr.Init();

                    return overlapScr;
                }
            }

            GameObject pfOverlap = GetUIPrefab(UIType.Overlap, nameOverlap);
            if (pfOverlap == null || !pfOverlap.GetComponent<BaseOverlap>())
            {
                throw new MissingReferenceException("Can not found" + nameOverlap + "overlap. !!!");
            }
            GameObject ob = Instantiate(pfOverlap) as GameObject;
            ob.transform.SetParent(this.cOverlap.transform);
            SetUpAnchorOffsetFullScreen(ob);
#if UNITY_EDITOR
            ob.name = "OVERLAP_" + nameOverlap;
#endif
            overlapScr = ob.GetComponent<BaseOverlap>();
            overlapScr.Init();
            return overlapScr;
        }

        public void HideAllOverlaps()
        {
            BaseOverlap overlapScr = null;

            foreach (KeyValuePair<string, BaseOverlap> item in overlaps)
            {
                overlapScr = item.Value;
                if (overlapScr == null || overlapScr.IsHide)
                    continue;
                overlapScr.Hide();

                if (overlaps.Count <= 0)
                    break;
            }
        }

        public void HideOverlap<T>() where T : BaseOverlap
        {
            string overlapName = typeof(T).Name;
            if (overlaps.TryGetValue(overlapName, out BaseOverlap overlap))
            {
                if (overlap.IsActive)
                {
                    overlap.Hide();
                }
            }
        }

        public T GetOverlap<T>() where T : BaseOverlap
        {
            string overlapName = typeof(T).Name;

            if (overlaps.TryGetValue(overlapName, out BaseOverlap cached))
                return cached as T;

            BaseOverlap newOverlap = GetNewOverLap<T>();
            if (newOverlap == null)
            {
                return null;
            }
            T typedOverlap = newOverlap as T;
            overlaps[overlapName] = typedOverlap;

            return typedOverlap;
        }

        public T ShowOverlap<T>(object data = null, bool forceShowData = false) where T : BaseOverlap
        {
            string overlapName = typeof(T).Name;
            BaseOverlap result = null;

            if (curOverlap != null)
            {
                var curName = curOverlap.GetType().Name;
                if (curName.Equals(overlapName))
                {
                    result = curOverlap;
                }
            }

            if (result == null)
            {
                if (!overlaps.ContainsKey(overlapName))
                {
                    BaseOverlap overlapScr = GetNewOverLap<T>();
                    if (overlapScr != null)
                    {
                        overlaps.Add(overlapName, overlapScr);
                    }
                }

                if (overlaps.ContainsKey(overlapName))
                {
                    result = overlaps[overlapName];
                }
            }

            bool isShow = false;
            if (result != null)
            {
                if (forceShowData)
                {
                    isShow = true;
                }
                else
                {
                    if (result.IsHide)
                    {
                        isShow = true;
                    }
                }
            }

            if (isShow)
            {
                curOverlap = result;
                result.transform.SetAsLastSibling();
                result.Show(data);
            }
            return result as T;
        }

        public bool IsOverlapActive<T>() where T : BaseOverlap
        {
            string overlapName = typeof(T).Name;
            if (overlaps.TryGetValue(overlapName, out BaseOverlap overlap))
            {
                return overlap.IsActive;
            }
            return false;
        }
        #endregion

        #region  Other

        private GameObject GetUIPrefab(UIType t, string uiName)
        {
            GameObject result = null;
            var defaultPath = "";
            if (result == null)
            {
                switch (t)
                {
                    case UIType.Screen:
                        {
                            defaultPath = SCREEN_RESOURCES_PATH + uiName;
                        }
                        break;
                    case UIType.Popup:
                        {
                            defaultPath = POPUP_RESOURCES_PATH + uiName;
                        }
                        break;
                    case UIType.Notify:
                        {
                            defaultPath = NOTIFY_RESOURCES_PATH + uiName;
                        }
                        break;
                    case UIType.Overlap:
                        {
                            defaultPath = OVERLAP_RESOURCES_PATH + uiName;
                        }
                        break;
                }

                result = Resources.Load(defaultPath) as GameObject;
            }
            return result;
        }

        private void SetUpAnchorOffsetFullScreen(GameObject obj)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect == null) return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localPosition = Vector3.zero;
            rect.localScale = Vector3.one;
        }
        #endregion
    }
}
