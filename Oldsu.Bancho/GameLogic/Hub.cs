using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Oldsu.Bancho.Enums;
using Oldsu.Bancho.Exceptions;
using Oldsu.Bancho.GameLogic.Multiplayer;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Logging;

namespace Oldsu.Bancho.GameLogic
{
    public class Hub
    {
        private readonly Dictionary<string, ChatChannel> _availableChatChannels;

        public IReadOnlyDictionary<string, ChatChannel> AvailableChatChannels => _availableChatChannels;
        public UserPanelManager UserPanelManager { get; }
        public Lobby Lobby { get; }

        public void RegisterChannel(ChatChannel channel)
        {
            _availableChatChannels.Add(channel.Tag, channel);
            UserPanelManager.BroadcastPacket(new ChannelAvailable{ChannelName = channel.Tag});
        }

        public Hub(UserPanelManager userPanelManager, Lobby lobby)
        {
            _availableChatChannels = new Dictionary<string, ChatChannel>();
            UserPanelManager = userPanelManager;
            Lobby = lobby;
        }
    }
}