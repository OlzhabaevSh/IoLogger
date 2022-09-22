using Microsoft.AspNetCore.SignalR;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.IoLogger.Core.Aspnet;
using Microsoft.IoLogger.Core.Http;
using Microsoft.IoLogger.Server.Hubs;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Text.Json;
using HttpRequestMessage = Microsoft.IoLogger.Core.Http.HttpRequestMessage;
using HttpResponseMessage = Microsoft.IoLogger.Core.Http.HttpResponseMessage;

namespace Microsoft.IoLogger.Server.Services
{
    public class LoggerService : IDisposable
    {
        private readonly ConcurrentDictionary<int, Task> processesExecutors = new ConcurrentDictionary<int, Task>();
        private readonly ConcurrentDictionary<int, CancellationTokenSource> processesCancelations = new ConcurrentDictionary<int, CancellationTokenSource>();

        private readonly IHubContext<LogsHub> hubContext;

        public LoggerService(IHubContext<LogsHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        /// <summary>
        /// Get list of dotnet processes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IReadOnlyCollection<int> GetProcesses()
        {
            var processIds = DiagnosticsClient.GetPublishedProcesses();
            IReadOnlyCollection<int> result = new ReadOnlyCollection<int>(processIds.ToList());
            return result;
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
        public Task Unsubscribe(int processId = 0)
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

            return Task.CompletedTask;
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
                        8)
                };

                var client = new DiagnosticsClient(processId);


                using (EventPipeSession session = client.StartEventPipeSession(providers, false))
                {
                    var source = new EventPipeEventSource(session.EventStream);

                    Guid? httpCorrelationId = null;
                    HttpRequestMessage httpRequest = null;
                    HttpResponseMessage httpResponse = null;

                    Guid? aspnetCorrelationId = null;
                    AspnetRequestMessage aspnetRequest = null;
                    AspnetResponseMessage aspnetResponse = null;

                    source.Dynamic.All += async (TraceEvent obj) =>
                    {
                        if (cancelationToken.IsCancellationRequested)
                        {
                            // TODO: need to check
                            source.StopProcessing();
                        }

                        var eventId = Convert.ToInt32(obj.PayloadByName("EventId"));
                        var loggerName = obj.PayloadStringByName("LoggerName");

                        // process http clients
                        if (loggerName == "System.Net.Http.HttpClient.Default.ClientHandler")
                        {
                            httpResponse = null;

                            var eventType = (HttpEventCodesEnum)eventId;
                            var data = obj.PayloadByName("ArgumentsJson");

                            if (eventType == HttpEventCodesEnum.RequestStart)
                            {
                                var converted = JsonSerializer.Deserialize<HttpRequestStartModel>(data.ToString());

                                httpCorrelationId = Guid.NewGuid();
                                httpRequest = new HttpRequestMessage()
                                {
                                    CorrelationId = httpCorrelationId.Value,
                                    Date = DateTime.Now,
                                    Uri = converted.Uri,
                                    Method = ConvertToEnum(converted.HttpMethod)
                                };
                                // TODO: send notificaiton
                                await this.hubContext.Clients.All.SendAsync("httpRequest", httpRequest);
                            }
                            else if (eventType == HttpEventCodesEnum.RequestHeader)
                            {
                                // TODO: understand how to read headers
                            }
                            else if (eventType == HttpEventCodesEnum.RequestEnd)
                            {
                                var converted = JsonSerializer.Deserialize<HttpRequestEndModel>(data.ToString());

                                httpResponse = new HttpResponseMessage()
                                {
                                    CorrelationId = httpCorrelationId.Value,
                                    Date = DateTime.Now,
                                    StatusCode = Convert.ToInt32(converted.StatusCode)
                                };

                                await this.hubContext.Clients.All.SendAsync("httpResponse", httpResponse);
                                // TODO: send notifications
                                httpRequest = null;
                                httpResponse = null;
                                httpCorrelationId = null;
                            }
                            else if (eventType == HttpEventCodesEnum.ResponseHeader)
                            {
                                // TODO: understand how to read headers
                            }
                        }

                        // process asp.net middlewares
                        if (loggerName == "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware")
                        {
                            aspnetResponse = null;

                            var eventType = (AspnetEventCodeEnum)eventId;
                            var data = obj.PayloadByName("ArgumentsJson");

                            if (eventType == AspnetEventCodeEnum.RequestLog)
                            {
                                aspnetCorrelationId = Guid.NewGuid();
                                var converted = JsonSerializer.Deserialize<AspnetRequestLogModel>(data.ToString());

                                var headers = new Dictionary<string, object>();

                                foreach (var item in converted.GetType().GetProperties())
                                {
                                    if (item.Name != "Method" && item.Name != "Path")
                                    {
                                        var value = item.GetValue(converted);
                                        headers.Add(item.Name, value);
                                    }
                                }

                                aspnetRequest = new AspnetRequestMessage()
                                {
                                    CorrelationId = aspnetCorrelationId.Value,
                                    Method = ConvertToEnum(converted.Method),
                                    Date = DateTime.Now,
                                    Uri = converted.Path,
                                    Headers = headers
                                };

                                await this.hubContext.Clients.All.SendAsync("aspnetRequest", aspnetRequest);
                            }
                            else if (eventType == AspnetEventCodeEnum.ResponseLog)
                            {
                                var converted = JsonSerializer.Deserialize<AspnetResponseLogModel>(data.ToString());
                                aspnetResponse = new AspnetResponseMessage()
                                {
                                    CorrelationId = aspnetCorrelationId.Value,
                                    Date = DateTime.Now,
                                    StatusCode = Convert.ToInt32(converted.StatusCode)
                                };
                            }
                            else if (eventType == AspnetEventCodeEnum.ResponseBody)
                            {
                                if (aspnetResponse != null)
                                {
                                    var converted = JsonSerializer.Deserialize<object>(data.ToString());
                                    aspnetResponse.Body = converted;

                                    await this.hubContext.Clients.All.SendAsync("aspnetRequest", aspnetRequest);

                                    aspnetRequest = null;
                                    aspnetResponse = null;
                                    aspnetCorrelationId = null;
                                }
                            }
                        }
                    };

                    try
                    {
                        source.Process();

                        // after finishing clear everything
                        RemoveProcess(processId);
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

        public void Dispose()
        {
            foreach (var item in this.processesExecutors.Keys)
            {
                RemoveProcess(item);
            }
        }
    }
}
