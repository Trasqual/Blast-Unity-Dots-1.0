using Aspects;
using Datas;
using Enums;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [UpdateAfter(typeof(BlockFallMovementSystem))]
    public partial struct BlockTypeAssigningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlockTypeData>();
            state.RequireForUpdate<BlockGridData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            BoardData boardData = SystemAPI.GetSingleton<BoardData>();
            int totalBlocks = boardData.RowCount * boardData.ColumnCount;

            NativeHashSet<Entity> proccessedBlocks = new NativeHashSet<Entity>(totalBlocks, Allocator.Temp);
            NativeHashMap<int2, BlockAspect> blockGridPositions = new NativeHashMap<int2, BlockAspect>(totalBlocks, Allocator.Temp);
            NativeQueue<int2> floodFillQueue = new NativeQueue<int2>(Allocator.Temp);

            NativeArray<int2> adjacentOffsets = new NativeArray<int2>(4, Allocator.Temp);

            adjacentOffsets[0] = new int2(-1, 0);
            adjacentOffsets[1] = new int2(1, 0);
            adjacentOffsets[2] = new int2(0, -1);
            adjacentOffsets[3] = new int2(0, 1);

            foreach (var blockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockClickableTag>().WithNone<BlockFallTag>())
            {
                int2 gridPosition = new int2(blockAspect.Column, blockAspect.Row);
                blockGridPositions.Add(gridPosition, blockAspect);
            }

            foreach (var blockAspect in blockGridPositions.GetValueArray(Allocator.Temp))
            {
                if (!proccessedBlocks.Contains(blockAspect.entity))
                {
                    proccessedBlocks.Add(blockAspect.entity);

                    NativeList<Entity> groupedEntities = new(totalBlocks, Allocator.Temp)
                    {
                        blockAspect.entity
                    };

                    int2 startPosition = new int2(blockAspect.Column, blockAspect.Row);
                    floodFillQueue.Enqueue(startPosition);

                    while (floodFillQueue.Count > 0)
                    {
                        int2 gridPos = floodFillQueue.Dequeue();

                        foreach (int2 offset in adjacentOffsets)
                        {
                            int2 adjacentGridPosition = gridPos + offset;
                            if (IsInBounds(adjacentGridPosition.x, adjacentGridPosition.y, boardData))
                            {
                                if (blockGridPositions.TryGetValue(adjacentGridPosition, out BlockAspect adjacentEntityData))
                                {
                                    if (!proccessedBlocks.Contains(adjacentEntityData.entity))
                                    {
                                        if (adjacentEntityData.MainBlockType == blockAspect.MainBlockType)
                                        {
                                            groupedEntities.Add(adjacentEntityData.entity);
                                            floodFillQueue.Enqueue(adjacentGridPosition);

                                            proccessedBlocks.Add(adjacentEntityData.entity);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    const int maxAvailableType = 6;

                    int typeIncrement = 0;

                    if (groupedEntities.Length >= boardData.MinDiscoCreationQuantity)
                    {
                        typeIncrement += maxAvailableType * 3;
                    }
                    else if (groupedEntities.Length >= boardData.MinBombCreationQuantity)
                    {
                        typeIncrement += maxAvailableType * 2;
                    }
                    else if (groupedEntities.Length >= boardData.MinRocketCreationQuantity)
                    {
                        typeIncrement += maxAvailableType;
                    }

                    foreach (Entity groupedEntity in groupedEntities)
                    {
                        int mainTypeAsInt = (int)blockAspect.MainBlockType;

                        ecb.SetComponent(groupedEntity, new BlockTypeData
                        {
                            BlockType = (BlockType)(mainTypeAsInt + typeIncrement),
                            MainBlockType = blockAspect.MainBlockType
                        });
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            proccessedBlocks.Dispose();
            blockGridPositions.Dispose();
            floodFillQueue.Dispose();

            adjacentOffsets.Dispose();
        }

        private bool IsInBounds(int column, int row, BoardData boardData)
        {
            return column >= 0 && row >= 0 && column < boardData.ColumnCount && row < boardData.RowCount;
        }
    }
}