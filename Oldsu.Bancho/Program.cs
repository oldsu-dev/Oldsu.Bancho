using System.Threading;
using System.Threading.Tasks;
using Oldsu.Bancho;

// Future usage from cli: oldsu ws://127.0.0.1/ or something like that
var server = new Server("ws://0.0.0.0:8080/");
await server.Run();

