namespace Oldsu.Bancho.Packet.Shared.Out
{
    public struct FellowSpectatorLeft : ISharedPacketOut
    {
        public int UserID { get; set; }
    }
}