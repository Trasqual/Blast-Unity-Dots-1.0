using Datas;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    public partial struct PlayerInputSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BlockClickableTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 screenPoint = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 100f));

                EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

                foreach (var (localTransform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<BlockClickableTag>().WithEntityAccess())
                {
                    bool didHit = screenPoint.x <= localTransform.ValueRO.Position.x + 0.5f &&
                        screenPoint.x >= localTransform.ValueRO.Position.x - 0.5f &&
                        screenPoint.y <= localTransform.ValueRO.Position.y + 0.5f &&
                        screenPoint.y >= localTransform.ValueRO.Position.y - 0.5f;

                    if (didHit)
                    {
                        ecb.AddComponent(entity, new BlockClickedTag());
                        break;
                    }
                }

                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
        }
    }
}