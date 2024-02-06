using Datas;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class BlockSpawnerAuthoring : MonoBehaviour
    {
        public GameObject BlockPrefab;

        private class BlockSpawnerBaker : Baker<BlockSpawnerAuthoring>
        {

            public override void Bake(BlockSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);


                AddComponent(entity, new BlockSpawnerData
                {
                    BlockPrefabEntity = GetEntity(authoring.BlockPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}