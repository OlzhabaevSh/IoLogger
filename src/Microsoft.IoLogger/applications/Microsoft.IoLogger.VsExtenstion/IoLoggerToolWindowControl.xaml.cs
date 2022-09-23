using Microsoft.IoLogger.Core;
using Microsoft.IoLogger.VsExtenstion.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.IoLogger.VsExtenstion
{

    /// <summary>
    /// Interaction logic for IoLoggerToolWindowControl.
    /// </summary>
    public partial class IoLoggerToolWindowControl : UserControl
    {
        private readonly LoggerService loggerService;
        
        private readonly LocalNotificationService localNotificationService;

        private readonly ToolWindowViewModel _viewModel = new ToolWindowViewModel();

        //public ObservableCollection<HttpRequestSimpleViewModel> HttpRequests { get; set; } = new ObservableCollection<HttpRequestSimpleViewModel>();
        public ObservableCollection<AspnetRequestSimpleViewModel> AspnetRequests { get; set; } = new ObservableCollection<AspnetRequestSimpleViewModel>();


        /// <summary>
        /// Initializes a new instance of the <see cref="IoLoggerToolWindowControl"/> class.
        /// </summary>
        public IoLoggerToolWindowControl()
        {
            this.InitializeComponent();

            this.DataContext = _viewModel;
            
            //this.dg_httpRequests.ItemsSource = this.HttpRequests;
            this.dg_aspnetRequests.ItemsSource = this.AspnetRequests;

            // config loggers
            this.localNotificationService = new LocalNotificationService();
            this.loggerService = new LoggerService(this.localNotificationService);

            this.localNotificationService.AspnetRequestMessageReceived += LocalNotificationService_AspnetRequestMessageReceived;
            this.localNotificationService.AspnetResponseMessageReceived += LocalNotificationService_AspnetResponseMessageReceived;
            this.localNotificationService.HttpRequestMessageReceived += LocalNotificationService_HttpRequestMessageReceived;
            this.localNotificationService.HttpResponseMessageReceived += LocalNotificationService_HttpResponseMessageReceived;
        }

        private void LocalNotificationService_HttpResponseMessageReceived(object sender, Core.Http.HttpResponseMessage e)
        {
            Dispatcher.Invoke(() =>
            {
                var item = _viewModel.HttpRequests.FirstOrDefault(x => x.CorrelationId == e.CorrelationId);
                
                if (item != null)
                {
                    item.ResponseDate = e.Date;
                    item.Status = e.StatusCode;
                }
            });
        }

        private void LocalNotificationService_HttpRequestMessageReceived(object sender, Core.Http.HttpRequestMessage e)
        {
            var item = new HttpRequestSimpleViewModel
            {
                Date = DateTime.Now,
                Method = e.Method,
                CorrelationId = e.CorrelationId,
                Name = e.Uri,
                RequestDate = e.Date
            };

            Dispatcher.Invoke(() => 
            {
                _viewModel.HttpRequests.Add(item);
            });
        }

        private void LocalNotificationService_AspnetResponseMessageReceived(object sender, Core.Aspnet.AspnetResponseMessage e)
        {
            Dispatcher.Invoke(() =>
            {
                var item = AspnetRequests.FirstOrDefault(x => x.CorrelationId == e.CorrelationId);

                if (item != null)
                {
                    item.ResponseDate = e.Date;
                    item.Status = e.StatusCode;
                }
            });
        }

        private void LocalNotificationService_AspnetRequestMessageReceived(object sender, Core.Aspnet.AspnetRequestMessage e)
        {
            var item = new AspnetRequestSimpleViewModel
            {
                Date = DateTime.Now,
                Method = e.Method,
                CorrelationId = e.CorrelationId,
                Name = e.Uri,
                RequestDate = e.Date
            };

            Dispatcher.Invoke(() =>
            {
                AspnetRequests.Add(item);
            });
        }

        /// <summary>
        /// Connect to process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            var processId = Convert.ToInt32(tb_processId.Text);
            this.loggerService.Subscribe(processId);
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.HttpRequests.Clear();
            AspnetRequests.Clear();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
