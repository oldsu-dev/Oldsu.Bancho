using System.Collections.Generic;
using Doron.Connections;

namespace Oldsu.Bancho.Extensions;

public static class WebSocketConnectionExtensions
{
    public static string GetRealIPAddress(this WebSocketConnection connection)
    {
        return connection.RequestHeader.HeaderFields.GetValueOrDefault("CF-Connecting-IP") ??
               connection.RawConnection.RemoteIPAddress?.ToString() ?? "UNKNOWN_IP";
    }  
}