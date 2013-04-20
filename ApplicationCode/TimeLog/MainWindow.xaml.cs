using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeLog.ViewModel;

namespace TimeLog
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : MetroWindow, INotifyPropertyChanged
  {
    private MainViewModel ViewModel { get; set; }

    private int _IdleTimeInMinutes;
    public int IdleTimeInMinutes
    {
      get { return _IdleTimeInMinutes; }
      set
      {
        _IdleTimeInMinutes = value;
        RaiseProperyChanged("IdleTimeInMinutes");
      }
    }


    public MainWindow()
    {
      this.Focusable = false;
      
      this.Loaded += (s, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
      this.Loaded += MainWindow_Loaded;
      this.PreviewMouseDoubleClick += MainWindow_PreviewMouseDoubleClick;
      this.DataContextChanged += MainWindow_DataContextChanged;

      SetDefaultFocusablePropertyToFalse();
      
      if (DesignerProperties.GetIsInDesignMode(this))
      {
        this.Background = Brushes.Black;
      }

      this.Closing += MainWindow_Closing;


      InitializeComponent();

      //this.EntryLabel.LostFocus += (sender, e) => { (Keyboard.FocusedElement as Control).Focusable = false; MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)); };
    }



    void MainWindow_Closing(object sender, CancelEventArgs e)
    {
      this.ViewModel.Settings.WriteSettingsFile();
    }
    

    
    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.ViewModel = e.NewValue as MainViewModel;
      this.ViewModel.Settings.PropertyChanged += Settings_PropertyChanged;
      this.ViewModel.QueryUserToChangeToCurrentLog += ViewModel_QueryUserToChangeToNewLog;
      this.ViewModel.QueryUserAboutBeingIdle += ViewModel_QueryUserAboutBeingIdle;

      UpdateTheme();
      UpdateAccentColor();
    }



    private void ViewModel_QueryUserAboutBeingIdle(object sender, TimeSpan e)
    {
      UIThread.Run(() =>
        {
          this.Activate();

          this.IdleQueryTransionContentControl.Content = this.Resources["IdleQueryContent"];

          this.IdleTimeInMinutes = (int)e.TotalMinutes;
          VisualStateManager.GoToElementState(this.RootVisual, "ShowIdleUserModal", true);
        });
    }



    private void ViewModel_QueryUserToChangeToNewLog(object sender, EventArgs e)
    {
      UIThread.Run(() =>
        {
          VisualStateManager.GoToElementState(this.RootVisual, "ShowDayChangeModal", true);
        });
    }



    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "UseDarkTheme")
      {
        UpdateTheme();
      }

      if (e.PropertyName == "AccentColor")
      {
        UpdateAccentColor();
      }
    }



    private void MainWindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }


    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      
    }


    //private void RemoveFocusability(FrameworkElement element)
    //{
    //  //var control = root as Control;


    //  if (element != null)
    //  {
    //    element.Focusable = false;

    //    foreach (object child in LogicalTreeHelper.GetChildren(element))
    //    {
    //      var childElement = child as FrameworkElement;
    //      if (childElement == null || childElement == this.EntryLabel)
    //        continue;

    //      RemoveFocusability(childElement);
    //    }
    //  }
    //}



    private void SetDefaultFocusablePropertyToFalse()
    {
      FocusableProperty.OverrideMetadata(typeof(ContentControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
    }
       


    private void UpdateTheme()
    {
      if (this.ViewModel.Settings.UseDarkTheme)
      {
        ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Orange"), Theme.Dark);
      }
      else
      {
        ThemeManager.ChangeTheme(this, ThemeManager.DefaultAccents.First(a => a.Name == "Orange"), Theme.Light);
      }

      UpdateAccentColor();
    }



    private void UpdateAccentColor()
    {
      var existingColor = (Color)this.Resources["AccentColor"];
      var targetColor = this.ViewModel.Settings.AccentColor;

      //this.Resources["AccentColor"] = new Color() { A = existingColor.A, R = targetColor.R, G = targetColor.G, B = targetColor.B };
      this.Resources["AccentColorBrush"] = new SolidColorBrush(new Color() { A = existingColor.A, R = targetColor.R, G = targetColor.G, B = targetColor.B });


      if (this.ViewModel.Settings.UseDarkTheme)
      {
        this.Resources["FlyoutBackgroundColor"] = new Color() { A = 255, R = (byte)(targetColor.R / 3), G = (byte)(targetColor.G / 3), B = (byte)(targetColor.B / 3) };
      }
      else
      {
        this.Resources["FlyoutBackgroundColor"] = new Color() { A = 255, R = (byte)Math.Min(255, targetColor.R * 1.25), G = (byte)Math.Min(255, targetColor.G * 1.25), B = (byte)Math.Min(255, targetColor.B * 1.25) };
      }
    }



    private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
    {
      this.Flyouts.First(x => x.Name == "SettingsFlyout").IsOpen = true;
      this.Flyouts.First(x => x.Name == "PastDaysFlyout").IsOpen = false;
    }



    private void OpenOtherDaysButton_Click(object sender, RoutedEventArgs e)
    {
      this.Flyouts.First(x => x.Name == "SettingsFlyout").IsOpen = false;
      this.Flyouts.First(x => x.Name == "PastDaysFlyout").IsOpen = true;
    }



    public event PropertyChangedEventHandler PropertyChanged;
    private void RaiseProperyChanged(string propertyName)
    {
      var handler = this.PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(propertyName));
      }
    }



    private void Click_Yes(object sender, RoutedEventArgs e)
    {
      UIThread.Run(() =>
        {
          this.ViewModel.GoToLogForToday();
          VisualStateManager.GoToElementState(this.RootVisual, "NoModal", true);
        });
    }



    private void Click_No(object sender, RoutedEventArgs e)
    {
      CloseModalDialogs();
    }



    private void CloseModalDialogs()
    {
      UIThread.Run(() =>
        {
          VisualStateManager.GoToElementState(this.RootVisual, "NoModal", true);
        });
    }



    private void OpenTimesheetSummary_Click(object sender, RoutedEventArgs e)
    {
      this.DaySummaryFlyout.IsOpen = true;
    }



    private void Click_IdleQuery_EnterMissingInformation(object sender, RoutedEventArgs e)
    {
      UIThread.Run(()=>
        {
          var queryAnswerContent = this.Resources["IdleAnswerTimesContent"] as FrameworkElement;
          //var beforeIdleAnswerTextBox = UIHelper.FindChild<TextBox>(queryAnswerContent, "BeforeIdleLabelTextBox");
          //beforeIdleAnswerTextBox.Focus();

          this.IdleQueryTransionContentControl.Content = queryAnswerContent;
        });
    }



    private void Click_IdleQuery_Ignore(object sender, RoutedEventArgs e)
    {
      CloseModalDialogs();

      UIThread.Run(() =>
      {
        this.IdleQueryTransionContentControl.Content = this.Resources["IdleQueryContent"];
        this.ViewModel.ResetIdle();
      });
    }


    private void Click_IdleQuery_EnterMissingEntries(object sender, RoutedEventArgs e)
    {
      CloseModalDialogs();

      UIThread.Run(() =>
      {
        var queryAnswerContent = this.Resources["IdleAnswerTimesContent"] as FrameworkElement;
        var beforeIdleAnswerTextBox = UIHelper.FindChild<TextBox>(queryAnswerContent, "BeforeIdleLabelTextBox");
        var whileIdleAnswerTextBox = UIHelper.FindChild<TextBox>(queryAnswerContent, "WhileIdleLabelTextBox");

        this.ViewModel.AddIdleEntriesToCurrentLog(beforeIdleAnswerTextBox.Text, whileIdleAnswerTextBox.Text);

        this.IdleQueryTransionContentControl.Content = this.Resources["IdleQueryContent"];
        this.ViewModel.ResetIdle();

        beforeIdleAnswerTextBox.Text = null;
        whileIdleAnswerTextBox.Text = null;
      });
    }


  }


  public static class UIHelper
  {
    public static T FindChild<T>(DependencyObject parent, string childName)
      where T : DependencyObject
    {    
      // Confirm parent and childName are valid. 
      if (parent == null) return null;

      T foundChild = null;

      int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < childrenCount; i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        // If the child is not of the request child type child
        T childType = child as T;
        if (childType == null)
        {
          // recursively drill down the tree
          //foundChild = FindChild<T>(child, childName);

          // If the child is found, break so we do not overwrite the found child. 
          if (foundChild != null) break;
        }
        else if (!string.IsNullOrEmpty(childName))
        {
          var frameworkElement = child as FrameworkElement;
          // If the child's name is set for search
          if (frameworkElement != null && frameworkElement.Name == childName)
          {
            // if the child's name is of the request name
            foundChild = (T)child;
            break;
          }
        }
        else
        {
          // child element found.
          foundChild = (T)child;
          break;
        }
      }

      return foundChild;
    }
  }

}
