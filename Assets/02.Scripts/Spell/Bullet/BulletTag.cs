using System;

[Flags]
public enum BulletTag
{
    Enemy  = 0b0001,
    Player = 0b0010,
    Power  = 0b0100,
    Score  = 0b1000,
    Item   = 0b1100,
}