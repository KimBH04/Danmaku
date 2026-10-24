using System;

[Serializable]
public struct Settings
{
    public static readonly Settings Default = new()
    {
        BGM = 0.3f,
        SFX = 0.3f,
        Mode = MovementInputMode.Stick,
        Position = MovementInputPosition.Left,
        Above = AboveButton.Slow,
        HUDPos = HUDPosition.Top,
    };

    public float BGM;
    public float SFX;

    public MovementInputMode Mode;
    public MovementInputPosition Position;
    public AboveButton Above;
    public HUDPosition HUDPos;

    public enum MovementInputMode
    {
        Stick,
        DPad,
    }

    public enum MovementInputPosition
    {
        Left,
        Right,
    }

    public enum AboveButton
    {
        Slow,
        Bomb,
    }

    public enum HUDPosition
    {
        Top,
        Down,
    }
}
