namespace HoleBox
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using BasePuzzle.PuzzlePackages.Core;

    public class QueueManagerUI : MonoBehaviour
    {
        [SerializeField] private Button _addQueueButton; // Button to add a new queue
        [SerializeField] private RectTransform _queuesContainer; // Content of the horizontal ScrollRect
        [SerializeField] private QueueUI _queuePrefab; // Prefab for a single queue
        // ScrollRect is optional but recommended for many queues
        [SerializeField] private ScrollRect _horizontalScrollRect;

        private List<QueueUI> _queues = new List<QueueUI>();
        private int _nextQueueCounter = 1;
        
        private List<ContainerQueueData> _containerQueueData = new List<ContainerQueueData>();

        private void Start()
        {
            _addQueueButton.onClick.RemoveAllListeners();
            _addQueueButton.onClick.AddListener(HandleAddQueueClick);

            // Ensure the add queue button is last if it's part of the container
            // Or manage its position separately if it's outside the scrollable content
            if (_addQueueButton != null && _addQueueButton.transform.parent == _queuesContainer)
            {
                _addQueueButton.transform.SetAsLastSibling();
            }
        }

        public void OnCreateNewMap()
        {
            _containerQueueData.Clear();
            foreach (var queue in _queues)
            {
                if (queue != null)
                {
                    queue.OnQueueBecameEmpty -= HandleQueueBecameEmpty;
                }
                
                Destroy(queue.gameObject);
            }
            
            _queues.Clear();

            _nextQueueCounter = 1;
            //HandleAddQueueClick(); // add first queue
        }

        public void SetQueueData(List<ContainerQueueData> containerQueueData)
        {
            _containerQueueData = containerQueueData;
        }

        public void SetUp(List<ContainerQueueData> containerQueueData)
        {
            _containerQueueData = containerQueueData;
            foreach (var queueData in _containerQueueData)
            {
                QueueUI newQueue = Instantiate(_queuePrefab, _queuesContainer);
                newQueue.SetUp(_nextQueueCounter, queueData);
                _nextQueueCounter++;
                _queues.Add(newQueue);
                
                newQueue.OnQueueBecameEmpty         += HandleQueueBecameEmpty; // Optional
                newQueue.OnRemoveThisQueueRequested += HandleRemoveQueueRequested; // Subscribe to remove request
                
                // If the _addQueueButton is part of the _queuesContainer (e.g. for layout purposes),
                // ensure it always stays at the end after adding a new queue.
                if (_addQueueButton != null && _addQueueButton.transform.parent == _queuesContainer)
                {
                    _addQueueButton.transform.SetAsLastSibling();
                }
            }
        }

        private void HandleAddQueueClick()
        {
            if (_queuePrefab == null || _queuesContainer == null) return;

            QueueUI newQueue = Instantiate(_queuePrefab, _queuesContainer);
            
            var containerQueueData = new ContainerQueueData();
            newQueue.Initialize(_nextQueueCounter, containerQueueData);
            _nextQueueCounter++;

            newQueue.OnQueueBecameEmpty         += HandleQueueBecameEmpty; // Optional
            newQueue.OnRemoveThisQueueRequested += HandleRemoveQueueRequested; // Subscribe to remove request

            _queues.Add(newQueue);
            _containerQueueData.Add(containerQueueData);

            // If the _addQueueButton is part of the _queuesContainer (e.g. for layout purposes),
            // ensure it always stays at the end after adding a new queue.
            if (_addQueueButton != null && _addQueueButton.transform.parent == _queuesContainer)
            {
                _addQueueButton.transform.SetAsLastSibling();
            }
            
            _horizontalScrollRect.horizontalNormalizedPosition = 1f; // Scroll to the right
        }

        private void HandleQueueBecameEmpty(QueueUI emptyQueue) // Optional
        {
            
        }
        
        private void HandleRemoveQueueRequested(QueueUI queueToRemove)
        {
            if (queueToRemove != null && _queues.Contains(queueToRemove))
            {
                queueToRemove.OnQueueBecameEmpty         -= HandleQueueBecameEmpty;
                queueToRemove.OnRemoveThisQueueRequested -= HandleRemoveQueueRequested;
                
                int index = _queues.IndexOf(queueToRemove);
                
                _queues.Remove(queueToRemove);
                _containerQueueData.RemoveAt(index);
                
                Destroy(queueToRemove.gameObject);

                ReIndexQueues();
            }
        }
        
        private void ReIndexQueues()
        {
            for (int i = 0; i < _queues.Count; i++)
            {
                _queues[i].UpdateQueueIndex(i + 1); // Update visual index (1-based)
            }
            
            _nextQueueCounter = _queues.Count + 1; // Update next queue index
        }

        void OnDestroy()
        {
            if (_addQueueButton != null)
            {
                _addQueueButton.onClick.RemoveAllListeners();
            }
            foreach (var queue in _queues)
            {
                if (queue != null)
                {
                    queue.OnQueueBecameEmpty -= HandleQueueBecameEmpty;
                }
            }

            _queues.Clear();
        }
    }
} 