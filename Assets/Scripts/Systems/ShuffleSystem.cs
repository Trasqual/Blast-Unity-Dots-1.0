using Aspects;
using Datas;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [UpdateBefore(typeof(BlockSpawningFromTopSystem))]
    public partial class ShuffleSystem : SystemBase
    {
        public event Action OnBoardLocked;

        protected override void OnCreate()
        {
            RequireForUpdate<BlockClickableTag>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            BoardData boardData = SystemAPI.GetSingleton<BoardData>();
            int totalBlocks = boardData.RowCount * boardData.ColumnCount;

            NativeHashMap<int2, BlockAspect> blockGridPositions = new NativeHashMap<int2, BlockAspect>(totalBlocks, Allocator.Temp);
            NativeList<int2> availablePositions = new NativeList<int2>(totalBlocks, Allocator.Temp);
            NativeList<BlockAspect> blocks = new NativeList<BlockAspect>(totalBlocks, Allocator.Temp);

            int movingBlocks = 0;
            foreach (var anyFallingBlockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockFallTag>())
            {
                movingBlocks++;
            }

            foreach (var anyMovingBlockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockShuffleTag>())
            {
                movingBlocks++;
            }

            if (movingBlocks > 0)
            {
                return;
            }

            foreach (var blockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockClickableTag>())
            {
                int2 gridPosition = new int2(blockAspect.Column, blockAspect.Row);
                blockGridPositions.Add(gridPosition, blockAspect);
                blocks.Add(blockAspect);
                availablePositions.Add(gridPosition);
            }

            NativeArray<int2> adjacentOffsets = new NativeArray<int2>(4, Allocator.Temp);

            adjacentOffsets[0] = new int2(-1, 0);
            adjacentOffsets[1] = new int2(1, 0);
            adjacentOffsets[2] = new int2(0, -1);
            adjacentOffsets[3] = new int2(0, 1);

            foreach (var blockAspect in blockGridPositions.GetValueArray(Allocator.Temp))
            {
                int2 gridPos = new int2(blockAspect.Column, blockAspect.Row);
                availablePositions.Add(gridPos);

                foreach (var offset in adjacentOffsets)
                {
                    int2 adjacentGridPosition = gridPos + offset;
                    if (IsInBounds(adjacentGridPosition.x, adjacentGridPosition.y, boardData))
                    {
                        if (blockGridPositions.TryGetValue(adjacentGridPosition, out BlockAspect adjacentEntityData))
                        {
                            if (adjacentEntityData.MainBlockType == blockAspect.MainBlockType)
                            {
                                //There is a match so cancel operation
                                adjacentOffsets.Dispose();
                                availablePositions.Dispose();
                                blockGridPositions.Dispose();
                                return;
                            }
                        }
                    }
                }
            }

            bool hasDuplicates = false;
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = i + 1; j < blocks.Length; j++)
                {
                    if (blocks[i].MainBlockType == blocks[j].MainBlockType)
                    {
                        hasDuplicates = true;
                        break;
                    }
                }

                if (hasDuplicates)
                {
                    break;
                }
            }

            if (!hasDuplicates)
            {
                //there are no possible solutions even with shuffle

                foreach (var blockAspect in SystemAPI.Query<BlockAspect>().WithAll<BlockClickableTag>())
                {
                    ecb.RemoveComponent<BlockClickableTag>(blockAspect.entity);
                }

                OnBoardLocked?.Invoke();
                return;
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < blocks.Length - i - 1; j++)
                {
                    if (blocks[j].MainBlockType == blocks[j + 1].MainBlockType)
                    {
                        BlockAspect temp = blocks[j];
                        blocks[j] = blocks[j + 1];
                        blocks[j + 1] = temp;
                    }
                }
            }

            int randomPositionIndex = UnityEngine.Random.Range(0, availablePositions.Length);
            int2 startPosition = availablePositions[randomPositionIndex];

            NativeQueue<int2> positionsQueue = new NativeQueue<int2>(Allocator.Temp);
            NativeList<int2> visitedPositions = new NativeList<int2>(Allocator.Temp);

            positionsQueue.Enqueue(startPosition);

            while (positionsQueue.Count > 0)
            {
                int2 currentPos = positionsQueue.Dequeue();

                if (IsInBounds(currentPos.x, currentPos.y, boardData) && !visitedPositions.Contains(currentPos) && availablePositions.Contains(currentPos))
                {
                    visitedPositions.Add(currentPos);

                    for (int i = 0; i < adjacentOffsets.Length; i++)
                    {
                        int2 testedPosition = currentPos + adjacentOffsets[i];

                        if (IsInBounds(testedPosition.x, testedPosition.y, boardData) && !visitedPositions.Contains(testedPosition) && availablePositions.Contains(currentPos))
                        {
                            positionsQueue.Enqueue(testedPosition);
                        }
                    }
                }
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                if (i < visitedPositions.Length && blocks[i].Column == visitedPositions[i].x && blocks[i].Row == visitedPositions[i].y)
                {
                    continue;
                }
                blocks[i].SetShuffleTarget(visitedPositions[i]);
                ecb.AddComponent(blocks[i].entity, new BlockShuffleTag());
            }
        }

        private bool IsInBounds(int column, int row, BoardData boardData)
        {
            return column >= 0 && row >= 0 && column < boardData.ColumnCount && row < boardData.RowCount;
        }
    }
}