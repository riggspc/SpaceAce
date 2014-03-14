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
        private double Left_Margin = 0;
        private double Right_Margin = 924;
        private double Top_Margin = 75;
        private double Bottom_Margin = 650;

        private long score = 0;
        // public List<Image> asteroids = new List<Image>();
        private List<Tuple<Image, int> > asteroids = new List<Tuple<Image, int>>();

        private enum opt { resume, returnToStart, exitGame };
        private opt curOpt = opt.resume;

        private bool TwoPlayer = false;
        private bool game_paused = false;
        private InputType P1, P2;
        private Difficulty diff;
        private long gameClock = 0;
        private bool countdownOn = true;
        public MainWindow(bool num_players, Difficulty _diff, InputType p1_in, InputType p2_in)
        {
            InitializeComponent();
            App.timer.Elapsed += main_timerElapsed;

            diff = _diff;
            P1 = p1_in;
            P2 = p2_in;

            //Check if a joystick is used
            if (p1_in == InputType.joy || p2_in == InputType.joy)
            {
                App.checkForJoy();
                App.inputEvent.HandleJoyUp += main_joyUp;
                App.inputEvent.HandleJoyDown += main_joyDown;
            }

            //Check if game is two player
            TwoPlayer = num_players;
            if (TwoPlayer)
            {
                this.Player2.Visibility = Visibility.Visible;
                this.Player2_Label.Visibility = Visibility.Visible;
                this.Score2.Visibility = Visibility.Visible;
            }
        }

        private void countdown()
        {
            if (countdownOn)
            {
                switch (gameClock)
                {
                    case 1:
                        Thickness player1Loc = this.Player1.Margin;
                        player1Loc.Left = Left_Margin;
                        if (TwoPlayer)
                        {
                            player1Loc.Top = (Bottom_Margin - Top_Margin + this.Player1_Label_View.ActualHeight)/3;
                            Thickness player2Loc = this.Player2.Margin;
                            player2Loc.Top = 2*player1Loc.Top;
                            player2Loc.Left = Left_Margin;
                            this.Player2.Margin = player2Loc;
                        }
                        else
                            player1Loc.Top = (Bottom_Margin - Top_Margin + this.Player1_Label_View.ActualHeight) / 2;
                        this.Player1.Margin = player1Loc;

                        this.count.Visibility = Visibility.Visible;
                        this.count.Text = "3";
                        break;
                    case 75:
                        this.count.Text = "2";
                        break;
                    case 150:
                        this.count.Text = "1";
                        break;
                    case 225:
                        this.count.Text = "GO!";
                        break;
                    case 300:
                        this.count.Visibility = Visibility.Collapsed;
                        countdownOn = false;
                        gameClock = 0;
                        break;
                }
            }
        }

        private void screenResize(object sender, System.EventArgs e)
        {
            //Add spaceship scaling?

            //Update Margins
            Right_Margin = this.ActualWidth - this.Player1.ActualWidth;
            Bottom_Margin = this.ActualHeight - this.Player1.ActualHeight;
            Top_Margin = this.Player1_Label_View.ActualHeight;

            //Make sure Player1 is in bounds
            Thickness playerLoc = this.Player1.Margin;
            if (playerLoc.Top < Top_Margin)
                playerLoc.Top = Top_Margin;
            if (playerLoc.Top > Bottom_Margin)
                playerLoc.Top = Bottom_Margin;
            if (playerLoc.Left > Right_Margin)
                playerLoc.Left = Right_Margin;
            this.Player1.Margin = playerLoc;

            //Make sure Player2 is in bounds
            if(TwoPlayer)
            {
                playerLoc = this.Player2.Margin;
                if (playerLoc.Top < Top_Margin)
                    playerLoc.Top = Top_Margin;
                if (playerLoc.Top > Bottom_Margin)
                    playerLoc.Top = Bottom_Margin;
                if (playerLoc.Left > Right_Margin)
                    playerLoc.Left = Right_Margin;
                this.Player2.Margin = playerLoc;
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

            if ((playerLoc.Left + ship_speed.X > Left_Margin) && (playerLoc.Left + ship_speed.X < Right_Margin))
                playerLoc.Left += ship_speed.X;
            if ((playerLoc.Top + ship_speed.Y > Top_Margin) && (playerLoc.Top + ship_speed.Y < Bottom_Margin))
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
            if (game_paused)
                pause_inputEvent(InputType.wasd, e.Key);

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    if (P1 == InputType.arrows)
                        adjustSpeedUp(true, e.Key);
                    else if (TwoPlayer && P2 == InputType.arrows)
                        adjustSpeedUp(false, e.Key);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (P1 == InputType.wasd)
                        adjustSpeedUp(true, e.Key);
                    else if (TwoPlayer && P2 == InputType.wasd)
                        adjustSpeedUp(false, e.Key);
                    break;
                case Key.Escape:
                    if (App.menuDelay != 0)
                        break;

                    if (countdownOn)
                        this.count.Visibility = Visibility.Collapsed;

                    this.pause_header.Visibility = Visibility.Visible;
                    this.pause_leftShip.Visibility = Visibility.Visible;
                    this.pause_rightShip.Visibility = Visibility.Visible;
                    this.pause_resume.Visibility = Visibility.Visible;
                    this.pause_returnToStart.Visibility = Visibility.Visible;
                    this.pause_exitGame.Visibility = Visibility.Visible;
                    this.pause_background.Opacity = 1;
                    game_paused = true;
                    break;
            }
        }
       
        public void main_keyUp(object sender, KeyEventArgs e)
        {
            if (game_paused)
                return;

            switch(e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    if(P1 == InputType.arrows)
                        adjustSpeedDown(true, e.Key);
                    else if(TwoPlayer && P2 == InputType.arrows)
                        adjustSpeedDown(false, e.Key);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (P1 == InputType.wasd)
                        adjustSpeedDown(true, e.Key);
                    else if (TwoPlayer && P2 == InputType.wasd)
                        adjustSpeedDown(false, e.Key);
                    break;
            }
        }

        public void main_joyDown(Key key)
        {
            if (game_paused)
                pause_inputEvent(InputType.joy, key);
            else if (P1 == InputType.joy)
                adjustSpeedUp(true, key);
            else if (TwoPlayer && P2 == InputType.joy)
                adjustSpeedUp(false, key);
        }

        public void main_joyUp(Key key)
        {
            if (game_paused)
                return;
            else if (P1 == InputType.joy)
                adjustSpeedDown(true, key);
            else if (TwoPlayer && P2 == InputType.joy)
                adjustSpeedDown(false, key);
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

                    gameClock++;
                    if (countdownOn)
                    {
                        countdown();
                        return;
                    }

                    //Move the players
                    moveShip(true);
                    if(TwoPlayer)
                        moveShip(false);
                    
                    //Update Scores
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
                loc.Left = Right_Margin + 200;
                newAsteroid.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newAsteroid.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                // Randomly place the asteroid somewhere along the edge,
                // sizes it, sets the image, and the rotation
                // basically makes it unique
                loc.Top = Math.Max(Bottom_Margin - rand.Next(0, (int) Bottom_Margin), Top_Margin);
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

        private void pause_inputEvent(InputType inType, Key key)
        {
            if (App.menuDelay != 0)
                return;

            switch (key)
            {
                case Key.Up:
                case Key.W:
                    if (curOpt == opt.resume)
                        updateFont(opt.exitGame);
                    else
                        updateFont(curOpt - 1);
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Down:
                case Key.S:
                    if (curOpt == opt.exitGame)
                        updateFont(opt.resume);
                    else
                        updateFont(curOpt + 1);
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Space:
                case Key.Enter:
                    selectOpt();
                    break;
                case Key.Escape:
                    updateFont(opt.resume);
                    App.menuDelay++;
                    selectOpt();
                    break;
            }
        }

        private void updateFont(opt nextOpt)
        {
            switch (curOpt)
            {
                case opt.resume:
                    this.pause_resume.Foreground = Brushes.White;
                    break;
                case opt.returnToStart:
                    this.pause_returnToStart.Foreground = Brushes.White;
                    break;
                case opt.exitGame:
                    this.pause_exitGame.Foreground = Brushes.White;
                    break;
            }

            switch (nextOpt)
            {
                case opt.resume:
                    this.pause_resume.Foreground = Brushes.Yellow;
                    break;
                case opt.returnToStart:
                    this.pause_returnToStart.Foreground = Brushes.Yellow;
                    break;
                case opt.exitGame:
                    this.pause_exitGame.Foreground = Brushes.Yellow;
                    break;
            }

            curOpt = nextOpt;
        }

        private void selectOpt()
        {
            switch(curOpt)
            {
                case opt.resume:
                    this.pause_header.Visibility = Visibility.Collapsed;
                    this.pause_leftShip.Visibility = Visibility.Collapsed;
                    this.pause_rightShip.Visibility = Visibility.Collapsed;
                    this.pause_resume.Visibility = Visibility.Collapsed;
                    this.pause_returnToStart.Visibility = Visibility.Collapsed;
                    this.pause_exitGame.Visibility = Visibility.Collapsed;
                    this.pause_background.Opacity = 0;
                    if (countdownOn)
                        this.count.Visibility = Visibility.Visible;
                    
                    p1_ship_speed.X = 0;
                    p1_ship_speed.Y = 0;
                    p2_ship_speed.X = 0;
                    p2_ship_speed.Y = 0;
                    game_paused = false;
                    break;
                case opt.returnToStart:
                    StartWindow start = new StartWindow();
                    App.Current.MainWindow = start;
                    start.Show();
                    this.Close();
                    break;
                case opt.exitGame:
                    Application.Current.Shutdown();
                    break;
            }
        }
    }
}
