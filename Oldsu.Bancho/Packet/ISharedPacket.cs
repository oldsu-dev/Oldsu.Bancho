﻿using System.Threading.Tasks;

namespace Oldsu.Bancho.Packet
{
    public interface ISharedPacket
    {
        Task Handle();
    }
}