using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cupertino.UI;
using Cupertino.Core;
using Cupertino.Platform.Win32;
using Cupertino.Platform.Win32.Decorations;
using System.Collections.ObjectModel;

namespace Cupertino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IWindowTracker tracker = new WindowTracker();

        public static readonly DependencyProperty StateProperty;
        public ViewState State
        {
            get { return (ViewState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        static MainWindow()
        {
            StateProperty = DependencyProperty.Register("State", typeof(ViewState), typeof(MainWindow));
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            State = new ViewState { ApplicationName = "Desktop" };
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            tracker.WindowSelected += Tracker_WindowSelected;
            Application.Current.Exit += (object sender, ExitEventArgs e) => {
                //DesktopAppBar.SetAppBar(this, AppBarEdge.None);
                Debug.Print("App.Exit");
            };
            tracker.Attatch();
        }

        private void Tracker_WindowSelected(IWindowRef window)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine("Switched to {0} - {1} (PID: {2}, {3})",
                window.ApplicationName, window.Title, window.ProcessID, window.ExecutablePath);
                ViewState newState = new ViewState 
                {
                    ApplicationName = window.ApplicationName,
                    MenuItems = MenuItemViewModel.FromMenuRef(window.Menu)
                };
                State = newState;
            });   
        }

        ~MainWindow()
        {
            try
            {
                tracker.Dispose();
            } catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
