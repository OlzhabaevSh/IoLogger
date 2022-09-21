using Microsoft.AspNetCore.SignalR;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.IoLogger.Core.Http;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Security.Cryptography;
using System.Text.Json;
using HttpRequestMessage = Microsoft.IoLogger.Core.Http.HttpRequestMessage;
using HttpResponseMessage = Microsoft.IoLogger.Core.Http.HttpResponseMessage;

namespace Microsoft.IoLogger.Server.Hubs
{
    public class LogsHub : Hub
    {
        private readonly ConcurrentDictionary<int, Task> processesExecutors = new ConcurrentDictionary<int, Task>();
        private readonly ConcurrentDictionary<int, CancellationTokenSource> processesCancelations = new ConcurrentDictionary<int, CancellationTokenSource>();

        /// <summary>
        /// Get list of dotnet processes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task GetProcesses() 
        {
            var processIds = DiagnosticsClient.GetPublishedProcesses();
            IReadOnlyCollection<int> result = new ReadOnlyCollection<int>(processIds.ToList());
            await Clients.Caller.SendAsync("ProcessesReceived", result);
        }

        /// <summary>
        /// Subscribe to required process by id.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task Subscribe(int processId) 
        {
            if (!this.processesExecutors.ContainsKey(processId)) 
            {
                var cts = new CancellationTokenSource();
                var task = CreateTask(processId, cts.Token);
                this.processesExecutors.TryAdd(processId, task);
                this.processesCancelations.TryAdd(processId, cts);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task Unsubscribe(int processId = 0) 
        {
            if (processId == 0)
            {
                foreach (var item in this.processesExecutors.Keys) 
                {
                    RemoveProcess(item);
                }
            }
            else 
            {
                RemoveProcess(processId);
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var item in this.processesExecutors.Keys)
            {
                RemoveProcess(item);
            }

            base.Dispose(disposing);
        }

        private void RemoveProcess(int processId) 
        {
            var cts = this.processesCancelations[processId];
            cts.Cancel();

            this.processesCancelations.Remove(processId, out cts);
            var task = this.processesExecutors[processId];
            this.processesExecutors.Remove(processId, out task);
        }

        private Task CreateTask(int processId, CancellationToken cancelationToken) 
        {
            var task = new Task(async () => 
            {
                var providers = new List<EventPipeProvider>()
                {
                    new EventPipeProvider(
                        "Microsoft-Extensions-Logging",
                        EventLevel.Verbose, 
                        4)
                };

                var client = new DiagnosticsClient(processId);


                using (EventPipeSession session = client.StartEventPipeSession(providers, false)) 
                {
                    var source = new EventPipeEventSource(session.EventStream);

                    Guid? correlationId = null;

                    HttpRequestMessage req = null;
                    HttpResponseMessage response = null;

                    source.Dynamic.All += async (TraceEvent obj) =>
                    {
                        if (cancelationToken.IsCancellationRequested) 
                        {
                            // TODO: need to check
                            source.StopProcessing();
                        }

                        if (obj.PayloadStringByName("LoggerName") == "System.Net.Http.HttpClient.Default.ClientHandler") 
                        {
                            var eventId = (EventCodesEnum)Convert.ToInt32(obj.PayloadByName("EventId"));

                            if (eventId == EventCodesEnum.RequestStart)
                            {
                                var data = obj.PayloadByName("ArgumentsJson");
                                var converted = JsonSerializer.Deserialize<RequestStartModel>(data.ToString());

                                correlationId = Guid.NewGuid();
                                req = new HttpRequestMessage() 
                                {
                                    CorrelationId = correlationId.Value,
                                    Date = DateTime.Now,
                                    Uri = converted.Uri,
                                    Method = ConvertToEnum(converted.HttpMethod)
                                };
                                // TODO: send notificaiton
                                await Clients.All.SendAsync("httpRequest", req);
                            }
                            else if (eventId == EventCodesEnum.RequestHeader)
                            {
                                // TODO: understand how to read headers
                            }
                            else if (eventId == EventCodesEnum.RequestEnd)
                            {
                                var data = obj.PayloadByName("ArgumentsJson");
                                var converted = JsonSerializer.Deserialize<RequestEndModel>(data.ToString());

                                response = new HttpResponseMessage() 
                                {
                                    CorrelationId = correlationId.Value,
                                    Date = DateTime.Now,
                                    StatusCode = converted.StatusCode
                                };

                                await Clients.All.SendAsync("httpResponse", response);
                                // TODO: send notifications
                                req = null;
                                response = null;
                                correlationId = null;
                            }
                            else if (eventId == EventCodesEnum.ResponseHeader) 
                            {
                                // TODO: understand how to read headers
                            }
                        }
                    };

                    try
                    {
                        source.Process();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error encountered while processing events");
                        Debug.WriteLine(e.ToString());
                    }
                }
            });

            task.Start();

            return task;
        }

        private HttpMethodEnum ConvertToEnum(string name) 
        {
            var unify = name.ToLower();

            switch (unify) 
            {
                case "get": return HttpMethodEnum.GET;
                case "post": return HttpMethodEnum.POST;
                case "delete": return HttpMethodEnum.DELETE;
                case "put": return HttpMethodEnum.PUT;
                default: return HttpMethodEnum.OTHER;
            }
        }
    }
}
