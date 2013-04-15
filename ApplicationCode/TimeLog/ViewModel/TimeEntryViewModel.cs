using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TimeLog.ViewModel
{
  public enum TimeEntryTypes { Normal, StartOfWorkday }


  public class TimeEntryViewModel : ViewModelBase
  {
    private bool _IsEditing;
    private DateTime _StartTime;
    private DateTime _EndTime;
    private string _Label;
    private TimeEntryTypes _Type;


    public TimeEntryTypes Type
    {
      get
      {
        return _Type;
      }

      set
      {
        this.Set(() => this.Type, ref _Type, value);
      }
    }

    public DateTime StartTime 
    { 
      get
      {
        return _StartTime;
      }

      set
      {
        this.Set(() => this.StartTime, ref _StartTime, value);

        if (this.StartTime >= this.EndTime && !this.IsInDesignMode)
        {
          this.EndTime = this.StartTime.AddMinutes(15).RoundToNearest15Minutes();
        }
      } 
    }

    public DateTime EndTime
    {
      get
      {
        return _EndTime;
      }

      set
      {
        this.Set(() => this.EndTime, ref _EndTime, value);

        if (this.EndTime <= this.StartTime && !this.IsInDesignMode)
        {
          this.StartTime = this.EndTime.AddMinutes(-15).RoundToNearest15Minutes();
        }
      }
    }

    public string Label
    {
      get
      {
        return _Label;
      }

      set
      {
        this.Set(() => this.Label, ref _Label, value);
      }
    }

    public bool IsEditing
    {
      get
      {
        return _IsEditing;
      }
      set
      {
        this.Set(() => this.IsEditing, ref _IsEditing, value);
      }
    }

    public string TaskIdentifier
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.Label))
        {
          return null;
        }

        var value = this.Label.Trim().Split(' ').FirstOrDefault();
        if (value != null)
        {
          return value.ToTitleCase();
        }

        return null;
      }
    }

    public string Comment
    {
      get
      {
        if (string.IsNullOrWhiteSpace(this.Label))
        {
          return null;
        }

        return this.Label.Trim().Remove(0, this.TaskIdentifier.Length);
      }
    }



    public ICommand DeleteEntryCommand { get; private set; }
    public ICommand ToggleEditCommand { get; private set; }



    public TimeEntryViewModel()
    {
      this.DeleteEntryCommand = new RelayCommand(DeleteEntryCommand_Execute);
      this.ToggleEditCommand = new RelayCommand(ToggleEditCommand_Execute);
      this.MessengerInstance.Register<TimeEntryEditingStartedMessage>(this, HandleTimeEntryEditingStartedMessage);

      if (IsInDesignMode)
      {
        this.StartTime = new DateTime(2013, 3, 27, 1, 23, 0);
        this.EndTime = new DateTime(2013, 3, 27, 2, 0, 0);
        this.Label = "Test Label";
      }
      else
      {
        this.StartTime = DateTime.Now;
      }
    }



    private void DeleteEntryCommand_Execute()
    {
      this.MessengerInstance.Send<DeleteTimeEntryMessage>(new DeleteTimeEntryMessage() { TimeEntry = this });
    }



    private void ToggleEditCommand_Execute()
    {
      this.IsEditing = !this.IsEditing;
      if (this.IsEditing)
      {
        this.MessengerInstance.Send<TimeEntryEditingStartedMessage>(new TimeEntryEditingStartedMessage() { TimeEntry = this });
      }
      else
      {
        this.MessengerInstance.Send<TimeEntryEditingCompleteMessage>(new TimeEntryEditingCompleteMessage() { TimeEntry = this });
      }
    }



    public override string ToString()
    {
      return string.Format("[{0} - {1} : {2}] {3}", this.StartTime.ToShortTimeString(), this.EndTime.ToShortTimeString(), this.Type, this.Label);
    }



    private void HandleTimeEntryEditingStartedMessage(TimeEntryEditingStartedMessage obj)
    {
      if (obj.TimeEntry != this)
      {
        this.IsEditing = false;
      }
    }
  }



  public class DeleteTimeEntryMessage
  {
    public TimeEntryViewModel TimeEntry { get; set; }
  }


  public class TimeEntryEditingStartedMessage
  {
    public TimeEntryViewModel TimeEntry { get; set; }
  }


  public class TimeEntryEditingCompleteMessage
  {
    public TimeEntryViewModel TimeEntry { get; set; }
  }
}
