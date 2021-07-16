using Oldsu.Enums;

namespace Oldsu.Bancho.Packet.Shared.Out
{
    public class BanchoPrivileges : ISharedPacketOut, Into<IB904PacketOut>
    {
        public Privileges Privileges { get; init; }

        public IB904PacketOut Into()
        {
            var packet = new Packet.Out.B904.BanchoPrivileges
            {
                Privileges = (int)Privileges
            };

            return packet;
        }
    }
}