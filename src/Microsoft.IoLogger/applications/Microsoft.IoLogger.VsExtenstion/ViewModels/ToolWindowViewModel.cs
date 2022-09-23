using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.IoLogger.Core;
using Microsoft.IoLogger.VsExtenstion.ViewModels.Commands;

namespace Microsoft.IoLogger.VsExtenstion.ViewModels
{
    public class ToolWindowViewModel : INotifyPropertyChanged
    {
        private int? _processId;

        private ICommand _connectCommand;

        private readonly LoggerService loggerService;

        private readonly LocalNotificationService localNotificationService;

        public ToolWindowViewModel()
        {
            this.localNotificationService = new LocalNotificationService();
            this.loggerService = new LoggerService(this.localNotificationService);

            this.localNotificationService.AspnetRequestMessageReceived += LocalNotificationService_AspnetRequestMessageReceived;
            this.localNotificationService.AspnetResponseMessageReceived += LocalNotificationService_AspnetResponseMessageReceived;
            this.localNotificationService.HttpRequestMessageReceived += LocalNotificationService_HttpRequestMessageReceived;
            this.localNotificationService.HttpResponseMessageReceived += LocalNotificationService_HttpResponseMessageReceived;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public ObservableCollection<HttpRequestSimpleViewModel> HttpRequests { get; } = new ObservableCollection<HttpRequestSimpleViewModel>();

        public int? ProcessId
        {
            get => _processId;
            set
            {
                _processId = value;
                OnPropertyChanged();
                
            }
        }

        public ICommand ConnectCommand => _connectCommand ?? (_connectCommand = new ConnectCommand(p =>
        {
            var processId = ((int?)p).Value;
            loggerService.Subscribe(processId);
        }));

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LocalNotificationService_HttpResponseMessageReceived(object sender, Core.Http.HttpResponseMessage e)
        {

            var item = HttpRequests.FirstOrDefault(x => x.CorrelationId == e.CorrelationId);

            if (item != null)
            {
                item.ResponseDate = e.Date;
                item.Status = e.StatusCode;
            }
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
            HttpRequests.Add(item);
        }

        private void LocalNotificationService_AspnetResponseMessageReceived(object sender, Core.Aspnet.AspnetResponseMessage e)
        {
            /*var item = AspnetRequests.FirstOrDefault(x => x.CorrelationId == e.CorrelationId);

            if (item != null)
            {
                item.ResponseDate = e.Date;
                item.Status = e.StatusCode;
            }*/
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

            //AspnetRequests.Add(item);
        }
    }
}