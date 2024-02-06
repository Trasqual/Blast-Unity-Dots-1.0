using Aspects;
using Datas;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [UpdateAfter(typeof(BlockPoppingSystem))]
    public partial struct BlockFallTargetAssigningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlockFallData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var blockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockClickableTag>())
            {
                int column = blockAspect.Column;
                int row = blockAspect.Row;

                if (row == 0)
                {
                    continue;
                }

                foreach (var columnAspect in SystemAPI.Query<ColumnAspect>())
                {
                    if (column == columnAspect.ColumnID)
                    {
                        int fallTargetRow = columnAspect.GetFallTarget(row);

                        if (row != fallTargetRow)
                        {
                            columnAspect.SetRowValue(row, 0);
                            columnAspect.SetRowValue(fallTargetRow, 1);

                            blockAspect.FallTargetColumn = column;
                            blockAspect.FallTargetRow = fallTargetRow;
                            blockAspect.Column = column;
                            blockAspect.Row = fallTargetRow;

                            if (!SystemAPI.HasComponent<BlockFallTag>(blockAspect.entity))
                            {
                                ecb.AddComponent(blockAspect.entity, new BlockFallTag());
                            }
                        }
                        break;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}