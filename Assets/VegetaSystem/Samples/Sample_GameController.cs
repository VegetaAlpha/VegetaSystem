using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VegetaSystem;

public class Sample_GameController : MonoBehaviour
{
    private List<ObjPoolable> objs = new();
    
    private void Start()
    {
        EventHelper.RegisterListener(ref Sample_EventContainer.OnClickSpawnCube, SpawnCube);
        EventHelper.RegisterListener(ref Sample_EventContainer.OnClickSpawnRedSphere, SpawnRedSphere);
        EventHelper.RegisterListener(ref Sample_EventContainer.OnClickSpawnBlueSphere, SpawnBlueSphere);
        EventHelper.RegisterListener(ref Sample_EventContainer.OnClickHomeBtn, BackToHome);

        AutoRelease().Forget();
    }

    private void OnDestroy()
    {
        EventHelper.UnregisterListener(ref Sample_EventContainer.OnClickSpawnCube, SpawnCube);
        EventHelper.UnregisterListener(ref Sample_EventContainer.OnClickSpawnRedSphere, SpawnRedSphere);
        EventHelper.UnregisterListener(ref Sample_EventContainer.OnClickSpawnBlueSphere, SpawnBlueSphere);
        EventHelper.UnregisterListener(ref Sample_EventContainer.OnClickHomeBtn, BackToHome);
    }

    private async UniTask AutoRelease()
    {
        var token = this.GetCancellationTokenOnDestroy();

        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: token);
            if (objs.Count > 0)
            {
                Sample_PoolManager.Instance.ReleaseObj(objs[0]);
                objs.RemoveAt(0);
            }
        }
    }

    private void BackToHome()
    {
        foreach (var obj in objs)
        {
            Sample_PoolManager.Instance.ReleaseObj(obj);
        }
        objs.Clear();
    }

    private void SpawnCube()
    {
        var cube = Sample_PoolManager.Instance.GetObj<Sample_Cube>();
        cube.transform.position = GenerateRandomPointInRect(-1, 1, 0, 3, -5);
        cube.SetActive(true);
        objs.Add(cube);
    }

    private void SpawnRedSphere()
    {
        var sphere = Sample_PoolManager.Instance.GetObj<Sample_Sphere>(Sample_SphereType.Red.ToString());
        sphere.transform.position = GenerateRandomPointInRect(-1, 1, 0, 3, -5);
        sphere.SetActive(true);
        objs.Add(sphere);
    }

    private void SpawnBlueSphere()
    {
        var sphere = Sample_PoolManager.Instance.GetObj<Sample_Sphere>(Sample_SphereType.Blue.ToString());
        sphere.transform.position = GenerateRandomPointInRect(-1, 1, 0, 3, -5);
        sphere.SetActive(true);
        objs.Add(sphere);
    }

    public Vector3 GenerateRandomPointInRect(
        float minX, float maxX,
        float minY, float maxY,
        float fixedZ)
    {
        float x = UnityEngine.Random.Range(minX, maxX);
        float y = UnityEngine.Random.Range(minY, maxY);
        return new Vector3(x, y, fixedZ);
    }
}
