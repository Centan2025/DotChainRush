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
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
                inPool.Add(obj);
            }
        }
    }

    // Creates a brand-new instance WITHOUT adding it to the pool queue.
    // Used by Get() when the pool is exhausted so the object is never
    // double-counted (active AND sitting in the queue simultaneously).
    private GameObject CreateOnDemandInstance()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }

    public GameObject Get()
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            inPool.Remove(obj);
        }
        else
        {
            // Pool exhausted: create a fresh object that is NOT in the queue
            obj = CreateOnDemandInstance();
        }
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
