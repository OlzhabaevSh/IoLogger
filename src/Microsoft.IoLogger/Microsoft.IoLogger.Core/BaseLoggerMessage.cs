using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoLogger.Core
{
    public abstract class BaseLoggerMessage
    {
        public virtual bool IsMessageCompleted => true;
    }
}
