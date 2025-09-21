using System.Collections.Generic;
using UnityEngine;

namespace HoleBox
{
    using System.Linq;
    
    [CreateAssetMenu(fileName = "MaterialStorage", menuName = "GameAssets/MaterialStorage")]
    public class MaterialStorage : ScriptableObject
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [System.Serializable]
        public class MaterialEntry
        {
            public int      id; // Unique ID for the material
            public Material material; // Material asset
            public Material materialHole; // Material asset
            public Mesh     mesh; // Mesh asset
            public Color    color; // Color of the material
            public LayerMask layerMask; // Layer mask for the material
        }

        [SerializeField] private List<MaterialEntry> materials = new List<MaterialEntry>();
        
        
        public int MaterialCount => materials.Count(_ => _.id > 0);

        /// <summary>
        /// Get a material by its ID.
        /// </summary>
        /// <param name="id">The ID of the material.</param>
        /// <returns>The corresponding material if found, null otherwise.</returns>
        public Material GetMaterialCharById(int id)
        {
            foreach (var entry in materials)
            {
                if (entry.id == id)
                {
                    return entry.material;
                }
            }

            Debug.LogWarning($"Material with ID '{id}' was not found!");
            return null;
        }
        
        public MaterialEntry GetMaterialEntryById(int id)
        {
            foreach (var entry in materials)
            {
                if (entry.id == id)
                {
                    return entry;
                }
            }

            Debug.LogWarning($"Material with ID '{id}' was not found!");
            return null;
        }

        /// <summary>
        /// Get a material by its ID.
        /// </summary>
        /// <param name="id">The ID of the material.</param>
        /// <returns>The corresponding material if found, null otherwise.</returns>
        public Color GetColorById(int id)
        {
            foreach (var entry in materials)
            {
                if (entry.id == id)
                {
                    return entry.color; // fallback
                }
            }

            Debug.LogWarning($"Material with ID '{id}' was not found!");
            return Color.white;
        }

        /// <summary>
        /// Get a material by its ID.
        /// </summary>
        /// <param name="id">The ID of the material.</param>
        /// <returns>The corresponding material if found, null otherwise.</returns>
        public Material GetMaterialHoleById(int id)
        {
            foreach (var entry in materials)
            {
                if (entry.id == id)
                {
                    return entry.materialHole;
                }
            }

            Debug.LogWarning($"Material with ID '{id}' was not found!");
            return null;
        }
        
        public Mesh GetMeshById(int id)
        {
            foreach (var entry in materials)
            {
                if (entry.id == id)
                {
                    return entry.mesh;
                }
            }
            
            return null;
        }
    }
}