namespace HoleBox
{
	using BasePuzzle.PuzzlePackages.Core;
	using UnityEngine;

	[DisallowMultipleComponent, SelectionBase]
	public class LETile : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _spriteRenderer;
		
		public void SendToPool()
		{
			_spriteRenderer.color= Color.gray;
			PrefabPool<LETile>.Release(this);
		}
		
		public void SetColor(Color color)
		{
			_spriteRenderer.color = color;
		}
	}
}