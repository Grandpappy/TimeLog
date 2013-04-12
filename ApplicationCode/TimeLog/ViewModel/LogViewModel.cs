using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace TimeLog.ViewModel
{

  public enum AddMethods
  {
    [Description("Since Last Entry")]
    SinceLastEntry,

    [Description("Manual")]
    Manual
  }


  //public class CondensedLog : ViewModelBase
  //{

  //}


  public class LogViewModel : ViewModelBase
  {
    private DateTime _LogDate;
    private TimeEntryViewModel _CurrentEntry;
    private AddMethods _CurrentAddMethod;

    public ObservableCollection<TimeEntryViewModel> TimeEntries { get; private set; }
    public ICommand AddEntryCommand { get; private set; }
    public SettingsViewModel Settings { get; private set; }


    public AddMethods CurrentAddMethod
    {
      get
      {
        return this._CurrentAddMethod;
      }
      set
      {
        this.Set(() => CurrentAddMethod, ref _CurrentAddMethod, value);
      }
    }

    public TimeEntryViewModel CurrentEntry
    {
      get
      {
        return _CurrentEntry;
      }
      private set
      {
        this.Set(() => CurrentEntry, ref _CurrentEntry, value);
      }
    }

    public DateTime LogDate
    {
      get
      {
        return _LogDate;
      }
      private set
      {
        this.Set(() => LogDate, ref _LogDate, value);
      }
    }



    internal LogViewModel(DateTime logDate, SettingsViewModel settings)
    {
      if (IsInDesignMode)
      {
        this.TimeEntries = new ObservableCollection<TimeEntryViewModel>()
        {
          new TimeEntryViewModel() {Type = TimeEntryTypes.StartOfWorkday, StartTime = new DateTime(), EndTime = this.Settings.WorkdayStart, Label = "Start the day" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-10), EndTime = DateTime.Now.AddHours(-9), Label = "Second Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-9), EndTime = DateTime.Now.AddHours(-8), Label = "Third Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-8), EndTime = DateTime.Now.AddHours(-7), Label = "First Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-7), EndTime = DateTime.Now.AddHours(-6), Label = "Second Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-6), EndTime = DateTime.Now.AddHours(-5), Label = "Third Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-5), EndTime = DateTime.Now.AddHours(-4), Label = "First Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-4), EndTime = DateTime.Now.AddHours(-3), Label = "Second Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-3), EndTime = DateTime.Now.AddHours(-2), Label = "Third Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-2), EndTime = DateTime.Now.AddHours(-1), Label = "Third Entry" },
          new TimeEntryViewModel() {Type = TimeEntryTypes.Normal, StartTime = DateTime.Now.AddHours(-1), EndTime = DateTime.Now.AddHours(-0), Label = "Third Entry" },
        };
      }
      else
      {
        this.TimeEntries = new ObservableCollection<TimeEntryViewModel>();
      }

      this.LogDate = logDate;
      this.Settings = settings;
      this.AddEntryCommand = new RelayCommand(AddEntryCommand_Execute, AddEntryCommand_CanExecute);
      this.MessengerInstance.Register<DeleteTimeEntryMessage>(this, HandleDeleteTimeEntryMessage);
      this.MessengerInstance.Register<TimeEntryEditingCompleteMessage>(this, HandleTimeEntryEditingCompleteMessage);
      this.MessengerInstance.Register<TimeEntryEditingStartedMessage>(this, HandleTimeEntryEditingStartedMessage);


      RefreshCurrentEntry();
    }



    private void HandleDeleteTimeEntryMessage(DeleteTimeEntryMessage message)
    {
      this.TimeEntries.Remove(message.TimeEntry);
      WriteToFileAsync();
    }



    private void HandleTimeEntryEditingCompleteMessage(TimeEntryEditingCompleteMessage obj)
    {
      if (!this.TimeEntries.Contains(obj.TimeEntry))
        return;

      this.WriteToFileAsync();
    }



    private void HandleTimeEntryEditingStartedMessage(TimeEntryEditingStartedMessage obj)
    {
      if (!this.TimeEntries.Contains(obj.TimeEntry))
        return;

      this.WriteToFileAsync();
    }



    private bool AddEntryCommand_CanExecute()
    {
      return this.CurrentEntry != null && !string.IsNullOrWhiteSpace(this.CurrentEntry.Label);
    }



    private void RefreshCurrentEntry()
    {
      var entry =new TimeEntryViewModel();
      entry.StartTime.RoundToNearest15Minutes();
      entry.EndTime.RoundToNearest15Minutes();

      this.CurrentEntry = entry;
    }



    private void AddEntryCommand_Execute()
    {
      if (this.CurrentAddMethod == AddMethods.SinceLastEntry)
      {
        if (this.TimeEntries.Count > 0)
        {
          this.CurrentEntry.StartTime = this.TimeEntries.Last().EndTime;
          this.CurrentEntry.EndTime = DateTime.Now.RoundToNearest15Minutes();
        }
      }

      this.TimeEntries.Add(this.CurrentEntry);
      WriteToFileAsync();

      RefreshCurrentEntry();
    }



    public void WriteToFileAsync()
    {
      Task.Factory.StartNew(() =>
        {
          WriteToFile();
        });
    }


    private object WriteLock = new object();


    public void WriteToFile()
    {
      lock (this.WriteLock)
      { 
        var builder = new StringBuilder();
        foreach (var entry in this.TimeEntries)
        {
          builder.AppendLine(entry.ToString());
        }

        var filename = this.Settings.GetFileNameByDate(this.LogDate);
        using (StreamWriter sw = new StreamWriter(filename, false))
        {
          sw.Write(builder.ToString());
        }
      }
    }



    public static LogViewModel LoadByDate(DateTime date, SettingsViewModel settings)
    {
      var filename = settings.GetFileNameByDate(date);

      return LoadByFileName(filename, date, settings);
    }



    public static LogViewModel LoadByFileName(string filename, DateTime date, SettingsViewModel settings)
    {
      if (File.Exists(filename))
      {
        var log = new LogViewModel(date, settings);

        using (var reader = new StreamReader(filename))
        {
          string currentLine;
          while ((currentLine = reader.ReadLine()) != null)
          {
            Match m = Regex.Match(currentLine, @"^\[(.*) - (.*) : (.*)\]\w*(.*)");
            if (m.Success)
            {
              try
              {
                var startDate = DateTime.Parse(m.Groups[1].Value);
                var endDate = DateTime.Parse(m.Groups[2].Value);
                var type = (TimeEntryTypes)Enum.Parse(typeof(TimeEntryTypes), m.Groups[3].Value);
                var label = m.Groups[4].Value.Trim();

                log.TimeEntries.Add(new TimeEntryViewModel() { StartTime = startDate, EndTime = endDate, Label = label, Type = type });
              }
              catch
              {

              }
            }
          }

        }

        return log;
      }
      else
      {
        var log = new LogViewModel(date, settings);
        log.TimeEntries.Add(new TimeEntryViewModel() { Type = TimeEntryTypes.StartOfWorkday, StartTime = new DateTime(), EndTime = settings.WorkdayStart, Label = "Start the day" });

        return log;
      }
    }
  }
}
