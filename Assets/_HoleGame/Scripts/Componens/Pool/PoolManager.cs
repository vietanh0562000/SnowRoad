using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoleBox
{
    using BasePuzzle.PuzzlePackages.Core;
    using global::HoleBox.HoleBox;
    using UnityEngine.Serialization;

    public class PoolManager : Singleton<PoolManager>
    {
        private readonly         Dictionary<Type, object> _pools = new();
        [SerializeField] private StickManBox              stickManPrefab;
        [SerializeField] private Hole                     holePrefab;
        [SerializeField] private Tunnel                   _tunnelPrefab;
        [SerializeField] private Obstacle                 _obstaclePrefab;

        [SerializeField] private Stickman      stickMan;
        [SerializeField] private TilePrefab    _tilePrefab;
        [SerializeField] private JumpFX        jumpFX;
        [SerializeField] private AutoDespawnFX autoDespawnFX;

        protected override void Awake()
        {
            base.Awake();
            
            // Register StickManPool
            var stickManPool = new StickManPool(stickManPrefab);
            RegisterPool(stickManPool);

            // Register HolePool
            var holePool = new HolePool(holePrefab);
            RegisterPool(holePool);

            // Register TunnelPool
            var tunnelPool = new TunnelPool(_tunnelPrefab);
            RegisterPool(tunnelPool);

            // Register ObstaclePool
            var obstaclePool = new ObstaclePool(_obstaclePrefab);
            RegisterPool(obstaclePool);

            PrefabPool<TilePrefab>.Create(_tilePrefab, 400, 1000, true);
            PrefabPool<Stickman>.Create(stickMan, 100, 500, true);
            PrefabPool<AutoDespawnFX>.Create(autoDespawnFX, 10, 50, true);
            PrefabPool<JumpFX>.Create(jumpFX, 20, 100, true);
        }

        public void ClearPool()
        {
            PrefabPool<TilePrefab>.ClearAll();
            PrefabPool<Stickman>.ClearAll();
            PrefabPool<AutoDespawnFX>.ClearAll();
            PrefabPool<JumpFX>.ClearAll();
        }

        // Register a pool in the dictionary
        private void RegisterPool<T, TX>(AHolePool<T, TX> pool) where T : BoxData where TX : BoxDataSetter<T>
        {
            var type = typeof(T);
            _pools.TryAdd(type, pool);
        }

        // Get the pool corresponding to a specific BoxData type
        public AHolePool<T, TX> GetPool<T, TX>() where T : BoxData where TX : BoxDataSetter<T>
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var pool))
            {
                return pool as AHolePool<T, TX>;
            }

            Debug.LogError($"No pool registered for type: {type}");
            return null;
        }

        // Get the pool corresponding to a specific BoxData type
        public object GetPool(Type type)
        {
            if (_pools.TryGetValue(type, out var pool))
            {
                return pool;
            }

            Debug.LogError($"No pool registered for type: {type}");
            return null;
        }

        public void ReleaseAll()
        {
            // Release các pool trong PrefabPool
            PrefabPool<TilePrefab>.ClearAll();
            PrefabPool<Stickman>.ClearAll();

            // Release tất cả các pool trong dictionary _pools
            foreach (var pool in _pools.Values)
            {
                // Nếu pool có phương thức Clear hoặc Release thì gọi
                var method = pool.GetType().GetMethod("ReleaseAll");
                if (method != null)
                {
                    method.Invoke(pool, null);
                }
            }

            PrefabPool<TilePrefab>.Create(_tilePrefab, 400, 1000, true);
            PrefabPool<Stickman>.Create(stickMan, 50, 100, true);
        }

        public IDataSetter Spawn(BoxData data, Vector3 position, Transform parent)
        {
            // Attempt to retrieve the correct pool for the given BoxData type
            var pool = GetPool(data.GetType());

            // Cast the pool explicitly to the known generic type
            var methodInfo = pool.GetType().GetMethod("Spawn");

            if (methodInfo != null)
            {
                // Use reflection to call the Spawn method
                var spawnedObject = methodInfo.Invoke(pool, new object[] { position, parent });

                // Ensure the spawned object matches the expected data setter type
                if (spawnedObject is IDataSetter dataSetter)
                {
                    dataSetter.SetData(data);
                    return dataSetter;
                }
            }

            // Log error if the pool or type matching fails
            Debug.LogError($"No valid pool or matching type found for: {data.GetType()}");
            return null;
        }
    }
}