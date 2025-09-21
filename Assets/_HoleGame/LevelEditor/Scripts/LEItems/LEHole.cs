using UnityEngine;

namespace HoleBox
{
    using System;
    using BasePuzzle.PuzzlePackages.Core;
    using TMPro;

    [DisallowMultipleComponent, SelectionBase]
    public class LEHole : ALESpawnItem
    {
        [SerializeField] private GameObject _closeGo;
        [SerializeField] private TMP_Text   _closeTxt;        
        [SerializeField] private TMP_Text   _idTxt;        
        [SerializeField] private GameObject _lockedGo;        
        
        private HoleBoxData _boxData = new();
        private LineRenderer _lineRenderer;
        private LEHole _linkedHole;

        public override void SetUpData()
        {
            _boxData.position   = Vector2Int.zero;
            _boxData.size       = Vector2Int.one * 4;
            _boxData.id         = 1;
            _boxData.closedHole = false;
            _boxData.lockedHole = false;
            _boxData.keyPos     = Vector2Int.zero;
            
            CreateLine();
            ChangeColor();
            _idTxt.SetText($"{_boxData.id}");
        }

        public override void Swap()
        {
            int currentID = _boxData.id;
            currentID++;
            
            int colorCount = GameAssetManager.Instance.TotalChangedColors;
            
            if (currentID > colorCount) currentID = 1;
            _boxData.id = currentID;
            
            ChangeColor();
            _idTxt.SetText($"{_boxData.id}");
        }
        
        public override void Highlight(bool value)
        {
            HlGameObject?.SetActive(value);
        }

        public override void CopyData(BoxData data)
        {
            var holeData = data as HoleBoxData;
            _boxData.id            = holeData.id;
            _boxData.closedHole    = holeData.closedHole;
            _boxData.numberToClose = holeData.numberToClose;
            _boxData.lockedHole    = holeData.lockedHole;
            _boxData.keyPos        = holeData.keyPos;
        }
        
        public override void UpdateFollowData()
        {
            _closeGo.SetActive(_boxData.closedHole);
            _closeTxt.text = _boxData.numberToClose.ToString();
            
            _lockedGo.SetActive(_boxData.lockedHole);
            
            // Clear previous link
            if (_linkedHole != null)
            {
                _linkedHole = null;
                _lineRenderer.enabled = false;
            }

            if (_boxData.lockedHole)
            {
                var holes = FindObjectsByType<LEHole>(FindObjectsSortMode.None);
                foreach (var hole in holes)
                {
                    var holeData = hole.Data as HoleBoxData;
                    if (holeData == null) continue;
				
                    if (hole != this && _boxData.keyPos == holeData.position)
                    {
                        _linkedHole = hole;
                        _lineRenderer.enabled = true;
                        UpdateLinePosition();
                        break;
                    }
                }
            }
            
            ChangeColor();
            _idTxt.SetText($"{_boxData.id}");
        }

        private void CreateLine()
        {
            // Initialize LineRenderer if not exists
            if (_lineRenderer == null)
            {
                _lineRenderer               = gameObject.AddComponent<LineRenderer>();
                _lineRenderer.startWidth    = 0.1f;
                _lineRenderer.endWidth      = 0.1f;
                _lineRenderer.material      = new Material(Shader.Find("Sprites/Default"));
                _lineRenderer.startColor    = Color.yellow;
                _lineRenderer.endColor      = Color.yellow;
                _lineRenderer.positionCount = 2;
                _lineRenderer.sortingOrder  = 2000;
                
                _lineRenderer.enabled = false;
            }
        }

        private void UpdateLinePosition()
        {
            if (_linkedHole != null && _lineRenderer != null)
            {
                _lineRenderer.SetPosition(0, Convert(transform.position, 4));
                _lineRenderer.SetPosition(1, Convert(_linkedHole.transform.position, 4));

                if (_linkedHole.Data == null)
                {
                    _boxData.lockedHole = false;
                    _boxData.keyPos     = Vector2Int.zero;
                    
                    _linkedHole         = null;
                    _lineRenderer.enabled = false;
                    
                    UpdateFollowData();
                }
                else if (_linkedHole.Data.position != _boxData.keyPos)
                {
                    _boxData.keyPos = _linkedHole.Data.position;
                }
            }
        }

        private Vector3 Convert(Vector3 origin, int size)
        {
            return origin + Vector3.one * ((size - 1) / 2f);
        }

        private void LateUpdate()
        {
            if (_linkedHole != null && _linkedHole.gameObject.activeInHierarchy)
            {
                UpdateLinePosition();
            }
            else if (_lineRenderer != null && _lineRenderer.enabled)
            {
                _boxData.lockedHole   = false;
                _boxData.keyPos      = Vector2Int.zero;

                _linkedHole = null;
                UpdateFollowData();
                
                _lineRenderer.enabled = false;
            }
        }

        public override BoxData Data               => _boxData;
        public override bool    IsAbleToChangeSwap => true;
        
        public override ALESpawnItem SpawnFromPool()
        {
            var inst = PrefabPool<LEHole>.Spawn(this);
            inst.SetUpData();
            return inst;
        }

        public override void SendToPool()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = false;
            }
            _linkedHole = null;
            PrefabPool<LEHole>.Release(this);
        }
    }
}
