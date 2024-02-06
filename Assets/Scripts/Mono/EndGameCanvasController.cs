using Systems;
using Unity.Entities;
using UnityEngine;

public class EndGameCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject _endGamePanel;

    private void Awake()
    {
        var shuffleSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ShuffleSystem>();

        shuffleSystem.OnBoardLocked += OpenEndGameScreen;
    }

    private void OpenEndGameScreen()
    {
        _endGamePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        World world = World.DefaultGameObjectInjectionWorld;

        if (world != null && world.IsCreated)
        {
            var boardInitializationSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ShuffleSystem>();
            boardInitializationSystem.OnBoardLocked -= OpenEndGameScreen;
        }
    }
}
