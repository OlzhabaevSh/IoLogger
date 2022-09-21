using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoLogger.Core
{
    public interface IProcessService
    {
        Task<IReadOnlyCollection<int>> GetAllProcesses();

        Task SubscribeProcess(int processId);

        Task UnsubscribeProcess(int processId = 0);
    }
}
