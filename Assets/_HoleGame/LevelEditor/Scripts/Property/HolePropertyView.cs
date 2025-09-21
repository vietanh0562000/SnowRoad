namespace HoleBox
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine.UI;

	public class HolePropertyView : ABasePreviewPropertyView
	{
		[SerializeField] private ColorButton    _buttonPrefab;
		[SerializeField] private Transform      _buttonContainer;
		[SerializeField] private Toggle         _closedToggle;
		[SerializeField] private TMP_InputField _closedIpt;
		[SerializeField] private Toggle         _lockedToggle;
		[SerializeField] private TMP_Dropdown   _ddLink;

		private HoleBoxData                  _holeBoxData;
		private List<ColorButton>            _colorButtons = new();
		private Dictionary<int, HoleBoxData> _dict         = new();
		private ColorButton                  _selectedButton;
		private int                          _currentCloseValue;

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
			
			_holeBoxData       = _current.Data as HoleBoxData;
			_currentCloseValue = _holeBoxData.numberToClose;
			
			UpdateFollowData();
		}
		
		private void UpdateFollowData()
		{
			_selectedButton = _colorButtons.Find(b => b.ColorInt == _holeBoxData.id);
			_selectedButton.SetSelected(true);
			
			_closedToggle.SetIsOnWithoutNotify(_holeBoxData.closedHole);
			_closedToggle.onValueChanged.RemoveAllListeners();
			_closedToggle.onValueChanged.AddListener(OnClosedToggleChanged);
			
			_closedIpt.SetTextWithoutNotify(_holeBoxData.numberToClose.ToString());
			_closedIpt.onValueChanged.RemoveAllListeners();
			_closedIpt.onValueChanged.AddListener(OnClosedNumberChanged);
			
			_lockedToggle.SetIsOnWithoutNotify(_holeBoxData.lockedHole);
			_lockedToggle.onValueChanged.RemoveAllListeners();
			_lockedToggle.onValueChanged.AddListener(OnLockedToggleChanged);
			
			_ddLink.gameObject.SetActive(!_isPreview && _holeBoxData.lockedHole);
			_ddLink.onValueChanged.RemoveAllListeners();
			
			_dict.Clear();
			if (!_isPreview)
			{
				InitDDLink();
				_ddLink.onValueChanged.AddListener(OnLinkChanged);
			}
		}

		private void InitDDLink()
		{
			var holes         = FindObjectsByType<LEHole>(FindObjectsSortMode.None);
			var keyHoleNames  = new List<string>();
			int selectedIndex = -1;
			int index = 0;
			
			for (int i = 0; i < holes.Length; i++)
			{
				var holeData = holes[i].Data as HoleBoxData;
				if (holeData == null) continue;
				
				if (holes[i] == _current)
				{
					keyHoleNames.Add("[Self]");
					_dict.Add(index, _holeBoxData);
					if (_holeBoxData.keyPos == Vector2Int.zero)
					{
						selectedIndex = index;
					}
				}
				else
			    {
			        keyHoleNames.Add($"Hole: {holeData.id}");
			        _dict.Add(index, holeData);
			        
			        if (holeData.position == _holeBoxData.keyPos)
			        {
			            selectedIndex = index; // Highlight currently selected keyHole
			        }
			    }
			        
				index++;
			}
			
			_ddLink.options.Clear();
			_ddLink.AddOptions(keyHoleNames);
			_ddLink.SetValueWithoutNotify(selectedIndex);
		}
		
		private void OnClosedNumberChanged(string arg0)
		{
			if (int.TryParse(arg0, out int num))
			{
				_holeBoxData.numberToClose = num;
				_currentCloseValue         = num;
				_current.UpdateFollowData();
			}
			else
			{
				_closedIpt.SetTextWithoutNotify(_currentCloseValue.ToString());
			}
		}

		private void OnLockedToggleChanged(bool arg0)
		{
			_holeBoxData.lockedHole = arg0;
			_ddLink.gameObject.SetActive(!_isPreview && arg0);
			_current.UpdateFollowData();
		}

		private void OnClosedToggleChanged(bool arg0)
		{
			_closedIpt.gameObject.SetActive(arg0);
			
			_holeBoxData.closedHole = arg0;
			_current.UpdateFollowData();
		}
		
		private void OnLinkChanged(int index)
		{
			var data = _dict[index];
			if (data != _holeBoxData)
			{
				_holeBoxData.keyPos = data.position;
				_current.UpdateFollowData();
			}
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
			
			_holeBoxData.id = color;
			_current.UpdateFollowData();
		}
	}
}