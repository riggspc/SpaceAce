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
        private Point p1_ship_speed, p2_ship_speed;

        // Constants
        private const int SHIP_SPEED = 5;
        private const int MAX_ASTEROID_SPEED = 10;
        private const int MIN_ASTEROID_SPEED = 6;

        // Margins may need to change depending on screen size and resolution
        private const int LEFT_MARGIN = 0;
        private const int RIGHT_MARGIN = 924;
        private const int TOP_MARGIN = 75;
        private const int BOTTOM_MARGIN = 650;

        private long score = 0;
        // public List<Image> asteroids = new List<Image>();
        private List<Tuple<Image, int> > asteroids = new List<Tuple<Image, int>>();

        private bool TwoPlayer = false;
        private bool game_paused = false;
        public MainWindow(bool num_players, Difficulty diff)
        {
            InitializeComponent();
            App.checkForJoy();
            App.timer.Elapsed += main_timerElapsed;
            App.inputEvent.HandleKeyDown += adjustSpeedUp;
            App.inputEvent.HandleKeyUp += adjustSpeedDown;

            TwoPlayer = num_players;
            if (TwoPlayer)
            {
                this.Player2.Visibility = Visibility.Visible;
                this.Player2_Label.Visibility = Visibility.Visible;
                this.Score2.Visibility = Visibility.Visible;
            }
        }

        public void moveShip(bool player1)
        {
            Thickness playerLoc;
            Point ship_speed;
            if (player1)
            {
                playerLoc = this.Player1.Margin;
                ship_speed = p1_ship_speed;

            }
            else
            {
                playerLoc = this.Player2.Margin;
                ship_speed = p2_ship_speed;
            }

            if ((playerLoc.Left + ship_speed.X > LEFT_MARGIN) && (playerLoc.Left + ship_speed.X < RIGHT_MARGIN))
                playerLoc.Left += ship_speed.X;
            if ((playerLoc.Top + ship_speed.Y > TOP_MARGIN) && (playerLoc.Top + ship_speed.Y < BOTTOM_MARGIN))
                playerLoc.Top += ship_speed.Y;

            if (player1)
                this.Player1.Margin = playerLoc;
            else
                this.Player2.Margin = playerLoc;
        }

        private void adjustSpeedUp(bool player1, Key key)
        {
            Point ship_speed;
            if (player1)
                ship_speed = p1_ship_speed;
            else
                ship_speed = p2_ship_speed;

            switch(key)
            {
                case Key.A:
                case Key.Left:
                    ship_speed.X = Math.Max(-SHIP_SPEED, ship_speed.X - SHIP_SPEED); 
                    break;
                case Key.D:
                case Key.Right:
                    ship_speed.X = Math.Min(SHIP_SPEED, ship_speed.X + SHIP_SPEED); 
                    break;
                case Key.W:
                case Key.Up:
                    ship_speed.Y = Math.Max(-SHIP_SPEED, ship_speed.Y - SHIP_SPEED); 
                    break;
                case Key.S:
                case Key.Down:
                    ship_speed.Y = Math.Min(SHIP_SPEED, ship_speed.Y + SHIP_SPEED); 
                    break;
            }

            if (player1)
                p1_ship_speed = ship_speed;
            else
                p2_ship_speed = ship_speed;
        }

        private void adjustSpeedDown(bool player1, Key key)
        {
            Point ship_speed;
            if (player1)
                ship_speed = p1_ship_speed;
            else
                ship_speed = p2_ship_speed;

            switch (key)
            {
                case Key.A:
                case Key.Left:
                    ship_speed.X = Math.Min(SHIP_SPEED, ship_speed.X + SHIP_SPEED);
                    break;
                case Key.D:
                case Key.Right:
                    ship_speed.X = Math.Max(-SHIP_SPEED, ship_speed.X - SHIP_SPEED);
                    break;
                case Key.W:
                case Key.Up:
                    ship_speed.Y = Math.Min(SHIP_SPEED, ship_speed.Y + SHIP_SPEED);
                    break;
                case Key.S:
                case Key.Down:
                    ship_speed.Y = Math.Max(-SHIP_SPEED, ship_speed.Y - SHIP_SPEED);
                    break;
            }

            if (player1)
                p1_ship_speed = ship_speed;
            else
                p2_ship_speed = ship_speed;
        }

        public void main_keyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    adjustSpeedUp(false, e.Key);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (TwoPlayer && !App.checkForJoy())
                        adjustSpeedUp(true, e.Key);
                    break;
                case Key.Escape:
                    game_paused = !game_paused;
                    break;
            }
        }

        public void main_keyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    adjustSpeedDown(false, e.Key);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if(TwoPlayer && !App.checkForJoy())
                        adjustSpeedDown(true, e.Key);
                    break;
            }
        }

        public void main_timerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // This check fixes a nullpointer exception when the window is closed while the
            // game is running
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (game_paused)
                        return;

                    //Move the players
                    moveShip(true);
                    if(TwoPlayer)
                        moveShip(false);
                    
                    //Update Scores
                    this.Label1.Content = "Timer";
                    score++;
                    this.Score1.Text = score.ToString();
                    this.Score2.Text = score.ToString();

                    //Generate Asteroids
                    generateAsteroids();
                });
            }
        }

        private void generateAsteroids()
        {
            Random rand = new Random();
            // Modify the RHS below to change asteroid creation
            // frequency
            if (rand.Next(0, 1000) > 975)
            {
                Image newAsteroid = new Image();
                // newAsteroid.Source = this.LargeAsteroidSource.Source;
                Thickness loc = newAsteroid.Margin;
                loc.Left = RIGHT_MARGIN + 200;
                newAsteroid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newAsteroid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                // Randomly place the asteroid somewhere along the edge,
                // sizes it, sets the image, and the rotation
                // basically makes it unique
                loc.Top = Math.Max(BOTTOM_MARGIN - rand.Next(0, BOTTOM_MARGIN), TOP_MARGIN);
                newAsteroid.Margin = loc;
                newAsteroid.Width = rand.Next(100, 250);

                // Determines which sprite the new asteroid will have
                int asteroidType = rand.Next(0, 3);
                Uri uri;
                switch (asteroidType)
                {
                    case 0:
                        // newAsteroid.Source = this.LargeAsteroidSource.Source;
                        uri = new Uri(@"../../Assets/asteroid_large.png", UriKind.Relative);
                        break;
                    case 1:
                        // newAsteroid.Source = this.MediumAsteroidSource.Source;
                        uri = new Uri(@"../../Assets/asteroid_small.png", UriKind.Relative);
                        break;
                    case 2:
                        // newAsteroid.Source = this.SmallAsteroidSource.Source;
                        uri = new Uri(@"../../Assets/asteroid_medium.png", UriKind.Relative);
                        break;
                    default:
                        // newAsteroid.Source = this.LargeAsteroidSource.Source;
                        uri = new Uri(@"../../Assets/asteroid_large.png", UriKind.Relative);
                        break;

                }

                // Set the asteroid's rotation
                TransformedBitmap tb = new TransformedBitmap();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = uri;
                bi.EndInit();
                tb.BeginInit();
                tb.Source = bi;
                RotateTransform transform = new RotateTransform(rand.Next(1, 4) * 90);
                tb.Transform = transform;
                tb.EndInit();
                newAsteroid.Source = tb;

                this.MainGrid.Children.Add(newAsteroid);
                asteroids.Add(new Tuple<Image, int>(newAsteroid, rand.Next(MIN_ASTEROID_SPEED, MAX_ASTEROID_SPEED)));

            }

            foreach (Tuple<Image, int> pair in asteroids)
            {
                Thickness loc = pair.Item1.Margin;
                loc.Left -= pair.Item2;
                pair.Item1.Margin = loc;
            }

            // If it's off the screen delete it
            for (int i = 0; i < asteroids.Count; i++)
            {

                if (asteroids[i].Item1.Margin.Left + asteroids[i].Item1.Width < 0)
                {
                    // this is hacky...might work though
                    asteroids.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
