using UnityEngine;

namespace HoleBox
{
    using BasePuzzle.PuzzlePackages.Core;

    [DisallowMultipleComponent, SelectionBase]
    public sealed class LEGrid : MonoBehaviour
    {
        [SerializeField] private Transform _mapContainer;

        private int _numOfRow, _numOfCol;

        private LETile[,] _grid;
        public  LETile[,] Grid => _grid;
        
        public Vector2Int Matrix => new Vector2Int(_numOfCol, _numOfRow);
        public int NumOfRow => _numOfRow;
        public int NumOfCol => _numOfCol;
        
        
        public void CreateGrid(int noCol, int noRow)
        {
            ClearMatrix();
            
            _numOfRow = noRow;
            _numOfCol = noCol;
            _grid     = new LETile[noRow, noCol];
            
            for (int i = 0; i < _numOfRow; i++)
            {
                for (int j = 0; j < _numOfCol; j++)
                {
                    var platformBlock = PrefabPool<LETile>.Spawn();
                    platformBlock.transform.position = new Vector3(j, 0, i);
                    _grid[i, j] = platformBlock;
                    platformBlock.name = $"Tile_{i}_{j}";
                    platformBlock.transform.SetParent(_mapContainer);
                }
            }
        }
        
        private void ClearMatrix()
        {
            if (_grid == null) return;

            for (int i = 0; i < _grid.GetLength(0); i++)
            {
                for (int j = 0; j < _grid.GetLength(1); j++)
                {
                    if (!_grid[i, j]) continue;
                    _grid[i, j].SendToPool();
                }
            }

            _grid     = null;
            _numOfRow = _numOfCol = 0;
        }
        
        

        public void SetTileColor(Vector2Int cell, Vector2Int size, Color color)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    _grid[cell.y + i, cell.x + j].SetColor(color);
                }
            }
        }

        public void CleanMatrix()
        {
            for (int i = 0; i < _numOfRow; i++)
            {
                for (int j = 0; j < _numOfCol; j++)
                {
                    _grid[i, j].SetColor(Color.gray);
                }
            }
        }

        public bool IsInsideMatrix(Vector2Int cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < _numOfCol && cell.y < _numOfRow;
        }

        public bool IsInsideMatrix(Vector2Int cell, Vector2Int size)
        {
            return cell.x >= 0 && cell.y >= 0 &&
                   cell.x + size.x <= _numOfCol &&
                   cell.y + size.y <= _numOfRow;
        }
        
        public Vector2Int ClampToMatrix(Vector2Int cell)
        {
            return new Vector2Int(
                Mathf.Clamp(cell.x, 0, _numOfCol - 1),
                Mathf.Clamp(cell.y, 0, _numOfRow - 1)
            );
        }
        
        public static Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x);
            int y = Mathf.FloorToInt(worldPos.z);
            return new Vector2Int(x, y);
        }
    }
}
