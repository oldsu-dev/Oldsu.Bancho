using System.Collections.Generic;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Bancho.User;
using Oldsu.Enums;
using Oldsu.Types;

namespace Oldsu.Bancho.Handshakes
{
    public class CommonHandshake : IHandshake
    {
        private IEnumerable<UserData> _users;
        private Privileges _privileges;
        private uint _userId;
        
        private Channel[] _autojoinChannels; 
        private Channel[] _availableChannels;

        private BanchoFriendsList _friendsList;

        // Todo: add channels
        public CommonHandshake(IEnumerable<UserData> users, uint userId, Privileges privileges,
            Channel[] autojoinChannels, Channel[] availableChannels, BanchoFriendsList friendsList)
        {
            _privileges        = privileges;
            _users             = users;
            _userId            = userId;
            _autojoinChannels  = autojoinChannels;
            _availableChannels = availableChannels;
            _friendsList       = friendsList;
        }
        
        public async Task Execute(AuthenticatedConnection connection)
        {
            await connection.SendPacketAsync(new BanchoPacket(
                new Login() { LoginStatus = (int) _userId }));
            
            await connection.SendPacketAsync(new BanchoPacket(
                new BanchoPrivileges { Privileges = _privileges }));
            
            foreach (var userData in _users)
            {
                await connection.SendPacketAsync(new BanchoPacket(
                    new SetPresence
                    {
                        Activity = userData.Activity, Stats = userData.Stats,
                        User = userData.UserInfo, Presence = userData.Presence
                    }));
            }
            
            foreach(var autojoinChannel in _autojoinChannels)
                await connection.SendPacketAsync(new BanchoPacket( 
                    new AutojoinChannelAvailable() { ChannelName = autojoinChannel.Tag }));
            
            foreach(var availableChannel in _availableChannels)
                await connection.SendPacketAsync(new BanchoPacket( 
                    new ChannelAvailable() { ChannelName = availableChannel.Tag }));

            await connection.SendPacketAsync(new BanchoPacket(new BanchoFriendsList() {
                Friendships = this._friendsList.Friendships
            }));
        }
    }
}