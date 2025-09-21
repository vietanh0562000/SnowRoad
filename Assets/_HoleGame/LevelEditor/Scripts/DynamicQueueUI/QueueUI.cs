namespace HoleBox
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using System;
    using System.Collections;
    using TMPro;

    public class QueueUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text       _dataText;
        [SerializeField] private Button         _removeThisQueueButton; 
        [SerializeField] private RectTransform  _elementsContainer; // Content of the ScrollRect where elements are parented
        [SerializeField] private QueueElementUI _elementPrefab; // Prefab for items in this queue
        
        // ScrollRect is optional but recommended for long lists
        [SerializeField] private ScrollRect _verticalScrollRect;

        [Space(5)] 
        [SerializeField] private ContentSizeFitter _fitter;

        private List<QueueElementUI> _elements           = new List<QueueElementUI>();
        private ContainerQueueData   _containerQueueData;

        public event Action<QueueUI> OnQueueBecameEmpty; // Optional: If manager needs to know
        public event Action<QueueUI> OnRemoveThisQueueRequested; // If queues can be removed by themselves

        public void Initialize(int id, ContainerQueueData containerQueueData)
        {
            _dataText.text  = "Q_" + id; // "Queue 1", "Queue 2", etc.
            gameObject.name = "Q_" + id;
            
            _containerQueueData = containerQueueData;
            
            AddNewElement(0, true); // add 1 first element
            Fitter();

            QueueElementUI.OnInspectRequested -= OnElementInspectRequested;
            QueueElementUI.OnInspectRequested += OnElementInspectRequested;
            
            if (_removeThisQueueButton != null)
            {
                _removeThisQueueButton.onClick.RemoveAllListeners();
                _removeThisQueueButton.onClick.AddListener(HandleRemoveThisQueueClick);
            }
        }

        public void SetUp(int id, ContainerQueueData containerQueueData)
        {
            _dataText.text  = "Q_" + id; // "Queue 1", "Queue 2", etc.
            gameObject.name = "Q_" + id;
            
            _containerQueueData = containerQueueData;
            
            if (_removeThisQueueButton != null)
            {
                _removeThisQueueButton.onClick.RemoveAllListeners();
                _removeThisQueueButton.onClick.AddListener(HandleRemoveThisQueueClick);
            }

            foreach (var containerData in containerQueueData.containerDatas)
            {
                QueueElementUI newElement = Instantiate(_elementPrefab, _elementsContainer);
                
                newElement.Initialize(containerData);

                newElement.OnRemoveRequested += HandleElementRemoveRequested;
                newElement.OnAddAboveRequested += HandleAddAboveRequested;
                newElement.OnAddBelowRequested += HandleAddBelowRequested;
                _elements.Add(newElement);
            }
            
            QueueElementUI.OnInspectRequested -= OnElementInspectRequested;
            QueueElementUI.OnInspectRequested += OnElementInspectRequested;

            if (containerQueueData.containerDatas.Count > 0)
            {
                _elements[0].OnSelfButtonClick();
            }
        }

        private void AddNewElement(int index, bool requestInspect)
        {
            if (_elementPrefab == null || _elementsContainer == null) return;

            QueueElementUI newElement = Instantiate(_elementPrefab, _elementsContainer);
            
            newElement.transform.SetSiblingIndex(index); 

            var containerData = new ContainerData()
            {
                capacity = 16
            };
            
            newElement.Initialize(containerData);
            
            if (index < _containerQueueData.containerDatas.Count)
            {
                _containerQueueData.containerDatas.Insert(index, containerData);
            }
            else
            {
                _containerQueueData.containerDatas.Add(containerData);
            }

            newElement.OnRemoveRequested += HandleElementRemoveRequested;
            newElement.OnAddAboveRequested += HandleAddAboveRequested;
            newElement.OnAddBelowRequested += HandleAddBelowRequested;
            _elements.Insert(index, newElement);

            if (requestInspect)
            {
                newElement.OnSelfButtonClick();
            }
        }
        
        private void HandleAddAboveRequested(QueueElementUI sourceElement)
        {
            int index = _elements.IndexOf(sourceElement);
            if (index > -1)
            {
                AddNewElement(index + 1, true);
            }
        }

        private void HandleAddBelowRequested(QueueElementUI sourceElement)
        {
            int index = _elements.IndexOf(sourceElement);
            if (index > -1)
            {
                AddNewElement(index, true);
            }
        }

        private void OnElementInspectRequested(QueueElementUI sourceElement)
        {
            Fitter();
        }

        private void Fitter()
        {
            _fitter.enabled = false;

            IEnumerator CoWait()
            {
                yield return null;
                _fitter.enabled = true;
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalScrollRect.content);
            }
            
            StartCoroutine(CoWait());
        }

        private void HandleElementRemoveRequested(QueueElementUI elementToRemove)
        {
            if (_elements.Count == 1) return; // Don't allow removing the last element
            
            if (elementToRemove != null)
            {
                elementToRemove.OnRemoveRequested -= HandleElementRemoveRequested;
                elementToRemove.OnAddAboveRequested -= HandleAddAboveRequested;
                elementToRemove.OnAddBelowRequested -= HandleAddBelowRequested;
                _elements.Remove(elementToRemove);
                
                _containerQueueData.RemoveContainer(elementToRemove.ContainerData);
                Destroy(elementToRemove.gameObject);
                

                if (_elements.Count == 0)
                {
                    OnQueueBecameEmpty?.Invoke(this);
                }
            }
        }
        
        private void HandleRemoveThisQueueClick()
        {
            OnRemoveThisQueueRequested?.Invoke(this);
        }
        
        public void UpdateQueueIndex(int i)
        {
            _dataText.text = "Q_" + i; // "Queue 1", "Queue 2", etc.
        }

        void OnDestroy()
        {
            QueueElementUI.OnInspectRequested -= OnElementInspectRequested;
            
            if (_removeThisQueueButton != null)
            {
                _removeThisQueueButton.onClick.RemoveAllListeners();
            }
            // Clean up elements and their events
            foreach (var element in _elements)
            {
                if (element != null) // Check in case it was already destroyed
                {
                    element.OnRemoveRequested -= HandleElementRemoveRequested;
                    element.OnAddAboveRequested -= HandleAddAboveRequested;
                    element.OnAddBelowRequested -= HandleAddBelowRequested;
                }
            }
            _elements.Clear();
            OnQueueBecameEmpty         = null;
            OnRemoveThisQueueRequested = null;
        }
    }
} 