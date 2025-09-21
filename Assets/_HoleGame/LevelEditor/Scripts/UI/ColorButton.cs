namespace HoleBox
{
	using UnityEngine;
	using UnityEngine.UI;
	using System;

	public class ColorButton : MonoBehaviour
	{
		[SerializeField] private Image      _colorImage;
		[SerializeField] private Button     _button;
		[SerializeField] private GameObject _selectedIndicator;
		
		private int _colorInt;
		private bool _isSelected;

		public int                 ColorInt => _colorInt;
		public event Action<int> OnColorSelected;
		
		private void Awake()
		{
			_button.onClick.AddListener(OnButtonClick);
		}
		
		public void Init(int colorInt, Color color)
		{
			_colorInt         = colorInt;
			_colorImage.color = color;
			SetSelected(false);
		}
		
		private void OnButtonClick()
		{
			if (!_isSelected)
			{
				SetSelected(true);
				OnColorSelected?.Invoke(_colorInt);
			}
		}
		
		public void SetSelected(bool selected)
		{
			_isSelected = selected;
			_selectedIndicator.SetActive(selected);
		}
	}
}