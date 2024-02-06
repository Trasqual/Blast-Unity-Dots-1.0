using Datas;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class BlockAuthoring : MonoBehaviour
    {
        public GameObject BlockPrefab;

        private class BlockBaker : Baker<BlockAuthoring>
        {
            public override void Bake(BlockAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                BlockPresentationGO blockPresentationGO = new BlockPresentationGO();
                blockPresentationGO.BlockPrefab = authoring.BlockPrefab;
                AddComponentObject(entity, blockPresentationGO);
            }
        }
    }
}