namespace HoleBox
{
    using UnityEngine;
    using System.Reflection;
    using System.Collections.Generic;

    public class DynamicLEAssetInfoUI : MonoBehaviour
    {
        [SerializeField] private Sprite[]             _icons; // Chunk, Hole, Obstacle, Tunnel
        [SerializeField] private LEAsset              _leAsset;
        [SerializeField] private LEAssetDisplayItemUI _uiItemPrefab; // Prefab for displaying each asset's info (e.g., a Panel with a Text child)
        [SerializeField] private Transform            _uiContainer;   // Parent transform for instantiated UI items
        

        public void Init()
        {
            PopulateUIFromLEAsset();
        }
        
        public void Show(bool show) { gameObject.SetActive(show); }

        void PopulateUIFromLEAsset()
        {
            if (_leAsset == null) return;

            PropertyInfo[] properties = _leAsset.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            int index = 0;
            foreach (PropertyInfo propInfo in properties)
            {
                // We are interested in properties that return Component or GameObject types,
                // as these are likely to hold our assets.
                if (typeof(Component).IsAssignableFrom(propInfo.PropertyType) || 
                    typeof(GameObject).IsAssignableFrom(propInfo.PropertyType))
                {
                    object propValue = null;
                    try
                    {
                        propValue = propInfo.GetValue(_leAsset);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to get value for property '{propInfo.Name}' of type '{propInfo.PropertyType.Name}' on LEAsset: {e.Message}");
                        continue;
                    }

                    if (propValue == null) continue;

                    GameObject assetGameObject = null;
                    if (propValue is Component componentValue)
                    {
                        assetGameObject = componentValue.gameObject;
                    }
                    else if (propValue is GameObject goValue)
                    {
                        assetGameObject = goValue;
                    }

                    if (assetGameObject != null)
                    {
                        ALESpawnItem spawnItem = assetGameObject.GetComponent<ALESpawnItem>();
                        if (spawnItem == null) // Try children if not on root
                        {
                            spawnItem = assetGameObject.GetComponentInChildren<ALESpawnItem>();
                        }

                        if (spawnItem != null && spawnItem.Data != null)
                        {
                            InstantiateAndSetupUIItem(propInfo.Name, GetSprite(index), spawnItem);
                            index++;
                        }
                        else
                        {
                            Debug.LogWarning($"Property '{propInfo.Name}' of type '{propInfo.PropertyType.Name}' " +
                                             $"on LEAsset does not have a usable ALESpawnItem component on itself or its children with valid Data.");
                        }
                    }
                }
            }
        }

        private Sprite GetSprite(int index)
        {
            if (index < 0 || index >= _icons.Length)
            {
                return null;
            }
            
            return _icons[index];
        }

        void InstantiateAndSetupUIItem(string propertyName, Sprite sprite, ALESpawnItem spawnItem)
        {
            // Instantiate the prefab, which should have LEAssetDisplayItemUI component
            LEAssetDisplayItemUI uiInstance = Instantiate(_uiItemPrefab, _uiContainer);
            uiInstance.gameObject.SetActive(true);
            
            if (uiInstance != null)
            {
                uiInstance.Setup(propertyName, sprite, spawnItem);
                uiInstance.OnItemSelected += HandleAssetItemSelected; // Subscribe to the event
                Debug.Log($"Created UI for Asset: {propertyName}, ID: {spawnItem.Data.id}");
            }
            else
            {
                // This case should ideally not happen if _uiItemPrefab is correctly assigned
                // and the prefab has the LEAssetDisplayItemUI script.
                Debug.LogError($"Failed to instantiate or get LEAssetDisplayItemUI component from prefab for '{propertyName}'. Ensure _uiItemPrefab is assigned and has the LEAssetDisplayItemUI script.");
            }
        }

        private void HandleAssetItemSelected(ALESpawnItem selectedSpawnItem)
        {
            LevelEditorManager.Instance.SelectedPrefab = selectedSpawnItem;
        }
        
        // It's good practice to also unsubscribe if DynamicLEAssetInfoUI itself is destroyed
        // to prevent LEAssetDisplayItemUI instances from holding a reference to a destroyed object.
        void OnDestroy()
        {
            foreach (Transform child in _uiContainer)
            {
                LEAssetDisplayItemUI itemUI = child.GetComponent<LEAssetDisplayItemUI>();
                if (itemUI != null)
                {
                    itemUI.OnItemSelected -= HandleAssetItemSelected;
                }
            }
        }
    }
} 