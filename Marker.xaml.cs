using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScreenCheckerNET
{
    /// <summary>
    /// Interaction logic for Marker.xaml
    /// </summary>
    public partial class Marker : Window
    {
        public Marker()
        {
            InitializeComponent();
        }

        public void SetSize(int x, int y)
        {
            if (x == 0 && y == 0)
            {
              
                Mark.BorderBrush = Brushes.Transparent;
            }
            else
            {
                Mark.BorderBrush = Brushes.Red;
                WindowMarker.Height = y;
                WindowMarker.Width = x;
            }
        }

        public void SetLocation(int x, int y)
        {
            WindowMarker.Top = y;
            WindowMarker.Left = x;
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var window = (Window)sender;
            window.Topmost = true;
        }
    }
}
