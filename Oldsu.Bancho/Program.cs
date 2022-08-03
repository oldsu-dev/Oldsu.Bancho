using System;
using System.Net;
using GenHTTP.Engine;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Webservices;
using Oldsu;
using Oldsu.Bancho;
using Oldsu.Bancho.GameLogic;
using Oldsu.Bancho.GameLogic.Multiplayer;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Logging.Strategies;
using Server = Oldsu.Bancho.Server;

#if DEBUG
var loggingManager = new LoggingManager(new Oldsu.Logging.Strategies.Console());
#else
var loggingManager = new LoggingManager(new Combined(new Oldsu.Logging.Strategies.Console(), new MongoDbWriter(
    Environment.GetEnvironmentVariable("OLDSU_MONGO_DB_CONNECTION_STRING")!)));
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

HubEventLoop hubEventLoop = new HubEventLoop(hub, loggingManager);

var server = new Server(IPAddress.Any, 13381, hubEventLoop, loggingManager);

System.Console.WriteLine("Server running");

var service = Layout.Create()
    .AddService("api", new WebInterface(hubEventLoop));

_ = server.Run();

Host.Create()
    .Handler(service)
    .Run();