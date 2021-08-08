using System.Threading;
using System.Threading.Tasks;
using Oldsu.Bancho;
using Oldsu.Bancho.Providers.InMemory;

var userDataProvider = new InMemoryUserStateProvider();
var streamingProvider = new InMemoryStreamingProvider();
var lobbyProvider = new InMemoryLobbyProvider();
var userRequestProvider = new InMemoryUserRequestProvider();

// Future usage from cli: oldsu ws://127.0.0.1/ or something like that
var server = new Server("ws://0.0.0.0:8080/", userDataProvider, streamingProvider, lobbyProvider, userRequestProvider);
await server.Run();

