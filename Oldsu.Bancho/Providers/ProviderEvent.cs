namespace Oldsu.Bancho.Providers
{
    public enum ProviderType
    {
        UserState,
        Streaming,
        Lobby,
        UserRequest
    }
    
    public enum ProviderEventType
    {
        BanchoPacket,
        UserRequest
    }
    
    public class ProviderEvent
    {
        public ProviderType ProviderType { get; internal init; }
        public ProviderEventType DataType { get; internal init; }
        public object? Data { get; internal init; }
    }
}