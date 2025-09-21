using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // For List
using System.Linq; // For Select

namespace HoleBox
{
    using System;
    using System.Collections;
    using DevTools.Extensions;
    using BasePuzzle.PuzzlePackages.Core;
    using Sirenix.OdinInspector;
    using UnityEngine.Serialization;

    public class PanelMapConfig : MonoBehaviour
    {
        [SerializeField] private LESpawner _spawner;
        
        [TabGroup("Static")] [SerializeField] private TMP_Dropdown _ddCount;
        [TabGroup("Static")] [SerializeField] private TMP_Dropdown _ddCapacity;
        
        [TabGroup("Queue")] [SerializeField] private QueueManagerUI _queueManagerUI;
        
        private const int MinCount = 1;
        private const int MaxCount = 5;
        public static int[] CapacitySizes => new[] { 4, 8, 16, 32, 64 };

        private void Awake()
        {
            SetupCapacityDropdown();
            SetupCountDropdown();
            
            GameEvent<PanelMapSettings>.Register(LEEvents.CLICK_BTN_CREATE_ANEW, OnCreateNewMap, this);
        }
        
        private void OnDestroy()
        {
            GameEvent<PanelMapSettings>.Unregister(LEEvents.CLICK_BTN_CREATE_ANEW, OnCreateNewMap, this);
        }
        
        private void OnCreateNewMap(PanelMapSettings panelMapSettings)
        {
            _queueManagerUI.SetQueueData(_spawner.ContainerQueues);
            _queueManagerUI.OnCreateNewMap();
        }

        public void OnLoadOld()
        {
            int count    = _spawner.StaticContainerConfig.Count;
            int capacity = _spawner.StaticContainerConfig.Capacity;
            
            _ddCount.SetValueWithoutNotify(count - MinCount);

            int indexCap = CapacitySizes.IndexOf(capacity);
            if (indexCap > -1)
            {
                _ddCapacity.SetValueWithoutNotify(indexCap);
            }
            
            _queueManagerUI.SetUp(_spawner.ContainerQueues);
        }

        private void SetupCapacityDropdown()
        {
            _ddCapacity.ClearOptions();

            List<TMP_Dropdown.OptionData> options = CapacitySizes
                    .Select(size => new TMP_Dropdown.OptionData(size.ToString()))
                    .ToList();
                _ddCapacity.AddOptions(options);
                
            _ddCapacity.SetValueWithoutNotify(3);
            _ddCapacity.onValueChanged.RemoveAllListeners();
            _ddCapacity.onValueChanged.AddListener(OnCapacityChanged);
        }
    
        private void SetupCountDropdown()
        {
            _ddCount.ClearOptions();
            for (int i = MinCount; i <= MaxCount; i++)
            {
                _ddCount.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
            }
            
            _ddCount.SetValueWithoutNotify(3);
            _ddCount.onValueChanged.RemoveAllListeners();
            _ddCount.onValueChanged.AddListener(OnCountChanged);
        }

        private void OnCapacityChanged(int index)
        {
            int count    = MinCount + _ddCount.value;
            int capacity = CapacitySizes[index];
            
            _spawner.SetStaticContainerConfig(count, capacity);
        }
        
        private void OnCountChanged(int index)
        {
            int count    = MinCount + index;
            int capacity = CapacitySizes[_ddCapacity.value];
            
            _spawner.SetStaticContainerConfig(count, capacity);
        }
    }
}