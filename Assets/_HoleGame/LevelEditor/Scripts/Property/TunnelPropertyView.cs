namespace HoleBox
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine.UI;

	public class TunnelPropertyView : ABasePreviewPropertyView
	{
		[SerializeField] private ColorButton      _buttonPrefab;
		[SerializeField] private Transform        _buttonContainer;
		[SerializeField] private GameObject       _remainObj;
		[SerializeField] private Toggle           _randomColor;
		[SerializeField] private RandomTunnelView _randomTunnel;
		[SerializeField] private TMP_InputField   _remainIpt;
		[SerializeField] private TMP_Dropdown     _ddDirection;

		private TunnelData        _tunnelData;
		private List<ColorButton> _colorButtons = new List<ColorButton>();
		private ColorButton       _selectedButton;
		private int               _currentRemainValue;
		

		public override void Init(ALESpawnItem item)
		{
			base.Init(item);
			
			if (_colorButtons.Count == 0)
			{
				SpawnColorButtons();
			}

			foreach (var b in _colorButtons)
			{
				b.SetSelected(false);
			}
			
			_tunnelData         = _current.Data as TunnelData;
			_currentRemainValue = _tunnelData.remainSpawn;
			
			UpdateFollowData();
		}

		private void UpdateFollowData()
		{
			_selectedButton = _colorButtons.Find(b => b.ColorInt == _tunnelData.id);
			_selectedButton.SetSelected(true);
			
			_randomColor.isOn = _tunnelData.randomColor;
			_randomColor.onValueChanged.AddListener(OnChooseRandomColor);
			
			_remainIpt.text = _tunnelData.remainSpawn.ToString();
			_remainIpt.onValueChanged.RemoveAllListeners();
			_remainIpt.onValueChanged.AddListener(OnRemainNumberChanged);

			_randomTunnel.SetData(_tunnelData.colorQueue);
			_randomTunnel.OnChangeValue(SetRandomColorQueue);
			
			var currentDir = _tunnelData.direction;
			int sum        = currentDir.x + currentDir.y;

			int index = 0;
			if (sum < 0)
			{
				// down, left
				if (currentDir.y < 0)
				{
					index = 1;
				}
				else
				{
					index = 2;
				}
			}
			else
			{
				// top, right
				if (currentDir.y > 0)
				{
					index = 0;
				}
				else
				{
					index = 3;
				}
			}
			
			_ddDirection.SetValueWithoutNotify(index);
			_ddDirection.onValueChanged.RemoveAllListeners();
			_ddDirection.onValueChanged.AddListener(OnDirectionChanged);

			RefreshView();
		}
		private void SetRandomColorQueue(Queue<int> listColor)
		{
			_tunnelData.colorQueue = listColor;
		}

		private void RefreshView()
		{
			var randomColor = _tunnelData.randomColor;

			_buttonContainer.gameObject.SetActive(!randomColor);
			_remainObj.SetActive(!randomColor);
			_randomTunnel.gameObject.SetActive(randomColor);
		}
		
		private void OnChooseRandomColor(bool b)
		{
			_tunnelData.randomColor = b;
			RefreshView();
		}

		private void OnRemainNumberChanged(string arg0)
		{
			if (int.TryParse(arg0, out int num))
			{
				_tunnelData.remainSpawn = num;
				_currentRemainValue     = num;
				_current.UpdateFollowData();
			}
			else
			{
				_remainIpt.SetTextWithoutNotify(_currentRemainValue.ToString());
			}
		}
		
		private void OnDirectionChanged(int index)
		{
			var dir = Vector2Int.up;
			if (index == 1)
			{
				dir = Vector2Int.down;
			}
			else if (index == 2)
			{
				dir = Vector2Int.left;
			}
			else if (index == 3)
			{
				dir = Vector2Int.right;
			}
			
			_tunnelData.direction = dir;
            _current.UpdateFollowData();
		}
		
		private void SpawnColorButtons()
		{
			// Spawn new buttons for each color
			int colorCount = GameAssetManager.Instance.TotalChangedColors;
			for (int i = 1; i <= colorCount; i++)
			{
				var button = Instantiate(_buttonPrefab, _buttonContainer);
				button.Init(i, GameAssetManager.Instance.GetColor(i));
				button.OnColorSelected += OnColorButtonSelected;
				_colorButtons.Add(button);
			}
		}
		
		private void OnColorButtonSelected(int color)
		{
			// Deselect previous button
			if (_selectedButton != null)
			{
				_selectedButton.SetSelected(false);
			}
			
			// Find and select new button
			_selectedButton = _colorButtons.Find(b => b.ColorInt == color);
			
			_tunnelData.id = color;
			_current.UpdateFollowData();
		}
	}
}