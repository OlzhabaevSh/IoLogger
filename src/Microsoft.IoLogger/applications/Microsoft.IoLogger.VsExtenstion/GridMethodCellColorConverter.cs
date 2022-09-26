using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.IoLogger.Core.Http;

namespace Microsoft.IoLogger.VsExtenstion
{
    
    internal class GridMethodCellColorConverter: IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var methodName = value is HttpMethodEnum methodEnum ? methodEnum : HttpMethodEnum.OTHER;
            switch (methodName)
            {
                case HttpMethodEnum.GET:
                    return Brushes.Green;
                case HttpMethodEnum.POST:
                    return Brushes.Blue;
                case HttpMethodEnum.PUT:
                    return Brushes.Orange;
                case HttpMethodEnum.DELETE:
                    return Brushes.Red;
                default:
                    return Brushes.DarkGray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
