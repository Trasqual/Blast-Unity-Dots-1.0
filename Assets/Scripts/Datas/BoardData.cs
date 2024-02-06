using Unity.Entities;

namespace Datas
{
    public struct BoardData : IComponentData
    {
        public int ColumnCount;
        public int RowCount;
        public int AvailableTypes;
        public int MinRocketCreationQuantity;
        public int MinBombCreationQuantity;
        public int MinDiscoCreationQuantity;
    }
}