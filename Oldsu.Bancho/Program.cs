using System;
using System.Net;
using Oldsu;
using Oldsu.Bancho;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Multiplayer;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Logging.Strategies;

#if DEBUG
var loggingManager = new LoggingManager(new NoLog());
#else
var loggingManager = new LoggingManager(new MongoDbWriter(
    Environment.GetEnvironmentVariable("OLDSU_MONGO_DB_CONNECTION_STRING")!));
#endif


UserPanelManager userPanelManager = new UserPanelManager(loggingManager);
Lobby lobby = new Lobby(loggingManager);
Hub hub = new Hub(userPanelManager, lobby);

hub.RegisterChannel(new ChatChannel("#osu", loggingManager, Privileges.Normal, Privileges.Normal, true));

await using (var database = new Database()) {
    await foreach (var channel in database.AvailableChannels.AsAsyncEnumerable())
        hub.RegisterChannel(new ChatChannel(
            channel.Tag, loggingManager, channel.RequiredPrivileges, channel.CanWrite ? Privileges.Normal : Privileges.Developer,
            channel.AutoJoin));
}

var server = new Server(IPAddress.Any, 13381, new HubEventLoop(hub, loggingManager), loggingManager);

Console.WriteLine("Server running");

await server.Run();

