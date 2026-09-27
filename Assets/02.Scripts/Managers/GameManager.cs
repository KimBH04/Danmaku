using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    public const int FPS = 60;
    public const float RATE = 1f / FPS;

    protected override void Awake()
    {
        base.Awake();
        Application.targetFrameRate = FPS;
    }
}
