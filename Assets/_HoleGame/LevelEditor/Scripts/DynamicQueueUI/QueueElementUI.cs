namespace HoleBox
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro; // For TextMeshPro components
    using System; // For Action

    public class QueueElementUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text      _dataText;
        [SerializeField] private Image         _image;
        [SerializeField] private Button        _selfButton;
        [SerializeField] private RectTransform _selfRect;
        
        [Space(4)]
        
        [SerializeField] private Button        _removeButton;
        [SerializeField] private Button        _addAboveButton;
        [SerializeField] private Button        _addBelowButton;

        private ContainerData _containerData;
        
        public ContainerData ContainerData => _containerData;

        public event Action<QueueElementUI> OnRemoveRequested;
        public event Action<QueueElementUI> OnAddAboveRequested;
        public event Action<QueueElementUI> OnAddBelowRequested;
        public static event Action<QueueElementUI> OnInspectRequested;

        public void Initialize(ContainerData containerData)
        {
            _containerData = containerData;

            _dataText.text = $"{containerData.capacity}";
            _image.color = GameAssetManager.Instance.GetColor(containerData.id);

            _removeButton.onClick.RemoveAllListeners(); // Clear previous listeners
            _removeButton.onClick.AddListener(HandleRemoveClick);
            
            _selfButton.onClick.RemoveAllListeners(); // Clear previous listeners
            _selfButton.onClick.AddListener(OnSelfButtonClick);
            
            _addAboveButton.onClick.RemoveAllListeners();
            _addAboveButton.onClick.AddListener(HandleAddAboveClick);

            _addBelowButton.onClick.RemoveAllListeners();
            _addBelowButton.onClick.AddListener(HandleAddBelowClick);
            
            OnInspectRequested -= HandleInspection;
            OnInspectRequested += HandleInspection;
        }

        public void ReUpdate()
        {
            _dataText.text = $"{_containerData.capacity}";
            _image.color   = GameAssetManager.Instance.GetColor(_containerData.id);
        }

        private void HandleRemoveClick()
        {
            OnRemoveRequested?.Invoke(this);
        }

        public void OnSelfButtonClick()
        {
            OnInspectRequested?.Invoke(this);
        }

        private void HandleAddAboveClick()
        {
            OnAddAboveRequested?.Invoke(this);
            HandleInspection(null);
        }

        private void HandleAddBelowClick()
        {
            OnAddBelowRequested?.Invoke(this);
            HandleInspection(null);
        }

        private void HandleInspection(QueueElementUI inspectedElement)
        {
            bool self = this == inspectedElement;
            
            var size = _selfRect.sizeDelta;
            size.y = self ? 175 : 65;
            _selfRect.sizeDelta = size;
            
            SetSelectionState(self);
        }

        private void SetSelectionState(bool isSelected)
        {
            if (_addAboveButton != null) _addAboveButton.gameObject.SetActive(isSelected);
            if (_addBelowButton != null) _addBelowButton.gameObject.SetActive(isSelected);
        }

        void OnDestroy()
        {
            OnInspectRequested -= HandleInspection;
            
            // Clean up event listeners from the button to prevent issues if the button outlives this script instance
            if (_removeButton != null)
            {
                _removeButton.onClick.RemoveAllListeners();
            }
            if (_addAboveButton != null)
            {
                _addAboveButton.onClick.RemoveAllListeners();
            }
            if (_addBelowButton != null)
            {
                _addBelowButton.onClick.RemoveAllListeners();
            }
            // Clear all subscribers to this item's event
            OnRemoveRequested = null;
            OnAddAboveRequested = null;
            OnAddBelowRequested = null;
        }
    }
} 