using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace lgRuntime
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class Debug : Window
    {
        private static Debug instance;

        private Debug()
        {
            InitializeComponent();
        }

        public static void log(string msg)
        {
            if (Debug.instance == null)
            {
                Debug.instance = new Debug();
                Debug.instance.Show();
            }
            Debug.instance.DebugOut.Text += (msg + "\n");

            if (!Debug.instance.IsVisible)
            {
                Debug.instance.Show();
            }

            Debug.instance.DebugOut.ScrollToEnd();
        }

        public static void Kill()
        {
            if (Debug.instance != null)
            {
                Debug.instance.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Debug.instance.Hide();
            e.Cancel = true;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Debug.instance.DebugOut.Text = String.Empty;
        }
    }
}
