using Microsoft.IoLogger.VsExtenstion.ViewModels;
using System;
using System.Windows.Controls;

namespace Microsoft.IoLogger.VsExtenstion
{

    /// <summary>
    /// Interaction logic for IoLoggerToolWindowControl.
    /// </summary>
    public partial class IoLoggerToolWindowControl : UserControl
    {
        private readonly ToolWindowViewModel _viewModel = new ToolWindowViewModel();


        /// <summary>
        /// Initializes a new instance of the <see cref="IoLoggerToolWindowControl"/> class.
        /// </summary>
        public IoLoggerToolWindowControl()
        {
            this.InitializeComponent();

            this.DataContext = _viewModel;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
