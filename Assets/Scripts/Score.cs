using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UniRx;

public class Score : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText = null;

    private IntReactiveProperty score = new IntReactiveProperty(0);
    private CompositeDisposable disposables = new CompositeDisposable();

    public Subject<Unit> OnItemGetObservable { get; } = new Subject<Unit>();

    public void StartSystem()
    {
        var data = Resources.Load<Data>("Data");
        var startTime = Time.time;

        score.Value = 0;
        scoreText.gameObject.SetActive(true);

        // time score
        Observable.Interval(System.TimeSpan.FromSeconds(data.ScoreInterval))
            .Subscribe(_ =>
            {
                // todo: time score 計算式
                score.Value += (int)(data.ScoreForTime * (1.0f + (Time.time - startTime) * data.ScoreForTimeImplovingRate));
            })
            .AddTo(disposables)
            .AddTo(this);

        // item score
        OnItemGetObservable
            .Subscribe(_ =>
            {
                // todo: item score計算式
                score.Value += (int)(data.ScoreForItem * (1.0f + (Time.time - startTime) * data.ScoreForItemImplovingRate));
            })
            .AddTo(disposables)
            .AddTo(this);

        // write score
        score.Subscribe(val => scoreText.text = $"Score: {val}")
            .AddTo(disposables)
            .AddTo(this);
    }

    public void StopSystem()
    {
        disposables.Clear();
        scoreText.gameObject.SetActive(false);
    }

    public int GetScore()
    {
        return score.Value;
    }
}
