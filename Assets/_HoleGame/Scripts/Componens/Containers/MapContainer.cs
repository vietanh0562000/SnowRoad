namespace HoleBox
{
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using BasePuzzle.PuzzlePackages.Core;
    using PuzzleGames;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public class MapContainer : Singleton<MapContainer>
    {
        [FoldoutGroup("Visualize"), SerializeField]
        private StaticContainer staticContainerPrefab;

        [FoldoutGroup("Visualize"), SerializeField]
        private QueueContainer queueContainerPrefab;

        [FoldoutGroup("Visualize"), SerializeField]
        private float _spawnRange = 2.5f;

        [FoldoutGroup("Visualize"), SerializeField]
        private float _spawnStaticRange = 2f;

        private List<ContainerQueueData> _containerQueues;
        private ContainerQueueData       _staticContainer;
        private ContainerLogic           _containerLogic;

        [SerializeField, ReadOnly] private List<StaticContainer> _staticContainers       = new List<StaticContainer>();
        [SerializeField, ReadOnly] private List<QueueContainer>  _queueContainers        = new List<QueueContainer>();
        [SerializeField, ReadOnly] private List<QueueContainer>  _currentQueueContainers = new List<QueueContainer>();

        private List<IngressData> listIngressData;
        private int               slotCountOnRevive = 2;
        
        private void Awake()
        {
            PrefabPool<StaticContainer>.Create(staticContainerPrefab, 5, 10, true);
            PrefabPool<QueueContainer>.Create(queueContainerPrefab, 5, 20, true);

            GameManager.OnGameRevive += OnRevive;
        }

        protected override void OnDestroy()
        {
            GameManager.OnGameRevive -= OnRevive;
            base.OnDestroy();
        }

        private void OnRevive()
        {
            for (int i = 0; i < listIngressData.Count; i++)
            {
                _containerLogic.AddIngressDataByUfo(listIngressData[i]);
            }

            _containerLogic.Revive(slotCountOnRevive);
            
            listIngressData.Clear();
        }

        public void SetUpContainers(List<ContainerQueueData> containerQueues, ContainerQueueData staticContainer, ContainerLogic containerLogic)
        {
            _containerQueues = containerQueues;
            _staticContainer = staticContainer;
            _containerLogic  = containerLogic;

            _containerLogic.OnLoseGame += OnLose;

            ReleaseContainers();
            SpawnContainers();

            listIngressData = new List<IngressData>();
        }

        /// <summary>
        /// Releases all static and queue containers.
        /// </summary>
        public void ReleaseContainers()
        {
            foreach (var staticContainer in _staticContainers)
            {
                staticContainer.ReleaseStickman();
                PrefabPool<StaticContainer>.Release(staticContainer);
            }

            _staticContainers.Clear();

            foreach (var queueContainer in _queueContainers)
            {
                PrefabPool<QueueContainer>.Release(queueContainer);
            }

            _queueContainers.Clear();
            _currentQueueContainers.Clear();
        }

        private void SpawnContainers()
        {
            var  containerData = _staticContainer.containerDatas;
            bool even          = containerData.Count % 2 == 0;

            for (int i = 0; i < containerData.Count; i++)
            {
                var staticContainer = PrefabPool<StaticContainer>.Spawn(staticContainerPrefab);
                staticContainer.transform.SetParent(transform);

                float xOffset = (i - containerData.Count / 2) * _spawnStaticRange + (even ? _spawnStaticRange / 2 : 0);
                staticContainer.transform.localPosition = new Vector3(xOffset, 0, 0);
                staticContainer.SetData(containerData[i]);
                _staticContainers.Add(staticContainer);
            }

            var colCount = _containerQueues.Count;

            for (int i = 0; i < colCount; i++)
            {
                var queueContainerData = _containerQueues[i].containerDatas;

                even = colCount % 2 == 0;

                for (int j = 0; j < queueContainerData.Count; j++)
                {
                    var queueContainer = PrefabPool<QueueContainer>.Spawn(queueContainerPrefab);
                    queueContainer.transform.SetParent(transform);

                    // ReSharper disable once PossibleLossOfFraction
                    float xOffset = (i - colCount / 2) * _spawnRange + (even ? _spawnRange / 2 : 0);
                    float zOffset = (j + 1) * _spawnRange;
                    //float zOffset = (j - queueContainerData.Count / 2) * _spawnRange + (even ? _spawnRange / 2 : 0);

                    queueContainer.transform.localPosition = new Vector3(xOffset, 0, zOffset);
                    queueContainer.SetData(queueContainerData[j]);
                    queueContainer.SetColumn(i);
                    _queueContainers.Add(queueContainer);

                    if (j == 0)
                    {
                        _currentQueueContainers.Add(queueContainer);
                    }
                }
            }
        }

        private void OnLose(IngressData ingressData)
        {
            listIngressData.Add(ingressData);
            
            var count = ingressData.Number;

            for (int i = 0; i < _currentQueueContainers.Count; i++)
            {
                var container = _currentQueueContainers[i];

                if (container.ContainerID == -1 || container.ContainerID == ingressData.ID)
                {
                    count -= container.Data.Remaining;
                }
            }

            for (int i = 0; i < _staticContainers.Count; i++)
            {
                var container = _staticContainers[i];

                if (container.ContainerID == -1 || container.ContainerID == ingressData.ID)
                {
                    count -= container.Data.Remaining;
                }
            }

            Debug.LogWarning($"Còn thừa {count} stickman id : {ingressData.ID}");

            GameManager.Instance.LoseGame();
            
            MovementThread.Instance.AddAction(async () =>
            {
                await StickmanTransporter.Instance.CallUFODieStickman(ingressData);
            });
        }

        public void UpdateQueue(QueueContainer queue)
        {
            MovementThread.Instance.AddAction(async () =>
            {
                bool addCurrentQueue = false;

                bool canMove = false;

                var duration = 0.2f;
                
                for (int i = 0; i < _queueContainers.Count; i++)
                {
                    if (queue == _queueContainers[i])
                    {
                        canMove = true;
                        continue;
                    }

                    if (!canMove)
                        continue;

                    if (_queueContainers[i].Column == queue.Column)
                    {
                        if (!addCurrentQueue)
                        {
                            _currentQueueContainers.Add(_queueContainers[i]);
                            addCurrentQueue = true;
                        }
                        
                        _queueContainers[i].MoveDown(_spawnRange, duration);
                    }
                }

                await UniTask.Delay(250);

                _queueContainers.Remove(queue);
                _currentQueueContainers.Remove(queue);

                _containerLogic.CheckEmptyContainer(queue.Data);

                if (_queueContainers.Count == 0)
                {
                    GameManager.Instance.WinGame();
                }
            });
        }
        public void AddSlot()
        {
            var newContainer = PrefabPool<StaticContainer>.Spawn(staticContainerPrefab);
            newContainer.transform.SetParent(transform);
            _staticContainers.Add(newContainer);

            var  containerData = _staticContainer.containerDatas;
            bool even          = containerData.Count % 2 == 0;

            for (int i = 0; i < _staticContainers.Count; i++)
            {
                var   staticContainer = _staticContainers[i];
                float xOffset         = (i - containerData.Count / 2) * _spawnStaticRange + (even ? _spawnStaticRange / 2 : 0);
                staticContainer.transform.localPosition = new Vector3(xOffset, 0, 0);
                staticContainer.SetData(containerData[i]);
            }
        }

        public QueueContainer FindQueueContainer(ContainerData containerData) { return _queueContainers?.FirstOrDefault(queue => queue.Data != null && queue.Data.Equals(containerData)); }

        public bool IsMaxSlot() => _staticContainers.Count >= 5;
    }
}