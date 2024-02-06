using Aspects;
using Datas;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [UpdateAfter(typeof(MatchFindingSystem))]
    [BurstCompile]
    public partial struct BlockPoppingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlockClickableTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var blockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockPopTag>())
            {
                blockAspect.TakeDamage();

                if (blockAspect.Health <= 0)
                {
                    foreach (var columnAspect in SystemAPI.Query<ColumnAspect>())
                    {
                        if (blockAspect.Column == columnAspect.ColumnID)
                        {
                            columnAspect.SetRowValue(blockAspect.Row, 0);
                        }
                    }

                    ecb.DestroyEntity(blockAspect.entity);
                }
                else
                {
                    ecb.RemoveComponent<BlockPopTag>(blockAspect.entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}