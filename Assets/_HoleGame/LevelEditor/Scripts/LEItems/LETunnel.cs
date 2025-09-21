using UnityEngine;

namespace HoleBox
{
    using System;
    using BasePuzzle.PuzzlePackages.Core;
    using TMPro;

    [DisallowMultipleComponent, SelectionBase]
    public class LETunnel : ALESpawnItem
    {
        [SerializeField] private GameObject[] _directions; // top, down, left, right
        [SerializeField] private TMP_Text     _remainTxt;
        
        private TunnelData _boxData = new();

        public override void SetUpData()
        {
            _boxData.position  = Vector2Int.zero;
            _boxData.size      = Vector2Int.one * 2;
            _boxData.id        = 1;
            _boxData.direction = new Vector2Int(0, 1); // top
            
            UpdateFollowData();
        }

        public override void Swap()
        {
            var currentDir = _boxData.direction;
            int sum        = currentDir.x + currentDir.y;
            if (sum < 0)
            {
                // down, left
                if (currentDir.y < 0)
                {
                    currentDir.y = 0;
                    currentDir.x = -1;
                }
                else
                {
                    currentDir.y = 1;
                    currentDir.x = 0;
                }
            }
            else
            {
                // top, right
                if (currentDir.y > 0)
                {
                    currentDir.y = 0;
                    currentDir.x = 1;
                }
                else
                {
                    currentDir.y = -1;
                    currentDir.x = 0;
                }
            }
            
            _boxData.direction = currentDir;
            UpdateFollowData();
        }
        
        public override void Highlight(bool value)
        {
            HlGameObject?.SetActive(value);
        }

        public override void CopyData(BoxData data)
        {
            var tunnelData = data as TunnelData;
            _boxData.id          = tunnelData.id;
            _boxData.remainSpawn = tunnelData.remainSpawn;
            _boxData.direction   = tunnelData.direction;
            _boxData.randomColor = tunnelData.randomColor;
            _boxData.colorQueue  = tunnelData.colorQueue;
        }

        public override void UpdateFollowData()
        {
            _remainTxt.text = _boxData.remainSpawn.ToString();
            for (int i = 0; i < _directions.Length; i++)
            {
                _directions[i].SetActive(false);
            }
            
            if (_boxData.direction.x == 0)
            {
                if (_boxData.direction.y > 0) _directions[0].SetActive(true);
                else _directions[1].SetActive(true);
            }
            else
            {
                if (_boxData.direction.x < 0) _directions[2].SetActive(true);
                else _directions[3].SetActive(true);
            }
            
            ChangeColor();
        }

        public override BoxData Data               => _boxData;
        public override bool    IsAbleToChangeSwap => true;
        
        public override ALESpawnItem SpawnFromPool()
        {
            var inst = PrefabPool<LETunnel>.Spawn(this);
            inst.SetUpData();
            return inst;
        }

        public override void SendToPool()
        {
            PrefabPool<LETunnel>.Release(this);
        }
    }
}
