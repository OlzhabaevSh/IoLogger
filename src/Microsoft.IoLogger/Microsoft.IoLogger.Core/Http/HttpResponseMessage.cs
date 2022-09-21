using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoLogger.Core.Http
{
    public class HttpResponseMessage : BaseLoggerMessage
    {
        public Guid CorrelationId { get; set; }
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public object Body { get; set; }

        public override bool IsMessageCompleted =>
            !object.Equals(this.CorrelationId, default(Guid))
            && !object.Equals(this.Date, default(DateTime))
            && !object.Equals(this.StatusCode, default(int))
            && !object.Equals(this.Headers, default(Dictionary<string, string>));
    }
}
