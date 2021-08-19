using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu;
using Oldsu.Bancho;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.Providers.InMemory;
using Oldsu.Enums;
using Oldsu.Logging;
using Oldsu.Logging.Strategies;
using Oldsu.Types;

var loggingManager = new LoggingManager(new MongoDbWriter(
    Environment.GetEnvironmentVariable("OLDSU_MONGO_DB_CONNECTION_STRING")!));

IChatProvider chatProvider = new InMemoryChatProvider(loggingManager);

await using (var database = new Database()) {
    await foreach (var channel in database.AvailableChannels.AsAsyncEnumerable())
        await chatProvider.RegisterChannel(channel);
}

var dependencies = new DependencyManagerBuilder()
    .Add<IUserStateProvider>(new InMemoryUserStateProvider(loggingManager))
    .Add<IStreamingProvider>(new InMemoryStreamingProvider(loggingManager))
    .Add<ILobbyProvider>(new InMemoryLobbyProvider(loggingManager))
    .Add<IUserRequestProvider>(new InMemoryUserRequestProvider(loggingManager))
    .Add(chatProvider)
    .Build();

// Future usage from cli: oldsu ws://127.0.0.1/ or something like that
var server = new Server("ws://0.0.0.0:13381/", dependencies, loggingManager);

await server.Run();

