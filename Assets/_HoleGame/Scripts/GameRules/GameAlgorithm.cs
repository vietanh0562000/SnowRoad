namespace HoleBox
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using global::HoleBox.Utils;
    using Random = UnityEngine.Random;

    public class GameAlgorithm
    {
        private const int HOLE_OFFSET_INDEX = 1000;
        public static int CountInHole       = 8;

        private readonly Vector2Int                        _matrix;
        private readonly List<BoxData>                     _boxes;
        private readonly List<BoxData>                     _holes;
        private readonly HashQueue<Vector2Int>             _queue;
        private readonly HashSet<int>                      _moveBoxes;
        private readonly Dictionary<int, List<Vector2Int>> _paths;

        private KeyValuePair<int, int> _output;

        private bool          _isInitialized;
        private int           _currentBoxIndex;
        private int           _countInHole;
        private int[,]        _matrixData;
        private int[,]        _matrixBoxIndex;
        private bool[,]       _isNodeVisited;
        private Vector2Int[,] _parents;
        private List<BoxData> _removedBoxes;
        private Vector2Int[]  _directions;

        private bool _isRainbow;

        public GameAlgorithm(Vector2Int matrix, List<BoxData> boxes, List<BoxData> holes)
        {
            _matrix = matrix;
            _boxes  = boxes;
            _holes  = holes;

            _queue     = new HashQueue<Vector2Int>();
            _moveBoxes = new HashSet<int>();
            _paths     = new Dictionary<int, List<Vector2Int>>();
            _directions = new[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized  = true;
            _matrixData     = new int[_matrix.x, _matrix.y];
            _isNodeVisited  = new bool[_matrix.x, _matrix.y];
            _matrixBoxIndex = new int[_matrix.x, _matrix.y];
            _parents        = new Vector2Int[_matrix.x, _matrix.y];
            _removedBoxes   = new List<BoxData>();

            UpdateMap();
        }

        private void Reset()
        {
            _queue.Clear();
            _moveBoxes.Clear();
            _paths.Clear();
            _removedBoxes.Clear();
            for (int i = 0; i < _matrix.x; i++)
            {
                for (int j = 0; j < _matrix.y; j++)
                {
                    _matrixData[i, j]     = 0;
                    _isNodeVisited[i, j]  = false;
                    _matrixBoxIndex[i, j] = -1;
                    _parents[i, j]        = new Vector2Int(-1, -1);
                }
            }

            UpdateMap();
        }

        public void UpdateMap()
        {
            for (int i = 0; i < _matrix.x; i++)
            {
                for (int j = 0; j < _matrix.y; j++)
                {
                    _matrixData[i, j]     = 0;
                    _isNodeVisited[i, j]  = false;
                    _matrixBoxIndex[i, j] = -1;
                    _parents[i, j]        = new Vector2Int(-1, -1);
                }
            }

            for (int k = 0; k < _boxes.Count; k++)
            {
                var box = _boxes[k];
                for (int i = 0; i < box.size.x; i++)
                {
                    for (int j = 0; j < box.size.y; j++)
                    {
                        if (box.MatrixValue < 0)
                        {
                            _matrixData[i + box.position.x, j + box.position.y] = box.MatrixValue;
                        }
                        else
                        {
                            _matrixData[i + box.position.x, j + box.position.y] = _isRainbow ? 0 : box.MatrixValue;
                        }

                        if (!box.IsClaimed)
                        {
                            _matrixBoxIndex[i + box.position.x, j + box.position.y] = k;
                        }
                    }
                }
            }

            for (int k = 0; k < _holes.Count; k++)
            {
                var hole = _holes[k];
                for (int i = 0; i < hole.size.x; i++)
                {
                    for (int j = 0; j < hole.size.y; j++)
                    {
                        _matrixData[i + hole.position.x, j + hole.position.y]     = hole.MatrixValue;
                        _matrixBoxIndex[i + hole.position.x, j + hole.position.y] = HOLE_OFFSET_INDEX + k;
                    }
                }
            }
        }

        private void Bfs(int selectedHoleIndex, bool toCheck = false)
        {
            // BFS algorithm to find the path from the selected hole to the boxes
            // and check if the boxes can be moved to the holes
            var selectedHole = _holes[selectedHoleIndex];
            var startPos     = selectedHole.position + new Vector2Int(Random.Range(0, selectedHole.size.x), Random.Range(0, selectedHole.size.y));
            _queue.Clear();
            _queue.Enqueue(startPos);

            while (_queue.Count > 0)
            {
                var currentPos = _queue.Dequeue();
                _isNodeVisited[currentPos.x, currentPos.y] = true;

                _directions = _directions.OrderBy(_ => Random.value).ToArray();
                foreach (var t in _directions)
                {
                    var newPos = new Vector2Int(currentPos.x + t.x, currentPos.y + t.y);
                    if (!CheckValidNodeToEnqueue(newPos, selectedHoleIndex)) continue;
                    _parents[newPos.x, newPos.y] = currentPos;
                    _queue.Enqueue(newPos);
                }

                _currentBoxIndex = _matrixBoxIndex[currentPos.x, currentPos.y];
                if (_currentBoxIndex < HOLE_OFFSET_INDEX)
                {
                    _moveBoxes.Add(_currentBoxIndex);
                }
            }

            int count = 0;
            foreach (var boxIndex in _moveBoxes)
            {
                if (boxIndex == -1) continue;

                if (_boxes[boxIndex].id != _holes[selectedHoleIndex].id && !_isRainbow) continue;

                if (_isRainbow && !IsBoxInRange(_boxes[boxIndex], _holes[selectedHoleIndex]))
                {
                    continue;
                }

                if (!toCheck)
                {
                    _boxes[boxIndex].IsClaimed = true;
                }

                var box    = _boxes[boxIndex];
                var parent = box.position;

                var path = _paths.TryGetValue(boxIndex, out var boxPath) ? boxPath : new List<Vector2Int>();
                path.Clear();
                _paths[boxIndex] = path;

                while (parent.x != -1 && parent.y != -1)
                {
                    path.Add(parent);
                    parent = _parents[parent.x, parent.y];
                }

                count += 1;
                _removedBoxes.Add(box);

                if (count >= _countInHole)
                {
                    break;
                }
            }

            _output = new KeyValuePair<int, int>(selectedHole.id, count);

            // foreach (var box in _removedBoxes)
            // {
            //     _boxes.Remove(box);
            // }
        }

        private bool CheckValidNodeToEnqueue(Vector2Int pos, int selectedHole)
        {
            if (pos.x < 0 || pos.x >= _matrix.x - 1) return false;
            if (pos.y < 0 || pos.y >= _matrix.y - 1) return false;

            if (pos.x % 2 == 1 && pos.y % 2 == 1)
                return false;

            if (_isNodeVisited[pos.x, pos.y]) return false;

            var targetValue = _holes[selectedHole].MatrixValue;

            if (_matrixData[pos.x, pos.y] == 0)
            {
                return (_matrixData[pos.x + 1, pos.y] == 0 || _matrixData[pos.x + 1, pos.y] == targetValue)
                       && (_matrixData[pos.x, pos.y + 1] == 0 || _matrixData[pos.x, pos.y + 1] == targetValue)
                       && (_matrixData[pos.x + 1, pos.y + 1] == 0 || _matrixData[pos.x + 1, pos.y + 1] == targetValue);
            }

            var boxIndex = _matrixBoxIndex[pos.x, pos.y];

            if (boxIndex == -1) return true;

            if (boxIndex < HOLE_OFFSET_INDEX)
            {
                return _boxes[boxIndex].MatrixValue == _holes[selectedHole].MatrixValue;
            }

            return boxIndex - HOLE_OFFSET_INDEX == selectedHole;
        }

        public bool IsBoxInRange(BoxData boxToCheck, BoxData targetHole, int range = 2)
        {
            // Lấy tọa độ phạm vi mở rộng của targetHole
            Vector2Int holePosition = targetHole.position;
            Vector2Int holeSize     = targetHole.size;

            // Xác định phạm vi xung quanh box với range
            int startX = holePosition.x - range;
            int endX   = holePosition.x + holeSize.x + range - 1;
            int startY = holePosition.y - range;
            int endY   = holePosition.y + holeSize.y + range - 1;

            // Lấy tọa độ của boxToCheck
            Vector2Int boxPosition = boxToCheck.position;
            Vector2Int boxSize     = boxToCheck.size;

            // Kiểm tra xem boxToCheck có giao nhau với phạm vi xung quanh targetHole hay không
            bool isInXRange = boxPosition.x + boxSize.x - 1 >= startX && boxPosition.x <= endX;
            bool isInYRange = boxPosition.y + boxSize.y - 1 >= startY && boxPosition.y <= endY;

            return isInXRange && isInYRange;
        }


        public void Process(int selectedHoleIndex, int countInHole, out Dictionary<int, List<Vector2Int>> path)
        {
            _currentBoxIndex = selectedHoleIndex;
            _countInHole     = countInHole;
            Reset();
            Bfs(selectedHoleIndex);
            path = new(_paths);
        }
        public bool IsShowHiddenBox(BoxData boxData)
        {
            // Extract bounding information
            int startX = boxData.position.x - 1;
            int startY = boxData.position.y - 1;
            int endX   = boxData.position.x + boxData.size.x;
            int endY   = boxData.position.y + boxData.size.y;

            // Loop through the boundary cells
            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    // Skip positions that are inside the box
                    if (boxData.InsideBox(new Vector2Int(x, y)))
                        continue;

                    // Skip corners
                    if ((x == startX && y == startY) || // Top-left
                        (x == endX && y == startY) || // Top-right
                        (x == startX && y == endY) || // Bottom-left
                        (x == endX && y == endY)) // Bottom-right
                    {
                        continue;
                    }

                    // Ensure boundary is within matrix bounds
                    if (x >= 0 && x < _matrixData.GetLength(0) && y >= 0 && y < _matrixData.GetLength(1))
                    {
                        // Check if the matrix value is 0
                        if (_matrixData[x, y] == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsEmptyWithDirectionAndOffset(BoxData boxData, Vector2Int dir)
        {
            // Calculate boundary based on direction
            int startX = boxData.position.x + dir.x * 2;
            int startY = boxData.position.y + dir.y * 2;
            int endX   = startX + 2;
            int endY   = startY + 2;

            // Ensure nodes are within bounds and check for emptiness
            for (int x = Math.Max(0, startX); x < Math.Min(_matrixData.GetLength(0) - 1, endX); x++)
            {
                for (int y = Math.Max(0, startY); y < Math.Min(_matrixData.GetLength(1) - 1, endY); y++)
                {
                    if (!boxData.InsideBox(new Vector2Int(x, y)) && _matrixData[x, y] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        public void SetRainbowMap(bool b = true) { _isRainbow = b; }

        public bool CheckLoseGame()
        {
            bool checkBarrier = false;
            bool hasBarrier   = _boxes.Any(data => data is ObstacleData { IsBarrier: true });

            if (!hasBarrier)
                return false;

            for (int i = 0; i < _boxes.Count; i++)
            {
                if (_boxes[i] is StickManData { IsClaimed: false })
                {
                    checkBarrier = true;
                    break;
                }
            }

            if (checkBarrier)
            {
                for (int i = 0; i < _holes.Count; i++)
                {
                    _currentBoxIndex = i;
                    _countInHole     = 8;
                    Reset();
                    Bfs(i, true);

                    if (_paths.Count > 0)
                    {
                        return false;
                    }
                }
            }
            else
                return false;

            return true;
        }
    }
}