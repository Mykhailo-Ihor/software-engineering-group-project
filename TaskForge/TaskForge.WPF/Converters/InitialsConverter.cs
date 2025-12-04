using System;
using System.Globalization;
using System.Windows.Data;
using TaskForge.Application.DTOs;

namespace TaskForge.WPF.Converters
{
    /// <summary>
    /// Converts a UserDto to initials (first letter of first name + first letter of last name).
    /// </summary>
    public class InitialsConverter : IValueConverter
  {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
        if (value is UserDto user)
    {
            var firstInitial = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName[0].ToString().ToUpper() : "";
     var lastInitial = !string.IsNullOrEmpty(user.LastName) ? user.LastName[0].ToString().ToUpper() : "";
                
      var initials = firstInitial + lastInitial;
       
                // If no initials available, use first letter of email
        if (string.IsNullOrEmpty(initials) && !string.IsNullOrEmpty(user.Email))
          {
       return user.Email[0].ToString().ToUpper();
             }
    
    return initials;
     }
     
         return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
     }
    }
}
