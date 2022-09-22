using Microsoft.IoLogger.Core.Aspnet;
using Microsoft.IoLogger.Core.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoLogger.Core
{
    public class LocalNotificationService : INotificationService
    {
        public event EventHandler<AspnetRequestMessage> AspnetRequestMessageReceived;
        public event EventHandler<AspnetResponseMessage> AspnetResponseMessageReceived;
        public event EventHandler<HttpRequestMessage> HttpRequestMessageReceived;
        public event EventHandler<HttpResponseMessage> HttpResponseMessageReceived;


        public Task AspnetRequest(AspnetRequestMessage request)
        {
            this.AspnetRequestMessageReceived?.Invoke(this, request);
            return Task.CompletedTask;
        }

        public Task AspnetResponse(AspnetResponseMessage response)
        {
            this.AspnetResponseMessageReceived?.Invoke(this, response);
            return Task.CompletedTask;
        }

        public Task HttpRequest(HttpRequestMessage request)
        {
            this.HttpRequestMessageReceived?.Invoke(this, request);
            return Task.CompletedTask;
        }

        public Task HttpResponse(HttpResponseMessage response)
        {
            this.HttpResponseMessageReceived?.Invoke(this, response);
            return Task.CompletedTask;
        }
    }
}
