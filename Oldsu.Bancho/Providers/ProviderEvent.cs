namespace Oldsu.Bancho.Providers
{
    public enum ProviderType
    {
        UserState,
        Streaming
    }
    
    public enum ProviderEventType
    {
        BanchoPacket
    }
    
    public class ProviderEvent
    {
        public ProviderType ProviderType { get; internal init; }
        public ProviderEventType DataType { get; internal init; }
        public object Data { get; internal init; }
    }
}