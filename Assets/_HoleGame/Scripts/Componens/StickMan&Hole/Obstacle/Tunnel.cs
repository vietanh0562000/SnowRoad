namespace HoleBox
{
    using System.Collections;
    using DG.Tweening;
    using BasePuzzle;
    using TMPro;
    using UnityEngine;

    public class Tunnel : BoxDataSetter<TunnelData>
    {
        private readonly int NOfChunk = 1;

        [SerializeField] private TextMeshPro leftTMP;

        [SerializeField] private Transform rotationObject;

        public int id = -1;

        StickManBox currentStickMan;
        
        public Vector3 Rotation => rotationObject.localRotation.eulerAngles - Vector3.up * 90;

        public override void SetData(TunnelData boxData)
        {
            Data.OnUpdateData -= OnClickHole;
            Data.OnUpdateData -= OnClickHole;
            Data.OnUpdateData += OnClickHole;

            id = boxData.id;

            switch (boxData.direction)
            {
                case var right when right == Vector2Int.right:
                    rotationObject.localRotation = Quaternion.Euler(0, 180, 0);
                    break;
                case var left when left == Vector2Int.left:
                    rotationObject.localRotation = Quaternion.Euler(0, 0, 0);
                    break;
                case var up when up == Vector2Int.up:
                    rotationObject.localRotation = Quaternion.Euler(0, 90, 0);
                    break;
                case var down when down == Vector2Int.down:
                    rotationObject.localRotation = Quaternion.Euler(0, -90, 0);
                    break;
                default:
                    Debug.LogError($"Unhandled direction: {boxData.direction}");
                    break;
            }

            leftTMP.gameObject.SetActive(true);
            leftTMP.SetText($"{boxData.Remain * NOfChunk}");

            var stickman = Data.GetCurrentStickman();

            if (stickman != null)
            {
                currentStickMan = TemporaryBoardVisualize.Instance.SpawnStickManBox(stickman, this);
            }
        }
        private void OnClickHole()
        {
            if (!TemporaryBoardVisualize.Instance.IsEmptyWithDirection(Data)) return;

            if (currentStickMan != null)
            {
                HapticController.instance.Play();
                TemporaryBoardVisualize.Instance.SpawnBox(currentStickMan);
                Data.SpawnStickman();
                currentStickMan = null;

                var after = Data.Remain * NOfChunk;

                leftTMP.DOTextInt(after + NOfChunk, after, 0.3f).OnComplete(() =>
                {
                    if (Data.Remain == 0)
                    {
                        leftTMP.gameObject.SetActive(false);
                    }
                });
            }

            var stickman = Data.GetCurrentStickman();

            if (stickman != null)
            {
                currentStickMan = TemporaryBoardVisualize.Instance.SpawnStickManBox(stickman, this);
                AudioController.PlaySound(SoundKind.TunnelSpawn);
            }
        }
    }
}