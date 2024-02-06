using Datas;
using Monos;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.U2D;

namespace Systems
{
    public partial class BlockPresentationGOSystem : SystemBase
    {
        public SpriteAtlas BlockAtlas;

        protected override void OnStartRunning()
        {
            BlockAtlas = Resources.Load<SpriteAtlas>("BlockAtlas");
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            BoardData boardData = SystemAPI.GetSingleton<BoardData>();

            foreach (var (blockPresenterGO, entity) in SystemAPI.Query<BlockPresentationGO>().WithEntityAccess())
            {
                GameObject go = Object.Instantiate(blockPresenterGO.BlockPrefab);
                go.AddComponent<EntityGameObject>().AssignEntity(entity, World);

                ecb.AddComponent(entity, new BlockTransform() { Transform = go.transform });
                ecb.AddComponent(entity, new BlockSpriteRenderer() { SpriteRenderer = go.GetComponent<SpriteRenderer>() });

                ecb.RemoveComponent<BlockPresentationGO>(entity);
            }

            foreach (var (blockTransform, blockSpriteRenderer, transform, blockType, blockGridIndex) in SystemAPI.Query<BlockTransform, BlockSpriteRenderer, LocalTransform, RefRO<BlockTypeData>, RefRO<BlockGridData>>())
            {
                blockTransform.Transform.position = transform.Position;
                blockSpriteRenderer.SpriteRenderer.sprite = BlockAtlas.GetSprite(blockType.ValueRO.BlockType.ToString());
                blockSpriteRenderer.SpriteRenderer.sortingOrder = blockGridIndex.ValueRO.Row * boardData.ColumnCount + blockGridIndex.ValueRO.Column;
            }

            var ecbEOS = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            foreach (var (blockTransform, entity) in SystemAPI.Query<BlockTransform>().WithNone<LocalToWorld>().WithEntityAccess())
            {
                if (blockTransform.Transform != null)
                {
                    Object.Destroy(blockTransform.Transform.gameObject);
                }
                ecbEOS.RemoveComponent<BlockTransform>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}