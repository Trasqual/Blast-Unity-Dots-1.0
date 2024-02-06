using Datas;
using Unity.Collections;
using Unity.Entities;

namespace Aspects
{
    public readonly partial struct ColumnAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRW<ColumnData> _columnData;
        private readonly DynamicBuffer<ColumnBuffer> _columnBuffer;

        public int ColumnID
        {
            get => _columnData.ValueRO.ID;
            set => _columnData.ValueRW.ID = value;
        }

        public void SetRowValue(int rowIndex, int value)
        {
            _columnBuffer.ElementAt(rowIndex).rowValue = value;
        }

        public int GetRowValue(int rowIndex)
        {
            return _columnBuffer.ElementAt(rowIndex).rowValue;
        }

        public int GetFallTarget(int startRow)
        {
            int fallTarget = startRow;

            for (int i = startRow - 1; i >= 0; i--)
            {
                if (_columnBuffer[i].rowValue == 0)
                {
                    fallTarget = i;
                }
                else
                {
                    break;
                }
            }

            return fallTarget;
        }

        public NativeList<int> GetEmptyRows()
        {
            NativeList<int> emptyCellIndecies = new NativeList<int>(_columnBuffer.Length, Allocator.Temp);

            for (int i = _columnBuffer.Length - 1; i >= 0; i--)
            {
                if (_columnBuffer[i].rowValue == 0)
                {
                    emptyCellIndecies.Add(i);
                }
                else
                {
                    break;
                }
            }

            return emptyCellIndecies;
        }
    }
}