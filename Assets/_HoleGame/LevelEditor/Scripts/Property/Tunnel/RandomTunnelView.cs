namespace HoleBox
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class RandomTunnelView : MonoBehaviour
    {
        [SerializeField] private RectTransform _queueContainer; // Parent container for the queue elements
        [SerializeField] private Button        _addButton; // Button for adding a new element to the queue
        [SerializeField] private GameObject    _queueElementPrefab; // Prefab for visualizing each element in the queue

        private Queue<int>          _colors  = new();
        private List<SelectColorUI> _queueUI = new();
        private Action<Queue<int>>  onChangeQueue;

        public void SetData(Queue<int> listColors)
        {
            _colors = listColors;

            _queueUI = new();

            // Clear current UI elements
            foreach (Transform child in _queueContainer)
            {
                Destroy(child.gameObject);
            }

            // Add UI elements for each color in the queue
            foreach (var colorId in _colors)
            {
                AddElementToUI(colorId);
            }
        }

        /// <summary>
        /// Refreshes the visual representation of the queue.
        /// </summary>
        private void RefreshQueue()
        {
            _colors = new();

            foreach (var queue in _queueUI)
            {
                _colors.Enqueue(queue.ID);
            }

            onChangeQueue?.Invoke(_colors);
        }

        /// <summary>
        /// Adds a new UI element to the queue view.
        /// </summary>
        /// <param name="colorId">The ID representing the color.</param>
        private void AddElementToUI(int colorId)
        {
            var element   = Instantiate(_queueElementPrefab, _queueContainer);
            var uiElement = element.GetComponent<SelectColorUI>();

            uiElement.Initialize(colorId);

            uiElement.OnChangeRequested += OnChangeValueQueue;
            uiElement.OnRemoveRequested += RemoveElementFromQueue;

            _queueUI.Add(uiElement);
        }
        private void OnChangeValueQueue() { RefreshQueue(); }

        /// <summary>
        /// Handles the removal of a specific UI element and updates the queue.
        /// </summary>
        /// <param name="elementUI">The element to be removed.</param>
        private void RemoveElementFromQueue(SelectColorUI elementUI)
        {
            _queueUI.Remove(elementUI);
            Destroy(elementUI.gameObject);
            RefreshQueue();
        }

        private void Start()
        {
            // Add button listener to insert new random elements
            _addButton.onClick.AddListener(() =>
            {
                AddElementToUI(1);
                RefreshQueue();
            });
        }
        public void OnChangeValue(Action<Queue<int>> action) { onChangeQueue = action; }
    }
}