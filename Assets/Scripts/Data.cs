using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Data : ScriptableObject
{
    public int ScoreNum { get; } = 10;

    public float CameraFOVEnd { get; } = 100;
    public float CameraFOVAcc { get; } = 0.1f;
    public float SkyboxRotateSpeed { get; } = 0.5f;

    private int colorNum = 2; //色数, 1 or 2
    public int ColorNum
    {
        get { return colorNum; }
        set { colorNum = Mathf.Clamp(value, 1, 2); }
    }
    public Color[] Colors { get; } = { Color.cyan, Color.yellow }; // 色リスト

    public float PositionRadius { get; } = 2.0f; // Obj/Player/etc.の居る円柱の半径

    public float SpawnIntervalInit { get; } = 1.8f; // Objスポーン間隔初期値, sec
    public float SpawnIntervalEnd { get; } = 0.4f; // Objスポーン間隔最終値, sec
    public float SpawnIntervalAcc { get; } = 0.03f; // Objスポーン間隔減少速度, /sec * spawn
    public float SpawnIntervalVariant { get; } = 0.2f; // Objスポーン間隔ランダム変化差分
    public float SpawnNumInit { get; } = 1.0f; // Objスポーン数初期値, floor(this)
    public float SpawnNumEnd { get; } = 4.0f; // Objスポーン数初期値, floor(this)
    public float SpawnNumAcc { get; } = 0.1f; // Objスポーン数初期値, floor(this), /spawn

    public float SpeedInit { get; } = 20.0f; // Obj速度初期値, /sec
    public float SpeedAcc { get; } = 0.3f; // Obj加速度, /sec * spawn

    public float ScoreInterval { get; } = 1.0f;  // Score入手間隔 /sec
    public int ScoreForTime { get; } = 10; // 時間ごとのスコア /time
    public float ScoreForTimeImplovingRate { get; } = 0.01f; // 時間ごとのスコアの時間当たりの増加率 /sec
    public int ScoreForItem { get; } = 500; // Objごとのスコア(基礎点) /time
    public float ScoreForItemImplovingRate { get; } = 0.01f; // Objごとのスコアの時間当たりの増加率 /sec
}
