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

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool keyDown = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            this.Label1.Focus();
            this.Label1.Content = "halsdkfjlaskdjf";
        }

        private void Label_KeyDown(object sender, KeyEventArgs e)
        {
            // this.Label1.Content = "BOOOOOOOM";
            Thickness currentLoc = this.Player1.Margin;
            /*
            if (e.Key == Key.W)
            {
                currentLoc.Top--;
                this.Player1.Margin = currentLoc;
            }
            */

            switch (e.Key)
            {
                case Key.W:
                    currentLoc.Top -= 5;
                    break;
                case Key.A:
                    currentLoc.Left -= 5;
                    break;
                case Key.D:
                    currentLoc.Left += 5;
                    break;
                case Key.S:
                    currentLoc.Top += 5;
                    break;

            }
            this.Player1.Margin = currentLoc;
        }

        private void get_Focus(object sender, RoutedEventArgs e)
        {
            this.Label1.Focus();
        }

        private void Label1_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this.Label1.Content = "wheeeee";
        }

        private void Label1_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Label1.Content = "clicked bitch";
        }


        
    }
}
