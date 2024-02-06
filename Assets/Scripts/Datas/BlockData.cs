using Unity.Entities;
using UnityEngine;

namespace Datas
{
    public class BlockPresentationGO : IComponentData
    {
        public GameObject BlockPrefab;
    }

    public class BlockTransform : ICleanupComponentData
    {
        public Transform Transform;
    }

    public class BlockSpriteRenderer : IComponentData
    {
        public SpriteRenderer SpriteRenderer;
    }
}