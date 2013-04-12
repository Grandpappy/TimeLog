using GalaSoft.MvvmLight;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Media;

namespace TimeLog.ViewModel
{
  [DataContract]
  public class SettingsViewModel : INotifyPropertyChanged
  {
    private string _LogFolder;
    private DateTime _WorkdayStart;
    private bool _UseDarkTheme;
    private Color _AccentColor;


    [DataMember]
    public DateTime WorkdayStart
    {
      get
      {
        return this._WorkdayStart;
      }
      set
      {
        this._WorkdayStart = value;
        NotifyPropertyChanged("WorkdayStart");
      }
    }

    [DataMember]
    public bool UseDarkTheme
    {
      get
      {
        return this._UseDarkTheme;
      }
      set
      {
        this._UseDarkTheme = value;
        NotifyPropertyChanged("UseDarkTheme");
      }
    }

    [DataMember]
    public Color AccentColor
    {
      get
      {
        return this._AccentColor;
      }
      set
      {
        this._AccentColor = value;
        NotifyPropertyChanged("AccentColor");
      }
    }

    [DataMember]
    public string LogFolder
    {
      get
      {
        return _LogFolder;
      }
      set
      {
        if (_LogFolder == value)
          return;

        if (string.IsNullOrWhiteSpace(value))
        {
          value = ".";
        }

        _LogFolder = value;
        NotifyPropertyChanged("LogFolder");
      }
    }


    public SettingsViewModel()
    {
      this.AccentColor = Colors.Teal;
      this.UseDarkTheme = true;
      this.WorkdayStart = DateTime.Now.Date.AddHours(8);
      this.LogFolder = ".";
    }


    public void WriteSettingsFile()
    {
      using (FileStream fs = new FileStream("Settings.json", FileMode.Create))
      {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsViewModel));
        serializer.WriteObject(fs, this);
      }
    }


    public static SettingsViewModel ReadSettingsFile()
    {
      if (!File.Exists("Settings.json"))
      {
        return new SettingsViewModel();
      }

      try
      {
        using (FileStream fs = new FileStream("Settings.json", FileMode.Open))
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SettingsViewModel));
          return serializer.ReadObject(fs) as SettingsViewModel;
        }
      }
      catch
      {
        return new SettingsViewModel();
      }
    }


    public string GetFileNameByDate(DateTime date)
    {
      return Path.Combine(this.LogFolder, date.ToShortDateString().Replace('/', '-')) + ".timelog";
    }


    private void NotifyPropertyChanged(string propertyName)
    {
      var handler = this.PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
