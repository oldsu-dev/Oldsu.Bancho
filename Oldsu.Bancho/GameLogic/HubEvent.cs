using System;
using System.Threading;
using System.Threading.Tasks;
using Oldsu.Bancho.Exceptions;

namespace Oldsu.Bancho.GameLogic
{
    public struct HubEventContext
    {
        public HubEventContext(Hub hub, HubEventLoop hubEventLoop, User? user)
        {
            Hub = hub;
            User = user;
            HubEventLoop = hubEventLoop;
        }
        
        public User? User { get; }
        public Hub Hub { get; }
        public HubEventLoop HubEventLoop { get; }
    }
    
    public abstract class HubEvent
    {
        public event Action? OnCompletion;
        
        public HubEvent(User? invoker)
        {
            Invoker = invoker;
        }

        public void Completed()
        {
            OnCompletion?.Invoke();
        }
        
        public User? Invoker { get; }
        
        public abstract void Handle(HubEventContext context);
    }
}