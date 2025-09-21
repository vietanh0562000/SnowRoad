using UnityEngine;

namespace HoleBox
{
    public class GameAssetManager : Singleton<GameAssetManager>
    {
        [SerializeField] private MaterialStorage materialStorage;

        /// <summary>
        /// Get material by ID from the MaterialStorage.
        /// </summary>
        /// <param name="id">The material ID.</param>
        /// <returns>The material if found, otherwise null.</returns>
        public MaterialStorage.MaterialEntry GetMaterialEntryById(int id)
        {
            if (materialStorage == null)
            {
                Debug.LogError("MaterialStorage is not assigned in GameAssetManager!");
                return null;
            }

            return materialStorage.GetMaterialEntryById(id);
        }

        public Material GetMaterialHole(int id)
        {
            if (materialStorage == null)
            {
                Debug.LogError("MaterialStorage is not assigned in GameAssetManager!");
                return null;
            }

            return materialStorage.GetMaterialHoleById(id);
        }
        
        public Color GetColor(int id)
        {
            return materialStorage.GetColorById(id);
        }
        
        public int TotalChangedColors => materialStorage.MaterialCount;
    }
}