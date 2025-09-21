namespace HoleBox
{
	using System;
	using System.Collections.Generic;
	using BasePuzzle.PuzzlePackages.Core;
	using Sirenix.OdinInspector;
	using UnityEngine;

	[DisallowMultipleComponent]
	public class LESpawner : MonoBehaviour
	{
		[SerializeField] private LEGrid    _grid;
		[SerializeField] private LEAsset   _asset;
		[SerializeField] private Transform _boxContainer;
		
		[TabGroup("Box Data")] [SerializeField, ReadOnly]
		private List<BoxData> _boxes = new();
		
		
		[TabGroup("Container")] [SerializeField, ReadOnly]
		private StaticContainerConfig _staticContainerConfig = new();
		
		
		[TabGroup("Queue")] [SerializeField, ReadOnly]
		private List<ContainerQueueData> _containerQueues = new();
		
		
		public StaticContainerConfig    StaticContainerConfig => _staticContainerConfig;
		public List<ContainerQueueData> ContainerQueues       => _containerQueues;

		private void Start()
		{
			GameEvent<PanelMapSettings>.Register(LEEvents.CLICK_BTN_CREATE_ANEW, OnCreateNewMap, this);
		}
		
		private void OnDestroy()
		{
			GameEvent<PanelMapSettings>.Unregister(LEEvents.CLICK_BTN_CREATE_ANEW, OnCreateNewMap, this);
		}
		
		private void OnCreateNewMap(PanelMapSettings panelMapSettings)
		{
			int count = _boxContainer.childCount;
			for (int i = count - 1; i >= 0; i--)
			{
				Destroy(_boxContainer.GetChild(i).gameObject);
			}
			
			_boxes.Clear();
		}

		public ALESpawnItem Spawn(ALESpawnItem item)
		{
			var i = item.SpawnFromPool();
			i.transform.SetParent(_boxContainer);

			return i;
		}

		public void SetStaticContainerConfig(int count, int capacity)
		{
			_staticContainerConfig.Count    = count;
			_staticContainerConfig.Capacity = capacity;
		}
		
		public LevelData GetLevelData()
		{
			return new LevelData(_grid.Matrix, _boxes, _staticContainerConfig, _containerQueues);
		}
		
		public bool IsOverlapping(Vector2Int position, Vector2Int size)
		{
			foreach (var box in _boxes)
			{
				bool xOverlap = position.x < box.position.x + box.size.x && position.x + size.x > box.position.x;
				bool yOverlap = position.y < box.position.y + box.size.y && position.y + size.y > box.position.y;

				if (xOverlap && yOverlap) return true;
			}

			return false;
		}
		
		public bool IsOverlappingWithoutBox(BoxData currentBox, Vector2Int newPosition, Vector2Int newSize)
		{
			foreach (var box in _boxes)
			{
				if (box == currentBox)
					continue;

				bool xOverlap = newPosition.x < box.position.x + box.size.x && newPosition.x + newSize.x > box.position.x;
				bool yOverlap = newPosition.y < box.position.y + box.size.y && newPosition.y + newSize.y > box.position.y;

				if (xOverlap && yOverlap)
				{
					return true;
				}
			}

			return false;
		}
		
		public void AddObject(BoxData cell)
		{
			if (!_boxes.Contains(cell))
			{
				_boxes.Add(cell);
			}
		}

		private BoxData GetBoxAtPosition(Vector2Int position)
		{
			foreach (var box in _boxes)
			{
				if (box.position == position)
				{
					return box;
				}
			}

			return null;
		}
		
		public void DeleteObject(BoxData cell)
		{
			if (_boxes.Contains(cell))
			{
				_boxes.Remove(cell);
			}
			else
			{
				var boxAtPosition = GetBoxAtPosition(cell.position);
				if (boxAtPosition != null)
				{
					_boxes.Remove(boxAtPosition);
				}	
			}
		}

		public void OnLoadOldLevel(LevelData data)
		{
			_containerQueues       = data.ContainerQueues;
			_staticContainerConfig = data.StaticConfig;
			
			_asset.ResetCache();
			_boxes.Clear();
			SpawnBoxes(data.Boxes);
			
			// spawned and then update
			_asset.UpdateItems();
		}

		public bool IsEmptyRow(int rowY)
		{
			foreach (var box in _boxes)
			{
				if (box.position.y == rowY || box.position.y + box.size.y >= rowY + 1)
				{
					return false;
				}
			}

			return true;
		}
		
		public bool IsEmptyCol(int colX)
		{
			foreach (var box in _boxes)
			{
				if (box.position.x == colX || box.position.x + box.size.x >= colX + 1)
				{
					return false;
				}
			}

			return true;
		}

		private void SpawnBoxes(List<BoxData> boxDatas)
		{
			foreach (var box in boxDatas)
			{
				var pos = new Vector3(box.position.x, 0, box.position.y);
				var data = _asset.SpawnFromPool(box, pos, _boxContainer);
				
				if (data != null) _boxes.Add(data);
			}
		}
	}
}