using VegetaSystem.UI;
using UnityEngine;
using UnityEngine.UI;
public class Sample_LoadingNotify : BaseNotify
{
    [SerializeField] Image processImg;

    public override void Show(object data)
    {
        processImg.fillAmount = 0;
        base.Show(data);
    }

    public override void Hide()
    {
        processImg.fillAmount = 0;
        base.Hide();
    }

    public void UpdateProcess (float value)
    {
        processImg.fillAmount = value;
    }
}
