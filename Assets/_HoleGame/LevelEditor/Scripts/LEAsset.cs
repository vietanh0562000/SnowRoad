using UnityEngine;

namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using BasePuzzle.PuzzlePackages.Core;
    using Sirenix.OdinInspector;

    [DisallowMultipleComponent, SelectionBase]
    public sealed class LEAsset : MonoBehaviour
    {
        [TabGroup("Board Visualizer")] [SerializeField] private LETile          _tilePrefab;
        [TabGroup("Board Visualizer")] [SerializeField] private LEStickManChunk _stickManChunk;
        [TabGroup("Board Visualizer")] [SerializeField] private LEHole          _hole;
        [TabGroup("Board Visualizer")] [SerializeField] private LEObstacle      _obstacle;
        [TabGroup("Board Visualizer")] [SerializeField] private LETunnel        _tunnel;
        [TabGroup("Board Visualizer")] [SerializeField] private LEFloorAdd      _floorAdd;
        [TabGroup("Board Visualizer")] [SerializeField] private LEFloorDel      _floorDel;

        private readonly Dictionary<Type, object> _pools         = new();
        private          List<ALESpawnItem>       _aleSpawnItems = new();

        public LEStickManChunk Chunk    => _stickManChunk;
        public LEHole          Hole     => _hole;
        public LEObstacle      Obstacle => _obstacle;
        public LETunnel        Tunnel   => _tunnel;
        public LEFloorAdd      FloorAdd => _floorAdd;
        public LEFloorDel      FloorDel => _floorDel;

        private void Awake()
        {
            PrefabPool<LETile>.Create(_tilePrefab, 400, 1000, true);
            
            RegisterPool<LEStickManChunk>(typeof(StickManData), _stickManChunk, 50, 100);
            RegisterPool<LEHole>(typeof(HoleBoxData), _hole, 10, 20);
            RegisterPool<LEObstacle>(typeof(ObstacleData), _obstacle, 20, 50);
            RegisterPool<LETunnel>(typeof(TunnelData), _tunnel, 5, 15);
        }
        
        private void RegisterPool<T>(Type type, T prefab, int cap, int max) where T : ALESpawnItem
        {
            var pool = PrefabPool<T>.Create(prefab, cap, max, true);
            _pools.TryAdd(type, pool);
        }
        
        public BoxData SpawnFromPool(BoxData data, Vector3 position, Transform parent = null)
        {
            var dataType = data.GetType();
            if (!_pools.TryGetValue(dataType, out var pool))
            {
                Debug.LogError($"Pool for {dataType.Name} not found");
                return null;
            }
            
            var methodInfo = pool.GetType().GetMethod("Spawn");
            if (methodInfo != null)
            {
                // Use reflection to call the Spawn method
                var spawnedObject = methodInfo.Invoke(pool, new object[] { });
                var go = spawnedObject as ALESpawnItem;

                if (go != null)
                {
                    go.transform.position = position;
                    go.transform.SetParent(parent);
                    
                    go.SetUpData();
                    go.SetPosition(new Vector2Int(data.position.x, data.position.y));
                    go.CopyData(data);
                    
                    _aleSpawnItems.Add(go);

                    return go.Data;
                }
            }

            return null;
        }

        public void ResetCache()
        {
            _aleSpawnItems.Clear();
        }

        public void UpdateItems()
        {
            foreach (var item in _aleSpawnItems)
            {
                item.UpdateFollowData();
            }
        }
    }
}
