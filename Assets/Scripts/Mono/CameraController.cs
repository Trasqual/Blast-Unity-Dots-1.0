using Systems;
using Unity.Entities;
using UnityEngine;

namespace Monos
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private int _minOrthoSize = 5;

        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();

            var boardInitializationSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<BoardInitializationSystem>();
            boardInitializationSystem.OnBoardGeneratedEvent += ResizeCamera;
        }

        private void ResizeCamera(int width, int height)
        {
            float screenRatio = (float)Screen.width / Screen.height;

            float boardRatio = (float)width / height;

            float orthoSize;
            if (screenRatio >= boardRatio)
            {
                orthoSize = height * 0.5f + 1;
            }
            else
            {
                orthoSize = (height * 0.5f + 1) * boardRatio / screenRatio;
            }

            _cam.orthographicSize = Mathf.Max(_minOrthoSize, orthoSize);
        }

        private void OnDestroy()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world != null && world.IsCreated)
            {
                var boardInitializationSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<BoardInitializationSystem>();
                boardInitializationSystem.OnBoardGeneratedEvent -= ResizeCamera;
            }
        }
    }
}