namespace HoleBox
{
    using System;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using UnityEngine;
    using Random = UnityEngine.Random;

    public class QueueContainer : AContainer
    {
        [SerializeField] private Renderer[] renderer;
        [SerializeField] private float      range;

        private int _column;

        public int Column => _column;

        public void SetColumn(int col) { _column = col; }

        public override void SetData(ContainerData data)
        {
            base.SetData(data);
            data.OnUfoUpdateQuantity = OnUFOAddQuantity;
        }

        private void OnUFOAddQuantity(int number, UfoTransporter ufo)
        {
            MovementThread.Instance.AddAction(this, async () =>
            {
                await StickmanTransporter.Instance.CallHelicopterDeliverStickman(ufo, new IngressData(Data.id, number),
                    this, onStep: () =>
                    {
                        AudioController.PlaySound(SoundKind.StickmanInHole);
                        Punch();
                        Data.AddFakeNumber();
                        _remainTMP.SetText($"{Data.FakeRemaining}");
                    });
            }, true);
        }

        protected override void SetVisual()
        {
            base.SetVisual();
            SetMaterial(GameAssetManager.Instance.GetMaterialHole(Data.id));
        }

        public void SetMaterial(Material material)
        {
            gameObject.SetActive(true);

            foreach (var r in renderer)
            {
                r.material = material; // Set the material to the renderer
            }
        }

        protected override void OnMinus(int count, ContainerData containerData) { }
        protected override void OnChangeID()                                    { }
        protected override void OnEmptyStack()                                  { }
        protected override void OnFullStack()
        {
            MovementThread.Instance.AddAction(this, async () =>
            {
                await UniTask.WaitUntil(() => Data.FakeRemaining == 0)
                    .Timeout(TimeSpan.FromSeconds(10))
                    .SuppressCancellationThrow(); 
                //AudioController.PlaySound(SoundKind.HoleClosed);
                ComboManager.Instance.IncreaseCombo();
                transform.DOKill();
                await transform.DOScale(0, 0.15f).AsyncWaitForCompletion();
                MapContainer.Instance.UpdateQueue(this);
            }, true);
        }

        protected override void OnUpdateQuantity(int count, bool useUFO = true)
        {
            if (!useUFO)
            {
                _remainTMP.SetText($"{Data.FakeRemaining}");
            }

            MovementThread.Instance.AddAction(this, async () =>
            {
                if (useUFO)
                {
                    StickmanTransporter.Instance.CallUfoDeliverStickman(new IngressData(Data.id, count),
                        this, onStep: () =>
                        {
                            AudioController.PlaySound(SoundKind.StickmanInHole);
                            Punch();
                            Data.AddFakeNumber();
                            _remainTMP.SetText($"{Data.FakeRemaining}");
                        });
                }
            }, true);
        }

        public override Vector3 StickmanPos => transform.position
                                               + new Vector3(Random.Range(-range, range), -3.5f, Random.Range(0, range));

        public void MoveDown(float range, float duration = 0.3f)
        {
            if (DOTween.IsTweening(transform)) return; 
            Vector3 targetPosition = transform.position + Vector3.back * range;
            transform.DOMove(targetPosition, duration);
        }

        public void AddStickman()
        {
            AudioController.PlaySound(SoundKind.StickmanInHole);
            Data.AddFakeNumber();
            Punch();
            _remainTMP.SetText($"{Data.FakeRemaining}");

            if (!Data.IsBusy && Data.Remaining == 0)
            {
                OnFullStack();
            }
        }

        [SerializeField] private Transform _punchHole;
        public void Punch()
        {
            HapticController.instance.Play();
            
            if (DOTween.IsTweening(_punchHole))
            {
                return;
            }

            _punchHole.DOKill();
            _punchHole.localScale = Vector3.one;
            _punchHole.DOPunchScale(Vector3.one * 0.02f, 0.1f, 1, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}