namespace HoleBox
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using com.ootii.Messages;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using BasePuzzle.PuzzlePackages.Core;
    using PuzzleGames;
    using HoleBox;
    using Sirenix.OdinInspector;
    using SRF;
    using UnityEngine;

    public class TemporaryBoardVisualize : Singleton<TemporaryBoardVisualize>
    {
        [TabGroup("Board Visualizer")] [SerializeField]
        private MapContainer _containerManager;

        [TabGroup("Board Visualizer")] [SerializeField]
        private Transform gridMap;

        [TabGroup("Board Visualizer")] [SerializeField]
        private Transform stickmanGroup;

        [TabGroup("Board Visualizer")] [SerializeField]
        private Transform holeGroup;

        [TabGroup("Board Visualizer")] [SerializeField]
        private DrawMatrix drawMatrix;

        public float ContainerRange = 2f;

        #region Level Data

        // algorithm data
        [TabGroup("Box Data")] [SerializeField, ReadOnly]
        private Vector2Int _matrix = new();

        [TabGroup("Box Data")] [SerializeField, ReadOnly]
        private List<BoxData> _boxes = new();

        [TabGroup("Box Data")] [SerializeField, ReadOnly]
        private List<BoxData> _holes = new();

        [TabGroup("Container Data")] [SerializeField, ReadOnly]
        private StaticContainerConfig _staticContainerConfig;

        [TabGroup("Container Data")] [SerializeField, ReadOnly]
        private List<ContainerQueueData> _containerQueues;

        [SerializeField] private GameObject darkBG;

        #endregion

        private GameAlgorithm                      _gameAlgorithm;
        private ContainerQueueData                 _staticContainer;
        private ContainerLogic                     _containerLogic;
        private Dictionary<int, List<Vector2Int>>  _paths;
        private Dictionary<int, List<StickManBox>> _stickMenByBoxId;

        private bool _isProcessing;

        public Vector2Int Matrix => _matrix;

        private bool useRainbowHole;
        private bool useUfo;

        public bool UseRainbowHole
        {
            get => useRainbowHole;
            set
            {
                if (useRainbowHole != value)
                {
                    useRainbowHole = value;
                    _gameAlgorithm.SetRainbowMap(value);

                    MessageDispatcher.SendMessage(value ? EventID.ACTIVE_RAINBOW : EventID.DEACTIVE_RAINBOW, 0);
                }

                SetBoosterRainbowHole(useRainbowHole);
            }
        }

        public bool UseUfo
        {
            get => useUfo;
            set
            {
                useUfo = value;
                SetBoosterUfo(useUfo);
            }
        }

        public List<BoxData> Boxes => _boxes;
        public List<BoxData> Holes => _holes;

        protected override void Awake()
        {
            base.Awake();

            //parse data from file text to get Matrix, Boxes, Holes, Queue,..
            _gameAlgorithm   = new GameAlgorithm(_matrix, _boxes, _holes);
            _stickMenByBoxId = new Dictionary<int, List<StickManBox>>();

            _staticContainer = new ContainerQueueData(_staticContainerConfig);
            _containerLogic  = new ContainerLogic(_containerQueues, _staticContainer);

            _gameAlgorithm.Initialize();
        }

        public void SetLevel(LevelData levelData)
        {
            _matrix = levelData.Matrix;
            var boxes = new List<BoxData>(levelData.Boxes);

            _staticContainerConfig = levelData.StaticConfig;
            _containerQueues       = levelData.ContainerQueues;

            SplitHoleAndBox(boxes);

            drawMatrix.SpawnMatrixBorder(_matrix);

            _gameAlgorithm   = new GameAlgorithm(_matrix, _boxes, _holes);
            _stickMenByBoxId = new Dictionary<int, List<StickManBox>>();

            _staticContainer = new ContainerQueueData(_staticContainerConfig);
            _containerLogic  = new ContainerLogic(_containerQueues, _staticContainer);

            _gameAlgorithm.Initialize();

            //ReleasePool();
            InitializeBoard();
        }

        private void SplitHoleAndBox(List<BoxData> boxes)
        {
            _holes = new List<BoxData>();
            _boxes = new List<BoxData>();

            foreach (var b in boxes)
            {
                b.InitData();

                if (b is HoleBoxData)
                {
                    _holes.Add(b);
                }
                else
                {
                    _boxes.Add(b);
                }
            }
        }
        
        private readonly float ExtendPos = 0.5f;

        public void InitializeBoard()
        {
            for (int i = 0; i < _matrix.x / 2; i++)
            {
                for (int j = 0; j < _matrix.y / 2; j++)
                {
                    if (_holes.Find(data => data.InsideBox(new Vector2(i * 2, j * 2))) != null)
                    {
                        continue;
                    }
                    
                    var tile = PrefabPool<TilePrefab>.Spawn();

                    tile.transform.position   = new Vector3(i * 2 + 0.5f, 0, j * 2 + 0.5f);
                    tile.transform.localScale = new Vector3(2, 2, 2);
                    tile.transform.SetParent(gridMap);
                }
            }

            if (_boxes != null)
            {
                for (int k = 0; k < _boxes.Count; k++)
                {
                    var box      = _boxes[k];
                    var stickMen = new List<StickManBox>();

                    for (int i = 0; i < box.size.x / 2; i++)
                    {
                        for (int j = 0; j < box.size.y / 2; j++)
                        {
                            var position = new Vector3(box.position.x + i * 2 + ExtendPos, 0, box.position.y + j * 2 + ExtendPos);
                            var b        = PoolManager.Instance.Spawn(box, position, stickmanGroup);

                            if (b is StickManBox stickMan)
                            {
                                stickMen.Add(stickMan);
                            }
                        }
                    }

                    if (stickMen.Count != 0)
                        _stickMenByBoxId[k] = stickMen;
                }
            }

            if (_holes != null)
            {
                int count = 0;
                foreach (var hole in _holes)
                {
                    var holePrefab = PoolManager.Instance.GetPool<HoleBoxData, Hole>()
                        .Spawn(hole.GetMiddlePosition(), holeGroup);

                    holePrefab.SetData(hole);
                    holePrefab.id = count++;
                    holePrefab.OnClick(Process);
                }
            }

            _containerManager.SetUpContainers(_containerQueues, _staticContainer, _containerLogic);
            _containerManager.transform.position = new Vector3(_matrix.x / 2f - 0.5f, 0, _matrix.y + ContainerRange);

            StickmanTransporter.Instance.UpdateStartPoint(_containerManager.transform.position);
        }

        [Button]
        public void Process(Hole choosingHole)
        {
            if (_gameAlgorithm == null)
            {
                Debug.LogError("GameAlgorithm is not initialized.");
                return;
            }

            _isProcessing = true;

            var index = choosingHole.id;

            _gameAlgorithm.Process(index, UseRainbowHole ? 12 : choosingHole.MaxCountInHole, out _paths);

            if (_paths.Count == 0)
            {
                _isProcessing = false;
                AudioController.PlaySound(SoundKind.OnTouchHoleFailed);
                Debug.Log("No path found");
                return;
            }

            bool holeFX = false;

            AudioController.PlaySound(SoundKind.OnTouchHole);

            if (UseRainbowHole)
            {
                UseRainbowHole = false;

                holeFX = true;

                choosingHole.UsingRainbow();

                StartCoroutine(CoShowFX());

                IEnumerator CoShowFX()
                {
                    // Move stickmen according to the output
                    foreach (var path in _paths)
                    {
                        var stickMen = _stickMenByBoxId[path.Key];

                        foreach (var stickMan in stickMen)
                        {
                            yield return null;
                            stickMan.ShowFX();
                        }
                    }
                }

                InGameTracker.UseBooster();
                AudioController.PlaySound(SoundKind.UseBoosterRainbow);
                var rainbowManager = ResourceType.Powerup_RainbowHole.Manager();
                rainbowManager?.Subtract(1);
                rainbowManager?.UI.UpdateUI();
                GameManager.Instance.SetPlayed();
                MessageDispatcher.SendMessage(EventID.USE_BOOSTER, 0);
            }

            _gameAlgorithm.UpdateMap();
            GameManager.Instance.OnClickHole?.Invoke();
            UpdateBoxData();

            _isProcessing = false;

            // Move stickmen according to the output
            foreach (var path in _paths)
            {
                var stickMen = _stickMenByBoxId[path.Key];

                var listBoxData = new List<StickManData>();
                foreach (var stickMan in stickMen)
                {
                    stickMan.MoveStickMan(path.Value, _holes[index], choosingHole.GetStickMan,
                        () =>
                        {
                            if (listBoxData.Contains(stickMan.Data))
                            {
                                return;
                            }

                            listBoxData.Add(stickMan.Data);
                            var ingressData = new IngressData(stickMan.Data.id, stickMan.Data.SizeCount);
                            //_containerManager.CheckCurrentContainers(ingressData);
                            _containerLogic.AddIngressData(ingressData);
                            checkLoseCts?.Cancel();
                            checkLoseCts?.Dispose();
                            checkLoseCts = new CancellationTokenSource();

                            // gọi async method
                            CheckLoseGame(checkLoseCts.Token).Forget();
                        }, holeFX);
                }
            }
        }

        private void UpdateBoxData()
        {
            for (int i = 0; i < _boxes.Count; i++)
            {
                _boxes[i].UpdateBoxData();
            }
        }

        public bool IsShowHiddenBox(BoxData boxData)         { return _gameAlgorithm.IsShowHiddenBox(boxData); }
        public bool IsEmptyWithDirection(TunnelData boxData) { return _gameAlgorithm.IsEmptyWithDirectionAndOffset(boxData, boxData.direction); }

        public StickManBox SpawnStickManBox(BoxData box, Tunnel tunnel)
        {
            var moveStickMan = PoolManager.Instance.Spawn(box, tunnel.transform.position, stickmanGroup);

            if (moveStickMan is StickManBox stickMan)
            {
                stickMan.RotateTunnel(tunnel.Rotation);
                return stickMan;
            }

            return null;
        }

        public async void SpawnBox(StickManBox box)
        {
            var position = new Vector3(box.Data.position.x +ExtendPos, 0, box.Data.position.y + ExtendPos);
            
            box.MoveTo(position);

            await UniTask.WaitUntil(() => !_isProcessing).Timeout(TimeSpan.FromSeconds(10))
                .SuppressCancellationThrow();;
            _boxes.Add(box.Data);
            _gameAlgorithm.UpdateMap();
            _stickMenByBoxId[_boxes.Count - 1] = new List<StickManBox> { box };
        }

        public HoleBoxData GetHole(Vector2Int pos)
        {
            // Iterate through the list of holes to find a matching position
            foreach (var hole in _holes)
            {
                if (hole.InsideBox(pos))
                {
                    Debug.Log($"Hole found at position: {hole.position}");
                    return (HoleBoxData)hole; // Exit the method once the hole is found
                }
            }

            Debug.LogError($"No hole found at position: {pos}");
            return null;
        }
        public void AddSlot()
        {
            _staticContainer.AddSlot();
            _containerManager.AddSlot();
            MessageDispatcher.SendMessage(EventID.REFRESH_CAMERA, 0);
        }

        public bool IsMaxSlot() { return _containerManager.IsMaxSlot(); }

        public void ClaimBoxes(List<BoxData> groupBoxes, int startingBoxID, Vector3 position)
        {
            var ingressData = new IngressData(startingBoxID, 0);

            var listStickman = new List<StickManData>();

            for (int i = 0; i < groupBoxes.Count; i++)
            {
                if (groupBoxes[i] is StickManData stickMan)
                {
                    stickMan.UseUfo();
                    ingressData.Number += stickMan.SizeCount;
                    listStickman.Add(stickMan);
                }
            }

            UseUfo = false;
            _ = StickmanTransporter.Instance.CallUFOToPickup(position,
                ufo =>
                {
                    for (int i = 0; i < listStickman.Count; i++)
                    {
                        listStickman[i].Claim(ufo);
                    }

                    _gameAlgorithm.UpdateMap();
                },
                ufo =>
                {
                    _containerLogic.AddIngressDataByHelicopter(ingressData, ufo);

                    _gameAlgorithm.UpdateMap();
                    UpdateBoxData();
                });
        }

        [Button]
        public void ValidateContainer() { _containerLogic.ValidateContainers(); }

        public Vector3 GetRandomStickmanBox()
        {
            var hole = Boxes.First(data => data is StickManData && data.IsAvailable);

            return hole.GetMiddlePosition();
        }

        public Vector3 GetRandomHole()
        {
            var hole = Holes.First(data => data.IsAvailable);

            return hole.GetMiddlePosition();
        }

        private void SetBoosterUfo(bool isActive)
        {
            darkBG.SetActive(isActive);

            if (isActive)
            {
                stickmanGroup.Highlight();
            }
            else
            {
                stickmanGroup.ResetLocal();
            }
        }

        private void SetBoosterRainbowHole(bool isActive)
        {
            darkBG.SetActive(isActive);

            if (isActive)
            {
                holeGroup.Highlight();
            }
            else
            {
                holeGroup.ResetLocal();
            }
        }

        public bool ExistStickmanMoving(int id = -1)
        {
            foreach (var listStickMan in _stickMenByBoxId)
            {
                foreach (var stickMan in listStickMan.Value)
                {
                    if (stickMan.IsMoving && (id == -1 || stickMan.Data.id == id))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private CancellationTokenSource checkLoseCts;

        private async UniTaskVoid CheckLoseGame(CancellationToken token)
        {
            try
            {
                await UniTask.WaitUntil(() => !ExistStickmanMoving(), cancellationToken: token).Timeout(TimeSpan.FromSeconds(10))
                    .SuppressCancellationThrow();;

                if (!token.IsCancellationRequested && _gameAlgorithm.CheckLoseGame())
                {
                    LevelManager.Instance.ForceLoseLevel();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}