using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.IoLogger.Core.Processes
{
    public class ProcessService : IProcessService
    {
        private readonly ConcurrentDictionary<int , object> _processes = new ConcurrentDictionary<int , object>();

        public ProcessService()
        {
        }

        public Task<IReadOnlyCollection<int>> GetAllProcesses()
        {
            var processIds = DiagnosticsClient.GetPublishedProcesses();

            IReadOnlyCollection<int> result = new ReadOnlyCollection<int>(processIds.ToList()); 

            return Task.FromResult(result);
        }

        public Task SubscribeProcess(int processId)
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeAll()
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeProcess(int processId)
        {
            throw new NotImplementedException();
        }
    }
}
