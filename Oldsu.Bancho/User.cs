using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Multiplayer;
using Oldsu.Bancho.Packet;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Types;
using Channel = System.Threading.Channels.Channel;

namespace Oldsu.Bancho
{
    public class User
    {
        public UserInfo UserInfo { get; }
        public Presence Presence { get; set; }
        public Activity Activity { get; set; }
        public StatsWithRank? Stats { get; set; }
        public Match? Match { get; set; }

        public string Username => UserInfo.Username;
        public uint UserID => UserInfo.UserID;
        
        private readonly BanchoConnection _connection;
        
        public User(
            UserInfo userInfo,
            Presence presence,
            Activity activity,
            StatsWithRank? stats,
            BanchoConnection connection)
        {
            UserInfo = userInfo;
            Presence = presence;
            Activity = activity;
            Stats = stats;
            
            _connection = connection;
            _cancellationTokenSource = new CancellationTokenSource();
            JoinedChannels = new HashSet<string>(); 
        }

        private readonly CancellationTokenSource _cancellationTokenSource;
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        
        public HashSet<string> JoinedChannels { get; }

        public bool Errored { get; set; }

        public void SendPrivateMessage(User sender, string content)
        {
            SendPacket(new SendMessage
            {
                Target = Username,
                Sender = sender.Username,
                Contents = content
            });
        }

        public void CancelRelatedTasks()
        {
            _cancellationTokenSource.Cancel();
        }

        public void SendPacket(ISerializable packet) => _ = _connection.SendPacketAsync(packet);
        public bool IsZombie => _connection.Zombie;

        public Task Disconnect(bool force = false) => _connection.DisconnectAsync(force);
    }
}