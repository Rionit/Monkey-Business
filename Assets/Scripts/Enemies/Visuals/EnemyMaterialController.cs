using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace MonkeyBusiness.Enemies.Visuals
{
    public class EnemyMaterialController : MonoBehaviour
    {
        public enum MaterialOrder
        {
            SKIN_CLOTHING,
            CLOTHING_SKIN
        }

        [field: SerializeField]
        [field: EnumButtons]
        public MaterialOrder Order { get; private set; } =  MaterialOrder.SKIN_CLOTHING;

        [FoldoutGroup("Material Variations")]
        [SerializeField]
        [Tooltip("Skin material for the gorilla")]
        [PreviewField(100, ObjectFieldAlignment.Right)]
        List<Material> _skinMaterials; 

        [FoldoutGroup("Material Variations")]
        [SerializeField]
        [Tooltip("Skin material for the gorilla")]
        [PreviewField(100, ObjectFieldAlignment.Right)]
        List<Material> _clothingMaterials; 


        [FoldoutGroup("Material Variations")]
        [ShowInInspector]
        [ReadOnly]
        [PreviewField(100, ObjectFieldAlignment.Right)]
        public Material UsedSkinMaterial { get; private set; }

        [FoldoutGroup("Material Variations")]
        [ShowInInspector]
        [ReadOnly]
        [PreviewField(100, ObjectFieldAlignment.Right)]
        public Material UsedClothingMaterial { get; private set; }

        [SerializeField]
        Renderer _renderer;

        void Awake()
        {
            UsedSkinMaterial = GetRandomMaterial(_skinMaterials);
            if(_clothingMaterials != null && _clothingMaterials.Count > 0)
            {
                UsedClothingMaterial = GetRandomMaterial(_clothingMaterials);
            }
            else
            {
                UsedClothingMaterial = null;
            }

            var materials = UsedClothingMaterial != null ? 
                Order switch
                {
                    MaterialOrder.SKIN_CLOTHING => new Material[2] { UsedSkinMaterial, UsedClothingMaterial },
                    MaterialOrder.CLOTHING_SKIN => new Material[2] { UsedClothingMaterial, UsedSkinMaterial },
                    _ => new Material[1] { UsedSkinMaterial }
                } : 
                new Material[1] { UsedSkinMaterial };

            _renderer.materials = materials;
        }

        Material GetRandomMaterial(List<Material> materials)
        {
            if (materials == null || materials.Count == 0)
            {
                Debug.LogError("Material list is empty. Returning null.");
                return null;
            }
            return materials[Random.Range(0, materials.Count)];
        }
    }
}
