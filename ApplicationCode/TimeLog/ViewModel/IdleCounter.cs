using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GalaSoft.MvvmLight;
using System.Windows.Threading;


namespace TimeLog.ViewModel
{
  public class IdleCounter : ObservableObject
  {
    private TimeSpan _IdleTime;
    public TimeSpan IdleTime
    {
      get
      {
        return _IdleTime;
      }
      private set
      {
        if (_IdleTime == value)
          return;

        this.Set(() => this.IdleTime, ref _IdleTime, value);
      }
    }


    // Unmanaged function from user32.dll
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    // Struct we'll need to pass to the function
    internal struct LASTINPUTINFO
    {
      public uint cbSize;
      public uint dwTime;
    }

    
    private DispatcherTimer IdleCheckTimer { get; set; }



    public IdleCounter()
    {
      this.IdleCheckTimer = new DispatcherTimer();
      this.IdleCheckTimer.Interval = TimeSpan.FromSeconds(15); // TimeSpan.FromSeconds(15);
      this.IdleCheckTimer.Tick += timer_Tick;
      this.IdleCheckTimer.Start();
    }



    private void timer_Tick(object sender, EventArgs e)
    {
      GetIdleTime();
    }



    public void GetIdleTime()
    {
      int systemUptime = Environment.TickCount;
      
      // Set the struct
      LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
      lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
      lastInputInfo.dwTime = 0;

      if (GetLastInputInfo(ref lastInputInfo))
      {
        // Get the number of ticks at the point when the last activity was seen
        var lastInputTicks = (int)lastInputInfo.dwTime;

        // Number of idle ticks = system uptime ticks - number of ticks at last input
        //var idleTicks = systemUptime - lastInputTicks;

        this.IdleTime = TimeSpan.FromMilliseconds(systemUptime - lastInputTicks);
      }

    }
  }
}
