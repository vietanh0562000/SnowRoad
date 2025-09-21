using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace HoleBox
{
    using System.Collections;
    using System.Threading.Tasks;
    using BasePuzzle.PuzzlePackages.Core;
    using Lofelt.NiceVibrations;
    using HapticController = HapticController;
    using Random = UnityEngine.Random;

    public class UfoTransporter : MonoBehaviour
    {
        [SerializeField] private float          durationMove     = 0.8f;
        [SerializeField] private float          distanceToRotate = 3;
        [SerializeField] private ParticleSystem ufoFx;
        [SerializeField] private AnimationCurve _curveMove;
        [SerializeField] private AnimationCurve _curveBack;

        public static int DelaySpawn = 50;

        public Transform spawnPoint;

        private int id;
        public  int ID => id;

        private Queue<(List<Stickman>, Action, bool)> _stickmenQueue     = new(); // Hàng đợi stickmen
        private bool                                  _isProcessingQueue = false; // Cờ để theo dõi trạng thái xử lý hàng đợi
        private AContainer                            _currentTarget; // Target hiện tại của UFO

        public bool IsAvailable => !_isProcessingQueue && _stickmenQueue.Count == 0 && _currentTarget == null;

        public bool IsProcessingQueue => _isProcessingQueue;

        public void SetID(int ID) { id = ID; }

        /// <summary>
        /// Di chuyển tới mục tiêu thả stickmen. *Không dừng tác vụ di chuyển.*
        /// </summary>
        private async UniTask MoveTo(Vector3 targetPosition)
        {
            transform.localScale            = Vector3.one;
            spawnPoint.transform.localScale = Vector3.one;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
            bool isRotate = false;
            transform.DOKill();
            // Di chuyển mà không sử dụng CancellationToken để không bị hủy
            await transform.DOMove(targetPosition, durationMove).OnUpdate(() =>
                {
                    if (!isRotate && Vector3.Distance(targetPosition, transform.position) <= distanceToRotate)
                    {
                        isRotate = true;
                        transform.DORotate(Vector3.zero, 2.5f).SetEase(Ease.InOutQuart);
                    }
                })
                .SetEase(_curveMove)
                .AsyncWaitForCompletion();
        }

        /// <summary>
        /// Gửi stickmen tới AContainer (vị trí đích).
        /// </summary>
        public async UniTask<bool> DeliverStickmen(List<Stickman> stickmen, AContainer dropPosition
            , Action onStep = null, bool releaseStickman = true, bool move = false)
        {
            _currentTarget = dropPosition;

            spawnPoint.transform.localScale = Vector3.one;

            transform.localScale = Vector3.one;
            if (move)
            {
                transform.DOKill();
                await transform.DOMove(dropPosition.transform.position, durationMove)
                    .SetEase(_curveBack).OnUpdate(() =>
                    {
                        Vector3 direction = (dropPosition.transform.position - transform.position).normalized;
                        if (direction != Vector3.zero)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6);
                        }
                    })
                    .AsyncWaitForCompletion();
            }
            else
            {
                ResetRotation();
                transform.position = dropPosition.transform.position;
            }

            // Thêm stickmen vào hàng đợi
            _stickmenQueue.Enqueue((stickmen, onStep, releaseStickman));
            Debug.Log("DeliverStickmen: Stickmen added to queue.");

            // Nếu hàng đợi chưa được xử lý, bắt đầu xử lý
            if (!_isProcessingQueue)
            {
                await ProcessQueue();
            }

            return true;
        }

        private void ResetRotation() { transform.rotation = Quaternion.Euler(Vector3.up * 160); }

        // Xử lý hàng đợi
        private async UniTask ProcessQueue()
        {
            try
            {
                if (_isProcessingQueue) return; // Chỉ xử lý nếu cờ _isProcessingQueue == false
                _isProcessingQueue = true;

                Debug.Log("Processing queue started.");
                while (_stickmenQueue.Count > 0)
                {
                    var dequeue = _stickmenQueue.Dequeue(); // Lấy nhóm stickmen đầu trong hàng đợi
                    ufoFx.Play();
                    // Giả lập xử lý stickmen
                    foreach (var stickman in dequeue.Item1)
                    {
                        stickman.transform.SetParent(_currentTarget.transform);
                        stickman.transform.position = spawnPoint.position;

                        if (_currentTarget is QueueContainer)
                        {
                            stickman.JumpMiddle();
                            stickman.transform.DOMove(_currentTarget.StickmanPos, 0.5f).OnComplete(() =>
                            {
                                if (dequeue.Item3 && stickman && stickman.gameObject.activeSelf)
                                {
                                    PrefabPool<Stickman>.Release(stickman);
                                }
                            });

                            stickman.transform.DOScale(Vector3.one * 0.82f, 0f).From(Vector3.zero);
                        }
                        else
                        {
                            stickman.transform.position = _currentTarget.StickmanPos;
                            stickman.Fall();

                            if (dequeue.Item3 && stickman && stickman.gameObject.activeSelf)
                            {
                                PrefabPool<Stickman>.Release(stickman);
                            }

                            stickman.transform.DOScale(Vector3.one * 0.82f, 0).From(Vector3.zero);
                        }

                        dequeue.Item2?.Invoke();

                        await UniTask.Delay(DelaySpawn); // Delay giữa mỗi stickman
                    }

                    if (_stickmenQueue.Count == 0)
                    {
                        await UniTask.WaitUntil(() => !TemporaryBoardVisualize.Instance.ExistStickmanMoving(id))
                            .Timeout(TimeSpan.FromSeconds(10))
                            .SuppressCancellationThrow();
                        ;
                        await UniTask.Delay(100);
                    }

                    Debug.Log("Finished processing current group of stickmen.");
                }

                Debug.Log("Queue is empty. Releasing UFO.");
                _isProcessingQueue = false;

                TemporaryBoardVisualize.Instance.ValidateContainer();

                Release();
            }
            catch (Exception e)
            {
                // ignored
            }
        }


        /// <summary>
        /// Gửi stickmen tới AContainer (vị trí đích).
        /// </summary>
        public async UniTask DieStickmen(List<Stickman> stickmen, Action onStep = null)
        {
            if (!IsAvailable) return;

            try
            {
                ResetRotation();
                transform.localScale            = Vector3.one;
                spawnPoint.transform.localScale = Vector3.one;

                var count = 0;
                ufoFx.Play();
                foreach (var stickman in stickmen)
                {
                    stickman.transform.position = spawnPoint.position;
                    stickman.transform.rotation = Quaternion.Euler(Random.Range(-90f, 90f), 180, Random.Range(-90f, 90f));

                    stickman.transform.DOMove(transform.position
                                              + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)),
                        0.3f).SetUpdate(true).OnComplete(() => count++);

                    stickman.transform.DOScale(Vector3.one * 0.9f, 0.3f).SetUpdate(true).From(Vector3.zero);
                    await UniTask.Delay(40);
                    onStep?.Invoke();
                   
                    HapticController.instance.Play(HapticPatterns.PresetType.LightImpact);
                }

                await UniTask.WaitUntil(() => count == stickmen.Count)
                    .Timeout(TimeSpan.FromSeconds(10))
                    .SuppressCancellationThrow();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Nhiệm vụ DeliverStickmen bị hủy.");
            }
        }

        /// <summary>
        /// Gom stickmen vào UFO.
        /// </summary>
        public async UniTask PickupStickmen(Vector3 pickupPosition, Action<UfoTransporter> onPickedUp = null, Action<UfoTransporter> onComplete = null)
        {
            if (!IsAvailable) return;
            // Di chuyển tới vị trí gom stickmen mà không bị hủy
            await MoveTo(pickupPosition);
            await UniTask.Delay(300);
            onPickedUp?.Invoke(this);

            ufoFx.Play();
            await UniTask.Delay(1500);
            onComplete?.Invoke(this);
        }

        /// <summary>
        /// Trả UFO về pool.
        /// </summary>
        public async void Release()
        {
            try
            {
                transform.DOKill();

                var     targetPosition = StickmanTransporter.Instance.GetRandomPoint;
                Vector3 direction      = (targetPosition - transform.position).normalized;
                transform.DORotate(Quaternion.LookRotation(direction).eulerAngles, 1.3f).SetEase(Ease.InOutQuart);
                await UniTask.Delay(1000);
                transform.DOKill();
                await transform.DOMove(targetPosition, 2)
                    .SetEase(_curveMove).OnUpdate(() =>
                    {
                        if (direction != Vector3.zero)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6);
                        }
                    })
                    .AsyncWaitForCompletion();

                _currentTarget     = null;
                _isProcessingQueue = false;
                PrefabPool<UfoTransporter>.Release(this);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}