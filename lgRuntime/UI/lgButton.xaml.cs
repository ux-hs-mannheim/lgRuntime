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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lgRuntime.UI
{
    /// <summary>
    /// Interaction logic for lgButton.xaml
    /// </summary>
    public partial class lgButton : UserControl
    {
        public static DependencyProperty TextValueProperty = DependencyProperty.Register("TextValue", typeof(string), typeof(lgButton));
        public string TextValue
        {
            get { return (string)GetValue(TextValueProperty); }
            set { SetValue(TextValueProperty, value); }
        }

        private static BrushConverter cConverter = new BrushConverter();
        public static SolidColorBrush NormalColorBrush = cConverter.ConvertFromString("#42a35a") as SolidColorBrush;
        public static SolidColorBrush HighlightColorBrush = cConverter.ConvertFromString("#5bbd73") as SolidColorBrush;
        public static SolidColorBrush SpecialColorBrush = cConverter.ConvertFromString("#812F9F") as SolidColorBrush;

        public lgButton()
        {
            InitializeComponent();
        }

        private void ButtonText_TouchLeave(object sender, TouchEventArgs e)
        {
            this.ColorNormal();
            this.ButtonText.Margin = new Thickness(0, 0, 0, 0);
        }

        private void ButtonText_TouchDown(object sender, TouchEventArgs e)
        {
            this.ColorHighlight();
            this.ButtonText.Margin = new Thickness(3, 3, 0, 0);
        }

        private void ButtonText_TouchUp(object sender, TouchEventArgs e)
        {
            this.ColorNormal();
            this.ButtonText.Margin = new Thickness(0, 0, 0, 0);
        }

        public void ColorHighlight()
        {
            this.ButtonText.Background = HighlightColorBrush;
        }

        public void ColorNormal()
        {
            this.ButtonText.Background = NormalColorBrush;
        }

    }
}
