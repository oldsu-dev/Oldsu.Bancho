using System.Collections.Generic;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;
using Oldsu.Enums;

namespace Oldsu.Bancho.Handshakes
{
    public class CommonHandshake : IHandshake
    {
        private IEnumerable<UserData> _users;
        private Privileges _privileges;
        private uint _userId;

        // Todo: add channels
        public CommonHandshake(IEnumerable<UserData> users, uint userId, Privileges privileges)
        {
            _privileges = privileges;
            _users = users;
            _userId = userId;
        }
        
        public void Execute(AuthenticatedConnection connection)
        {
            connection.SendPacket(new BanchoPacket(
                new Login() { LoginStatus = (int) _userId }));
            
            connection.SendPacket(new BanchoPacket(
                new BanchoPrivileges { Privileges = _privileges }));
            
            foreach (var userData in _users)
            {
                connection.SendPacket(new BanchoPacket(
                    new SetPresence
                    {
                        Activity = userData.Activity, Stats = userData.Stats,
                        User = userData.UserInfo, Presence = userData.Presence
                    }));
            }

            connection.SendPacket(new BanchoPacket( 
                new JoinChannel { ChannelName = "#osu" }));
        }
    }
}