using System;

namespace Oldsu.Bancho.GameLogic.Events
{
    public class HubEventAction : HubEvent
    {
        public HubEventAction(User user, Action<HubEventContext> action) : base(user)
        {
            _action = action;
        }

        private readonly Action<HubEventContext> _action;
        
        public override void Handle(HubEventContext context)
        {
            _action.Invoke(context);
        }
    }
}