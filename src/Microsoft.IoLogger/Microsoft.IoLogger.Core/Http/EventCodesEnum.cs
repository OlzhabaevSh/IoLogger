using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoLogger.Core.Http
{
    public enum EventCodesEnum
    {
        RequestStart = 100,
        RequestEnd = 101,
        RequestHeader = 102,
        ResponseHeader = 103
    }

    public class RequestStartModel 
    {
        public string HttpMethod { get; set; }
        public string Uri { get; set; }
    }

    public class RequestEndModel
    {
        public int StatusCode { get; set; }
    }
}
