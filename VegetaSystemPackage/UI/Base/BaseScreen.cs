using UnityEngine;

namespace VegetaSystem.UI
{
    public class BaseScreen : BaseUIElement
    {
        public override void Init()
        {
            base.Init();
            this.uiType = UIType.Screen;
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
