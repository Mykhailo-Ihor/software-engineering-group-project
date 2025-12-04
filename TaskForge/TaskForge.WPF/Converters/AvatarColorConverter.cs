using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TaskForge.WPF.Converters
{
    /// <summary>
   /// Converts a user ID to a consistent avatar background color.
/// Uses a set of predefined colors and picks one based on the ID.
    /// </summary>
    public class AvatarColorConverter : IValueConverter
    {
        private static readonly string[] AvatarColors = new[]
 {
    "#4CAF50", // Green
  "#9C27B0", // Purple
   "#2196F3", // Blue
         "#FF9800", // Orange
      "#E91E63", // Pink
   "#00BCD4", // Cyan
 "#673AB7", // Deep Purple
   "#FF5722", // Deep Orange
        "#3F51B5", // Indigo
           "#009688", // Teal
         "#FFC107", // Amber
       "#795548"  // Brown
        };

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
 {
          if (value is int id)
 {
       var colorIndex = Math.Abs(id) % AvatarColors.Length;
   var colorHex = AvatarColors[colorIndex];
          
 try
        {
    return (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex);
                }
 catch
     {
           return new SolidColorBrush(Color.FromRgb(75, 85, 99)); // Default gray
      }
       }
  
 // Default color
   return new SolidColorBrush(Color.FromRgb(75, 85, 99));
        }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
 }
    }
}
