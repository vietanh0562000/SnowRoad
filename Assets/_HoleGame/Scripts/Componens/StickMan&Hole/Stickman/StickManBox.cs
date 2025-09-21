namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using BasePuzzle.PuzzlePackages.Core;
    using PuzzleGames;
    using UnityEngine;
    using Random = Unity.Mathematics.Random;

    public class StickManBox : BoxDataSetter<StickManData>
    {
        [SerializeField] private FrozenBox _frozenBox;

        private List<Stickman> stickmans;

        private bool isMoving;
        public  bool IsMoving => isMoving;

        public override void SetData(StickManData boxData)
        {
            Data.OnUpdateData -= OnClickHole;
            Data.OnUpdateData -= OnClickHole;
            Data.OnUpdateData += OnClickHole;

            Data.OnClaim -= OnUFOClaim;
            Data.OnClaim -= OnUFOClaim;
            Data.OnClaim += OnUFOClaim;

            Data.ShowFX = ShowFX;

            if (Data.IsFrozen)
            {
                GameManager.Instance.OnStickmanMoveHole -= OnStickmanMoveHole;
                GameManager.Instance.OnStickmanMoveHole -= OnStickmanMoveHole;
                GameManager.Instance.OnStickmanMoveHole += OnStickmanMoveHole;
            }

            ReleaseStickman();

            stickmans = new List<Stickman>();


            var moveStickMan = PrefabPool<Stickman>.Spawn();

            moveStickMan.transform.SetParent(transform);
            moveStickMan.transform.localPosition = Vector3.zero;

            moveStickMan.Reset();

            stickmans.Add(moveStickMan);

            if (Data.intFrozen <= 0)
            {
                StickmanGroup.Instance.AddStickman(moveStickMan);
            }
            
            Data.OnReset();

            _frozenBox.SetFrozen(Data.intFrozen);

            if (Data.IsHidden)
            {
                OnHidden();
            }
            else
            {
                SetMaterial(GameAssetManager.Instance.GetMaterialEntryById(boxData.id));
            }
        }

        private void OnClickHole()
        {
            if (!Data.IsHidden) return;
            if (!TemporaryBoardVisualize.Instance.IsShowHiddenBox(Data)) return;
            if (Data.IsClaimed) return;
            Data.ShowHidden();
            ShowHidden();
        }

        private async void OnUFOClaim(UfoTransporter ufo)
        {
            try
            {
                foreach (var stickman in stickmans)
                {
                    stickman.transform.DOMove(ufo.spawnPoint.position,
                        0.5f).SetUpdate(true);

                    stickman.transform.DOScale(Vector3.zero, 0.5f).SetUpdate(true);
                    await UniTask.Delay(50);
                }

                ReleaseStickman();
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        private void OnStickmanMoveHole()
        {
            if (Data.IsFrozen)
            {
                _frozenBox.Melting();
            }

            Data.OnStickmanMoveHole();
        }

        private void OnHidden()
        {
            foreach (var stickman in stickmans)
            {
                stickman.SetHidden();
            }

            SetMaterial(GameAssetManager.Instance.GetMaterialEntryById(-1));
        }

        private void ShowHidden()
        {
            foreach (var stickman in stickmans)
            {
                stickman.ShowHidden();
            }

            SetMaterial(GameAssetManager.Instance.GetMaterialEntryById(Data.id));
        }

        private void SetMaterial(MaterialStorage.MaterialEntry material)
        {
            foreach (var stickman in stickmans)
            {
                stickman.SetMaterial(material);
            }
        }

        public void MoveStickMan(List<Vector2Int> pathValue, BoxData hole, Action onEndMove, Action firstEndAction = null, bool holeFX = false)
        {
            isMoving = true;
            var count = stickmans.Count;
            for (int i = 0; i < count; i++)
            {
                var index = i;
                stickmans[i].MoveStickMan(pathValue, hole, () =>
                {
                    if (index == count - 1)
                    {
                        isMoving = false;
                    }

                    onEndMove?.Invoke();

                    if (index == 0)
                    {
                        firstEndAction?.Invoke();
                    }
                }, holeFX);
            }

            stickmans = new List<Stickman>();
        }
        private void ReleaseStickman() { stickmans.ReleaseStickman(); }

        
        public void MoveTo(Vector3 endPos)
        {
            foreach (var stickman in stickmans)
            {
                stickman.MoveTo(endPos);
            }
        }
        
        public void RotateTunnel(Vector3 rotate)
        {
            foreach (var stickman in stickmans)
            {
                stickman.transform.localRotation = Quaternion.Euler(rotate);
                stickman.SetInTunnel();
            }
        }

        public AutoDespawnFX rainbowFX;

        public void ShowFX() { PrefabPool<AutoDespawnFX>.Spawn(rainbowFX, transform.position, Quaternion.identity, transform); }
    }
}