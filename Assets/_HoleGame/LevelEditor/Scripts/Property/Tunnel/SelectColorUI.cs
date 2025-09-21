namespace HoleBox
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class SelectColorUI : MonoBehaviour
    {
        [SerializeField] private ColorDropdown _colorDropdown;
        [SerializeField] private Image         _dropdownImg;

        [SerializeField] private Button _removeButton;

        private int _id;

        public int ID => _id;

        public event Action<SelectColorUI> OnRemoveRequested;
        public event Action                OnChangeRequested;

        public void Initialize(int containerData)
        {
            _id = containerData;

            _removeButton.onClick.RemoveAllListeners(); // Clear previous listeners
            _removeButton.onClick.AddListener(HandleRemoveClick);
            PopulateColorDropdown();

            _colorDropdown.onValueChanged.RemoveAllListeners();
            _colorDropdown.onValueChanged.AddListener(HandleColorChange);

            _dropdownImg.color = GameAssetManager.Instance.GetColor(_id);
        }

        private void PopulateColorDropdown()
        {
            // Spawn new buttons for each color
            int colorCount = GameAssetManager.Instance.TotalChangedColors;

            _colorDropdown.ClearOptions();

            for (int i = 1; i <= colorCount; i++)
            {
                var colorOption = new TMP_Dropdown.OptionData("");
                colorOption.color = GameAssetManager.Instance.GetColor(i);

                _colorDropdown.options.Add(colorOption);
            }

            // Set the dropdown's value to match the current color ID
            int currentIndex = _id - 1;
            if (currentIndex >= 0)
                _colorDropdown.value = currentIndex;

            _colorDropdown.RefreshShownValue();
        }

        private void HandleColorChange(int selectedIndex)
        {
            // Update the container data and image color
            _id = selectedIndex + 1;

            OnChangeRequested?.Invoke();
            _dropdownImg.color = GameAssetManager.Instance.GetColor(_id);
        }


        private void HandleRemoveClick() { OnRemoveRequested?.Invoke(this); }


        void OnDestroy()
        {
            // Clean up event listeners from the button to prevent issues if the button outlives this script instance
            if (_removeButton != null)
            {
                _removeButton.onClick.RemoveAllListeners();
            }

            // Clear all subscribers to this item's event
            OnRemoveRequested = null;
        }
    }
}