using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;
using CycleUtils;
using System.Linq;

public class SystemManager : MonoBehaviour
{
    [SerializeField] private Player player = null;
    [SerializeField] private Score score = null;
    [SerializeField] private ObjSpawner objSpawner = null;

    [SerializeField] private GameObject titleText = null;
    [SerializeField] private GameObject scoreTextParent = null;
    [SerializeField] private TextMeshProUGUI[] scoreText = null;
    [SerializeField] private TextMeshProUGUI currentScoreText = null;
    [SerializeField] private GameObject waitInputText = null;

    private Data data;

    private void Start()
    {
        data = Resources.Load<Data>("Data");
        ShowTitle();
    }

    public void ShowTitle()
    {
        titleText.SetActive(true);

        InputManager.Instance.GetButton(0, 0)
            .SkipWhile(val => val)
            .DistinctUntilChanged()
            .Where(val => val)
            .First()
            .Subscribe(_ =>
            {
                Debug.Log("ShowTitle -> StartGame");
                titleText.SetActive(false);
                StartGame();
            });
    }

    public void StartGame()
    {
        var fov = Camera.main.fieldOfView;
        var skyboxMat = RenderSettings.skybox;
        float rotation = 0.0f;
        skyboxMat.SetFloat("_Rotation", rotation);

        player.StartSystem();
        score.StartSystem();
        objSpawner.StartSystem();

        SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();

        disposable.Disposable = Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                // Camera FOV changing
                fov += data.CameraFOVAcc * Time.deltaTime;
                if (fov > data.CameraFOVEnd) fov = data.CameraFOVEnd;
                Camera.main.fieldOfView = fov;

                // Skybox rotation
                rotation += data.SkyboxRotateSpeed * Time.deltaTime;
                if (rotation > 360.0f) rotation -= 360.0f;

                skyboxMat.SetFloat("_Rotation", rotation);

                // GameOver
                if (!player.gameObject.activeSelf)
                {
                    Debug.Log("StartGame -> ShowScore");
                    disposable.Dispose();

                    player.StopSystem();
                    score.StopSystem();
                    objSpawner.StopSystem();

                    ShowScore();
                }
            });
    }

    [System.Serializable]
    private struct ScoreData
    {
        public int[] scores;

        public ScoreData(int num)
        {
            scores = new int[num];
        }
    }

    public void ShowScore()
    {
        var path = Application.persistentDataPath + "/scores";
        var currentScore = score.GetScore();

        ScoreData scoreData;
        if (File.Exists(path)) scoreData = JsonUtility.FromJson<ScoreData>(File.ReadAllText(path));
        else scoreData = new ScoreData(data.ScoreNum);

        List<int> newScore = scoreData.scores.ToList();
        newScore.Add(currentScore);
        newScore = newScore.OrderBy(val => -val).ToList();
        var currentRank = newScore.FindIndex(val => val == currentScore);

        // show score
        var suffixes = new string[] { "st", "nd", "rd", "th", "th", "th", "th", "th", "th", "th" };
        var scoreStrs = new List<string>();
        for (int i = 0; i < data.ScoreNum; ++i)
        {
            scoreStrs.Add($"{i + 1}{suffixes[i]}:\t{newScore[i],8}");
            if (i == currentRank)
            {
                scoreStrs[i] = $"<color=#ff8>{scoreStrs[i]}</color>";
            }
        }

        scoreText[0].text = string.Join("\n", scoreStrs.Take(data.ScoreNum / 2));
        scoreText[1].text = string.Join("\n", scoreStrs.Skip(data.ScoreNum / 2));

        currentScoreText.text = $"Current Score:\t{currentScore}";
        waitInputText.SetActive(false);

        scoreTextParent.SetActive(true);

        scoreData.scores = newScore.Take(data.ScoreNum).ToArray();
        File.WriteAllText(path, JsonUtility.ToJson(scoreData));

        Observable.Timer(System.TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ =>
            {
                waitInputText.SetActive(true);
                InputManager.Instance.GetButton(0, 0)
                    .SkipWhile(val => val)
                    .DistinctUntilChanged()
                    .Where(val => val)
                    .First()
                    .Subscribe(__ =>
                    {
                        Debug.Log("ShowScore -> ShowTitle");
                        scoreTextParent.SetActive(false);
                        ShowTitle();
                    });
            });

    }
}
