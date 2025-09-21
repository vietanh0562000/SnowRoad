namespace HoleBox
{
    using UnityEngine;
    using UnityEngine.UI; // Required for the Text component
    using System;
    using TMPro; // Required for Action
    // Assuming ALESpawnItem is defined. If it's in a namespace, add: using YourNamespace;

    public class LEAssetDisplayItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _propertyNameText; // Assign this in the Inspector for your UI prefab
        [SerializeField] private Image    _image;
        [SerializeField] private Button   _selectButton; // Assign this in the Inspector
        
        private ALESpawnItem _associatedSpawnItem;

        public event Action<ALESpawnItem> OnItemSelected;

        public void Setup(string propertyName, Sprite sprite, ALESpawnItem spawnItem)
        {
            _associatedSpawnItem = spawnItem;

            _propertyNameText.text = $"{propertyName}";
            _image.sprite          = sprite;
            
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners(); // Clear previous listeners if any
                _selectButton.onClick.AddListener(HandleButtonClick);
            }
            else
            {
                Debug.LogWarning("SelectButton is not assigned in LEAssetDisplayItemUI on prefab: " + gameObject.name);
            }
        }

        private void HandleButtonClick()
        {
            if (_associatedSpawnItem != null)
            {
                OnItemSelected?.Invoke(_associatedSpawnItem);
                Debug.Log($"Button clicked for: {_associatedSpawnItem.Data.id}");
            }
        }

        // It's good practice to remove listeners when the object is destroyed, 
        // especially if the event publisher (this) outlives the subscriber, 
        // or if subscribers can be dynamically added/removed.
        void OnDestroy()
        {
            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveListener(HandleButtonClick);
            }
            // Clear out all subscribers to this item's event to prevent memory leaks
            // if this item is destroyed but subscribers are still around.
            OnItemSelected = null; 
        }
    }
} 