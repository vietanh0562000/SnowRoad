namespace HoleBox
{
	using BasePuzzle.PuzzlePackages.Core;
	using UnityEngine;

	public abstract class ALESpawnItem : MonoBehaviour
	{
		[SerializeField] protected GameObject HlGameObject;
		
		[SerializeField] protected SpriteRenderer[] SpriteRenderers;
		[SerializeField] protected Sprite           NormalSprite;
		
		public GameObject HLGo => HlGameObject;
		
		private void OnValidate()
		{
			if (SpriteRenderers.Length == 0)
			{
				SpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
			}
		}

		public virtual bool IsActionItem                                                           => false;
		public virtual bool IsPlaceable                                                            => true;
		public virtual bool NeedGridCheck                                                          => true;
		public virtual bool InnerOverlapCheck(LEGrid grid, LESpawner spawner, Vector2Int position) => false;
		public virtual void OnPlacedAction(LEGrid grid, LESpawner spawner, Vector2Int position)    { }


		public abstract BoxData Data                { get; }
		public abstract bool    IsAbleToChangeSwap { get; }

		public abstract ALESpawnItem SpawnFromPool();
		public abstract void         SendToPool();
		public abstract void         SetUpData();
		public abstract void         Swap();
		
		public abstract void         Highlight(bool value);

		public abstract void CopyData(BoxData data);
		public abstract void UpdateFollowData();
		
		protected void ChangeColor()
		{
			var color = GameAssetManager.Instance.GetColor(Data.id);
			ChangeColorInternal(color);
		}

		protected void ChangeColorInternal(Color color)
		{
			foreach (var spriteRenderer in SpriteRenderers)
			{
				spriteRenderer.color = color;
			}
		}

		protected void ChangeSprite(Sprite sprite)
		{
			foreach (var spriteRenderer in SpriteRenderers)
			{
				spriteRenderer.sprite = sprite;
			}
		}

		protected void ChangeNormalSprite() { ChangeSprite(NormalSprite); }
		
		protected void ChangeSprite(Sprite sprite, Color color)
		{
			foreach (var spriteRenderer in SpriteRenderers)
			{
				spriteRenderer.sprite = sprite;
				spriteRenderer.color = color;
			}
		}

		public void SetPosition(Vector2Int position)
		{
			Data.position = position;
		}
	}
}