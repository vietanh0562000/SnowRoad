using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HoleBox
{
    using System.Threading;
    using BasePuzzle.PuzzlePackages.Core;
    using PuzzleGames;
    using Sirenix.OdinInspector;
    using Random = UnityEngine.Random;

    public class StickmanTransporter : Singleton<StickmanTransporter>
    {
        [SerializeField] private UfoTransporter ufoPrefab; // Prefab UFO sẽ được tạo bằng PrefabPool
        [SerializeField] private Transform      ufoStartPoint; // Vị trí bắt đầu chung của UFO
        [SerializeField] private Transform      dieTrans;
        [SerializeField] private Transform[]    randomPoints;

        public Vector3 GetRandomPoint => randomPoints[Random.Range(0, randomPoints.Length)].position;

        private void Start()
        {
            // Tạo Pool cho UFO
            PrefabPool<UfoTransporter>.Create(ufoPrefab, 5, 10, true, null); // 5 UAV ban đầu, tối đa 10

            GameManager.OnGameRevive += OnReviveGame;
        }

        protected override void OnDestroy()
        {
            GameManager.OnGameRevive -= OnReviveGame;
            base.OnDestroy();
        }

        private void OnReviveGame() { ReleaseStickman(); }

        public void UpdateStartPoint(Vector3 point) => ufoStartPoint.position = point;

        public void CallUfoDeliverStickman(IngressData ingressData, AContainer target, Action onStep = null, bool releaseStickman = true)
        {
            var stickmans = SpawnStickman(ingressData);
            CallUFODeliverStickman(stickmans, ingressData.ID, target, onStep, releaseStickman);
        }

        private Dictionary<AContainer, UfoTransporter> dict = new();

        public void CallUFODeliverStickman(List<Stickman> stickmans, int id, AContainer target, Action onStep = null, bool releaseStickman = true)
        {
            var            find = dict.ContainsKey(target);
            UfoTransporter ufoTransporter;

            if (find)
            {
                var ufo = dict[target];

                if (ufo == null || !ufo.gameObject.activeSelf ||
                    !ufo.IsProcessingQueue || ufo.ID != id)
                {
                    var newUfo = PrefabPool<UfoTransporter>.Spawn();
                    dict[target]   = newUfo;
                    ufoTransporter = newUfo;
                }
                else
                {
                    ufoTransporter = ufo;
                }
            }
            else
            {
                var newUfo = PrefabPool<UfoTransporter>.Spawn();
                dict.TryAdd(target, newUfo);
                ufoTransporter = newUfo;
            }

            ufoTransporter.transform.position   = ufoStartPoint.position;
            ufoTransporter.transform.localScale = Vector3.one;
            ufoTransporter.SetID(id);

            // Gọi UFO để thực hiện nhiệm vụ
            _ = ufoTransporter.DeliverStickmen(stickmans, target, onStep, releaseStickman: releaseStickman);
        }

        public bool HasUFOInContainer(AContainer target, out UfoTransporter ufoTransporter)
        {
            var find = dict.ContainsKey(target);

            ufoTransporter = null;

            if (find)
            {
                var ufo = dict[target];

                if (ufo == null || !ufo.gameObject.activeSelf ||
                    !ufo.IsProcessingQueue)
                {
                    dict.Remove(target);
                    return false;
                }

                ufoTransporter = ufo;
                return true;
            }

            return false;
        }

        public async UniTask CallHelicopterDeliverStickman(UfoTransporter ufoTransporter, IngressData ingressData, AContainer target
            , Action onStep = null, bool releaseStickman = true)
        {
            var stickmans = SpawnStickman(ingressData);
            ufoTransporter.transform.localScale = Vector3.one;

            // Gọi UFO để thực hiện nhiệm vụ
            await ufoTransporter.DeliverStickmen(stickmans, target, onStep, releaseStickman, true);
        }

        [Button]
        public void DropStickman(int id, int number) { _ = CallUFODieStickman(new IngressData(id, number)); }

        public async UniTask CallUFODieStickman(IngressData ingressData, Action onStep = null)
        {
            var stickmans = SpawnStickman(ingressData, true);

            // Nếu không có UFO nào, tạo mới một UFO và gán nó cho target
            var ufoTransporter = PrefabPool<UfoTransporter>.Spawn();

            ufoTransporter.transform.position   = ufoStartPoint.position;
            ufoTransporter.transform.localScale = Vector3.one;

            // Gọi UFO để thực hiện nhiệm vụ
            await ufoTransporter.DieStickmen(stickmans, () => { onStep?.Invoke(); });
            ufoTransporter.Release();
        }

        public async UniTask CallUFOToPickup(Vector3 pickupPosition, Action<UfoTransporter> onPickup = null, Action<UfoTransporter> onComplete = null)
        {
            var availableUfo = PrefabPool<UfoTransporter>.Spawn(ufoPrefab);

            // Đặt vị trí của UFO
            availableUfo.transform.SetParent(transform);
            availableUfo.transform.position   = ufoStartPoint.position;
            availableUfo.transform.localScale = Vector3.one;

            // Dọn stickman
            await availableUfo.PickupStickmen(pickupPosition, onPickup, onComplete);
        }

        public List<Stickman> SpawnStickman(IngressData ingressData, bool isDie = false)
        {
            var result = new List<Stickman>();

            try
            {
                var entry = GameAssetManager.Instance.GetMaterialEntryById(ingressData.ID);

                for (int j = 0; j < ingressData.Number; j++)
                {
                    var moveStickman = PrefabPool<Stickman>.Spawn();
                    moveStickman.transform.SetParent(isDie ? dieTrans : transform);
                    moveStickman.transform.localPosition = Vector3.zero;
                    moveStickman.transform.rotation      = Quaternion.Euler(new Vector3(0, 180, 0));
                    moveStickman.transform.localScale    = Vector3.zero;
                    moveStickman.SetMaterial(entry);
                    result.Add(moveStickman);
                }

                return result;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

            return result;
        }

        public void ReleaseStickman()
        {
            for (int i = dieTrans.childCount - 1; i >= 0; i--)
            {
                var child = dieTrans.GetChild(i).gameObject;

                var t = child.GetComponent<Stickman>();
                if (t && t.gameObject.activeSelf)
                {
                    PrefabPool<Stickman>.Release(t);
                }
            }
        }
    }
}