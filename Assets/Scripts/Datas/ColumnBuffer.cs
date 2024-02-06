using Unity.Entities;

namespace Datas
{
    [InternalBufferCapacity(10)]
    public struct ColumnBuffer : IBufferElementData
    {
        public int rowValue;

        public static implicit operator ColumnBuffer(int value)
        {
            return new ColumnBuffer { rowValue = value };
        }
    }
}