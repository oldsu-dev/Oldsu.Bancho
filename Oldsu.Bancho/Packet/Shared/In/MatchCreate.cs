using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;

namespace Oldsu.Bancho.Packet.Shared.In
{
    public class MatchCreate : ISharedPacketIn
    {
        public MatchSettings MatchSettings { get; set; }
        
        public async Task Handle(UserContext userContext, Connection _)
        {
            /*if (self.MatchMediator != null)
                return;
            
            var match = new Match(MatchSettings);
            var slotId = match.TryJoin(self, MatchSettings.GamePassword)!.Value;

            await self.ServerMediator.Lobby.WriteAsync(async lobby =>
            {
                var matchWrapper = lobby.RegisterMatch(match);
                
                if (matchWrapper != null)
                {
                    self.MatchMediator = new Match.Mediator
                    {
                        CurrentMatch = matchWrapper,
                        CurrentSlot = slotId
                    };
                    
                    await self.Connection.SendPacketAsync(
                        new BanchoPacket(new MatchJoinSuccess { Match = match }));
                }
                else
                {
                    await self.Connection.SendPacketAsync(
                        new BanchoPacket(new MatchJoinFail()));
                }
            });*/
        }
    }
}