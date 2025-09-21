using Sirenix.OdinInspector;
using UnityEngine;

namespace PuzzleGames
{
    public class UILoadingScaler : MonoBehaviour
    {
        [Header("Root Ratio (W/H)")] public float designAspect = 9f / 16f; // = 0.5625

        private void Start() { UpdateRatio(); }

        [Button]
        public void UpdateRatio()
        {
            float currentAspect = Camera.main.aspect;

            if (currentAspect > designAspect)
            {
                float scaleFactor = designAspect / currentAspect;
                transform.localScale = Vector3.one * scaleFactor;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
    }
}