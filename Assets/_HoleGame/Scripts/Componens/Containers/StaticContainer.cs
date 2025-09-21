using System.Collections.Generic;
using UnityEngine;

namespace HoleBox
{
    using System;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;

    public class StaticContainer : AContainer
    {
        [SerializeField] private Vector3       gridStartPoint = Vector3.zero; // Điểm bắt đầu của lưới
        [SerializeField] private Vector2       gridSize       = new Vector2(1f, 1f); // Khoảng cách giữa các điểm
        private                  List<Vector3> positions      = new List<Vector3>(); // Danh sách vị trí lưới được sinh ra

        [Header("Gizmos Settings")] // Cài đặt cho Gizmos
        [SerializeField]
        private Color gizmoColor = Color.green; // Màu của Gizmos

        [SerializeField] private float          gizmoSize = 0.2f; // Kích thước của các vị trí Gizmos
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private List<Stickman> _stickmans = new();

        private int cachedID;

        // Sinh ra các vị trí trong lưới
        private void Start() { GeneratePositions(); }

        public override void SetData(ContainerData data)
        {
            base.SetData(data);
            data.OnReset = OnReset;
        }

        private void OnReset()
        {
            MovementThread.Instance.AddAction(this, async () =>
            {
                ReleaseStickman();
                SetVisual();
            });
        }

        /// <summary>
        /// Tạo lưới 4x8 và lưu vào 'positions'.
        /// </summary>
        private void GeneratePositions()
        {
            positions.Clear();
            int rows = 4, cols = 2; // Lưới 8 hàng, 4 cột

            for (int row = rows; row > 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    Vector3 pos = transform.position + gridStartPoint + new Vector3(col * gridSize.x, 0, row * gridSize.y);
                    positions.Add(pos);
                }
            }
        }

        /// <summary>
        /// Trả về vị trí tiếp theo dựa vào Data.Number.
        /// </summary>
        public override Vector3 StickmanPos
        {
            get
            {
                if (positions.Count == 0)
                {
                    Debug.LogWarning("StaticContainer: No positions generated.");
                    return Vector3.zero;
                }

                if (Data.FakeNumber <= 0)
                {
                    return positions[0];
                }

                int targetIndex = Mathf.Clamp(Data.FakeNumber, 0, positions.Count - 1);
                return positions[targetIndex];
            }
        }

        /// <summary>
        /// Vẽ Gizmos trực quan các vị trí trong lưới.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;

            // Vẽ lưới nếu đã sinh ra danh sách vị trí
            if (positions != null && positions.Count > 0)
            {
                foreach (var pos in positions)
                {
                    Gizmos.DrawSphere(pos, gizmoSize); // Vẽ một sphere nhỏ tại mỗi vị trí
                }
            }
            else
            {
                // Nếu chưa sinh lưới trong Editor, vẽ lưới cơ bản (Static).
                int     rows  = 4, cols = 2;
                Vector3 start = gridStartPoint;

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        Vector3 pos = transform.position + start + new Vector3(col * gridSize.x, 0, row * gridSize.y);
                        Gizmos.DrawSphere(pos, gizmoSize);
                    }
                }
            }
        }

        protected override void SetVisual()
        {
            base.SetVisual();
            GeneratePositions();
            var idColor = GameAssetManager.Instance.GetColor(Data.ID);
            _remainTMP.color      = idColor;
            _spriteRenderer.color = idColor;
        }

        protected override void OnEmptyStack() { }
        protected override void OnMinus(int count, ContainerData containerData)
        {
            var targetContainer = MapContainer.Instance.FindQueueContainer(containerData);

            if (targetContainer == null)
            {
                Debug.LogWarning("Target container not found.");
                return;
            }

            MovementThread.Instance.AddAction(this, async () =>
            {
                if (_stickmans == null || _stickmans.Count == 0)
                {
                    Debug.LogWarning("No stickmans to process.");
                    return;
                }

                if (StickmanTransporter.Instance.HasUFOInContainer(this, out var ufo))
                {
                    await UniTask.WaitUntil(() => !ufo.IsProcessingQueue).Timeout(TimeSpan.FromSeconds(10))
                        .SuppressCancellationThrow();;
                }

                // Duyệt danh sách _stickmans từ cuối lên
                var check = 0;
                for (int i = 0; i < count; i++)
                {
                    // Lấy stickman tại vị trí i
                    var stickman = _stickmans[^1];

                    _stickmans.Remove(stickman);

                    Data.MinusFakeNumber();
                    _remainTMP.SetText($"{Data.FakeRemaining}");

                    check++;
                    stickman.MoveToQueue(targetContainer.StickmanPos, () => { targetContainer.AddStickman(); });
                    await UniTask.Yield();
                }

                _remainTMP.SetText($"{Data.Remaining}");
                SetVisual();
            });
        }
        protected override void OnChangeID()
        {
            cachedID = Data.ID;
            SetVisual();
        }
        protected override void OnFullStack() { }
        protected override void OnUpdateQuantity(int count, bool useUFO = true)
        {
            MovementThread.Instance.AddAction(this, async () =>
            {
                var addStickmans = StickmanTransporter.Instance.SpawnStickman(new IngressData(cachedID, count));

                _stickmans.AddRange(addStickmans);

                StickmanTransporter.Instance.CallUFODeliverStickman(addStickmans, cachedID,
                    this, onStep: () =>
                    {
                        if (Data.FakeNumber < Data.Number)
                        {
                            Data.AddFakeNumber();
                            _remainTMP.SetText($"{Data.FakeRemaining}");
                        }
                    }, false);

                await UniTask.Delay(addStickmans.Count * UfoTransporter.DelaySpawn);
            });
        }
        public void ReleaseStickman()
        {
            _stickmans.ReleaseStickman();
            _stickmans = new List<Stickman>();
        }
    }
}