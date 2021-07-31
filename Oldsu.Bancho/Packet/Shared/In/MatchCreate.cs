using System.Threading.Tasks;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchCreate : ISharedPacketIn
    {
        public string GameName { get; init; }
        public string GamePassword { get; init; }
        public string BeatmapName { get; init; }
        public string BeatmapChecksum { get; init; }
        public int BeatmapID { get; init; }
        
        public async Task Handle(OnlineUser self)
        {
            if (self.MatchMediator != null)
                return;
            
            var match = new Match(GameName, GamePassword, BeatmapID, BeatmapName, BeatmapChecksum);
            var slotId = match.TryJoin(self, GamePassword)!.Value;

            await self.ServerMediator.Lobby.WriteAsync(async lobby =>
            {
                var matchWrapper = await lobby.RegisterMatchAsync(match);
                
                if (matchWrapper != null)
                {
                    self.MatchMediator = new Match.Mediator
                    {
                        CurrentMatch = matchWrapper,
                        CurrentSlot = slotId
                    };
                    
                    await self.Connection.SendPacketAsync(new BanchoPacket(new MatchJoinSuccess { Match = match }));
                }
            });
        }
    }
}