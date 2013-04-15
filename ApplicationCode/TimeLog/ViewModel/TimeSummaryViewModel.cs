using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLog.ViewModel
{
  public class TimeSummaryViewModel : ViewModelBase
  {
    public ObservableCollection<TaskSummary> SummaryLineItems { get; private set; }



    public TimeSummaryViewModel()
    {
      this.SummaryLineItems = new ObservableCollection<TaskSummary>();
      this.MessengerInstance.Register<GenerateTimeSummaryMessage>(this, HandleTimeSummaryGeneration);
    }



    private void HandleTimeSummaryGeneration(GenerateTimeSummaryMessage message)
    {
      var entries = message.Log.TimeEntries;

      var distinctTags = entries
                  .Where(e => e.Type == TimeEntryTypes.Normal)
                  .Select(t => t.TaskIdentifier)
                  .Distinct(StringComparer.InvariantCultureIgnoreCase);

      var currentLineItems = 
        from distinctTag in distinctTags
        select
          new TaskSummary() 
          { 
            TaskIdentifier = distinctTag,

            HoursSpent = 
              entries
                .Where(x => x.TaskIdentifier == distinctTag)
                .Select(x => x.EndTime - x.StartTime)
                .Sum(x => x.TotalHours),

            Comments = 
            string.Join(", ", 
              entries
                .Where(x => x.TaskIdentifier == distinctTag && !string.IsNullOrWhiteSpace(x.Comment))
                .Select(x => x.Comment.Trim())
              )
                //.Aggregate((n, s) => !string.IsNullOrWhiteSpace(n) ? n + ", " + s : string.Empty)
          };


      this.SummaryLineItems.Clear();
      foreach (var lineItem in currentLineItems)
      {
        this.SummaryLineItems.Add(lineItem);
      }

    }
    
  }



  public class TaskSummary
  {
    public string TaskIdentifier { get; set; }
    public double HoursSpent { get; set; }
    public string Comments { get; set; }
  }



  public class GenerateTimeSummaryMessage
  {
    public LogViewModel Log { get; private set; }

    public GenerateTimeSummaryMessage(LogViewModel log)
    {
      this.Log = log;
    }
  }



  public static class StringExtensions
  {
    public static string ToTitleCase(this string s)
    {
      return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s);
    }
  }



  public static class ObjectExtensions
  {
    public static T UseIfNull<T>(this T value, Func<T> returnFunction)
    {
      if (value == null)
      {
        return returnFunction();
      }
      else
      {
        return value;
      }
    }
  }
}
