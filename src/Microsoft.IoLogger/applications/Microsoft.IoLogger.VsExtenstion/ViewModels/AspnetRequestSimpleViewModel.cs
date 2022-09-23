using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.IoLogger.Core.Http;

namespace Microsoft.IoLogger.VsExtenstion.ViewModels
{
    public class AspnetRequestSimpleViewModel : INotifyPropertyChanged
    {
        public DateTime Date { get; set; }
        public Guid CorrelationId { get; set; }

        public HttpMethodEnum Method { get; set; }

        public string Name { get; set; }

        private int _status;

        public int Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChange();
            }
        }

        public TimeSpan Time => this.ResponseDate - this.RequestDate;

        private DateTime _requestDate;
        public DateTime RequestDate
        {
            get => _requestDate;
            set
            {
                _requestDate = value;
                OnPropertyChange();
                OnPropertyChange("Time");
            }
        }

        private DateTime _responseDate;
        public DateTime ResponseDate
        {
            get => _responseDate;
            set
            {
                _responseDate = value;
                OnPropertyChange();
                OnPropertyChange("Time");
            }
        }

        protected void OnPropertyChange([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
