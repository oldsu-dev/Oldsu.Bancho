
using Oldsu.Bancho.Connections;
using Oldsu.Bancho.Providers;

namespace Oldsu.Bancho
{
    public class UserContext
    {
        public UserContext(uint userId, string username, IUserDataProvider userDataProvider)
        {
            UserID = userId;
            Username = username;
            UserDataProvider = userDataProvider;
        }
        
        public IUserDataProvider UserDataProvider { get; }
        
        // private readonly AsyncRwLockWrapper<GameSpectator> _gameSpectator;
        // private readonly AsyncRwLockWrapper<GameBroadcaster> _gameBroadcaster;

        public ConnectedUserContext Connect(Connection connection)
            => new ConnectedUserContext(this, connection);
        
        public uint UserID { get; }
        public string Username { get; }
    }

    public class ConnectedUserContext : UserContext
    {
        public Connection Connection { get; }

        internal ConnectedUserContext(UserContext context, Connection connection) 
            : base(context.UserID, context.Username, context.UserDataProvider)
        {
            Connection = connection;
        }
    }
}