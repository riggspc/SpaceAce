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
using System.Text.RegularExpressions;
using System.Timers;

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private class Projectile
        {
            public System.Windows.Controls.Image image;
            public TransformedBitmap bitmap;
            public System.Windows.Point speed;
        }

        private class Spaceship : Projectile
        {
            public int shield = 100;
            public long score = 0;
        }

        private class Coin : Projectile
        {
            public int value;
        }

        private class Health : Projectile
        {
            public int value;
        }

        // Constants
        private const int SHIP_SPEED = 5;
        private const int BASE_MAX_PROJECTILE_SPEED = 6;
        private const int BASE_MIN_PROJECTILE_SPEED = 4;

        // Margins may need to change depending on screen size and resolution
        private double Left_Margin = 0;
        private double Right_Margin = 924;
        private double Top_Margin = 75;
        private double Bottom_Margin = 650;

        // Ships, coins, and asteroids
        private Spaceship p1_ship = new Spaceship();
        private Spaceship p2_ship = new Spaceship();
        private List<Projectile> asteroids = new List<Projectile>();
        private List<Coin> coins = new List<Coin>();
        private List<Health> healths = new List<Health>();

        // Menu Options
        private enum opt { resume, returnToStart, exitGame };
        private opt curOpt = opt.resume;

        // Scoreboard
        Scoreboard scoreboard = new Scoreboard();

        private bool TwoPlayer = false;
        private bool game_paused = false;
        private bool game_over = false;
        private InputType p1_in, p2_in;
        private Difficulty diff;
        private int difficulty_multiplier;
        private long gameClock = 0;
        private bool countdownOn = true;
        public MainWindow(bool num_players, Difficulty _diff, InputType P1, InputType P2)
        {
            InitializeComponent();
            App.timer.Elapsed += main_timerElapsed;

            diff = _diff;
            p1_in = P1;
            p2_in = P2;

            // Note: this can probably be done by casting enum to an int
            // but this is more readable
            switch (diff)
            {
                case Difficulty.easy:
                    difficulty_multiplier = 1;
                    break;
                case Difficulty.med:
                    difficulty_multiplier = 2;
                    break;
                case Difficulty.hard:
                    difficulty_multiplier = 3;
                    break;
            }

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
                this.Shield2.Visibility = Visibility.Visible;
            }

            //Initialize players' ships
            p1_ship.image = this.Player1;
            p1_ship.speed.X = 0;
            p1_ship.speed.Y = 0;
            p1_ship.bitmap = new TransformedBitmap();
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(@"../../Assets/player1_small_with_fire.png", UriKind.Relative);
            bmpImage.EndInit();
            p1_ship.bitmap.BeginInit();
            p1_ship.bitmap.Source = bmpImage;
            ScaleTransform transform = new ScaleTransform(1, 1);
            p1_ship.bitmap.Transform = transform;
            p1_ship.bitmap.EndInit();
            this.Player1.Source = p1_ship.bitmap;

            if (TwoPlayer)
            {
                p2_ship.image = this.Player2;
                p2_ship.speed.X = 0;
                p2_ship.speed.Y = 0;
                p2_ship.bitmap = new TransformedBitmap();
                bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.UriSource = new Uri(@"../../Assets/player2_small_with_fire.png", UriKind.Relative);
                bmpImage.EndInit();
                p2_ship.bitmap.BeginInit();
                p2_ship.bitmap.Source = bmpImage;
                p2_ship.bitmap.Transform = transform;
                p2_ship.bitmap.EndInit();
                this.Player2.Source = p2_ship.bitmap;
            }

            this.WindowState = System.Windows.WindowState.Maximized;
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
                            player1Loc.Top = (Bottom_Margin + Top_Margin) / 3;
                            Thickness player2Loc = this.Player2.Margin;
                            player2Loc.Top = 2 * player1Loc.Top;
                            player2Loc.Left = Left_Margin;
                            this.Player2.Margin = player2Loc;
                        }
                        else
                            player1Loc.Top = (Bottom_Margin + Top_Margin) / 2;
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
            Top_Margin = this.Player1_Label_View.ActualHeight + this.Shield1_View.ActualHeight;

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
            if (TwoPlayer)
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
                ship_speed = p1_ship.speed;
            }
            else
            {
                playerLoc = this.Player2.Margin;
                ship_speed = p2_ship.speed;
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
                ship_speed = p1_ship.speed;
            else
                ship_speed = p2_ship.speed;

            switch (key)
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
                p1_ship.speed = ship_speed;
            else
                p2_ship.speed = ship_speed;
        }

        private void adjustSpeedDown(bool player1, Key key)
        {
            Point ship_speed;
            if (player1)
                ship_speed = p1_ship.speed;
            else
                ship_speed = p2_ship.speed;

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
                p1_ship.speed = ship_speed;
            else
                p2_ship.speed = ship_speed;
        }

        public void main_keyDown(object sender, KeyEventArgs e)
        {
            if (highScoreInput == InputType.wasd)
            {
                if(e.Key == Key.W || e.Key == Key.A || e.Key == Key.S || 
                   e.Key == Key.D || e.Key == Key.Space || e.Key == Key.Enter)
                    hs_inputEvent(InputType.wasd, e.Key);
                return;
            }
            else if (highScoreInput == InputType.arrows)
            {
                if (e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Down || 
                    e.Key == Key.Right || e.Key == Key.Space || e.Key == Key.Enter)
                    hs_inputEvent(InputType.arrows, e.Key);
                return;
            }
            else if (game_paused || game_over)
            {
                pause_inputEvent(InputType.wasd, e.Key);
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    if (p1_in == InputType.arrows)
                        adjustSpeedUp(true, e.Key);
                    else if (TwoPlayer && p2_in == InputType.arrows)
                        adjustSpeedUp(false, e.Key);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (p1_in == InputType.wasd)
                        adjustSpeedUp(true, e.Key);
                    else if (TwoPlayer && p2_in == InputType.wasd)
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
            if (game_paused || game_over)
                return;

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                    if (p1_in == InputType.arrows)
                        adjustSpeedDown(true, e.Key);
                    else if (TwoPlayer && p2_in == InputType.arrows)
                        adjustSpeedDown(false, e.Key);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (p1_in == InputType.wasd)
                        adjustSpeedDown(true, e.Key);
                    else if (TwoPlayer && p2_in == InputType.wasd)
                        adjustSpeedDown(false, e.Key);
                    break;
            }
        }

        public void main_joyDown(Key key)
        {
            if (highScoreInput == InputType.joy)
                hs_inputEvent(InputType.joy, key);
            else if (game_paused || game_over)
                pause_inputEvent(InputType.joy, key);
            else if (p1_in == InputType.joy)
                adjustSpeedUp(true, key);
            else if (TwoPlayer && p2_in == InputType.joy)
                adjustSpeedUp(false, key);
        }

        public void main_joyUp(Key key)
        {
            if (game_paused || game_over)
                return;
            else if (p1_in == InputType.joy)
                adjustSpeedDown(true, key);
            else if (TwoPlayer && p2_in == InputType.joy)
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
                    if (game_paused || game_over)
                        return;

                    gameClock++;
                    if (countdownOn)
                    {
                        countdown();
                        return;
                    }

                    //Move the players
                    moveShip(true);
                    if (TwoPlayer)
                        moveShip(false);


                    //Generate asteroids and coins
                    generateAsteroids();
                    generateCoins();
                    generateHealth();

                    //Update asteroid and coin positions
                    moveProjectiles(asteroids, this.Asteroid_Grid);
                    moveProjectiles(coins.OfType<Projectile>().ToList(), this.Coin_Grid);
                    moveProjectiles(healths.OfType<Projectile>().ToList(), this.Health_Grid);

                    //Check for collisions
                    detectCollision();

                    //Update Scores
                    p1_ship.score++;
                    p2_ship.score++;
                    this.Score1.Text = p1_ship.score.ToString();
                    this.Score2.Text = p2_ship.score.ToString();

                    //Check if game is over
                    if (p1_ship.shield <= 0 || p2_ship.shield <= 0)
                        gameOver();
                });
            }
        }

        private const int MIN_ASTEROID_WIDTH = 100;
        private const int MAX_ASTEROID_WIDTH = 250;
        private const int HEALTH_WIDTH = 50;
        private const int HEALTH_HEIGHT = 50;
        private void generateHealth()
        {
            Random rand = new Random();

            if (rand.Next(0, 1000) > 500)
            {
                Health newHealth = new Health();
                newHealth.image = new Image();

                // Randomly place the health somewhere along the edge
                Thickness loc = newHealth.image.Margin;
                loc.Left = Right_Margin + 200;
                newHealth.image.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newHealth.image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                loc.Top = Math.Max(Bottom_Margin - rand.Next(0, (int)Bottom_Margin), Top_Margin);
                newHealth.image.Margin = loc;

                //Size the image
                newHealth.image.Width = HEALTH_HEIGHT;
                newHealth.image.Height = HEALTH_HEIGHT;

                // Initialize coin's bitmap
                Uri uri = new Uri(@"../../Assets/health.png", UriKind.Relative);
                newHealth.bitmap = new TransformedBitmap();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = uri;
                bi.EndInit();
                newHealth.bitmap.BeginInit();
                newHealth.bitmap.Source = bi;
                RotateTransform transform = new RotateTransform(0);
                newHealth.bitmap.Transform = transform;
                newHealth.bitmap.EndInit();
                newHealth.image.Source = newHealth.bitmap;

                // Initialize coin's speed
                newHealth.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newHealth.speed.Y = 0;

                //Initialize coin's value
                newHealth.value = 25;

                //Add coin to grid
                healths.Add(newHealth);
                this.Coin_Grid.Children.Add(newHealth.image);
            }
            
        }
        private void generateAsteroids()
        {
            Random rand = new Random();
            // Modify the RHS below to change asteroid creation
            // frequency
            int threshold;
            switch (diff)
            {
                case Difficulty.easy:
                    threshold = 975;
                    break;
                case Difficulty.med:
                    threshold = 965;
                    break;
                case Difficulty.hard:
                    threshold = 945;
                    break;
                default:
                    threshold = 975;
                    break;
            }
            if (rand.Next(0, 1000) > threshold)
            {
                Projectile newAsteroid = new Projectile();
                newAsteroid.image = new Image();
                // newAsteroid.Source = this.LargeAsteroidSource.Source;
                Thickness loc = newAsteroid.image.Margin;
                loc.Left = Right_Margin + 200;
                newAsteroid.image.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newAsteroid.image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

                // Randomly place the asteroid somewhere along the edge,
                // sizes it, sets the image, and the rotation
                // basically makes it unique
                loc.Top = Math.Max(Bottom_Margin - rand.Next(0, (int)Bottom_Margin), Top_Margin);
                newAsteroid.image.Margin = loc;
                newAsteroid.image.Width = rand.Next(MIN_ASTEROID_WIDTH, MAX_ASTEROID_WIDTH);

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
                newAsteroid.bitmap = new TransformedBitmap();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = uri;
                bi.EndInit();
                newAsteroid.bitmap.BeginInit();
                newAsteroid.bitmap.Source = bi;
                RotateTransform transform = new RotateTransform(rand.Next(0, 3) * 90);
                newAsteroid.bitmap.Transform = transform;
                newAsteroid.bitmap.EndInit();
                newAsteroid.image.Source = newAsteroid.bitmap;

                this.Asteroid_Grid.Children.Add(newAsteroid.image);
                newAsteroid.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newAsteroid.speed.Y = 0;
                asteroids.Add(newAsteroid);
            }
        }

        private const int COIN_WIDTH = 50;
        private const int COIN_HEIGHT = 50;
        private void generateCoins()
        {
            Random rand = new Random();
            // Modify the RHS below to change coin creation
            // frequency
            if (rand.Next(0, 1000) > 980)
            {
                Coin newCoin = new Coin();
                newCoin.image = new Image();

                // Randomly place the coin somewhere along the edge
                Thickness loc = newCoin.image.Margin;
                loc.Left = Right_Margin + 200;
                newCoin.image.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                newCoin.image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                loc.Top = Math.Max(Bottom_Margin - rand.Next(0, (int)Bottom_Margin), Top_Margin);
                newCoin.image.Margin = loc;

                //Size the image
                newCoin.image.Width = COIN_WIDTH;
                newCoin.image.Height = COIN_HEIGHT;

                // Initialize coin's bitmap
                Uri uri = new Uri(@"../../Assets/gold_coin.png", UriKind.Relative);
                newCoin.bitmap = new TransformedBitmap();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = uri;
                bi.EndInit();
                newCoin.bitmap.BeginInit();
                newCoin.bitmap.Source = bi;
                RotateTransform transform = new RotateTransform(0);
                newCoin.bitmap.Transform = transform;
                newCoin.bitmap.EndInit();
                newCoin.image.Source = newCoin.bitmap;

                // Initialize coin's speed
                newCoin.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newCoin.speed.Y = 0;

                //Initialize coin's value
                newCoin.value = 100;

                //Add coin to grid
                coins.Add(newCoin);
                this.Coin_Grid.Children.Add(newCoin.image);
            }
        }

        private void moveProjectiles(List<Projectile> projectiles, Canvas grid)
        {
            //Update projectile positions
            for (int i = 0; i < projectiles.Count; i++)
            {
                Thickness loc = projectiles[i].image.Margin;
                loc.Left -= projectiles[i].speed.X;
                loc.Top += projectiles[i].speed.Y;

                // If it's off the screen delete it, otherwise update position
                if (loc.Left + projectiles[i].image.ActualWidth < 0 ||
                    loc.Top + projectiles[i].image.ActualHeight < 0)
                {
                    grid.Children.Remove(projectiles[i].image);
                    projectiles.RemoveAt(i);
                    i--;
                }
                else
                    projectiles[i].image.Margin = loc;
            }
        }

        private void detectCollision()
        {
            bool collision;
            //Check for collisions with asteroids
            for (int i = 0; i < asteroids.Count; ++i)
            {
                collision = false;
                if (checkCollision(p1_ship, asteroids[i]))
                {
                    calculateDamage(p1_ship, asteroids[i]);
                    this.Shield1.Text = "SHIELDS: " + p1_ship.shield.ToString() + "%";
                    collision = true;
                }

                if (TwoPlayer && checkCollision(p2_ship, asteroids[i]))
                {
                    calculateDamage(p2_ship, asteroids[i]);
                    this.Shield2.Text = "SHIELDS: " + p2_ship.shield.ToString() + "%";
                    collision = true;
                }

                if (collision)
                {
                    this.Asteroid_Grid.Children.Remove(asteroids[i].image);
                    asteroids.RemoveAt(i);
                    i--;
                }
            }

            //Check for collision with coins
            for (int i = 0; i < coins.Count; ++i)
            {
                collision = false;
                if (checkCollision(p1_ship, coins[i]))
                {
                    p1_ship.score += coins[i].value;
                    collision = true;
                }

                if (TwoPlayer && checkCollision(p2_ship, coins[i]))
                {
                    p2_ship.score += coins[i].value;
                    collision = true;
                }

                if (collision)
                {
                    this.Coin_Grid.Children.Remove(coins[i].image);
                    coins.RemoveAt(i);
                    i--;
                }
            }

            //Check for collision with healths
            for (int i = 0; i < healths.Count; ++i)
            {
                collision = false;
                if (checkCollision(p1_ship, healths[i]))
                {
                    p1_ship.score += healths[i].value;
                    collision = true;
                }

                if (TwoPlayer && checkCollision(p2_ship, healths[i]))
                {
                    p2_ship.score += healths[i].value;
                    collision = true;
                }

                if (collision)
                {
                    this.Health_Grid.Children.Remove(healths[i].image);
                    healths.RemoveAt(i);
                    i--;
                }
            }
        }

        private bool checkCollision(Projectile proj1, Projectile proj2)
        {
            //Perform a basic collision detection using image margins
            double top1, bot1, left1, right1,
                   top2, bot2, left2, right2,
                   colTop, colBot, colLeft, colRight, colHeight, colWidth;

            //proj1 Dimensions
            top1 = proj1.image.Margin.Top;
            bot1 = proj1.image.Margin.Top + proj1.image.ActualHeight;
            left1 = proj1.image.Margin.Left;
            right1 = proj1.image.Margin.Left + proj1.image.ActualWidth;

            //proj2 Dimensions
            top2 = proj2.image.Margin.Top;
            bot2 = proj2.image.Margin.Top + proj2.image.ActualHeight;
            left2 = proj2.image.Margin.Left;
            right2 = proj2.image.Margin.Left + proj2.image.ActualWidth;

            //Check for collision on y-axis
            if ((top1 > top2 && top1 <= bot2) || (top1 == top2))
                colTop = top1;
            else if (top1 < top2 && bot1 >= top2)
                colTop = top2;
            else
                return false;

            if (bot1 < bot2)
                colBot = bot1;
            else
                colBot = bot2;

            colHeight = colBot - colTop;

            //Check for collison on x-axis
            if ((left1 > left2 && left1 <= right2) || (left1 == left2))
                colLeft = left1;
            else if (left1 < left2 && right1 >= left2)
                colLeft = left2;
            else
                return false;

            if (right1 < right2)
                colRight = right1;
            else
                colRight = right2;

            colWidth = colRight - colLeft;

            //Perform more accurate collision detection using projectile bitmaps
            ImageSource ims1 = proj1.image.Source;
            BitmapImage bi1 = (BitmapImage)proj1.bitmap.Source;
            PixelColor[,] pixels1 = CopyPixels(bi1);

            ImageSource ims2 = proj2.image.Source;
            BitmapImage bi2 = (BitmapImage)proj2.bitmap.Source;
            PixelColor[,] pixels2 = CopyPixels(bi2);

            int x1, y1, x2, y2;
            double actToPix_1w, actToPix_1h, actToPix_2w, actToPix_2h;
            int w1_pix, h1_pix, w2_pix, h2_pix;

            h1_pix = bi1.PixelHeight - 1;
            w1_pix = bi1.PixelWidth - 1;
            h2_pix = bi2.PixelHeight - 1;
            w2_pix = bi2.PixelWidth - 1;

            actToPix_1h = h1_pix / (bot1 - top1);
            actToPix_1w = w1_pix / (right1 - left1);
            actToPix_2h = h2_pix / (bot2 - top2);
            actToPix_2w = w2_pix / (right2 - left2);

            for (double i = 0; i < colHeight; ++i)
            {
                y1 = (int)(actToPix_1h * ((colTop + i) - top1));
                y2 = (int)(actToPix_2h * ((colTop + i) - top2));
                for (double j = 0; j < colWidth; ++j)
                {
                    x1 = (int)(actToPix_1w * ((colLeft + j) - left1));
                    x2 = (int)(actToPix_2w * ((colLeft + j) - left2));
                    if (pixels1[x1, y1].Alpha > 0 && pixels2[x2, y2].Alpha > 0)
                        return true;
                }
            }

            return false;
        }

        private struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }

        private PixelColor[,] CopyPixels(BitmapSource source)
        {
            int height = source.PixelHeight;
            int width = source.PixelWidth;
            int stride = width * 4;

            PixelColor[,] pixels = new PixelColor[width, height];
            var pixelBytes = new byte[height * width * 4];
            source.CopyPixels(pixelBytes, stride, 0);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x, y] = new PixelColor
                    {
                        Blue = pixelBytes[(y * width + x) * 4 + 0],
                        Green = pixelBytes[(y * width + x) * 4 + 1],
                        Red = pixelBytes[(y * width + x) * 4 + 2],
                        Alpha = pixelBytes[(y * width + x) * 4 + 3],
                    };
                }
            }

            return pixels;
        }

        private void calculateDamage(Spaceship ship, Projectile asteroid)
        {
            double speed = ship.speed.X + asteroid.speed.X;
            double sizeRatio = (asteroid.image.ActualHeight * asteroid.image.ActualWidth) / (ship.image.ActualHeight * ship.image.ActualWidth);

            ship.shield = Math.Max(0, ship.shield - Math.Max(1, ((int)(speed * sizeRatio) / 4)));
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
                        pause_updateFont(opt.exitGame);
                    else
                        pause_updateFont(curOpt - 1);
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Down:
                case Key.S:
                    if (curOpt == opt.exitGame)
                        pause_updateFont(opt.resume);
                    else
                        pause_updateFont(curOpt + 1);
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Space:
                case Key.Enter:
                    pause_selectOpt();
                    break;
                case Key.Escape:
                    if (!game_over)
                    {
                        pause_updateFont(opt.resume);
                        App.menuDelay++;
                        pause_selectOpt();
                    }
                    break;
            }
        }

        private void pause_updateFont(opt nextOpt)
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

        private void pause_selectOpt()
        {
            switch (curOpt)
            {
                case opt.resume:
                    if (game_paused)
                    {
                        this.pause_header.Visibility = Visibility.Collapsed;
                        this.pause_leftShip.Visibility = Visibility.Collapsed;
                        this.pause_rightShip.Visibility = Visibility.Collapsed;
                        this.pause_resume.Visibility = Visibility.Collapsed;
                        this.pause_returnToStart.Visibility = Visibility.Collapsed;
                        this.pause_exitGame.Visibility = Visibility.Collapsed;
                        this.pause_background.Opacity = 0;
                        if (countdownOn)
                            this.count.Visibility = Visibility.Visible;

                        p1_ship.speed.X = 0;
                        p1_ship.speed.Y = 0;
                        p2_ship.speed.X = 0;
                        p2_ship.speed.Y = 0;
                        game_paused = false;
                    }
                    else
                    {
                        MainWindow main = new MainWindow(TwoPlayer, diff, p1_in, p2_in);
                        App.Current.MainWindow = main;
                        main.Show();
                        this.Close();
                    }
                    break;
                case opt.returnToStart:
                    StartWindow start = new StartWindow();
                    App.Current.MainWindow = start;
                    start.Show();
                    this.Close();
                    break;
                case opt.exitGame:
                    System.Environment.Exit(0);
                    break;
            }
        }

        private void gameOver()
        {
            game_over = true;

            // Check for high scores
            if (scoreboard.checkHighScore(p1_ship.score))
                enterHighScore(true);
            else if (TwoPlayer && scoreboard.checkHighScore(p2_ship.score))
                enterHighScore(false);
            else
            {
                //Show game over menu
                this.pause_header.Text = "GAME OVER";
                this.pause_resume.Text = "     PLAY AGAIN     ";
                this.pause_header.Visibility = Visibility.Visible;
                this.pause_leftShip.Visibility = Visibility.Visible;
                this.pause_rightShip.Visibility = Visibility.Visible;
                this.pause_resume.Visibility = Visibility.Visible;
                this.pause_returnToStart.Visibility = Visibility.Visible;
                this.pause_exitGame.Visibility = Visibility.Visible;
                this.pause_background.Opacity = 1;
            }
        }

        private InputType highScoreInput = InputType.none;
        private char[] hs_letters = new char[10];
        private int hs_curLetter = 0;
        private void enterHighScore(bool player1)
        {
            this.hs_header.Visibility = System.Windows.Visibility.Visible;
            this.hs_leftShip.Visibility = System.Windows.Visibility.Visible;
            this.hs_rightShip.Visibility = System.Windows.Visibility.Visible;
            this.hs_info.Visibility = System.Windows.Visibility.Visible;
            this.hs_name0.Visibility = System.Windows.Visibility.Visible;
            this.hs_name1.Visibility = System.Windows.Visibility.Visible;
            this.hs_name2.Visibility = System.Windows.Visibility.Visible;
            this.hs_name3.Visibility = System.Windows.Visibility.Visible;
            this.hs_name4.Visibility = System.Windows.Visibility.Visible;
            this.hs_name5.Visibility = System.Windows.Visibility.Visible;
            this.hs_name6.Visibility = System.Windows.Visibility.Visible;
            this.hs_name7.Visibility = System.Windows.Visibility.Visible;
            this.hs_name8.Visibility = System.Windows.Visibility.Visible;
            this.hs_name9.Visibility = System.Windows.Visibility.Visible;
            this.hs_border0.Visibility = System.Windows.Visibility.Visible;
            this.hs_border1.Visibility = System.Windows.Visibility.Visible;
            this.hs_border2.Visibility = System.Windows.Visibility.Visible;
            this.hs_border3.Visibility = System.Windows.Visibility.Visible;
            this.hs_border4.Visibility = System.Windows.Visibility.Visible;
            this.hs_border5.Visibility = System.Windows.Visibility.Visible;
            this.hs_border6.Visibility = System.Windows.Visibility.Visible;
            this.hs_border7.Visibility = System.Windows.Visibility.Visible;
            this.hs_border8.Visibility = System.Windows.Visibility.Visible;
            this.hs_border9.Visibility = System.Windows.Visibility.Visible;
            this.pause_background.Opacity = 1;

            if(player1)
            {
                highScoreInput = p1_in;
                this.hs_info.Text = "     PLAYER 1 PLEASE ENTER NAME     ";
            }
            else
            {
                highScoreInput = p2_in;
                this.hs_info.Text = "     PLAYER 2 PLEASE ENTER NAME     ";
            }

            hs_letters[0] = 'G';
            hs_letters[1] = 'R';
            hs_letters[2] = 'A';
            hs_letters[3] = 'C';
            hs_letters[4] = 'E';
            for (int i = 5; i < 10; ++i)
                hs_letters[i] = ' ';
            for(int i = 9; i >= 0; --i)
                hs_updateFont(i);
            hs_updateFont(0);
        }

        private void hs_inputEvent(InputType inType, Key key)
        {
            if (App.menuDelay != 0)
                return;

            switch (key)
            {
                case Key.Up:
                case Key.W:
                    if (hs_letters[hs_curLetter] == ' ')
                        hs_letters[hs_curLetter] = 'Z';
                    else if (hs_letters[hs_curLetter] == 'A')
                        hs_letters[hs_curLetter] = ' ';
                    else
                        hs_letters[hs_curLetter]--;
                    hs_updateFont(hs_curLetter);
                    break;
                case Key.Down:
                case Key.S:
                    if (hs_letters[hs_curLetter] == ' ')
                        hs_letters[hs_curLetter] = 'A';
                    else if (hs_letters[hs_curLetter] == 'Z')
                        hs_letters[hs_curLetter] = ' ';
                    else
                        hs_letters[hs_curLetter]++;
                    hs_updateFont(hs_curLetter);
                    break;
                case Key.Left:
                case Key.A:
                    if (hs_curLetter > 0)
                        hs_updateFont(hs_curLetter - 1);
                    break;
                case Key.Right:
                case Key.D:
                    if (hs_curLetter < 9)
                        hs_updateFont(hs_curLetter + 1);
                    break;
                case Key.Space:
                case Key.Enter:
                    long score;
                    if (highScoreInput == p1_in)
                    {
                        score = p1_ship.score;
                        p1_ship.score = 0;
                    }
                    else
                    {
                        score = p2_ship.score;
                        p2_ship.score = 0;
                    }

                    string highScoreName = "";
                    for (int i = 0; i < 10; ++i)
                        highScoreName += hs_letters[i];
                    scoreboard.addHighScore(highScoreName.Trim(), score);
                    highScoreInput = InputType.none;

                    this.hs_header.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_leftShip.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_rightShip.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_info.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name0.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name1.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name2.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name3.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name4.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name5.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name6.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name7.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name8.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_name9.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border0.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border1.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border2.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border3.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border4.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border5.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border6.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border7.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border8.Visibility = System.Windows.Visibility.Collapsed;
                    this.hs_border9.Visibility = System.Windows.Visibility.Collapsed;
                    this.pause_background.Opacity = 0;

                    highScoreInput = InputType.none;
                    gameOver();
                    break;
            }
        }

        private void hs_updateFont(int hs_nextLetter)
        {
            switch (hs_curLetter)
            {
                case 0:
                    this.hs_name0.Text = hs_letters[0].ToString();
                    this.hs_name0.Foreground = Brushes.White;
                    this.hs_border0.BorderBrush = Brushes.White;
                    break;
                case 1:
                    this.hs_name1.Text = hs_letters[1].ToString();
                    this.hs_name1.Foreground = Brushes.White;
                    this.hs_border1.BorderBrush = Brushes.White;
                    break;
                case 2:
                    this.hs_name2.Text = hs_letters[2].ToString();
                    this.hs_name2.Foreground = Brushes.White;
                    this.hs_border2.BorderBrush = Brushes.White;
                    break;
                case 3:
                    this.hs_name3.Text = hs_letters[3].ToString();
                    this.hs_name3.Foreground = Brushes.White;
                    this.hs_border3.BorderBrush = Brushes.White;
                    break;
                case 4:
                    this.hs_name4.Text = hs_letters[4].ToString();
                    this.hs_name4.Foreground = Brushes.White;
                    this.hs_border4.BorderBrush = Brushes.White;
                    break;
                case 5:
                    this.hs_name5.Text = hs_letters[5].ToString();
                    this.hs_name5.Foreground = Brushes.White;
                    this.hs_border5.BorderBrush = Brushes.White;
                    break;
                case 6:
                    this.hs_name6.Text = hs_letters[6].ToString();
                    this.hs_name6.Foreground = Brushes.White;
                    this.hs_border6.BorderBrush = Brushes.White;
                    break;
                case 7:
                    this.hs_name7.Text = hs_letters[7].ToString();
                    this.hs_name7.Foreground = Brushes.White;
                    this.hs_border7.BorderBrush = Brushes.White;
                    break;
                case 8:
                    this.hs_name8.Text = hs_letters[8].ToString();
                    this.hs_name8.Foreground = Brushes.White;
                    this.hs_border8.BorderBrush = Brushes.White;
                    break;
                case 9:
                    this.hs_name9.Text = hs_letters[9].ToString();
                    this.hs_name9.Foreground = Brushes.White;
                    this.hs_border9.BorderBrush = Brushes.White;
                    break;
            }

            switch (hs_nextLetter)
            {
                case 0:
                    this.hs_name0.Foreground = Brushes.Yellow;
                    this.hs_border0.BorderBrush = Brushes.Yellow;
                    break;
                case 1:
                    this.hs_name1.Foreground = Brushes.Yellow;
                    this.hs_border1.BorderBrush = Brushes.Yellow;
                    break;
                case 2:
                    this.hs_name2.Foreground = Brushes.Yellow;
                    this.hs_border2.BorderBrush = Brushes.Yellow;
                    break;
                case 3:
                    this.hs_name3.Foreground = Brushes.Yellow;
                    this.hs_border3.BorderBrush = Brushes.Yellow;
                    break;
                case 4:
                    this.hs_name4.Foreground = Brushes.Yellow;
                    this.hs_border4.BorderBrush = Brushes.Yellow;
                    break;
                case 5:
                    this.hs_name5.Foreground = Brushes.Yellow;
                    this.hs_border5.BorderBrush = Brushes.Yellow;
                    break;
                case 6:
                    this.hs_name6.Foreground = Brushes.Yellow;
                    this.hs_border6.BorderBrush = Brushes.Yellow;
                    break;
                case 7:
                    this.hs_name7.Foreground = Brushes.Yellow;
                    this.hs_border7.BorderBrush = Brushes.Yellow;
                    break;
                case 8:
                    this.hs_name8.Foreground = Brushes.Yellow;
                    this.hs_border8.BorderBrush = Brushes.Yellow;
                    break;
                case 9:
                    this.hs_name9.Foreground = Brushes.Yellow;
                    this.hs_border9.BorderBrush = Brushes.Yellow;
                    break;
            }

            hs_curLetter = hs_nextLetter;
        }

        private class Scoreboard
        {
            public string[] names;
            public long[] scores;
            private int highScoreIndex;
            private string path;

            public Scoreboard()
            {
                names = new string[10];
                scores = new long[10];
                highScoreIndex = -1;
                path = String.Format("{0}..\\..\\Assets\\scores.txt", System.AppDomain.CurrentDomain.BaseDirectory);

                readInScoreboard();
            }

            private void readInScoreboard()
            {
                for (int i = 0; i < 10; ++i)
                {
                    names[i] = "";
                    scores[i] = 0;
                }

                if (!System.IO.File.Exists(path))
                {
                    string defaultText = "";
                    System.IO.File.WriteAllText(path, defaultText);
                }
                else
                {
                    string readText = System.IO.File.ReadAllText(path);

                    string delimStr = "\r\n\t";
                    char[] delimiter = delimStr.ToCharArray();
                    int maxSubstr = 20;
                    string[] split = readText.Split(delimiter, maxSubstr);

                    for (int i = 0; i < split.Length / 2; ++i)
                    {
                        names[i] = Regex.Replace(split[2 * i], @"\t|\n|\r", "");
                        scores[i] = Convert.ToInt64(Regex.Replace(split[(2 * i) + 1], @"\t|\n|\r", ""));
                    }
                }
            }

            public bool checkHighScore(long score)
            {
                highScoreIndex = -1;
                for(int i = 9; i >= 0; --i)
                {
                    if (score > scores[i])
                        highScoreIndex = i;
                    else
                        break; ;
                }

                if (highScoreIndex > -1)
                    return true;
                else
                    return false;
            }

            public void addHighScore(string name, long score)
            {
                System.Diagnostics.Debug.Assert(highScoreIndex > -1);
                for(int i = 9; i > highScoreIndex; --i)
                {
                    names[i] = names[i - 1];
                    scores[i] = scores[i - 1];
                }

                names[highScoreIndex] = name;
                scores[highScoreIndex] = score;

                string writeText = "";
                for(int i = 0; i < 10; ++i)
                {
                    if (scores[i] > 0)
                        writeText += names[i] + "\t" + scores[i] + "\n";
                    else
                        break;
                }

                System.IO.File.WriteAllText(path, writeText);
            }
        }
    }
}
