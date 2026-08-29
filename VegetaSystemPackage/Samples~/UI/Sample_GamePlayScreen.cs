using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VegetaSystem;
using VegetaSystem.UI;

public class Sample_GamePlayScreen : BaseScreen
{
    [SerializeField] Button spawnCubeBtn;
    [SerializeField] Button spawnRedSphereBtn;
    [SerializeField] Button spawnBlueSphereBtn;
    [SerializeField] Button settingBtn;


    private void Start()
    {
        spawnCubeBtn.onClick.AddListener(OnClickSpawnCube);
        spawnRedSphereBtn.onClick.AddListener(OnClickSpawnRedSphere);
        spawnBlueSphereBtn.onClick.AddListener(OnClickSpawnBlueSphere);
        settingBtn.onClick.AddListener(OnClickSettingBtn);
    }

    private void OnDestroy()
    {
        spawnCubeBtn.onClick.RemoveListener(OnClickSpawnCube);
        spawnRedSphereBtn.onClick.RemoveListener(OnClickSpawnRedSphere);
        spawnBlueSphereBtn.onClick.RemoveListener(OnClickSpawnBlueSphere);
        settingBtn.onClick.RemoveListener(OnClickSettingBtn);
    }

    private void OnClickSpawnCube()
    {
        Sample_EventContainer.OnClickSpawnCube?.Invoke();
    }

    private void OnClickSpawnRedSphere()
    {
        Sample_EventContainer.OnClickSpawnRedSphere?.Invoke();
    }

    private void OnClickSpawnBlueSphere()
    {
        Sample_EventContainer.OnClickSpawnBlueSphere?.Invoke();
    }

    private void OnClickSettingBtn ()
    {
        Sample_UIManger.Instance.HideAllPopups();
        Sample_UIManger.Instance.ShowPopup<Sample_GamePlaySettingPopup>();
    }
}
