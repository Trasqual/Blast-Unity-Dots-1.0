using Datas;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class BoardAuthoring : MonoBehaviour
    {
        [Range(2, 10)] public int Columns;
        [Range(2, 10)] public int Rows;
        [Range(1, 6)] public int AvailableTypes;
        public int MinRocketCreationQuantity = 4;
        public int MinBombCreationQuantity = 7;
        public int MinDiscoCreationQuantity = 9;

        private class BoardBaker : Baker<BoardAuthoring>
        {
            public override void Bake(BoardAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BoardData
                {
                    ColumnCount = authoring.Columns,
                    RowCount = authoring.Rows,
                    AvailableTypes = authoring.AvailableTypes,
                    MinRocketCreationQuantity = authoring.MinRocketCreationQuantity,
                    MinBombCreationQuantity = authoring.MinBombCreationQuantity,
                    MinDiscoCreationQuantity = authoring.MinDiscoCreationQuantity,
                });
            }
        }
    }
}