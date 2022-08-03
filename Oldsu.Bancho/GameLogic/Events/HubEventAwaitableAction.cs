using System;
using System.Threading.Tasks;

namespace Oldsu.Bancho.GameLogic.Events;

public class HubEventAwaitableAction<T> : HubEvent
{
    private TaskCompletionSource<T> _taskCompletionSource = new TaskCompletionSource<T>();

    private Func<Hub, T> _action;

    public Task<T> Task => _taskCompletionSource.Task;

    public HubEventAwaitableAction(Func<Hub, T> action, User? invoker = null) : base(invoker)
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

public class HubEventAwaitableAction : HubEvent
{
    private TaskCompletionSource _taskCompletionSource = new TaskCompletionSource();

    private Action<Hub> _action;

    public Task Task => _taskCompletionSource.Task;

    public HubEventAwaitableAction(Action<Hub> action, User? invoker = null) : base(invoker)
    {
        _action = action;
    }

    public override void Handle(HubEventContext context)
    {
        try
        {
            _action.Invoke(context.Hub);
        }
        catch (Exception e)
        {
            _taskCompletionSource.SetException(e);
            return;
        }

        _taskCompletionSource.SetResult();
    }
}