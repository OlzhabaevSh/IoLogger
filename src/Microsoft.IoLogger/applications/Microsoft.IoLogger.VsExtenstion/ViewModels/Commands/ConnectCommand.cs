using System;

namespace Microsoft.IoLogger.VsExtenstion.ViewModels.Commands
{
    public class ConnectCommand : RelayCommand
    {
        public ConnectCommand(Action<object> execute) : base(execute, p => p != null)
        {
        }
    }
}