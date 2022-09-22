using Microsoft.IoLogger.Core.Aspnet;
using Microsoft.IoLogger.Core.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoLogger.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface INotificationService
    {
        Task HttpRequest(HttpRequestMessage request);

        Task HttpResponse(HttpResponseMessage response);

        Task AspnetRequest(AspnetRequestMessage request);

        Task AspnetResponse(AspnetResponseMessage response);
    }
}
