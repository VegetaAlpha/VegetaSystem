using UnityEngine;

namespace VegetaSystem.UI
{
    public class BasePopup : BaseUIElement
    {
        public override void Init()
        {
            base.Init();
            this.uiType = UIType.Popup;
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void Show(object data)
        {
            base.Show(data);
        }

        public override void OnClickedBackButton()
        {
            base.OnClickedBackButton();
        }
    }
}
