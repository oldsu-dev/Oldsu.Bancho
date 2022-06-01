using System;
using System.Threading.Tasks;

namespace Oldsu.Bancho.GameLogic.Events;

public class HubEventAsyncRequest<T> : HubEvent
{
    private TaskCompletionSource<T> _taskCompletionSource = new TaskCompletionSource<T>();

    private Func<Hub, T> _action;

    public Task<T> Task => _taskCompletionSource.Task;

    public HubEventAsyncRequest(Func<Hub, T> action, User? invoker = null) : base(invoker)
    {
        _action = action;
    }

    public override void Handle(HubEventContext context)
    {
        T result;
        try
        {
            result = _action.Invoke(context.Hub);
        }
        catch (Exception e)
        {
            _taskCompletionSource.SetException(e);
            return;
        }

        _taskCompletionSource.SetResult(result);
    }
}