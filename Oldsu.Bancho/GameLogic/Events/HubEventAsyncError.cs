using System;

namespace Oldsu.Bancho.GameLogic.Events;

public class HubEventAsyncError : HubEvent
{
    public Exception Exception { get; }
    
    public HubEventAsyncError(Exception exception, User invoker) : base(invoker)
    {
        Exception = exception;
    }

    public override void Handle(HubEventContext context)
    {
        throw Exception;
    }
}