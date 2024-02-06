using Unity.Entities;
using UnityEngine;

namespace Monos
{
    public class EntityGameObject : MonoBehaviour
    {
        private Entity _entity;
        private World _world;

        public void AssignEntity(Entity entity, World world)
        {
            _entity = entity;
            _world = world;
        }

        private void OnDestroy()
        {
            if (_world.IsCreated && _world.EntityManager.Exists(_entity))
            {
                _world.EntityManager.DestroyEntity(_entity);
            }
        }
    }
}