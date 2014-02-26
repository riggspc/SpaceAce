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

        // Constants
        public const int SHIP_SPEED = 6;
        public const int ASTEROID_SPEED = 10;
        // Margins may need to change depending on screen size and resolution
        public const int LEFT_MARGIN = 0;
        public const int RIGHT_MARGIN = 924;
        public const int TOP_MARGIN = 75;
        public const int BOTTOM_MARGIN = 650;

        public long score = 0;
        public List<Image> asteroids = new List<Image>();

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
                    // this is ugly, fix it later
                    // logic here is 'if the next tic would take the ship off
                    // of the screen, prevent it'
                    if ((currentLoc.Left <= (LEFT_MARGIN + SHIP_SPEED)) && (ship_speed.x < 0))
                    {
                        ship_speed.x = 0;
                    }
                    if ((currentLoc.Left >= (RIGHT_MARGIN - SHIP_SPEED)) && (ship_speed.x > 0))
                    {
                        ship_speed.x = 0;
                    }
                    if ((currentLoc.Top <= (TOP_MARGIN + SHIP_SPEED)) && (ship_speed.y < 0))
                    {
                        ship_speed.y = 0;
                    }
                    if ((currentLoc.Top >= (BOTTOM_MARGIN - SHIP_SPEED)) && (ship_speed.y > 0))
                    {
                        ship_speed.y = 0;
                    }
                    currentLoc.Left += ship_speed.x;
                    currentLoc.Top += ship_speed.y;
                    this.Player1.Margin = currentLoc;
                    this.Label1.Content = "Timer";
                    score++;
                    this.Score.Text = score.ToString();

                    if (score % 100 == 0)
                    {
                        Image newAsteroid = new Image();
                        newAsteroid.Source = this.LargeAsteroidSource.Source;
                        Thickness loc = newAsteroid.Margin;
                        loc.Left = RIGHT_MARGIN + 200;
                        newAsteroid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        newAsteroid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                        // Randomly place the asteroid somewhere along the edge,
                        // sizes it, sets the image, and the rotation
                        // basically makes it unique
                        Random rand = new Random();
                        loc.Top = Math.Max(BOTTOM_MARGIN - rand.Next(0, BOTTOM_MARGIN), TOP_MARGIN);
                        newAsteroid.Margin = loc;
                        newAsteroid.Width = rand.Next(100, 250);
                        int asteroidType = rand.Next(0, 2);
                        switch (asteroidType)
                        {
                            case 0:
                                newAsteroid.Source = this.LargeAsteroidSource.Source;
                                break;
                            case 1:
                                newAsteroid.Source = this.MediumAsteroidSource.Source;
                                break;
                            case 2:
                                newAsteroid.Source = this.SmallAsteroidSource.Source;
                                break;
                            default:
                                newAsteroid.Source = this.LargeAsteroidSource.Source;
                                break;

                        }

                        this.MainGrid.Children.Add(newAsteroid);
                        asteroids.Add(newAsteroid);
                        
                    }

                    foreach (Image asteroid in asteroids)
                    {
                        Thickness loc = asteroid.Margin;
                        loc.Left -= ASTEROID_SPEED;
                        asteroid.Margin = loc;
                    }

                    // If it's off the screen delete it
                    for (int i = 0; i < asteroids.Count; i++)
                    {

                        if (asteroids[i].Margin.Left + asteroids[i].Width < 0)
                        {
                            // this is hacky...might work though
                            asteroids.RemoveAt(i);
                            i--;
                        }
                    }
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
                    ship_speed.y = SHIP_SPEED * -1;
                    break;
                case Key.A:
                    //currentLoc.Left -= 5;
                    ship_speed.x = SHIP_SPEED * -1;
                    break;
                case Key.D:
                    //currentLoc.Left += 5;
                    ship_speed.x = SHIP_SPEED;
                    break;
                case Key.S:
                    //currentLoc.Top += 5;
                    ship_speed.y = SHIP_SPEED;
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
