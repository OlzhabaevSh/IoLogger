using System;

namespace Microsoft.IoLogger.VsExtenstion.ViewModels.Commands
{
    public class ClearCommand: RelayCommand
    {
        public ClearCommand(Action<object> execute) : base(execute)
        {
        }
    }
}
