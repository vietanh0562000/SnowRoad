namespace HoleBox
{
    using DG.Tweening;
    using BasePuzzle.PuzzlePackages.Core;
    using UnityEngine;

    public class TileSpawn : MonoBehaviour
    {
        public float timeToDespawn = 1.5f;

        void OnEnable() { DOVirtual.DelayedCall(timeToDespawn, () => { PrefabPool<TileSpawn>.Release(this); }); }
    }
}