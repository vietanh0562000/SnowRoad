using UnityEngine;

namespace HoleBox
{
    using System;
    using BasePuzzle.PuzzlePackages.Core;
    using TMPro;

    [DisallowMultipleComponent, SelectionBase]
    public class LEObstacle : ALESpawnItem
    {
        [SerializeField] private Sprite   _barrierSprite;
        [SerializeField] private TMP_Text _txt;
        
        private ObstacleData _boxData = new();

        public override void SetUpData()
        {
            _boxData.position = Vector2Int.zero;
            _boxData.size     = Vector2Int.one * 2;
            _boxData.id       = -1;
        }

        public override void Swap() { }
        
        public override void Highlight(bool value)
        {
            HlGameObject?.SetActive(value);
        }

        public override void CopyData(BoxData data)
        {
            var obstacleData = data as ObstacleData;
            _boxData.id            = obstacleData.id;
            _boxData.IsBarrier     = obstacleData.IsBarrier;
            _boxData.IsOpenBarrier = obstacleData.IsOpenBarrier;
        }
        
        public override void UpdateFollowData()
        {
            _txt.gameObject.SetActive(_boxData.IsBarrier);
            _txt.text = _boxData.IsOpenBarrier ? "" : "X";

            if (_boxData.IsBarrier)
            {
                ChangeSprite(_barrierSprite, Color.white);
            }
            else
            {
                ChangeNormalSprite();
                ChangeColorInternal(Color.black);
            }
        }

        public override BoxData Data               => _boxData;
        public override bool    IsAbleToChangeSwap => false;
        
        public override ALESpawnItem SpawnFromPool()
        {
            var inst = PrefabPool<LEObstacle>.Spawn(this);
            inst.SetUpData();
            return inst;
        }

        public override void SendToPool()
        {
            PrefabPool<LEObstacle>.Release(this);
        }
    }
}
