using System;
using System.Globalization;
using System.Windows.Data;
using TaskForge.Domain.Enums;

namespace TaskForge.WPF.Converters
{
    public class TransactionTypeToSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType type)
            {
                return type == TransactionType.Expense ? "-" : "+";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}