using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.Threading;

namespace TimeLog.ViewModel
{
  public class MainViewModel : ViewModelBase
  {

    private LogViewModel _CurrentLog;

    public LogViewModel CurrentLog
    {
      get
      {
        return this._CurrentLog;
      }
      set
      {
        this.Set(() => CurrentLog, ref _CurrentLog, value);
      }
    }


    public SettingsViewModel Settings { get; private set; }
    public LogListingViewModel LogListing { get; private set; }
    private Timer DateChangeTimer { get; set; }
    private DateTime LastKnownDate { get; set; }
    public event EventHandler QueryUserToChangeToCurrentLog;


    
    public MainViewModel()
    {
      this.Settings = SettingsViewModel.ReadSettingsFile();
      this.Settings.PropertyChanged += Settings_PropertyChanged;
      this.LastKnownDate = DateTime.Now.Date;
      this.DateChangeTimer = new Timer(HandleDateChangeTimerChange, null, 1000, 1000);

      this.LogListing = new LogListingViewModel(this.Settings);

      if (!IsInDesignMode)
      {
        this.CurrentLog = LogViewModel.LoadByDate(DateTime.Now.Date, this.Settings);
      }
      else
      {
        this.CurrentLog = new LogViewModel(DateTime.Now, this.Settings);
      }

      this.MessengerInstance.Register<LoadLogMessage>(this, HandleLoadLogMessage);
    }



    private void HandleDateChangeTimerChange(object state)
    {
      if (DateTime.Now.Date != this.LastKnownDate)
      {
        this.LastKnownDate = DateTime.Now.Date;

        if (this.CurrentLog != null)
        {
          this.CurrentLog.WriteToFileAsync();
        }

        // Create the file for the next day and save it synchronously, so that it shows up in the log listing
        var log = LogViewModel.LoadByDate(this.LastKnownDate, this.Settings);
        log.WriteToFile();

        this.LogListing.LoadAllLogs();

        RaiseEvent(this.QueryUserToChangeToCurrentLog);
      }
    }



    internal void GoToLogForToday()
    {
      this.CurrentLog = LogViewModel.LoadByDate(this.LastKnownDate, this.Settings);
    }



    private void HandleLoadLogMessage(LoadLogMessage message)
    {
      this.CurrentLog = LogViewModel.LoadByFileName(message.LogToLoad.FileName, message.LogToLoad.Date, this.Settings);
    }



    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "LogFolder")
      {
        this.CurrentLog.WriteToFileAsync();
      }
    }



    private void RaiseEvent(EventHandler handler)
    {
      if (handler != null)
      {
        handler(this, EventArgs.Empty);
      }
    }

    
  }
}