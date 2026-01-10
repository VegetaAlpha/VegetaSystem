using UnityEngine;
using VegetaSystem.UI;

public class Sample_UIManger : UIManagerPersist
{
    void Start()
    {
        HideAllScreens();
        ShowScreen<Sample_Menuscreen>();
    }
}
