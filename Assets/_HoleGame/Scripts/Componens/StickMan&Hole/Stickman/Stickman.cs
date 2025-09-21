namespace HoleBox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using DevTools.Extensions;
    using DG.Tweening;
    using BasePuzzle.PuzzlePackages.Core;
    using PuzzleGames;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using Random = UnityEngine.Random;

    public class Stickman : MonoBehaviour
    {
        [SerializeField] private StickmanAnimation _movement;
        [SerializeField] private GameObject        hiddenObject;
        [SerializeField] private GameObject        characterObject;
        [SerializeField] private GameObject        shadow;
        [SerializeField] private GameObject        trailFX;
        [SerializeField] private float             _distanceJump     = 2;
        [SerializeField] private float             _distanceJumpHole = 3;
        [SerializeField] private float             _jumpSpeed        = 5;

        [MinMaxSlider(5, 30)] [SerializeField] private Vector2 speedRun = new Vector2(12, 14);

        private Sequence movementSequence;

        public void SetInTunnel() { _movement.SetInTunnel(); }

        public void MoveStickMan(List<Vector2Int> pathValue, BoxData hole, Action onEndMove = null, bool holeFX = false)
        {
            StickmanGroup.Instance.RemoveStickman(this);
            
            trailFX?.SetActive(true);

            movementSequence.Kill();
            movementSequence = DOTween.Sequence();

            if (holeFX)
            {
                movementSequence.AppendInterval(0.4f);
            }

            var speed    = Random.Range(speedRun.x, speedRun.y);
            var multiple = speed / speedRun.x;
            movementSequence.AppendCallback(() =>
                {
                    shadow.SetActive(false);
                    _movement.TriggerRun(multiple);
                }
            );

            var deltaOffset = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < pathValue.Count; i++)
            {
                Vector2 point          = pathValue[i] + deltaOffset;
                Vector3 targetPosition = new Vector3(point.x, transform.position.y, point.y);

                var previousPos = i == 0 ? transform.position : new Vector3(pathValue[i - 1].x + 0.5f, transform.position.y, pathValue[i - 1].y + 0.5f);

                if (hole.InsideBox(point) || Vector3.Distance(hole.GetMiddlePosition(), new Vector3(point.x, 0, point.y)) <= _distanceJumpHole)
                {
                    Vector3 holeMiddlePosition = hole.GetMiddlePosition();
                    targetPosition = holeMiddlePosition + new Vector3(Random.Range(-0.35f, 0.35f), 0, Random.Range(-0.35f, 0.35f));
                    float duration = Vector3.Distance(previousPos, targetPosition) / _jumpSpeed;

                    movementSequence.AppendCallback(() =>
                    {
                        _movement.TriggerJump();
                        AudioController.PlaySound(SoundKind.StickmanJump);
                    });
                    movementSequence.Append(transform.DOMove(targetPosition, duration).SetEase(Ease.Linear)
                        .OnUpdate(() =>
                        {
                            Vector3 direction = (targetPosition - transform.position).normalized;
                            if (direction != Vector3.zero)
                            {
                                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10);
                            }
                        }));
                    break;
                }
                else
                {
                    float duration = Vector3.Distance(previousPos, targetPosition) / speed;

                    movementSequence.Append(
                        transform.DOMove(targetPosition, duration).SetEase(Ease.Linear).OnUpdate(() =>
                        {
                            Vector3 direction = (targetPosition - transform.position).normalized;
                            if (direction != Vector3.zero)
                            {
                                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10);
                            }
                        })
                    );
                }
            }

            movementSequence.AppendCallback(() => onEndMove?.Invoke());
            movementSequence.AppendInterval(0.4f);
            movementSequence.AppendCallback(() =>
            {
                trailFX?.SetActive(false);

                if (holeFX)
                    ShowJumpFX();

                HapticController.instance.Play();
                
                gameObject.SetActive(false);
                
                if (GameManager.Instance.CurrentState == GameState.Playing)
                {
                    Release();
                }
            });

            movementSequence.SetLink(gameObject);
        }

        public void SetMaterial(MaterialStorage.MaterialEntry material)
        {
            gameObject.SetActive(true);
            // Assume the Stickman GameObject or one of its children has a Renderer component
            var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

            if (meshRenderer != null)
            {
                meshRenderer.material   = material.material; // Set the material to the renderer
                meshRenderer.sharedMesh = material.mesh;

                int layerIndex = (int)Mathf.Log(material.layerMask.value, 2);

                meshRenderer.gameObject.SetGameLayerRecursive(layerIndex);
            }
            else
            {
                Debug.LogWarning("Stickman: Renderer component not found to set the material.");
            }
        }

        private void Release()
        {
            movementSequence.Kill();
            if (gameObject.activeSelf)
                PrefabPool<Stickman>.Release(this);
        }

        public void SetHidden()
        {
            hiddenObject.gameObject.SetActive(true);
            characterObject.SetActive(false);
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


            AudioController.PlaySound(SoundKind.OnTouchHole);
            _movement.OnTap();
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

        public void ShowHidden()
        {
            hiddenObject.gameObject.SetActive(false);
            characterObject.SetActive(true);
        }
        public void Reset()
        {
            movementSequence.Kill();
            _movement.Reset();
            shadow.SetActive(true);
            hiddenObject.gameObject.SetActive(false);

            transform.rotation   = Quaternion.Euler(0, 180, 0);
            transform.localScale = Vector3.one;
        }
        public void MoveToQueue(Vector3 pos, Action action)
        {
            _movement.TriggerRun();
            bool isJump = false;

            pos.y = 0;

            transform.rotation = Quaternion.LookRotation(pos - transform.position);

            transform.DOMove(pos, 1).OnUpdate(() =>
                {
                    if (isJump || !(Vector3.Distance(transform.position, pos) <= _distanceJump)) return;

                    isJump = true;
                    _movement.TriggerJump();
                    AudioController.PlaySound(SoundKind.StickmanJump);
                    HapticController.instance.Play();
                })
                .SetEase(Ease.OutQuart).OnComplete(() =>
                {
                    action?.Invoke();
                    if (gameObject.activeSelf)
                        PrefabPool<Stickman>.Release(this);
                });
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void MoveTo(Vector3 pos)
        {
            _movement.TriggerRun();
            transform.DOMove(pos, 0.3f).OnComplete(() =>
            {
                transform.DORotate(Vector3.up * 180, 0.2f);
                _movement.Reset();
            });
        }


        public  JumpFX    jumpFX;
        public  Transform jumpTrans;
        private void      ShowJumpFX() { PrefabPool<JumpFX>.Spawn(jumpFX, jumpTrans.position, Quaternion.identity, null); }

        public void Fall() { _movement.TriggerFall(); }

        private void OnDestroy()  { movementSequence.Kill(); }
        public  void PlayWave()   { _movement.WaveAnim(); }
        public  void JumpMiddle() { _movement.TriggerJumpMiddle(); }
    }
}