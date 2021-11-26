namespace Oldsu.Bancho.GameLogic.Events
{
    public class HubEventDisconnect : HubEvent
    {
        public HubEventDisconnect(User invoker) : base(invoker)
        {
        }

        public override void Handle(HubEventContext context)
        {
            if (!context.User.CancellationToken.IsCancellationRequested)
            {
                context.User.CancelRelatedTasks();
                
                if (context.User.JoinedChannels.Contains("#lobby"))
                    context.Hub.Lobby.Leave(context.User);

                foreach (var channel in context.User.JoinedChannels)
                    context.Hub.AvailableChatChannels[channel].Leave(context.User);
                
                context.User.Match?.Leave(context.User);

                UserPanelManagerEntity entity = context.Hub.UserPanelManager.EntitiesByUserID[context.User.UserID];

                foreach (var spectator in entity.Spectators)
                    spectator.SpectatingEntity = null;
                
                entity.SpectatingEntity?.RemoveSpectator(entity);

                context.Hub.UserPanelManager.UnregisterUser(context.User);
            }

            if (!context.User.IsZombie)
                context.User.Disconnect();
        }
    }
}