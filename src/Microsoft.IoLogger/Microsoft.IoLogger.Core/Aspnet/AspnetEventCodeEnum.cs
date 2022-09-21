using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoLogger.Core.Aspnet
{
    public enum AspnetEventCodeEnum
    {
        RequestLog = 1,
        ResponseLog = 2,
        ResponseBody = 4
    }

    public class AspnetRequestLogModel
    {
        public string Protocol { get; set; }

        public string Method { get; set; }

        public string Scheme { get; set; }
        
        public string PathBase { get; set; }

        public string Path { get; set; }

        public string Accept { get; set; }

        public string Host { get; set; }

        public string UserAgent { get; set; }

        public string AcceptEncoding { get; set; }
    }

    public class AspnetResponseLogModel
    {
        public int StatusCode { get; set; }

        public string ContentType { get; set; }
    }

    public class AspnetResponseBodyModel 
    {
        public object Body { get; set; }
    }
}
