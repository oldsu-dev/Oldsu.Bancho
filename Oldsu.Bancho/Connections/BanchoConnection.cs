using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Doron;
using Doron.Connections;
using Nito.AsyncEx;
using Oldsu.Bancho.Packet;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho.Connections;

public class BanchoConnection
{
    public bool Zombie => WebSocketConnection.RawConnection.Available;

    public WebSocketConnection WebSocketConnection { get; }
    public Version Version { set; get; }

    private AsyncLock _sendLock;
    
    public BanchoConnection(WebSocketConnection connection, Version version)
    {
        _sendLock = new AsyncLock();
        
        WebSocketConnection = connection;
        Version = version;
    }

    private volatile bool _disconnected = false;

    public async Task SendPacketAsync(ISerializable serializable)
    {        
        if (!WebSocketConnection.RawConnection.Available || _disconnected)
            return;

        using (await _sendLock.LockAsync())
        {
            var serializedData = serializable.SerializeDataByVersion(Version);
            if (serializedData == null)
                return;

            await WebSocketConnection.SendMessageAsync(new WebSocketMessage.Binary(serializedData.Value.ToArray()));
        }
    }

    public async IAsyncEnumerable<ISharedPacketIn> GetPacketEnumerable()
    {
        while (true)
        {
            WebSocketConnection.WebSocketActionResult<WebSocketMessage> receiveResult =
                await WebSocketConnection.ReceiveMessageAsync();

            if (receiveResult.Status == WebSocketConnection.WebSocketActionStatus.ConnectionClosed)
                break;

            if (receiveResult.Status == WebSocketConnection.WebSocketActionStatus.Exception)
                throw receiveResult.Exception!;
            
            if (receiveResult.Result is not WebSocketMessage.Binary binary)
                throw new Exception("Invalid message type received");

            var obj = BanchoSerializer.Deserialize(binary.Data, this.Version);
            if (obj == null)
            {
                continue;
            }
            
            ISharedPacketIn packet = ((IntoPacket<ISharedPacketIn>) obj).IntoPacket();
            yield return packet;
        }
    }

    public async Task DisconnectAsync(bool force = false)
    {
        _disconnected = true;

        IDisposable? lockHandle = null;
        if (!force)
            lockHandle = await _sendLock.LockAsync();
        
        WebSocketConnection.Dispose();
        lockHandle?.Dispose();
    }
}