using System;
using System.Linq;
using System.Threading.Tasks;
using Oldsu.Bancho.GameLogic;
using GenHTTP.Api;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Bancho.GameLogic.Multiplayer;

namespace Oldsu.Bancho;


public class WebInterface
{
    public class Response<T>
    {
        public Response(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
    }

    public class SendMessageRequest
    {
        public string? Message { get; set; }
    }
    
    public class KickUserRequest
    {
        public string Username { get; set; }
    }

    private HubEventLoop _hubEventLoop;

    public WebInterface(HubEventLoop hubEventLoop)
    {
        _hubEventLoop = hubEventLoop;
    }

    private async ValueTask<object> WrapIntoResponse<TResult>(Func<Task<TResult>> action)
    {
        try
        {
            return new Response<TResult>(await action.Invoke());
        }
        catch (Exception e)
        {
            return new Response<string>(e.ToString());
        }
    } 
    
    [ResourceMethod(RequestMethod.GET, "matches")]
    public ValueTask<object> GetMatches()
    {
        return WrapIntoResponse(async () =>
        {
            var ev = new HubEventAwaitableAction<object>((hub) =>
            {
                var data = hub.Lobby.AvailableMatches.Select(b => new
                {
                    b.MatchID,
                    b.AllCompleted,
                    b.AllLoaded,
                    b.AllSkipped,
                    b.InProgress,
                    b.HostID, 
                    b.Settings
                }).ToArray();
                return data;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.GET, "matches/:id/slots")]
    public ValueTask<object> GetMatchSlots(byte id)
    {
        return WrapIntoResponse(async () =>
        {
            var ev = new HubEventAwaitableAction<object>((hub) =>
            {
                var data = hub.Lobby.GetMatchByID(id).MatchSlots.Select(b => new
                {
                    b.UserID,
                    b.User?.Username,
                    b.LastScoreFrame,
                    b.SlotTeam,
                    b.SlotStatus,
                    b.Skipped,
                    b.Completed,
                    b.Loaded,
                }).ToArray();
                return data;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.POST, "matches/:id/start")]
    public ValueTask<object> StartMatch(byte id)
    {
        return WrapIntoResponse(async () =>
        {
            var ev = new HubEventAwaitableAction<object>((hub) =>
            {
                Match match = hub.Lobby.GetMatchByID(id);
                match.Start();
                return true;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.POST, "matches/:id/slots/:slotId/kick")]
    public ValueTask<object> KickSlot(byte id, byte slotId)
    {
        return WrapIntoResponse(async () =>
        {
            var ev = new HubEventAwaitableAction<object>((hub) =>
            {
                Match match = hub.Lobby.GetMatchByID(id);
                match.LockSlot(slotId);
                match.LockSlot(slotId);

                return true;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.GET, "users")]
    public ValueTask<object> GetUsers()
    {
        return WrapIntoResponse(async () =>
        {
            var ev = new HubEventAwaitableAction<object>((hub) =>
            {
                var data = hub.UserPanelManager.Entities.Select(
                    b => new { b.User.Username, Activity = (object)b.User.Activity, b.User.UserID, b.User.Presence }).ToArray();
                return data;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.POST, "kickUser")]
    public ValueTask<object> KickUser(KickUserRequest request)
    {
        return WrapIntoResponse(async () =>
        {
            HubEventAwaitableAction<bool> ev = new HubEventAwaitableAction<bool>((hub) =>
            {
                hub.UserPanelManager.EntitiesByUsername[request.Username].User.Disconnect();
                return true;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.POST, "channel/:tag/messages")]
    public ValueTask<object> SendMessage(string tag, SendMessageRequest request)
    {
        return WrapIntoResponse(async () =>
        {
            if (request.Message == null)
                throw new NullReferenceException();
            
            HubEventAwaitableAction<bool> ev = new HubEventAwaitableAction<bool>((hub) =>
            {
                hub.AvailableChatChannels["#" + tag].SendMessage("System", request.Message);
                return true;
            });

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
    
    [ResourceMethod(RequestMethod.GET, "channel/:tag/messages")]
    public ValueTask<object> GetMessages(string tag)
    {
        return WrapIntoResponse(async () =>
        {
            HubEventAwaitableAction<object> ev = new HubEventAwaitableAction<object>(
                (hub) => hub.AvailableChatChannels["#" + tag].MessageHistory.Objects.ToArray());

            _hubEventLoop.SendEvent(ev);

            return await ev.Task;
        });
    }
}