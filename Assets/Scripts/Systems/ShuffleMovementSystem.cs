using Aspects;
using Datas;
using Unity.Burst;
using Unity.Entities;

namespace Systems
{
    [UpdateAfter(typeof(ShuffleSystem))]
    public partial struct ShuffleMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlockShuffleTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EndInitializationEntityCommandBufferSystem.Singleton ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();

            BoardData boardData = SystemAPI.GetSingleton<BoardData>();

            float deltaTime = SystemAPI.Time.DeltaTime;
            float totalColumnCount = boardData.ColumnCount;
            float totalRowCount = boardData.RowCount;

            new BlockShuffleMoveJob
            {
                DeltaTime = deltaTime,
                TotalColumnCount = totalColumnCount,
                TotalRowCount = totalRowCount,
                ECB = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(BlockShuffleTag))]
    public partial struct BlockShuffleMoveJob : IJobEntity
    {
        public float DeltaTime;
        public float TotalColumnCount;
        public float TotalRowCount;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(BlockAspect block, [EntityIndexInQuery] int sortKey)
        {
            block.MoveToShuffleTarget(DeltaTime, TotalColumnCount, TotalRowCount);

            if (block.ShouldStopMoving(TotalColumnCount, TotalRowCount))
            {
                ECB.RemoveComponent<BlockShuffleTag>(sortKey, block.entity);
            }
        }
    }
}