using UnityEngine;

namespace VegetaSystem.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BaseUIElement : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;
        protected UIType uiType = UIType.Unknown;
        protected bool isHide;
        private bool isInited;

        public bool IsActive { get => !isHide; }
        public bool IsHide { get => isHide; }
        public bool IsInited { get => isInited; }
        public UIType UIType { get => uiType; }

        public virtual void Init()
        {
            this.isInited = true;
            if (!this.gameObject.GetComponent<CanvasGroup>())
            {
                this.gameObject.AddComponent<CanvasGroup>();
            }
            this.canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
            this.gameObject.SetActive(true);

            Hide();
        }

        public virtual void Show(object data)
        {
            this.gameObject.SetActive(true);
            this.isHide = false;
            SetActiveGroupCanvas(true);
        }

        public virtual void Hide()
        {
            this.isHide = true;
            SetActiveGroupCanvas(false);
        }

        public virtual void Clear()
        {

        }

        private void SetActiveGroupCanvas(bool isAct)
        {
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = isAct;
                canvasGroup.alpha = isAct ? 1 : 0;
            }
        }

        public virtual void OnClickedBackButton()
        {

        }
    }
}