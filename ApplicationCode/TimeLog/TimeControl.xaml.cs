using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeLog
{
  /// <summary>
  /// Interaction logic for TimeControl.xaml
  /// </summary>
  public partial class TimeControl : UserControl, INotifyPropertyChanged
  {
    public TimeControl()
    {
      this.MinuteRoundingValue = 15;
      InitializeComponent();

      //this.Value = DateTime.Now.TimeOfDay;
      //this.DateTime = DateTime.Now;
    }


    public DateTime DateTime
    {
      get { return (DateTime)GetValue(DateTimeProperty); }
      set { SetValue(DateTimeProperty, value); }
    }

    public static readonly DependencyProperty DateTimeProperty =
    DependencyProperty.Register("DateTime", typeof(DateTime), typeof(TimeControl),
    new UIPropertyMetadata(DateTime.Now, new PropertyChangedCallback(OnDateTimeChanged)));

    //, new CoerceValueCallback(OnCoerceDateTimeChanged)
    //private static object OnCoerceDateTimeChanged(DependencyObject d, object baseValue)
    //{
 	    
    //}


    private static void OnDateTimeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      TimeControl control = obj as TimeControl;
      var timeOfDay = ((DateTime)e.NewValue).TimeOfDay;

      control.Hours = NormalizeHour(timeOfDay.Hours);
      control.Minutes = NormalizeMinutes(timeOfDay.Minutes, control.MinuteRoundingValue);

      control.CalculateShoulderValues();

      control.ChangeTime(control.Hours, control.Minutes);
    }



    //public TimeSpan Value
    //{
    //  get { return (TimeSpan)GetValue(ValueProperty); }
    //  set { SetValue(ValueProperty, value); }
    //}

    //public static readonly DependencyProperty ValueProperty =
    //DependencyProperty.Register("Value", typeof(TimeSpan), typeof(TimeControl),
    //new UIPropertyMetadata(DateTime.Now.TimeOfDay, new PropertyChangedCallback(OnValueChanged)));

    //private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    //{
    //  TimeControl control = obj as TimeControl;
    //  control.Hours = NormalizeHour(((TimeSpan)e.NewValue).Hours);
    //  control.Minutes = ((TimeSpan)e.NewValue).Minutes;

    //  control.Minutes = (int)(Math.Round((double)control.Minutes / control.MinuteRoundingValue) * control.MinuteRoundingValue);

    //  CalculateShoulderValues(control);
    //}

    public void ChangeTime(int hours, int minutes)
    {
      if (minutes < 0) hours -= 1;
      if (minutes >= 60) hours += 1;


      var temp = new DateTime(this.DateTime.Year, this.DateTime.Month, this.DateTime.Day, NormalizeHour(hours), NormalizeMinutes(minutes, this.MinuteRoundingValue), 0);
      this.DateTime = temp.RoundToNearest15Minutes();

      CalculateShoulderValues();
    }


    private int _Hours;
    private int _Minutes;

    public int Hours
    {
      get { return this._Hours; }
      set { this._Hours = value; RaiseProperyChanged("Hours"); }
    }

    public int Minutes
    {
      get { return this._Minutes; }
      set { this._Minutes = value; RaiseProperyChanged("Minutes"); }
    }


    private void CalculateShoulderValues()
    {
      this.PreviousHour = NormalizeHour(this.Hours - 1);
      this.NextHour = NormalizeHour(this.Hours + 1);
      this.PreviousMinute = this.Minutes - this.MinuteRoundingValue;
      this.NextMinute = this.Minutes + this.MinuteRoundingValue;

      if (this.Hours < 12)
      {
        this.PreviousAMPM = false;
        this.NextAMPM = true;
      }
      else
      {
        this.PreviousAMPM = true;
        this.NextAMPM = false;
      }
    }

    public int MinuteRoundingValue { get; set; }

    private int _PreviousHour;
    private int _NextHour;
    private int _PreviousMinute;
    private int _NextMinute;
    private bool _PreviousAMPM;
    private bool _NextAMPM;

    public int PreviousHour
    {
      get { return this._PreviousHour; }
      set { this._PreviousHour = value; RaiseProperyChanged("PreviousHour"); }
    }

    public int NextHour
    {
      get { return this._NextHour; }
      set { this._NextHour = value; RaiseProperyChanged("NextHour"); }
    }

    public int PreviousMinute
    {
      get { return this._PreviousMinute; }
      set { this._PreviousMinute = value; RaiseProperyChanged("PreviousMinute"); }
    }

    public int NextMinute
    {
      get { return this._NextMinute; }
      set { this._NextMinute = value; RaiseProperyChanged("NextMinute"); }
    }

    public bool PreviousAMPM
    {
      get { return this._PreviousAMPM; }
      set { this._PreviousAMPM = value; RaiseProperyChanged("PreviousAMPM"); }
    }

    public bool NextAMPM
    {
      get { return this._NextAMPM; }
      set { this._NextAMPM = value; RaiseProperyChanged("NextAMPM"); }
    }



    
    //private void Down(object sender, KeyEventArgs args)
    //{
    //  switch (((Grid)sender).Name)
    //  {
    //    case "sec":
    //      if (args.Key == Key.Up)
    //        this.Seconds++;
    //      if (args.Key == Key.Down)
    //        this.Seconds--;
    //      break;

    //    case "min":
    //      if (args.Key == Key.Up)
    //        this.Minutes++;
    //      if (args.Key == Key.Down)
    //        this.Minutes--;
    //      break;

    //    case "hour":
    //      if (args.Key == Key.Up)
    //        this.Hours++;
    //      if (args.Key == Key.Down)
    //        this.Hours--;
    //      break;
    //  }
    //}


    public event PropertyChangedEventHandler PropertyChanged;
    private void RaiseProperyChanged(string propertyName)
    {
      var handler = this.PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }


    private static int NormalizeHour(int hour)
    {
      while (hour < 0) hour += 24;
      while (hour >= 24) hour -= 24;

      return hour;
    }

    private static int NormalizeMinutes(int minutes, int roundingValue)
    {
      minutes = (int)(Math.Round((double)minutes / roundingValue) * roundingValue) % 60;

      while (minutes < 0) minutes += 60;
      while (minutes >= 60) minutes -= 60;

      return minutes;
    }

    private void PreviousHourButton_Click_1(object sender, RoutedEventArgs e)
    {
      ChangeTime(this.Hours - 1, this.Minutes);
    }

    private void NextHourButton_Click_1(object sender, RoutedEventArgs e)
    {
      ChangeTime(this.Hours + 1, this.Minutes);
    }

    private void PreviousMinuteButton_Click_1(object sender, RoutedEventArgs e)
    {
      ChangeTime(this.Hours, this.Minutes - this.MinuteRoundingValue);
    }

    private void NextMinuteButton_Click_1(object sender, RoutedEventArgs e)
    {
      ChangeTime(this.Hours, this.Minutes + this.MinuteRoundingValue);
    }

    private void PreviousAMPMButton_Click_1(object sender, RoutedEventArgs e)
    {
      ChangeTime(this.Hours - 12, this.Minutes);
    }

    private void NextAMPMButton_Click_1(object sender, RoutedEventArgs e)
    {
      ChangeTime(this.Hours + 12, this.Minutes);
    }
  } 
}
