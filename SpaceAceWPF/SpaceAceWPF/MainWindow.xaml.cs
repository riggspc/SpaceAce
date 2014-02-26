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
using System.Timers;

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
   
    public partial class MainWindow : Window
    {
        public struct ship_speed_t
        {
            public int x;
            public int y;
        }

        public ship_speed_t ship_speed;
        public const int SPEED = 2;

        public bool keyDown = false;
        public MainWindow()
        {
            InitializeComponent();

            var aTimer = new Timer(10);
            aTimer.Elapsed += ATimerOnElapsed;
            aTimer.Interval = 10;
            aTimer.Enabled = true;
        }

        private void Label_Loaded(object sender, RoutedEventArgs e)
        {
            this.Label1.Focus();
            this.Label1.Content = "halsdkfjlaskdjf";
        }

        private void ATimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // This check fixes a nullpointer exception when the window is closed while the
            // game is running
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Thickness currentLoc = this.Player1.Margin;
                    currentLoc.Left += ship_speed.x;
                    currentLoc.Top += ship_speed.y;
                    this.Player1.Margin = currentLoc;
                    this.Label1.Content = "Timer";
                });
            }
        }

        private void Label_KeyUp(object sender, KeyEventArgs e)
        {
            this.Label1.Content = "Up";
            switch (e.Key)
            {
                case Key.W:
                    //currentLoc.Top -= 5;
                    ship_speed.y = 0;
                    break;
                case Key.A:
                    //currentLoc.Left -= 5;
                    ship_speed.x = 0;
                    break;
                case Key.D:
                    //currentLoc.Left += 5;
                    ship_speed.x = 0;
                    break;
                case Key.S:
                    //currentLoc.Top += 5;
                    ship_speed.y = 0;
                    break;


            }
            //this.Player1.Margin = currentLoc;
        }



        private void Label_KeyDown(object sender, KeyEventArgs e)
        {
            this.Label1.Content = "down";
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
                    //currentLoc.Top -= 5;
                    ship_speed.y = SPEED * -1;
                    break;
                case Key.A:
                    //currentLoc.Left -= 5;
                    ship_speed.x = SPEED * -1;
                    break;
                case Key.D:
                    //currentLoc.Left += 5;
                    ship_speed.x = SPEED;
                    break;
                case Key.S:
                    //currentLoc.Top += 5;
                    ship_speed.y = SPEED;
                    break;

            }
            //this.Player1.Margin = currentLoc;
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
