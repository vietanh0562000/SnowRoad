using UnityEngine;

namespace HoleBox
{
    using System;
    using BasePuzzle.PuzzlePackages.Core;

    [DisallowMultipleComponent, SelectionBase]
    public class LEFloorDel : ALESpawnItem
    {
        private FloorData _boxData = new();

        public override BoxData Data               => _boxData;
        public override bool    IsAbleToChangeSwap => false;
        public override bool    IsActionItem       => true;
        public override bool    NeedGridCheck      => false;
        public override bool    IsPlaceable        => false;

        public override bool InnerOverlapCheck(LEGrid grid, LESpawner spawner, Vector2Int position)
        {
            var check = position.x >= 0 && position.y >= 0 &&
                        position.x < grid.NumOfCol && position.y < grid.NumOfRow && 
                        (position.x >= grid.NumOfCol - 2 || position.y >= grid.NumOfRow - 2);
            
            bool isHorizontalDel = position.y >= grid.NumOfRow - 2;
            bool isVerticalDel   = position.x >= grid.NumOfCol - 2;

            bool isEmptyRow = isHorizontalDel && spawner.IsEmptyRow(grid.NumOfRow - 2);
            bool isEmptyCol = isVerticalDel && spawner.IsEmptyCol(grid.NumOfCol - 2);
            
            var valid = check && (isEmptyRow || isEmptyCol);

            var color = valid ? new Color(0, 1, 0, 0.6f) :  new Color(1, 0, 0, 0.6f);
            ChangeColorInternal(color);

            return check;
        }

        public override void OnPlacedAction(LEGrid grid, LESpawner spawner, Vector2Int position)
        {
            bool isHorizontalDel = position.y >= grid.NumOfRow - 2;
            bool isVerticalDel   = position.x >= grid.NumOfCol - 2;
            
            bool isEmptyRow = isHorizontalDel && spawner.IsEmptyRow(grid.NumOfRow - 2);
            bool isEmptyCol = isVerticalDel && spawner.IsEmptyCol(grid.NumOfCol - 2);
            
            if (isEmptyRow || isEmptyCol)
            {
                var data = (isEmptyRow, isEmptyCol);
                GameEvent<(bool, bool)>.Emit(LEEvents.ON_DELETE_MAP_LEVEL_EDITOR, data);
            }
        }

        public override ALESpawnItem SpawnFromPool()
        {
            var inst = PrefabPool<LEFloorDel>.Spawn(this);
            inst.SetUpData();
            return inst;
        }

        public override void SendToPool()
        {
            PrefabPool<LEFloorDel>.Release(this);
        }
        
        public override void SetUpData()
        {
            _boxData.position = Vector2Int.zero;
            _boxData.size     = Vector2Int.one * 2;
            _boxData.id       = -1;
        }
        
        public override void Swap() { }
        
       
        public override void Highlight(bool value) { }
        
        public override void CopyData(BoxData data) { }
        
        public override void UpdateFollowData() { }
    }
}
