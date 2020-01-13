using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Linq;
using CycleUtils;

public class Player : MonoBehaviour
{
    [SerializeField] private Score scoreManager = null;
    [SerializeField] private Transform effectParentTransform = null;
    [SerializeField] private GameObject brakeEffectPrefab = null;

    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private AudioClip seGetItem = null;
    [SerializeField] private AudioClip seDefeated = null;

    private readonly float moveSpeed = 0.1f; // /rad
    private readonly Vector3 originPos = new Vector3(0, 0, -5);

    private FloatReactiveProperty abstPos = new FloatReactiveProperty(0);
    private Color colorTag;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void StartSystem()
    {
        Debug.Assert(scoreManager != null);
        Debug.Assert(effectParentTransform != null);
        Debug.Assert(brakeEffectPrefab != null);

        var data = Resources.Load<Data>("Data");
        var inputManager = InputManager.Instance;

        colorTag = data.Colors[0];

        transform.position = originPos;
        gameObject.SetActive(true);

        // move +
        inputManager.GetAxisPositive(0, 0)
            .Where(val => val)
            .Subscribe(_ =>
            {
                var v = abstPos.Value;
                v += moveSpeed;
                if (v > Mathf.PI * 2) v -= Mathf.PI * 2;
                abstPos.Value = v;
            })
            .AddTo(disposables)
            .AddTo(this);

        // move -
        inputManager.GetAxisNegative(0, 0)
            .Where(val => val)
            .Subscribe(_ =>
            {
                var v = abstPos.Value;
                v -= moveSpeed;
                if (v < 0) v += Mathf.PI * 2;
                abstPos.Value = v;
            })
            .AddTo(disposables)
            .AddTo(this);

        // move process
        abstPos.Subscribe(val =>
            {
                var pos = originPos;
                pos += Vector3.up * data.PositionRadius * Mathf.Sin(val);
                pos += Vector3.right * data.PositionRadius * Mathf.Cos(val);
                transform.position = pos;
            })
            .AddTo(disposables)
            .AddTo(this);

        // initial position set
        abstPos.SetValueAndForceNotify(Mathf.PI * 1.5f);

        // change color
        var changeColorMaterial = GetComponent<MeshRenderer>().material;
        var colorID = 0;
        inputManager.GetButton(0, 0)
            .DistinctUntilChanged()
            .Where(val => val)
            .Subscribe(_ =>
            {
                colorID = (colorID + 1) % data.ColorNum;
                var color = data.Colors[colorID];
                colorTag = color;
                changeColorMaterial.color = color;
            })
            .AddTo(disposables)
            .AddTo(this);

        // initial color set
        colorTag = data.Colors[colorID];
        changeColorMaterial.color = data.Colors[colorID];

        // on hit to obj
        //var audioSource = GetComponent<AudioSource>();
        this.OnTriggerEnterAsObservable()
            .Where(val => val.tag == "Obstacle")
            .Subscribe(val =>
            {
                if(colorTag == val.GetComponent<ObjSystem>().ColorTag)
                {
                    // match
                    scoreManager.OnItemGetObservable.OnNext(Unit.Default);
                    val.gameObject.SetActive(false);
                    audioSource.PlayOneShot(seGetItem);
                }
                else
                {
                    // differ
                    var obj = Instantiate(brakeEffectPrefab, transform.position, Quaternion.identity, effectParentTransform);
                    Observable.Timer(System.TimeSpan.FromSeconds(2.0f))
                        .First()
                        .Subscribe(_ =>
                        {
                            Destroy(obj);
                        });
                    audioSource.PlayOneShot(seDefeated);
                    gameObject.SetActive(false);
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
