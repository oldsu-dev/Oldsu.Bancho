using System.Collections.Generic;
using System.Linq;
using Oldsu.Bancho.Exceptions.ChatChannel;
using Oldsu.Bancho.Packet.Shared.Out;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Utils;

namespace Oldsu.Bancho.GameLogic
{
    public class ChatChannel
    {
        public class ChatMessage
        {
            public string Content { init; get; }
            public string Sender { init; get; }
            public string Tag { init; get; }
        }
        
        public Privileges PrivilegesToJoin { get; }
        public Privileges PrivilegesToWrite { get; }
        
        public string Tag { get; }
        public bool AutoJoin { get; }
        
        private LoggingManager _loggingManager;

        public Dictionary<uint, User> _usersByUserID { get; }

        public IEnumerable<User> Users => _usersByUserID.Values;

        public LimitedBag<ChatMessage> MessageHistory { get;}

        public ChatChannel(string tag, LoggingManager loggingManager, 
            Privileges privilegesToJoin = Privileges.Normal, 
            Privileges privilegesToWrite = Privileges.Normal, 
            bool autoJoin = false)
        {
            Tag = tag;
            PrivilegesToJoin = privilegesToJoin;
            PrivilegesToWrite = privilegesToWrite;
            AutoJoin = autoJoin;

            MessageHistory = new LimitedBag<ChatMessage>(20);
            _loggingManager = loggingManager;
            _usersByUserID = new Dictionary<uint, User>();
        }

        public void Join(User user)
        {
            if ((user.UserInfo.Privileges & PrivilegesToJoin) != PrivilegesToJoin)
                throw new NoPrivilegesToJoinException();

            if (_usersByUserID.ContainsKey(user.UserID))
                throw new UserAlreadyInChatChannelException();
            
            _usersByUserID.Add(user.UserID, user);
            
            user.SendPacket(new ChannelJoined{ChannelName = Tag});
            user.JoinedChannels.Add(Tag);

            ChatMessage[] messages = MessageHistory.Objects.ToArray();

            if (messages.Length != 0)
            {
                user.SendPacket(new SendMessage
                {
                    Contents = $"Here the last {messages.Length} in case you missed them", Sender = "System", Target = Tag
                });

                foreach (var message in MessageHistory.Objects)
                    user.SendPacket(new SendMessage
                        {Contents = message.Content, Sender = message.Sender, Target = message.Tag});
            }

            #region Logging
            
            _loggingManager.LogInfoSync<ChatChannel>("User joined a chat channel.", dump: new
            {
                Tag,
                user.UserID
            });
            
            #endregion
        }

        public void SendMessage(string senderName, string content)
        {
            CachedBanchoPacket packet = new CachedBanchoPacket(new SendMessage
            {
                Contents = content,
                Sender = senderName,
                Target = Tag
            });

            foreach (var user in _usersByUserID.Values)
                user.SendPacket(packet);
            
            #region Logging
            
            _loggingManager.LogInfoSync<ChatChannel>("User sent message in a chat channel.", dump: new
            {
                Tag,
                content,
            });
            
            #endregion
        }
        
        public void SendMessage(User sender, string content)
        {
            if (!_usersByUserID.ContainsKey(sender.UserID))
                throw new UserNotInChatChannelException();
 
            if ((sender.UserInfo.Privileges & PrivilegesToWrite) != PrivilegesToWrite)
                return;

            MessageHistory.Push(new ChatMessage {Content = content, Sender = sender.Username, Tag = Tag});
            
            CachedBanchoPacket packet = new CachedBanchoPacket(new SendMessage
            {
                Contents = content,
                Sender = sender.Username,
                Target = Tag
            });

            foreach (var user in _usersByUserID.Values.Where(user => user.UserID != sender.UserID))
                user.SendPacket(packet);

            #region Logging
            
            _loggingManager.LogInfoSync<ChatChannel>("User sent message in a chat channel.", dump: new
            {
                Tag,
                content,
                sender.UserID
            });
            
            #endregion
        }

        public void Leave(User user)
        {
            if (!_usersByUserID.ContainsKey(user.UserID))
                throw new UserNotInChatChannelException();
            
            _usersByUserID.Remove(user.UserID);
            
            #region Logging
            
            _loggingManager.LogInfoSync<ChatChannel>("User left a chat channel.", dump: new
            {
                Tag,
                user.UserID
            });
            
            user.JoinedChannels.Remove(Tag);
            
            user.SendPacket(new ChannelLeft{ChannelName = Tag});
            
            #endregion
        }
    }
}