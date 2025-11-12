using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskForge.WPF.Converters
{
    public class PasswordToDotsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var password = value as string;
            if (string.IsNullOrEmpty(password)) return string.Empty;
            return new string('•', password.Length);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
