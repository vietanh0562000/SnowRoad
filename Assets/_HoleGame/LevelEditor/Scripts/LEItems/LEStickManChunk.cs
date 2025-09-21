using UnityEngine;

namespace HoleBox
{
    using System;
    using BasePuzzle.PuzzlePackages.Core;
    using TMPro;

    [DisallowMultipleComponent, SelectionBase]
    public class LEStickManChunk : ALESpawnItem
    {
        [SerializeField] private Sprite         _hiddenSprite;
        [SerializeField] private SpriteRenderer _hiddenVisual;
        [SerializeField] private TMP_Text       _frozenTxt;
        
        private StickManData _boxData = new();

        public override void SetUpData()
        {
            _boxData.position = Vector2Int.zero;
            _boxData.size     = Vector2Int.one * 2;
            _boxData.id       = 1;
            
            ChangeColor();
        }

        public override void Swap()
        {
            int currentID = _boxData.id;
            currentID++;

            int colorCount = GameAssetManager.Instance.TotalChangedColors;
            
            if (currentID > colorCount) currentID = 1;
            _boxData.id = currentID;

            if (!_boxData.IsHidden)
            {
                ChangeColor();
            }
            
            _hiddenVisual.color = GameAssetManager.Instance.GetColor(currentID);
        }
        
        public override void Highlight(bool value)
        {
            HlGameObject?.SetActive(value);
        }

        public override void CopyData(BoxData data)
        {
            var stickManData = data as StickManData;
            _boxData.id        = stickManData.id;
            _boxData.IsHidden  = stickManData.IsHidden;
            _boxData.intFrozen = stickManData.intFrozen;
        }
        
        public override void UpdateFollowData()
        {
            _frozenTxt.gameObject.SetActive(_boxData.intFrozen > 0);
            _frozenTxt.text    = _boxData.intFrozen.ToString();
            _hiddenVisual.transform.parent.gameObject.SetActive(_boxData.IsHidden);

            if (_boxData.IsHidden)
            {
                ChangeSprite(_hiddenSprite, Color.white);
            }
            else
            {
                ChangeNormalSprite();
                ChangeColor();
            }
            
            _hiddenVisual.color = GameAssetManager.Instance.GetColor(Data.id);
        }

        public override BoxData Data               => _boxData;
        public override bool    IsAbleToChangeSwap => true;
        
        public override ALESpawnItem SpawnFromPool()
        {
            var inst = PrefabPool<LEStickManChunk>.Spawn(this);
            inst.SetUpData();
            return inst;
        }

        public override void SendToPool()
        {
            PrefabPool<LEStickManChunk>.Release(this);
        }
    }
}
