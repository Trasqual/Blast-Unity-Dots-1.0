using Unity.Entities;

namespace Datas
{
    public struct BlockFallData : IComponentData
    {
        public float FallSpeed;
        public int FallTargetRow;
        public int FallTargetColumn;
    }
}