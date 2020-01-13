using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

[RequireComponent(typeof(Rigidbody))]
public class ObjSystem : MonoBehaviour
{
    private static float moveLimit = -10.0f;
    private Rigidbody rigid;

    public Color ColorTag { get; private set; }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        FloatReactiveProperty position = new FloatReactiveProperty();

        this.UpdateAsObservable()
            .Where(_ => gameObject.activeSelf && transform.position.z < moveLimit)
            .Subscribe(_ =>
            {
                gameObject.SetActive(false);
            })
            .AddTo(this);
    }

    public void Initialize(float velocity, Color color)
    {
        rigid.velocity = Vector3.back * velocity;
        GetComponent<MeshRenderer>().material.color = color;
        ColorTag = color;
    }
}
