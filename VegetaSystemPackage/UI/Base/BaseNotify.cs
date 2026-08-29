using UnityEngine;

namespace VegetaSystem.UI
{
    public class BaseNotify : BaseUIElement
    {
        public override void Init()
        {
            base.Init();
            this.uiType = UIType.Notify;
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
