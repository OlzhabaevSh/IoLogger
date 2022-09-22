using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Microsoft.IoLogger.VsExtenstion.ViewModels
{
    public class AspnetRequestSimpleViewModel : INotifyPropertyChanged
    {
        public Guid CorrelationId { get; set; }

        public Brush MethodColor { get; set; }

        public string Method { get; set; }

        public string Name { get; set; }

        private int _status;

        public int Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChange("Status");
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
                OnPropertyChange("RequestDate");
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
                OnPropertyChange("ResponseDate");
                OnPropertyChange("Time");
            }
        }

        protected void OnPropertyChange(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
