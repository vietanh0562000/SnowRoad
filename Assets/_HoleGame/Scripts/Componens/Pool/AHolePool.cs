namespace HoleBox
{
    using System.Collections.Generic;
    using BasePuzzle.PuzzlePackages.Core;
    using UnityEngine;

    public abstract class AHolePool<T, TX> where T : BoxData where TX : BoxDataSetter<T>
    {
        protected abstract int capacity { get; }
        protected abstract int maxSize  { get; }

        private TX spawnPrefab;

        private readonly List<TX> cached;

        public AHolePool(TX t)
        {
            cached      = new();
            spawnPrefab = t;
            PrefabPool<TX>.Create(spawnPrefab, capacity, maxSize, true);
        }

        public TX Spawn(Vector3 pos, Transform parent)
        {
            var box = PrefabPool<TX>.Spawn();

            box.transform.position = pos;
            box.transform.SetParent(parent);

            cached.Add(box);

            return box;
        }

        public void ReleaseAll()
        {
            foreach (var item in cached)
            {
                if (!item || !item.gameObject.activeSelf) continue;

                PrefabPool<TX>.Release(item);
            }
        }
    }
}