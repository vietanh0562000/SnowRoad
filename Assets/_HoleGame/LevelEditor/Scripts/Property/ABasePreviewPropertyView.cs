namespace HoleBox
{
	using TMPro;
	using UnityEngine;

	[DisallowMultipleComponent]
	public abstract class ABasePreviewPropertyView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _nameTxt;
		
		protected ALESpawnItem _current;
		protected bool         _isPreview;
		
		public void SetCurrent(ALESpawnItem item)
		{
			_current = item;
		}

		public virtual void Init(ALESpawnItem item)
		{
			SetCurrent(item);
			_nameTxt.text = item.gameObject.name;
			_isPreview    = item.gameObject.name.Contains("Preview");
		}

		public void SetVisible(bool value)
		{
			gameObject.SetActive(value);
		}
	}
}