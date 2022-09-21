using Microsoft.IoLogger.Core.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoLogger.Core.Aspnet
{
    public class AspnetRequestMessage : BaseLoggerMessage
    {
        public Guid CorrelationId { get; set; }
        public DateTime Date { get; set; }
        public HttpMethodEnum Method { get; set; }
        public string Uri { get; set; }
        public Dictionary<string, object> Headers { get; set; }
        public object Body { get; set; }
    }
}
