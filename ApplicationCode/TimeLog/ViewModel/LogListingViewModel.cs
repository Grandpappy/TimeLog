using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace TimeLog.ViewModel
{
  public class LogListingViewModel : ViewModelBase
  {
    public ObservableCollection<LogStub> AllLogs { get; set; }
    private SettingsViewModel Settings { get; set; }
    //public CollectionViewSource CSV { get; private set; }
    public ICollectionView AllLogsView { get; private set; }


    public LogListingViewModel(SettingsViewModel settings)
    {
      this.AllLogs = new ObservableCollection<LogStub>();
      this.Settings = settings;

      //this.CSV = new CollectionViewSource();
      //this.CSV.Source = this.AllLogs;

      this.AllLogsView = CollectionViewSource.GetDefaultView(this.AllLogs);
      this.AllLogsView.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Descending));
      this.AllLogsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
      //this.AllLogsView.Refresh

      LoadAllLogs();

      //this.AllLogsView.Refresh();
    }
    

    public void LoadAllLogs()
    {
      List<LogStub> stubsToAdd = new List<LogStub>();
      var loadDirectory = this.Settings.LogFolder;

      var directory = new DirectoryInfo(loadDirectory);
      var files = directory.GetFiles("*.timelog");
      foreach (var file in files)
      {
        var fileDateByName = file.Name.ToLower().Replace(".timelog", string.Empty).Replace('-', '/');

        DateTime logDate;
        if (!DateTime.TryParse(fileDateByName, out logDate))
          continue;

        var logStub =
          new LogStub()
          {
            FileName = file.FullName,
            Date = logDate.Date,
            Group = GetGroupByLogDate(logDate.Date)
          };

        stubsToAdd.Add(logStub);
      }

      UIThread.Run(() =>
        {
          this.AllLogs.Clear();
          stubsToAdd.ForEach(x => this.AllLogs.Add(x));
        });
    }


    private string GetGroupByLogDate(DateTime logDate)
    {
      var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;

      if (logDate == DateTime.Now.Date)
      {
        return "Today";
      }

      //if (logDate == DateTime.Now.Date.AddDays(-1))
      //{
      //  return "Yesterday";
      //}

      var logDateWeekOfYear = cal.GetWeekOfYear(logDate, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
      var currentWeekOfYear = cal.GetWeekOfYear(DateTime.Now, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

      if (logDateWeekOfYear == currentWeekOfYear)
      {
        return "This week";
      }

      if (logDateWeekOfYear == currentWeekOfYear - 1)
      {
        return "Last week";
      }
      
      return "OMG so long ago";
      
    }
  }


  public class LogStub : ViewModelBase
  {
    public string FileName { get; set; }
    public DateTime Date { get; set; }
    public string Group { get; set; }

    public ICommand ViewLogCommand { get; private set; }


    public LogStub()
    {
      this.ViewLogCommand = new RelayCommand(ViewLogCommand_Execute);
    }


    private void ViewLogCommand_Execute()
    {
      this.MessengerInstance.Send<LoadLogMessage>(new LoadLogMessage() { LogToLoad = this });
    }
  }


  public class LoadLogMessage
  {
    public LogStub LogToLoad { get; set; }
  }
}
