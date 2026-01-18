using VegetaSystem.UI;
using VegetaSystem;
using UnityEngine;
using UnityEngine.UI;

public class Sample_Menuscreen : BaseScreen
{
    [SerializeField] Button nextBtn;
    [SerializeField] Button settingBtn;

    private void Start()
    {
        nextBtn.onClick.AddListener(OnClickNextBtn);
        settingBtn.onClick.AddListener(OnClickSettingBtn);
    }

    private void OnDestroy()
    {
        nextBtn.onClick.RemoveListener(OnClickNextBtn);
        settingBtn.onClick.RemoveListener(OnClickSettingBtn);
    }

    private void OnClickNextBtn()
    {
        var loadingNotify = Sample_UIManger.Instance.GetNotify<Sample_LoadingNotify>();
        ConfigLoadScene config = new ConfigLoadScene
        (
           sceneName: Sample_Constant.Sample_GamePlayScreen,
           onBeforeLoad: () =>
           {
               loadingNotify.Show(null);
           },
           onProgress: (value) =>
           {
               loadingNotify.UpdateProcess(value);
           },
           delayCompleted: 0.2f,
           onAfterLoad: () =>
           {
               Sample_UIManger.Instance.HideScreen<Sample_Menuscreen>();
               Sample_UIManger.Instance.ShowScreen<Sample_GamePlayScreen>();
               loadingNotify.Hide();
           }
        );

        LoadSceneSystem.LoadNewScene(config);
    }

    private void OnClickSettingBtn()
    {
        Sample_UIManger.Instance.HideAllPopups();
        Sample_UIManger.Instance.ShowPopup<Sample_MenuSettingPopup>();
    }
}
