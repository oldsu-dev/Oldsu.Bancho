using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Oldsu.Bancho.Exceptions;
using Oldsu.Bancho.GameLogic.Events;
using Oldsu.Logging;

namespace Oldsu.Bancho.GameLogic
{
    public class HubEventLoop
    {
        private LoggingManager _loggingManager;
        private Hub _hub;
        
        public HubEventLoop(Hub hub, LoggingManager loggingManager)
        {
            _loggingManager = loggingManager;
            _hub = hub;
            _eventQueue = System.Threading.Channels.Channel.CreateUnbounded<HubEvent>();
        }

        private System.Threading.Channels.Channel<HubEvent> _eventQueue;

        private volatile bool _started;

        public async Task Run()
        {
            if (_started)
                throw new InvalidOperationException();

            _started = true;
        
            for (;;)
            {
                var hubEvent = await _eventQueue.Reader.ReadAsync();

                try
                {
                    if ((hubEvent.Invoker.Errored || hubEvent.Invoker.CancellationToken.IsCancellationRequested)
                        && hubEvent is not HubEventDisconnect)
                        continue;

                    hubEvent.Handle(new HubEventContext(_hub, this, hubEvent.Invoker));

                    hubEvent.Completed();
                }
                catch (OldsuException exception)
                {
                    #region Logging

                    _loggingManager.LogCriticalSync<HubEventLoop>(
                        "An exception occurred while processing an event.",
                        exception,
                        new {hubEvent.Invoker.UserID});

                    #endregion
                    

                    // Disconnect and make messages unprocessable
                    hubEvent.Invoker.Errored = true;
                    SendEvent(new HubEventDisconnect(hubEvent.Invoker));
                    
                    Debug.WriteLine(exception);
                }
            }
        }
        
        public async void SendEvent(HubEvent hubEvent)
        {
            await _eventQueue.Writer.WriteAsync(hubEvent);
        }
    }
}