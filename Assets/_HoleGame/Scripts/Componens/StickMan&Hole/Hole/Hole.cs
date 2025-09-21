namespace HoleBox
{
    using System;
    using UnityEngine;

    namespace HoleBox
    {
        using System.Collections.Generic;
        using com.ootii.Messages;
        using DG.Tweening;
        using PuzzleGames;
        using Lofelt.NiceVibrations;
        using TMPro;
        using UnityEngine.EventSystems;
        using HapticController = HapticController;

        public class Hole : BoxDataSetter<HoleBoxData>
        {
            [SerializeField] private Renderer[]  renderer;
            [SerializeField] private TextMeshPro textDisplay;
            [SerializeField] private LockHole    _lockHole;
            [SerializeField] private GameObject  _closedHole;
            [SerializeField] private GameObject  _mask;
            [SerializeField] private Transform   _punchHole;

            public ParticleSystem fxJump;
            public ParticleSystem fxRainbow;
            public ParticleSystem fxClickHole;

            public int id;

            public int MaxCountInHole => IsClosedHole ? Data.numberToClose : GameAlgorithm.CountInHole;

            private Action<Hole> onClick;

            public override void SetData(HoleBoxData boxData)
            {
                UpdateVisual();

                if (Data.lockedHole)
                {
                    var hole = TemporaryBoardVisualize.Instance.GetHole(Data.keyPos);
                    if (hole != null)
                    {
                        SetKeyHole(hole);
                    }
                }

                SetMaterial(GameAssetManager.Instance.GetMaterialHole(Data.id));
            }
            private void SetKeyHole(HoleBoxData hole)
            {
                hole.SetKey(() =>
                {
                    Data.SetUnlocked();
                    _lockHole.Unlock();
                });
                _lockHole.SetKeyHole(hole, Data.id);
            }

            public void SetMaterial(Material material)
            {
                gameObject.SetActive(true);

                foreach (var r in renderer)
                {
                    r.material = material; // Set the material to the renderer
                }
            }

            // Called when the user clicks on the object
            private void OnMouseDown()
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;

#if !UNITY_EDITOR
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (IsTouchOverUI(touch))
                    {
                        return;
                    }
                }
#endif

                if ((GameManager.Instance.CurrentState is not (GameState.Playing or GameState.Prepare)) || !Data.IsAvailable) return;

                if (TemporaryBoardVisualize.Instance.UseUfo)
                {
                    return;
                }
                
                GameManager.Instance.SetPlayed();
                Punch();
                fxClickHole.Play();
                // Trigger the onClick action if it is set
                onClick?.Invoke(this);
                HapticController.instance.Play(HapticPatterns.PresetType.LightImpact);
            }

            private bool IsTouchOverUI(Touch touch)
            {
                // Khởi tạo dữ liệu raycast
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = touch.position // Vị trí cảm ứng
                };

                // Tạo danh sách kết quả raycast
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                // Debug thông tin về UI bị trúng
                foreach (var result in results)
                {
                    Debug.Log("UI Hit: " + result.gameObject.name);
                }

                // Trả về true nếu có bất kỳ UI nào bị trúng
                return results.Count > 0;
            }


            public bool IsClosedHole => Data.closedHole;
            public void GetStickMan()
            {
                Punch();
                AudioController.PlaySound(SoundKind.StickmanInHole);
                GameManager.Instance.OnStickmanMoveHole?.Invoke();
                fxJump.Play();
                Data.UnlockKey();

                if (IsClosedHole)
                {
                    Data.OnStickEndMove();
                    UpdateVisual();
                }
            }

            private bool isClosed;

            private void UpdateVisual()
            {
                _closedHole.SetActive(IsClosedHole);

                if (IsClosedHole)
                {
                    if (Data.numberToClose > 0)
                    {
                        textDisplay.SetText(Data.numberToClose.ToString());
                    }
                    else
                    {
                        if (!isClosed)
                        {
                            TileSpawner.Instance.SpawnTile(Data);
                            AudioController.PlaySound(SoundKind.HoleClosed);
                            transform.DOKill();
                            textDisplay.SetText(String.Empty);
                            transform.DOScale(0, 0.6f);
                            isClosed = true;
                        }
                    }
                }
                else
                {
                    transform.localScale = Vector3.one;
                    textDisplay.SetText(String.Empty);
                }

                _lockHole.gameObject.SetActive(Data.lockedHole);
            }

            public void OnClick(Action<Hole> process)
            {
                onClick = null;
                onClick = process;
            }

            public void Punch()
            {
                if (DOTween.IsTweening(_punchHole))
                {
                    return;
                }

                _punchHole.DOKill();
                _punchHole.localScale = Vector3.one;
                _punchHole.DOPunchScale(Vector3.one * 0.05f, 0.2f, 1, 0.5f).SetEase(Ease.OutQuad);
            }
            public void UsingRainbow() { fxRainbow.Play(); }

            public void SetMask(bool active) { _mask.SetActive(active); }

            void Awake()
            {
                MessageDispatcher.AddListener(EventID.DEACTIVE_RAINBOW, UseBooster, true);
                MessageDispatcher.AddListener(EventID.ACTIVE_RAINBOW, ActiveBooster, true);
            }

            void OnDestroy()
            {
                MessageDispatcher.RemoveListener(EventID.DEACTIVE_RAINBOW, UseBooster, true);
                MessageDispatcher.RemoveListener(EventID.ACTIVE_RAINBOW, ActiveBooster, true);
            }
            private void ActiveBooster(IMessage rMessage) { SetMask(false); }
            private void UseBooster(IMessage rMessage)    { SetMask(true); }
        }
    }
}