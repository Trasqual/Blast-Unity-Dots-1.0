using Aspects;
using Datas;
using Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateAfter(typeof(BlockTypeAssigningSystem))]
    public partial struct BlockSpawningFromTopSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ColumnData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            BoardData boardData = SystemAPI.GetSingleton<BoardData>();
            BlockSpawnerData blockSpawnerData = SystemAPI.GetSingleton<BlockSpawnerData>();

            int columnsCount = boardData.ColumnCount;
            int rowsCount = boardData.RowCount;

            EntityCommandBuffer ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (columnData, columnEntity) in SystemAPI.Query<RefRW<ColumnData>>().WithEntityAccess())
            {
                ColumnAspect columnAspect = SystemAPI.GetAspect<ColumnAspect>(columnEntity);

                NativeList<int> emptyRows = columnAspect.GetEmptyRows();

                for (int i = 0; i < emptyRows.Length; i++)
                {
                    Entity entity = ecb.Instantiate(blockSpawnerData.BlockPrefabEntity);

                    float xPosition = -columnsCount / 2f + 0.5f + columnData.ValueRW.ID;
                    float yPosition = rowsCount / 2f + 0.5f + emptyRows.Length - 1 - i;

                    int blockType = UnityEngine.Random.Range(0, boardData.AvailableTypes);

                    ecb.SetComponent(entity, LocalTransform.FromPosition(new float3(xPosition, yPosition, 0)));
                    ecb.AddComponent(entity, new BlockTypeData
                    {
                        BlockType = (BlockType)blockType,
                        MainBlockType = (MainBlockType)blockType
                    });
                    ecb.AddComponent(entity, new BlockGridData
                    {
                        Column = columnData.ValueRW.ID,
                        Row = emptyRows[i]
                    });
                    ecb.AddComponent(entity, new BlockFallData
                    {
                        FallSpeed = 5f,
                        FallTargetColumn = columnData.ValueRW.ID,
                        FallTargetRow = emptyRows[i]

                    });
                    ecb.AddComponent(entity, new BlockHealthData
                    {
                        Health = 1
                    });
                    ecb.AddComponent(entity, new BlockShuffleData());

                    ecb.AddComponent(entity, new BlockFallTag());
                    ecb.AddComponent(entity, new BlockClickableTag());

                    columnAspect.SetRowValue(emptyRows[i], 1);
                }

                emptyRows.Dispose();
            }
        }
    }
}