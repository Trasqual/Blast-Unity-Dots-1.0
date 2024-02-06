using Aspects;
using Datas;
using Unity.Burst;
using Unity.Entities;

namespace Systems
{
    [UpdateAfter(typeof(BlockFallTargetAssigningSystem))]
    public partial struct BlockFallMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlockFallTag>();
            state.RequireForUpdate<BlockFallData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            BeginInitializationEntityCommandBufferSystem.Singleton ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();

            BoardData boardData = SystemAPI.GetSingleton<BoardData>();

            float deltaTime = SystemAPI.Time.DeltaTime;
            float totalRowCount = boardData.RowCount;

            new BlockFallJob
            {
                DeltaTime = deltaTime,
                TotalRowCount = totalRowCount,
                ECB = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    [WithAll(typeof(BlockFallTag))]
    public partial struct BlockFallJob : IJobEntity
    {
        public float DeltaTime;
        public float TotalRowCount;
        public EntityCommandBuffer.ParallelWriter ECB;

        private void Execute(BlockAspect block, [EntityIndexInQuery] int sortKey)
        {
            block.Fall(DeltaTime);

            if (block.ShouldStopFalling(TotalRowCount))
            {
                ECB.RemoveComponent<BlockFallTag>(sortKey, block.entity);
            }
        }
    }
}