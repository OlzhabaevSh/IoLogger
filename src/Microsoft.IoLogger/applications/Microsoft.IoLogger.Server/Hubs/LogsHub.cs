using Microsoft.AspNetCore.SignalR;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using Microsoft.IoLogger.Core.Aspnet;
using Microsoft.IoLogger.Core.Http;
using Microsoft.IoLogger.Server.Services;
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
        private readonly LoggerService loggerService;

        public LogsHub(LoggerService loggerService)
        {
            this.loggerService = loggerService;
        }

        /// <summary>
        /// Get list of dotnet processes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task GetProcesses() 
        {
            var processIds = this.loggerService.GetProcesses();
            await Clients.Caller.SendAsync("ProcessesReceived", processIds);
        }

        /// <summary>
        /// Subscribe to required process by id.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task Subscribe(int processId) 
        {
            return this.loggerService.Subscribe(processId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task Unsubscribe(int processId = 0) 
        {
            return this.loggerService.Unsubscribe(processId);
        }
    }
}
