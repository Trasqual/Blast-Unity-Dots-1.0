using Datas;
using Enums;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Aspects
{
    public readonly partial struct BlockAspect : IAspect
    {
        public readonly Entity entity;

        [Optional]
        private readonly RefRW<BlockFallData> BlockFallData;
        [Optional]
        private readonly RefRW<BlockShuffleData> BlockShuffleData;

        private readonly RefRW<LocalTransform> Transform;
        private readonly RefRW<BlockTypeData> BlockTypeData;
        private readonly RefRW<BlockGridData> BlockGridData;
        private readonly RefRW<BlockHealthData> BlockHealthData;

        public int2 Position => new int2(BlockGridData.ValueRO.Column, BlockGridData.ValueRO.Row);

        public BlockType BlockType
        {
            get => BlockTypeData.ValueRO.BlockType;
            set => BlockTypeData.ValueRW.BlockType = value;
        }

        public MainBlockType MainBlockType
        {
            get => BlockTypeData.ValueRO.MainBlockType;
            set => BlockTypeData.ValueRW.MainBlockType = value;
        }

        public int Column
        {
            get => BlockGridData.ValueRO.Column;
            set => BlockGridData.ValueRW.Column = value;
        }

        public int Row
        {
            get => BlockGridData.ValueRO.Row;
            set => BlockGridData.ValueRW.Row = value;
        }

        public int FallTargetRow
        {
            get => BlockFallData.ValueRO.FallTargetRow;
            set => BlockFallData.ValueRW.FallTargetRow = value;
        }

        public int FallTargetColumn
        {
            get => BlockFallData.ValueRO.FallTargetColumn;
            set => BlockFallData.ValueRW.FallTargetColumn = value;
        }

        public int Health => BlockHealthData.ValueRO.Health;


        private float _speed => BlockFallData.ValueRO.FallSpeed;

        private const float cellHalfSize = 0.5f;
        private const float stopDistanceSqr = 0.01f;
        private const float stopDistance = 0.1f;

        public void Fall(float deltaTime)
        {
            Transform.ValueRW.Position += new float3(0f, -_speed, 0f) * deltaTime;
        }

        public bool ShouldStopFalling(float totalRowCount)
        {
            float targetY = FallTargetRow + cellHalfSize - totalRowCount / 2f;

            if (Transform.ValueRW.Position.y - targetY < stopDistance)
            {
                Transform.ValueRW.Position = new float3(Transform.ValueRO.Position.x, targetY, Transform.ValueRO.Position.z);
                return true;
            }

            return false;
        }

        public void TakeDamage()
        {
            BlockHealthData.ValueRW.Health--;

            if (Health > 0 && MainBlockType == MainBlockType.Box)
            {
                BlockType--;
            }
        }

        public void SetShuffleTarget(int2 targetPosition)
        {
            BlockGridData.ValueRW.Column = targetPosition.x;
            BlockGridData.ValueRW.Row = targetPosition.y;
            BlockShuffleData.ValueRW.TargetColumn = targetPosition.x;
            BlockShuffleData.ValueRW.TargetRow = targetPosition.y;
        }

        public void MoveToShuffleTarget(float deltaTime, float totalColumnCount, float totalRowCount)
        {
            float targetX = BlockShuffleData.ValueRW.TargetColumn + cellHalfSize - totalColumnCount / 2f;
            float targetY = BlockShuffleData.ValueRW.TargetRow + cellHalfSize - totalRowCount / 2f;

            float3 target = new float3(targetX, targetY, Transform.ValueRO.Position.z);

            float3 direction = target - Transform.ValueRO.Position;
            float3 normalizedDirection = math.normalize(direction);
            Transform.ValueRW.Position += normalizedDirection * deltaTime * _speed;
        }

        public bool ShouldStopMoving(float totalColumnCount, float totalRowCount)
        {
            float targetX = BlockShuffleData.ValueRW.TargetColumn + cellHalfSize - totalColumnCount / 2f;
            float targetY = BlockShuffleData.ValueRW.TargetRow + cellHalfSize - totalRowCount / 2f;

            float3 targetPostion = new float3(targetX, targetY, Transform.ValueRO.Position.z);

            if (math.distancesq(Transform.ValueRW.Position, targetPostion) < stopDistanceSqr)
            {
                Transform.ValueRW.Position = targetPostion;

                return true;
            }

            return false;
        }
    }
}
