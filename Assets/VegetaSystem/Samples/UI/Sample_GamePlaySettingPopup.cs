using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VegetaSystem;
using VegetaSystem.UI;

public class Sample_GamePlaySettingPopup : BasePopup
{
    [SerializeField] Button closeBtn;
    [SerializeField] Button homeBtn;


    void Start()
    {
        closeBtn.onClick.AddListener(() => Sample_UIManger.Instance.HidePopup<Sample_GamePlaySettingPopup>());
        homeBtn.onClick.AddListener(OnClickHomeBtn);
    }

    void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(() => Sample_UIManger.Instance.HidePopup<Sample_GamePlaySettingPopup>());
        homeBtn.onClick.RemoveListener(OnClickHomeBtn);
    }

    private void OnClickHomeBtn()
    {
        Sample_EventContainer.OnClickHomeBtn?.Invoke();

        ConfigLoadScene config = new ConfigLoadScene
        (
           sceneName: Sample_Constant.Sample_MenuScreen,
           ignoreDisplayProgress : true,
           delayCompleted : 0.2f,
           onBeforeLoad: () =>
           {
               Sample_UIManger.Instance.ShowOverlap<Sample_BlockOverlap>();
           },
           onAfterLoad: () =>
           {
               Sample_UIManger.Instance.HideAllPopups();
               Sample_UIManger.Instance.HideAllOverlaps();
               Sample_UIManger.Instance.ShowScreen<Sample_Menuscreen>();
               Sample_UIManger.Instance.HideScreen<Sample_GamePlayScreen>();
           }
        );

        Sample_LoadSceneManager.Instance.LoadNewScene(config);
    }
}
