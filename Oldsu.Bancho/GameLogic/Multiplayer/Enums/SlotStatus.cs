using System;

namespace Oldsu.Bancho.GameLogic.Multiplayer.Enums
{
    [Flags]
    public enum SlotStatus : byte
    {
        Open = 1,
        Locked = 2,
        NotReady = 4,
        Ready = 8,
        NoMap = 16,
        Playing = 32,
        Complete = 64,
        HasPlayer = NotReady | Ready | NoMap | Playing | Complete,
        Quit = 128
    }
}