using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoLogger.Core.Http
{
    public class HttpRequestMessage : BaseLoggerMessage
    {
        public Guid CorrelationId { get; set; }
        public DateTime Date { get; set; }
        public HttpMethodEnum Method { get; set; }
        public string Uri { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public object Body { get; set; }

        public override bool IsMessageCompleted =>
            !object.Equals(this.CorrelationId, default(Guid))
            && !object.Equals(this.Date, default(DateTime))
            && !object.Equals(this.Method, default(HttpMethodEnum))
            && !object.Equals(this.Uri, default(string))
            && !object.Equals(this.Headers, default(Dictionary<string, string>));
    }
}
