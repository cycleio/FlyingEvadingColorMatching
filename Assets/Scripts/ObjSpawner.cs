using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using System;

[RequireComponent(typeof(GameObjectPool))]
public class ObjSpawner : MonoBehaviour
{
    private GameObjectPool objPool;
    private CompositeDisposable disposables = new CompositeDisposable();

    public void StartSystem()
    {
        var data = Resources.Load<Data>("Data");
        objPool = GetComponent<GameObjectPool>();

        disposables.Clear();

        var spawnIntervalBase = data.SpawnIntervalInit;
        var spawnInterval = spawnIntervalBase + UnityEngine.Random.Range(-data.SpawnIntervalVariant, data.SpawnIntervalVariant);
        var spawnVelocity = data.SpeedInit;
        var spawnNum = data.SpawnNumInit;

        // spawnIntervalごとにobj生成
        // Timerでは実装困難(再帰的になる&誤差があるかも)なのでUpdateでやる
        float time = 0;
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                time += Time.deltaTime;
                if(time > spawnInterval)
                {
                    time -= spawnInterval;
                    spawnInterval = Mathf.Max(spawnInterval - data.SpawnIntervalAcc, data.SpawnIntervalEnd);
                    spawnInterval += UnityEngine.Random.Range(-data.SpawnIntervalVariant, data.SpawnIntervalVariant);

                    var radNums = Enumerable.Range(0, 8).OrderBy(__ => Guid.NewGuid()).Take(UnityEngine.Random.Range(1, (int)spawnNum + 1));
                    spawnNum = Mathf.Min(spawnNum + data.SpawnNumAcc, data.SpawnNumEnd);

                    foreach (var radNum in radNums)
                    {
                        var obj = objPool.GetGameObject();

                        // set pos
                        var pos = transform.position;
                        pos += Vector3.up * data.PositionRadius * Mathf.Sin(Mathf.PI / 4 * radNum);
                        pos += Vector3.right * data.PositionRadius * Mathf.Cos(Mathf.PI / 4 * radNum);
                        pos += Vector3.down * spawnVelocity * time;
                        obj.transform.position = pos;

                        // set velocity/color
                        obj.GetComponent<ObjSystem>().Initialize(spawnVelocity,
                            data.Colors[UnityEngine.Random.Range(0, data.ColorNum)]);
                        spawnVelocity += data.SpeedAcc;

                        obj.SetActive(true);
                    }
                }
            })
            .AddTo(disposables)
            .AddTo(this);
    }

    public void StopSystem()
    {
        disposables.Clear();
    }
}
