using Aspects;
using Datas;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [UpdateAfter(typeof(PlayerInputSystem))]
    public partial struct MatchFindingSystem : ISystem
    {
        [BurstCompile]
        public void OnStart(ref SystemState state)
        {
            state.RequireForUpdate<BlockClickedTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            BoardData boardData = SystemAPI.GetSingleton<BoardData>();
            int totalBlocks = boardData.RowCount * boardData.ColumnCount;

            NativeHashSet<Entity> proccessedBlocks = new NativeHashSet<Entity>(totalBlocks, Allocator.Temp);
            NativeHashSet<Entity> boxes = new NativeHashSet<Entity>(totalBlocks, Allocator.Temp);
            NativeHashMap<int2, BlockAspect> entityGridPositions = new NativeHashMap<int2, BlockAspect>(totalBlocks, Allocator.Temp);
            NativeQueue<int2> floodFillQueue = new NativeQueue<int2>(Allocator.Temp);

            NativeArray<int2> adjacentOffsets = new NativeArray<int2>(4, Allocator.Temp);

            adjacentOffsets[0] = new int2(-1, 0);
            adjacentOffsets[1] = new int2(1, 0);
            adjacentOffsets[2] = new int2(0, -1);
            adjacentOffsets[3] = new int2(0, 1);

            foreach (var blockAspect in SystemAPI.Query<BlockAspect>().WithNone<BlockFallTag>())
            {
                int2 gridPosition = new int2(blockAspect.Column, blockAspect.Row);
                entityGridPositions.Add(gridPosition, blockAspect);
            }

            foreach (var clickedBlockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockClickedTag>())
            {
                if (!proccessedBlocks.Contains(clickedBlockAspect.entity))
                {
                    proccessedBlocks.Add(clickedBlockAspect.entity);

                    NativeList<Entity> groupedEntities = new(100, Allocator.Temp)
                    {
                        clickedBlockAspect.entity
                    };

                    int2 startPosition = new int2(clickedBlockAspect.Column, clickedBlockAspect.Row);
                    floodFillQueue.Enqueue(startPosition);

                    while (floodFillQueue.Count > 0)
                    {
                        int2 gridPos = floodFillQueue.Dequeue();

                        foreach (int2 offset in adjacentOffsets)
                        {
                            int2 adjacentGridPosition = gridPos + offset;
                            if (IsInBounds(adjacentGridPosition.x, adjacentGridPosition.y, boardData))
                            {
                                if (entityGridPositions.TryGetValue(adjacentGridPosition, out BlockAspect adjacentEntityData))
                                {
                                    if (!proccessedBlocks.Contains(adjacentEntityData.entity))
                                    {
                                        if (adjacentEntityData.MainBlockType == clickedBlockAspect.MainBlockType)
                                        {
                                            groupedEntities.Add(adjacentEntityData.entity);
                                            floodFillQueue.Enqueue(adjacentGridPosition);

                                            proccessedBlocks.Add(adjacentEntityData.entity);
                                        }
                                        else if (adjacentEntityData.MainBlockType == Enums.MainBlockType.Box)
                                        {
                                            boxes.Add(adjacentEntityData.entity);
                                            proccessedBlocks.Add(adjacentEntityData.entity);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (groupedEntities.Length >= 2)
                    {
                        foreach (Entity entity in groupedEntities)
                        {
                            ecb.AddComponent(entity, new BlockPopTag());
                        }

                        foreach (Entity boxEntity in boxes)
                        {
                            ecb.AddComponent(boxEntity, new BlockPopTag());
                        }
                        break;
                    }
                    else
                    {
                        foreach (Entity entity in groupedEntities)
                        {
                            if (SystemAPI.HasComponent<BlockClickedTag>(entity))
                            {
                                ecb.RemoveComponent<BlockClickedTag>(entity);
                            }
                        }
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            proccessedBlocks.Dispose();
            boxes.Dispose();
            entityGridPositions.Dispose();
            floodFillQueue.Dispose();

            adjacentOffsets.Dispose();
        }

        private bool IsInBounds(int column, int row, BoardData boardData)
        {
            return column >= 0 && row >= 0 && column < boardData.ColumnCount && row < boardData.RowCount;
        }
    }
}
