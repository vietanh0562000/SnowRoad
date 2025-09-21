using UnityEngine;

namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DevTools.Extensions;
    using TMPro;

    public class PanelInspectContainer : MonoBehaviour
    {
        [SerializeField] private GameObject _goContainer;
        [SerializeField] private TMP_Dropdown _ddCapacity;
        
        [SerializeField] private ColorButton _buttonPrefab;
        [SerializeField] private Transform   _buttonContainer;
		
        private List<ColorButton> _colorButtons = new List<ColorButton>();
        private ColorButton       _selectedButton;
        
        private QueueElementUI _current;

        private void Start()
        {
            SetupCapacityDropdown();
            SpawnColorButtons();
            
            QueueElementUI.OnInspectRequested -= QueueElementUIOnOnInspectRequested;
            QueueElementUI.OnInspectRequested += QueueElementUIOnOnInspectRequested;
        }
        
        private void SetupCapacityDropdown()
        {
            _ddCapacity.ClearOptions();

            List<TMP_Dropdown.OptionData> options = PanelMapConfig.CapacitySizes
                .Select(size => new TMP_Dropdown.OptionData(size.ToString()))
                .ToList();
            _ddCapacity.AddOptions(options);
                
            _ddCapacity.SetValueWithoutNotify(3);
            _ddCapacity.onValueChanged.AddListener(OnCapacityChanged);
        }
        
        private void OnCapacityChanged(int arg0)
        {
            _current.ContainerData.capacity = PanelMapConfig.CapacitySizes[arg0];
            _current.ReUpdate();
        }

        private void QueueElementUIOnOnInspectRequested(QueueElementUI obj)
        {
            _goContainer.SetActive(true);
            
            _current = obj;
            int index = PanelMapConfig.CapacitySizes.IndexOf(_current.ContainerData.Capacity);
            _ddCapacity.SetValueWithoutNotify(index);
            
            foreach (var b in _colorButtons)
            {
                b.SetSelected(false);
            }
            
            _selectedButton = _colorButtons.Find(b => b.ColorInt == _current.ContainerData.id);
            _selectedButton.SetSelected(true);
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

            _current.ContainerData.id = color;
            _current.ReUpdate();
        }

        private void OnDestroy()
        {
            QueueElementUI.OnInspectRequested -= QueueElementUIOnOnInspectRequested;
        }
    }
}
