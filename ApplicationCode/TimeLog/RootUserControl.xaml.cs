using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
  /// Interaction logic for RootUserControl.xaml
  /// </summary>
  public partial class RootUserControl : UserControl
  {
    private MainViewModel ViewModel { get; set; }

    public RootUserControl()
    {
      //this.DataContextChanged += MainWindow_DataContextChanged;

      InitializeComponent();
    }



    //private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    //{
    //  this.ViewModel = e.NewValue as MainViewModel;
    //  this.ViewModel.Settings.PropertyChanged += Settings_PropertyChanged;

    //  UpdateTheme();
    //  UpdateAccentColor();
    //}

  }
}
