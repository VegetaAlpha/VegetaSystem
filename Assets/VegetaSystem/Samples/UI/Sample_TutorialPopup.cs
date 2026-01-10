using System;
using UnityEngine;
using UnityEngine.UI;
using VegetaSystem.UI;


public class Hello 
{
    public Action onon;
}
public class Sample_TutorialPopup : BasePopup
{
    [SerializeField] Button closeBtn;

    private void Start()
    {
        closeBtn.onClick.AddListener(() => Sample_UIManger.Instance.HidePopup<Sample_TutorialPopup>());
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(() => Sample_UIManger.Instance.HidePopup<Sample_TutorialPopup>());
    }

    public override void Show(object data)
    {
        base.Show(data);
    }
}
