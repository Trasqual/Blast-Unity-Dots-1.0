using Unity.Entities;

namespace Datas
{
    public struct BlockGridData : IComponentData, IEnableableComponent
    {
        public int Row;
        public int Column;
    }
}