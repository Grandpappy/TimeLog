using System;
using System.Windows.Threading;

namespace TimeLog
{
  public class UIThread
  {
    private static bool _invokeSynchronously;

    static UIThread()
    {
      var app = System.Windows.Application.Current;
      UIThread.Dispatcher = app != null ? app.Dispatcher : Dispatcher.CurrentDispatcher;
    }

    public static void Initialize(bool invokeSynchronously)
    {
      _invokeSynchronously = invokeSynchronously;
    }

    public static void Initialize(Dispatcher uiThreadDispatcher)
    {
      UIThread.Dispatcher = uiThreadDispatcher;
    }

    public static void Initialize(Dispatcher uiThreadDispatcher, bool invokeSynchronously)
    {
      _invokeSynchronously = invokeSynchronously;
      UIThread.Dispatcher = uiThreadDispatcher;
    }

    public static Dispatcher Dispatcher { get; private set; }

    public static void Run(Action action, DispatcherPriority priority)
    {
      if (!_invokeSynchronously)
      {
        UIThread.Dispatcher.BeginInvoke(action, priority);
      }
      else
      {
        action();
      }
    }

    public static void Run(Action action)
    {
      if (!_invokeSynchronously)
      {
        if (UIThread.Dispatcher.CheckAccess())
          action();
        else
          UIThread.Dispatcher.BeginInvoke(action);
      }
      else
      {
        action();
      }
    }
  }
}
