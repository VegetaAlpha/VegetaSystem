using UnityEngine;
using UnityEngine.UI;
using VegetaSystem;
using VegetaSystem.UI;

public class Sample_MenuSettingPopup : BasePopup
{
    [SerializeField] Button closeBtn;
    [SerializeField] Button tutorialBtn;


    void Start()
    {
        closeBtn.onClick.AddListener(() => Sample_UIManger.Instance.HidePopup<Sample_MenuSettingPopup>());
        tutorialBtn.onClick.AddListener(() => Sample_UIManger.Instance.ShowPopup<Sample_TutorialPopup>());
    }

    void OnDestroy ()
    {
        closeBtn.onClick.RemoveListener(() => Sample_UIManger.Instance.HidePopup<Sample_MenuSettingPopup>());
        tutorialBtn.onClick.RemoveListener(() => Sample_UIManger.Instance.ShowPopup<Sample_TutorialPopup>());
    }
}
