namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct FellowSpectatorJoined : ISharedPacketOut
    {
        public int UserID { get; set; }
    }
}