﻿namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class UserQuit : ISharedPacketOut, Into<IGenericPacketOut>
    {
        public int UserID { get; init; }
        public IGenericPacketOut Into()
        {
            var packet = new Packet.Out.Generic.UserQuit
            {
                UserID = UserID
            };

            return packet;
        }
    }
}