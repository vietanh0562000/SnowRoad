using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoleBox
{
    using BasePuzzle.PuzzlePackages.Core;

    public class TileSpawner : Singleton<TileSpawner>
    {
        public TileSpawn tile;

        private void Start() { PrefabPool<TileSpawn>.Create(tile, 32, 100, true); }

        public void SpawnTile(BoxData boxData) { StartCoroutine(SpawnTilesWithDelay(boxData)); }

        private IEnumerator SpawnTilesWithDelay(BoxData boxData)
        {
            List<Vector2> positionsByRings = GetPositionsByRings(boxData);
            foreach (var ring in positionsByRings)
            {
                SpawnTileAtPosition(ring);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void SpawnTileAtPosition(Vector2 position)
        {
            // Instantiate tile tại vị trí cụ thể
            var spawnPosition = new Vector3(position.x, 0, position.y);
            var fx            = PrefabPool<TileSpawn>.Spawn(tile, spawnPosition, Quaternion.identity, transform); // Spawn từ pool
            fx.transform.localScale = new Vector3(2, 2, 2);

            var tilePrefab = PrefabPool<TilePrefab>.Spawn();

            tilePrefab.transform.position   = spawnPosition;
            tilePrefab.transform.localScale = new Vector3(2, 2, 2);
            tilePrefab.transform.SetParent(transform);
        }

        private List<Vector2> GetPositionsByRings(BoxData boxData)
        {
            List<Vector2> rings = new List<Vector2>();

            var offset = Vector2.one * 0.5f;

            rings.Add(boxData.position + offset);
            rings.Add(boxData.position + Vector2Int.right * 2 + offset);
            rings.Add(boxData.position + Vector2Int.one * 2 + offset);
            rings.Add(boxData.position + Vector2Int.up * 2 + offset);

            return rings;
        }
    }
}