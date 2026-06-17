using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 15;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly HashSet<GameObject> inPool = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePool();
    }

    public void InitializePool()
    {
        // Clear any existing items
        foreach (var obj in pool)
        {
            if (obj != null) Destroy(obj);
        }
        pool.Clear();
        inPool.Clear();

        if (prefab != null)
        {
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewInstance();
            }
        }
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
        inPool.Add(obj);
        return obj;
    }

    public GameObject Get()
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : CreateNewInstance();
        inPool.Remove(obj);
        obj.transform.SetParent(null);
        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        if (inPool.Add(obj))
        {
            pool.Enqueue(obj);
        }
    }
}
