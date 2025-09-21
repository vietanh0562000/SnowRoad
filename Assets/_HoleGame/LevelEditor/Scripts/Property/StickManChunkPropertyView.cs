namespace HoleBox
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using TMPro;
	using UnityEngine.UI;

	public class StickManChunkPropertyView : ABasePreviewPropertyView
	{
		[SerializeField] private ColorButton    _buttonPrefab;
		[SerializeField] private Transform      _buttonContainer;
		[SerializeField] private Toggle         _hiddenToggle;
		[SerializeField] private TMP_InputField _frozenIpt;

		private StickManData      _stickManData;
		private List<ColorButton> _colorButtons = new List<ColorButton>();
		private ColorButton       _selectedButton;
		private int               _currentFrozenValue;

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

			_stickManData       = _current.Data as StickManData;
			_currentFrozenValue = _stickManData.intFrozen;
			
			UpdateFollowData();
		}

		private void UpdateFollowData()
		{
			_selectedButton = _colorButtons.Find(b => b.ColorInt == _stickManData.id);
			_selectedButton.SetSelected(true);
			
			_hiddenToggle.SetIsOnWithoutNotify(_stickManData.IsHidden);
			_hiddenToggle.onValueChanged.RemoveAllListeners();
			_hiddenToggle.onValueChanged.AddListener(OnHiddenToggleChanged);
			
			_frozenIpt.SetTextWithoutNotify(_stickManData.intFrozen.ToString());
			_frozenIpt.onValueChanged.RemoveAllListeners();
			_frozenIpt.onValueChanged.AddListener(OnFrozenChanged);
		}
		
		private void OnFrozenChanged(string arg0)
		{
			if (int.TryParse(arg0, out int frozen))
			{
				_stickManData.intFrozen = frozen;
				_currentFrozenValue     = frozen;
				_current.UpdateFollowData();
			}
			else
			{
				_frozenIpt.SetTextWithoutNotify(_currentFrozenValue.ToString());
			}
		}

		private void OnHiddenToggleChanged(bool arg0)
		{
			_stickManData.IsHidden = arg0;
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
			
			_stickManData.id = color;
			_current.UpdateFollowData();
		}
	}
}