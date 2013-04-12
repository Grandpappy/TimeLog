using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace TimeLog
{
  public class MinutesValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        var stringValue = value.ToString();
        int parsedInt;
        if (int.TryParse(stringValue, out parsedInt))
        {
          if (parsedInt < 0)
          {
            parsedInt = 60 + parsedInt;
          }
          if (parsedInt > 60)
          {
            parsedInt = parsedInt % 60;
          }

          if (parsedInt == 0 || parsedInt == 60)
          {
            return "00";
          }

          return parsedInt;
        }

        return stringValue;
      }
      catch
      {
        return "00";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class HoursValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        var stringValue = value.ToString();
        int parsedInt;
        if (int.TryParse(stringValue, out parsedInt))
        {
          if (parsedInt < 0)
          {
            parsedInt = 24 + parsedInt;
          }
          if (parsedInt > 24)
          {
            parsedInt = parsedInt % 24;
          }

          parsedInt %= 12;

          if (parsedInt == 0)
          {
            return "12";
          }

          return parsedInt;
        }

        return stringValue;
      }
      catch
      {
        return "00";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }


  public class AMPMValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      try
      {
        var stringValue = value.ToString();
        int parsedInt;
        if (int.TryParse(stringValue, out parsedInt))
        {
          if (parsedInt < 12)
            return "AM";
          else
            return "PM";
        }
        else
          return "AM";
      }
      catch
      {
        return "AM";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
