using Enums;
using Unity.Entities;

namespace Datas
{
    public struct BlockTypeData : IComponentData
    {
        public BlockType BlockType;
        public MainBlockType MainBlockType;
    }
}