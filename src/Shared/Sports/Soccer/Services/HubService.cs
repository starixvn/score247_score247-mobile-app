using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LiveScore.Common.Services;
using LiveScore.Core;
using LiveScore.Core.Services;
using LiveScore.Soccer.PubSubEvents;
using LiveScore.Soccer.PubSubEvents.Matches;
using LiveScore.Soccer.PubSubEvents.Odds;
using LiveScore.Soccer.PubSubEvents.Teams;
using Microsoft.AspNetCore.SignalR.Client;
using Prism.Events;

namespace LiveScore.Soccer.Services
{
    public class SoccerHubService : IHubService
    {
        private const int NumOfDelayMillisecondsBeforeReConnect = 3_000;

        private readonly IEventAggregator eventAggregator;
        private readonly IHubConnectionBuilder hubConnectionBuilder;
        private readonly INetworkConnection networkConnectionManager;
        private readonly IConfiguration configuration;

        private HubConnection hubConnection;
        private bool isConnecting;

        public SoccerHubService(
            IHubConnectionBuilder hubConnectionBuilder,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            INetworkConnection networkConnectionManager)
        {
            this.hubConnectionBuilder = hubConnectionBuilder;
            this.configuration = configuration;
            this.eventAggregator = eventAggregator;
            this.networkConnectionManager = networkConnectionManager;
        }

        public async Task Start()
        {
            try
            {
                hubConnection = hubConnectionBuilder
                    .WithUrl($"{configuration.SignalRHubEndPoint}/soccerhub")
                    .Build();

                RegisterHubEvents();

                await hubConnection.StartAsync().ConfigureAwait(false);

                SubscribeReConnectEvent();
            }
            catch (Exception ex)
            {
            }
        }

        private void RegisterHubEvents()
        {
            var handlers = BuildHubEventHandlers();

            foreach (var handler in handlers)
            {
                hubConnection.On(
                    handler.HubMethod,
                    (Action<string>)((jsonString) =>
                    {
                        try
                        {
                            handler.Handle(jsonString);
                        }
                        catch (Exception ex)
                        {
                        }
                    }));
            }

            hubConnection.Closed += HubConnection_Closed;
        }

        private void SubscribeReConnectEvent()
            => eventAggregator.GetEvent<ConnectionChangePubSubEvent>()
                            .Subscribe(async (isConnected) =>
                            {
                                if (isConnected)
                                {
                                    await ReConnect();
                                }
                            });

        private IEnumerable<IPubSubEventHandler> BuildHubEventHandlers()
        {
            Debug.WriteLine($"BuildHubEventHandlers at {DateTime.Now.ToString("hh:mm:tt zzz")}");
            return new List<IPubSubEventHandler>
            {
                new MatchEventPubSubEventHandler(eventAggregator),
                new LiveMatchPubSubEventHandler(eventAggregator),
                new OddsComparisonPubSubEventHandler(eventAggregator),
                new OddsMovementPubSubEventHandler(eventAggregator),
                new TeamStatisticPubSubEventHandler(eventAggregator)
            };
        }

        private async Task HubConnection_Closed(Exception arg)
        {
            if (networkConnectionManager.IsFailureConnection())
            {
                return;
            }

            var ex = new InvalidOperationException($"{DateTime.Now} HubConnection_Closed {arg?.Message}", arg);

            await ReConnect().ConfigureAwait(false);
        }

        public async Task ReConnect(byte retryTimes = 5)
        {
            var retryCount = 0;

            while (retryCount < retryTimes
                && hubConnection.State == HubConnectionState.Disconnected
                && !isConnecting)
            {
                retryCount++;
                isConnecting = true;
                try
                {
                    await StopCurrentConnection().ConfigureAwait(false);
                    await Task.Delay(NumOfDelayMillisecondsBeforeReConnect).ConfigureAwait(false);
                    await hubConnection.StartAsync().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                }
                isConnecting = false;
            }
        }

        private async Task StopCurrentConnection()
        {
            try
            {
                await hubConnection.StopAsync().ConfigureAwait(false);
            }
            catch (Exception disposeException)
            {
            }
        }
    }
}