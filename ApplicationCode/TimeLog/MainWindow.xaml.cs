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

    //private string _ModalText;
    //public string ModalText 
    //{
    //  get { return _ModalText; }
    //  set
    //  {
    //    _ModalText = value;
    //    RaiseProperyChanged("ModalText");
    //  }
    //}


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

      UpdateTheme();
      UpdateAccentColor();
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
      UIThread.Run(() =>
        {
          VisualStateManager.GoToElementState(this.RootVisual, "NoModal", true);
        });
    }



    private void OpenTimesheetInfo_Click(object sender, RoutedEventArgs e)
    {

    }


  }
}
