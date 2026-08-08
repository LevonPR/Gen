using System.Collections.Generic;
using UnityEngine;

namespace MicroEvolution.World
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

        void Awake()
        {
            Instance = this;
        }

        public GameObject Get(string key, System.Func<GameObject> factory)
        {
            if (_pools.TryGetValue(key, out var q) && q.Count > 0)
            {
                var go = q.Dequeue();
                go.SetActive(true);
                return go;
            }

            return factory();
        }

        public void Release(string key, GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            if (!_pools.TryGetValue(key, out var q))
            {
                q = new Queue<GameObject>();
                _pools[key] = q;
            }

            q.Enqueue(go);
        }
    }
}
