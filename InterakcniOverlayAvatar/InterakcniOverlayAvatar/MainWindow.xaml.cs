using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InterakcniOverlayAvatar
{
    public partial class MainWindow : Window
    {
        // Definice instance třídy GlobalHook
        GlobalHook odposlech;

        int activityCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            // Vytvoření instance třídy GlobalHook
            odposlech = new GlobalHook(this);

            // Otevření a zavření okna (začátek a konec odposlechu)
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            this.StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            odposlech.Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            odposlech.Stop();

            // ukončit celý proces
            Environment.Exit(0);
        }

        public void ZaznamenejAktivitu()
        {
            activityCount++;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ActivityText.Text = $"Aktivita: {activityCount}";
            }));
        }

        public void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized) this.WindowState = WindowState.Maximized;
        }
    }
}