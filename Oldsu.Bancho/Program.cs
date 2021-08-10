using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oldsu;
using Oldsu.Bancho;
using Oldsu.Bancho.Providers;
using Oldsu.Bancho.Providers.InMemory;
using Oldsu.Enums;
using Oldsu.Types;

var userDataProvider = new InMemoryUserStateProvider();
var streamingProvider = new InMemoryStreamingProvider();
var lobbyProvider = new InMemoryLobbyProvider();
var userRequestProvider = new InMemoryUserRequestProvider();

IChatProvider chatProvider = new InMemoryChatProvider();

await using (var database = new Database())
{
    await foreach (var channel in database.AvailableChannels.AsAsyncEnumerable())
        await chatProvider.RegisterChannel(channel);
}

// Future usage from cli: oldsu ws://127.0.0.1/ or something like that
var server = new Server("ws://0.0.0.0:8080/", 
    userDataProvider, streamingProvider, lobbyProvider, userRequestProvider, chatProvider);

await server.Run();

