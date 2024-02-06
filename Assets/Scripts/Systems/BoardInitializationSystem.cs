using Datas;
using Enums;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class BoardInitializationSystem : SystemBase
    {
        public event Action<int,int> OnBoardGeneratedEvent;

        protected override void OnCreate()
        {
            RequireForUpdate<BlockSpawnerData>();
        }

        protected override void OnUpdate()
        {
            Enabled = false;

            BlockSpawnerData blockSpawnerData = SystemAPI.GetSingleton<BlockSpawnerData>();
            BoardData boardData = SystemAPI.GetSingleton<BoardData>();

            int columnsCount = boardData.ColumnCount;
            int rowsCount = boardData.RowCount;

            OnBoardGeneratedEvent?.Invoke(columnsCount, rowsCount);

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            NativeArray<Entity> tempArray = new NativeArray<Entity>(columnsCount * rowsCount, Allocator.Temp);

            ecb.Instantiate(blockSpawnerData.BlockPrefabEntity, tempArray);

            int randomBoxIndex = UnityEngine.Random.Range(0, tempArray.Length);

            for (int i = 0; i < columnsCount; i++)
            {
                Entity columnEntity = ecb.CreateEntity();
                ecb.AddComponent(columnEntity, new ColumnData
                {
                    ID = i,
                });
                DynamicBuffer<ColumnBuffer> buffer = ecb.AddBuffer<ColumnBuffer>(columnEntity);
                for (int j = 0; j < rowsCount; j++)
                {
                    buffer.Add(new ColumnBuffer
                    {
                        rowValue = 1,
                    });
                }
            }

            for (int k = 0; k < tempArray.Length; k++)
            {
                Entity spawnedEntity = tempArray[k];

                float xPosition = -columnsCount / 2f + 0.5f + k % columnsCount;
                float yPosition = -rowsCount / 2f + 0.5f + math.floor(k / columnsCount);

                int blockType = k == randomBoxIndex ? (int)BlockType.Box1 : UnityEngine.Random.Range(0, boardData.AvailableTypes);

                ecb.SetComponent(spawnedEntity, LocalTransform.FromPosition(new float3(xPosition, yPosition, 0)));
                ecb.AddComponent(spawnedEntity, new BlockTypeData
                {
                    BlockType = (BlockType)blockType,
                    MainBlockType = blockType == (int)BlockType.Box1 ? (MainBlockType)6 : (MainBlockType)blockType
                });
                ecb.AddComponent(spawnedEntity, new BlockGridData
                {
                    Column = k % columnsCount,
                    Row = (int)math.floor(k / columnsCount)
                });
                ecb.AddComponent(spawnedEntity, new BlockHealthData
                {
                    Health = k == randomBoxIndex ? 2 : 1
                });
                if (k != randomBoxIndex)
                {
                    ecb.AddComponent(spawnedEntity, new BlockClickableTag());
                    ecb.AddComponent(spawnedEntity, new BlockShuffleData());
                    ecb.AddComponent(spawnedEntity, new BlockFallData
                    {
                        FallSpeed = 5f
                    });
                }
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}