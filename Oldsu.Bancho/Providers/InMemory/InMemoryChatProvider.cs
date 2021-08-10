using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Types;
using Oldsu.Utils.Threading;

namespace Oldsu.Bancho.Providers.InMemory
{
    public class InMemoryChannel : InMemoryObservable<ProviderEvent>, IChatChannel
    {
        public Channel ChannelInfo { get; }
        
        public InMemoryChannel(Channel channelInfo)
        {
            ChannelInfo = channelInfo;
        }
        
        public async Task SendMessage(string username, string content)
        {
            if (!ChannelInfo.CanWrite)
                return;

            await Notify(new ProviderEvent
            {
                DataType = ProviderEventType.BanchoPacket,
                ProviderType = ProviderType.Chat,
                Data = new BanchoPacket(new SendMessage
                {
                    Contents = content,
                    Sender = username,
                    Target = ChannelInfo!.Tag
                })
            });
        }
    } 
    
    public class InMemoryChatProvider : InMemoryObservable<ProviderEvent>, IChatProvider
    {
        private AsyncRwLockWrapper<Dictionary<string, InMemoryChannel>> _channels;
        private AsyncRwLockWrapper<Dictionary<string, InMemoryChannel>> _userChannels;

        public InMemoryChatProvider()
        {
            _channels = new AsyncRwLockWrapper<Dictionary<string, InMemoryChannel>>(
                new Dictionary<string, InMemoryChannel>
                {
                    {
                        "#osu",
                        new InMemoryChannel(
                            new Channel
                            {
                                Tag = "#osu", 
                                Topic = "Discussion in English.", 
                                AutoJoin = true, 
                                CanWrite = true, 
                                RequiredPrivileges = Privileges.Normal
                            })
                    },
                    {
                        "#announce",
                        new InMemoryChannel(
                            new Channel
                            {
                                Tag = "#announce", 
                                Topic = "Announces for recent activities.", 
                                AutoJoin = true, 
                                CanWrite = false, 
                                RequiredPrivileges = Privileges.Normal
                            })
                    }
                });
            
            _userChannels = new AsyncRwLockWrapper<Dictionary<string, InMemoryChannel>>(new ());
        }
        
        public Task RegisterUser(string username) =>
            _userChannels.WriteAsync(channels => channels.Add(username, new InMemoryChannel(new Channel
            {
                Tag = username,
                Topic = string.Empty,
                AutoJoin = false,
                CanWrite = true,
                RequiredPrivileges = Privileges.Normal
            })));

        public Task<Channel[]> GetAvailableChannelInfo(Privileges privileges) =>
            _channels.ReadAsync(channels => channels
                .Where(channel => (channel.Value.ChannelInfo.RequiredPrivileges & privileges) > 0)
                .Select(channel => channel.Value.ChannelInfo).ToArray());
        
        public Task<Channel[]> GetAutojoinChannelInfo(Privileges privileges) =>
            _channels.ReadAsync(channels => channels
                .Where(channel => (channel.Value.ChannelInfo.RequiredPrivileges & privileges) > 0 &&
                                  channel.Value.ChannelInfo.AutoJoin) 
                .Select(channel => channel.Value.ChannelInfo).ToArray());

        public async Task RegisterChannel(Channel channel)
        {
            await _channels.WriteAsync(channels => channels.Add(channel.Tag, new InMemoryChannel(channel)));
            
            await Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new ChannelAvailable {ChannelName = channel.Tag}),
                DataType = ProviderEventType.BanchoPacket,
                ProviderType = ProviderType.Chat
            });
        }

        public async Task UnregisterChannel(string channelTag)
        {
            await _channels.WriteAsync(channels => channels.Remove(channelTag));
            
            await Notify(new ProviderEvent
            {
                Data = new BanchoPacket(new ChannelAvailable {ChannelName = channelTag}),
                DataType = ProviderEventType.BanchoPacket,
                ProviderType = ProviderType.Chat
            });
        }
        
        public Task<IChatChannel?> GetUserChannel(string username) =>
            _userChannels.ReadAsync(channels =>
            {
                if (!channels.TryGetValue(username, out var channel))
                    return (IChatChannel?)null;

                return channel;
            });

        public async Task<IChatChannel?> GetChannel(string tag, Privileges userPrivileges)
        {
            using var channelsLock = await _channels.AcquireReadLockGuard();
            
            if (!channelsLock.Value.TryGetValue(tag, out var channel))
                return null;

            if ((channel.ChannelInfo.RequiredPrivileges & userPrivileges) == 0)
                return null;

            return channel;
        }

        public Task UnregisterUser(string username) =>
            _userChannels.WriteAsync(channels => channels.Remove(username));
    }
}