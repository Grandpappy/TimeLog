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
    public TimeSummaryViewModel Summary { get; private set; }
    private Timer DateChangeTimer { get; set; }
    private DateTime LastKnownDate { get; set; }
    public event EventHandler QueryUserToChangeToCurrentLog;
    public event EventHandler<TimeSpan> QueryUserAboutBeingIdle;
    public ICommand GenerateLogSummaryForCurrentLogCommand { get; private set; }
    private IdleCounter IdleCounter { get; set; }
    private DateTime? IdleStart { get; set; }


    
    public MainViewModel()
    {
      this.Settings = SettingsViewModel.ReadSettingsFile();
      this.Settings.PropertyChanged += Settings_PropertyChanged;
      this.LastKnownDate = DateTime.Now.Date;
      this.DateChangeTimer = new Timer(HandleDateChangeTimerChange, null, 1000, 1000);
      this.GenerateLogSummaryForCurrentLogCommand = new RelayCommand<LogViewModel>(GenerateLogSummaryForCurrentLogCommand_Execute);

      this.LogListing = new LogListingViewModel(this.Settings);
      this.Summary = new TimeSummaryViewModel();

      this.IdleCounter = new IdleCounter();
      this.IdleCounter.PropertyChanged += IdleCounter_PropertyChanged;

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



    private void IdleCounter_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!this.IdleStart.HasValue && this.Settings.QueryWhenIdle && this.IdleCounter.IdleTime > TimeSpan.FromMinutes(10) && this.CurrentLog.LogDate == DateTime.Now.Date)// TimeSpan.FromMinutes(10))
      {
        this.IdleStart = DateTime.Now.RoundToNearest15Minutes();

        var handler = this.QueryUserAboutBeingIdle;
        if (handler != null)
        {
          handler(this, this.IdleCounter.IdleTime);
        }
      }
      //else
      //{
      //  this.IdleStart = null;
      //}
    }


    public void ResetIdle()
    {
      this.IdleStart = null;
    }



    private void GenerateLogSummaryForCurrentLogCommand_Execute(LogViewModel logToGenerate)
    {
      if (logToGenerate == null)
        return;

      this.MessengerInstance.Send<GenerateTimeSummaryMessage>(new GenerateTimeSummaryMessage(logToGenerate));
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
      this.MessengerInstance.Send<GenerateTimeSummaryMessage>(new GenerateTimeSummaryMessage(this.CurrentLog));
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



    public void AddIdleEntriesToCurrentLog(string beforeIdleLabel, string whileIdleLabel)
    {
      if (this.CurrentLog.LogDate != DateTime.Now.Date || !this.IdleStart.HasValue)
        return;

      this.CurrentLog.AddEntry(new TimeEntryViewModel() { StartTime = this.CurrentLog.TimeEntries.Last().EndTime.RoundToNearest15Minutes(), EndTime = this.IdleStart.Value.RoundToNearest15Minutes(), Label = beforeIdleLabel });
      this.CurrentLog.AddEntry(new TimeEntryViewModel() { StartTime = this.IdleStart.Value.RoundToNearest15Minutes(), EndTime = DateTime.Now.RoundToNearest15Minutes(), Label = whileIdleLabel });
      
      this.ResetIdle();
    }
  }



  //public class UserIsIdleMessage
  //{
  //  public TimeSpan IdleTime { get; set; }
  //}
}