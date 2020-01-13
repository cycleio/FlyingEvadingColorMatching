using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class GameObjectPool : MonoBehaviour
{
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int addingPoolSize = 5;
    [SerializeField] private GameObject objPrefab = null;

    private List<GameObject> objPool = new List<GameObject>();

    void Start()
    {
        Debug.Assert(objPrefab != null);

        // objPool Initialization
        ExtendPool(initialPoolSize);
    }

    public GameObject GetGameObject()
    {
        // Find usable obj
        foreach(var obj in objPool)
        {
            if (!obj.activeSelf) return obj;
        }

        // not found; Create new objs
        ExtendPool(addingPoolSize);
        return objPool.Last();
    }

    private void ExtendPool(int num)
    {
        foreach (var _ in Enumerable.Range(0, num))
        {
            var obj = Instantiate(objPrefab, gameObject.transform);
            obj.SetActive(false);
            objPool.Add(obj);
        }
    }
}
