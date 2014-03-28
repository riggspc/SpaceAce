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
        // Game classes
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
            public int shielded = 0;
            public int boosted = 0;
            public int speed_multiplier = 1;
            private bool invulnerable = false;
            private int timeLeftInvulnerable = MAX_TIME_INVULNERABLE;

            public void becomeInvulnerable()
            {
                timeLeftInvulnerable = MAX_TIME_INVULNERABLE;
                invulnerable = true;
            }

            public bool isInvulnerable()
            {
                return invulnerable;
            }

            public void checkInvulnerability()
            {
                if (invulnerable) 
                {
                    timeLeftInvulnerable--;

                    if (timeLeftInvulnerable <= 0)
                    {
                        invulnerable = false;
                        this.image.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        // the mod 3 is to make the blinking more obvious 
                        if ((this.image.Visibility == System.Windows.Visibility.Visible) && (timeLeftInvulnerable % 3 == 0))
                        {
                            this.image.Visibility = System.Windows.Visibility.Hidden;
                        }
                        else if(timeLeftInvulnerable % 3 == 0)
                        {
                            this.image.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                }
            }

            public bool isAlive()
            {
                if (shield > 0)
                    return true;
                else
                    return false;
            }

            public void update_Speed()
            {
                if (boosted > 0)
                    boosted--;
                if (boosted == 1)
                {
                    speed_multiplier = 1;
                    boosted = 0;
                }
            }

        }

        private class Coin : Projectile
        {
            public int value;
        }

        private class Health : Projectile
        {
            public int value;
        }

        private class Shield : Projectile
        {
            
        }

        private class Speed_Boost : Projectile
        {

        }

        private class Bomb : Projectile
        {

        }
        /***** Constants *****/
        // Speeds
        private const int SHIP_SPEED = 5;
        private const int BASE_MAX_PROJECTILE_SPEED = 6;
        private const int BASE_MIN_PROJECTILE_SPEED = 4;
        // Thresholds for creation
        private const int ASTEROID_THRESHOLD_EASY = 975;
        private const int ASTEROID_THRESHOLD_MEDIUM = 965;
        private const int ASTEROID_THRESHOLD_HARD = 945;
        private const int COIN_THRESHOLD = 980;
        private const int HEALTH_THRESHOLD = 997;
        private const int POWERUP_THRESHOLD = 998;
        // Sizes of projectiles
        private const int MIN_ASTEROID_WIDTH = 100;
        private const int MAX_ASTEROID_WIDTH = 250;
        private const int HEALTH_WIDTH = 80;
        private const int HEALTH_HEIGHT = 80;
        private const int COIN_WIDTH = 50;
        private const int COIN_HEIGHT = 50;
        private const int SHIELD_WIDTH = 75;
        private const int SHIELD_HEIGHT = 75;
        private const int SPEED_HEIGHT = 75;
        private const int SPEED_WIDTH = 75;
        private const int BOMB_HEIGHT = 75;
        private const int BOMB_WIDTH = 75;
        // Other
        private const int MAX_TIME_INVULNERABLE = 100;
        private const int MIN_COLLISION_DAMAGE = 5;

        // Margins may need to change depending on screen size and resolution
        private double Left_Margin = 0;
        private double Right_Margin = 924;
        private double Top_Margin = 75;
        private double Bottom_Margin = 650;

        // Ships, coins, asteroids, and health
        private Spaceship p1_ship = new Spaceship();
        private Spaceship p2_ship = new Spaceship();
        private List<Projectile> asteroids = new List<Projectile>();
        private List<Coin> coins = new List<Coin>();
        private List<Health> healths = new List<Health>();
        private List<Shield> shields = new List<Shield>();
        private List<Speed_Boost> speedUps = new List<Speed_Boost>();
        private List<Bomb> bombs = new List<Bomb>();

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

        // Source of all randomness, to prevent multiple Random objects
        // being seeded with the same number
        private Random rand = new Random();
        public MainWindow(bool num_players, Difficulty _diff, InputType P1, InputType P2)
        {
            this.Cursor = Cursors.None;
            InitializeComponent();
            App.timer.Elapsed += main_timerElapsed;

            
            scoreboard.nameChars.Add(hs_name0);
            scoreboard.nameChars.Add(hs_name1);
            scoreboard.nameChars.Add(hs_name2);
            scoreboard.nameChars.Add(hs_name3);
            scoreboard.nameChars.Add(hs_name4);
            scoreboard.nameChars.Add(hs_name5);
            scoreboard.nameChars.Add(hs_name6);
            scoreboard.nameChars.Add(hs_name7);
            scoreboard.nameChars.Add(hs_name8);
            scoreboard.nameChars.Add(hs_name9);

            scoreboard.borders.Add(hs_border0);
            scoreboard.borders.Add(hs_border1);
            scoreboard.borders.Add(hs_border2);
            scoreboard.borders.Add(hs_border3);
            scoreboard.borders.Add(hs_border4);
            scoreboard.borders.Add(hs_border5);
            scoreboard.borders.Add(hs_border6);
            scoreboard.borders.Add(hs_border7);
            scoreboard.borders.Add(hs_border8);
            scoreboard.borders.Add(hs_border9);
            

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
            App.joyDown += new EventHandler<JoyDownArgs>(main_joyDown);
            App.joyUp += new EventHandler<JoyUpArgs>(main_joyUp);

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
            initializePlayer(1, p1_ship);

            if (TwoPlayer)
            {
                initializePlayer(2, p2_ship);
            }


            this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void initializePlayer(int playerNum, Spaceship player)
        {
            // p2_ship.image = this.Player2;
            player.speed.X = 0;
            player.speed.Y = 0;
            player.bitmap = new TransformedBitmap();
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.UriSource = new Uri(@"../../Assets/player" + playerNum.ToString() + @"_small_with_fire.png", UriKind.Relative);
            bmpImage.EndInit();
            player.bitmap.BeginInit();
            player.bitmap.Source = bmpImage;
            player.bitmap.Transform = new ScaleTransform(1, 1);
            player.bitmap.EndInit();

            switch (playerNum)
            {
                case 1:
                    player.image = this.Player1;
                    this.Player1.Source = player.bitmap;
                    break;
                case 2:
                    player.image = this.Player2;
                    this.Player2.Source = player.bitmap;
                    break;
                default:
                    player.image = this.Player1;
                    this.Player1.Source = player.bitmap;
                    break;
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
            double speedX, speedY;
            if (player1)
            {
                playerLoc = this.Player1.Margin;
                ship_speed = p1_ship.speed;
                speedX = ship_speed.X * p1_ship.speed_multiplier;
                speedY = ship_speed.Y * p1_ship.speed_multiplier;
            }
            else
            {
                playerLoc = this.Player2.Margin;
                ship_speed = p2_ship.speed;
                speedX = ship_speed.X * p2_ship.speed_multiplier;
                speedY = ship_speed.Y * p2_ship.speed_multiplier;
            }

            if ((playerLoc.Left + speedX > Left_Margin) && (playerLoc.Left + speedX < Right_Margin))
                playerLoc.Left += speedX;
            if ((playerLoc.Top + speedY > Top_Margin) && (playerLoc.Top + speedY < Bottom_Margin))
                playerLoc.Top += speedY;

            if (player1)
                this.Player1.Margin = playerLoc;
            else
                this.Player2.Margin = playerLoc;
        }

        private void adjustSpeed(bool player1, Key key, bool increaseSpeed)
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
                    if(increaseSpeed)
                        ship_speed.X = Math.Max(-SHIP_SPEED, ship_speed.X - SHIP_SPEED);
                    else
                        ship_speed.X = Math.Min(SHIP_SPEED, ship_speed.X + SHIP_SPEED);
                    break;
                case Key.D:
                case Key.Right:
                    if (increaseSpeed)
                        ship_speed.X = Math.Min(SHIP_SPEED, ship_speed.X + SHIP_SPEED);
                    else
                        ship_speed.X = Math.Max(-SHIP_SPEED, ship_speed.X - SHIP_SPEED);
                    break;
                case Key.W:
                case Key.Up:
                    if (increaseSpeed)
                        ship_speed.Y = Math.Max(-SHIP_SPEED, ship_speed.Y - SHIP_SPEED);
                    else
                        ship_speed.Y = Math.Min(SHIP_SPEED, ship_speed.Y + SHIP_SPEED);
                    break;
                case Key.S:
                case Key.Down:
                    if (increaseSpeed)
                        ship_speed.Y = Math.Min(SHIP_SPEED, ship_speed.Y + SHIP_SPEED);
                    else
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
            else if(highScoreInput == InputType.joy)
            {
                if (e.Key == Key.Space || e.Key == Key.Enter)
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
                        adjustSpeed(true, e.Key, true);
                    else if (TwoPlayer && p2_in == InputType.arrows)
                        adjustSpeed(false, e.Key, true);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (p1_in == InputType.wasd)
                        adjustSpeed(true, e.Key, true);
                    else if (TwoPlayer && p2_in == InputType.wasd)
                        adjustSpeed(false, e.Key, true);
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
                        adjustSpeed(true, e.Key, false);
                    else if (TwoPlayer && p2_in == InputType.arrows)
                        adjustSpeed(false, e.Key, false);
                    break;
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    if (p1_in == InputType.wasd)
                        adjustSpeed(true, e.Key, false);
                    else if (TwoPlayer && p2_in == InputType.wasd)
                        adjustSpeed(false, e.Key, false);
                    break;
            }
        }

        public void main_joyDown(object sender, JoyDownArgs e)
        {
            if (highScoreInput == InputType.joy)
                hs_inputEvent(InputType.joy, e.Key);
            else if ((game_paused || game_over) && highScoreInput == InputType.none)
                pause_inputEvent(InputType.joy, e.Key);
            else if (p1_in == InputType.joy)
                adjustSpeed(true, e.Key, true);
            else if (TwoPlayer && p2_in == InputType.joy)
                adjustSpeed(false, e.Key, true);
        }

        public void main_joyUp(object sender, JoyUpArgs e)
        {
            if (game_paused || game_over)
                return;
            else if (p1_in == InputType.joy)
                adjustSpeed(true, e.Key, false);
            else if (TwoPlayer && p2_in == InputType.joy)
                adjustSpeed(false, e.Key, false);
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
                    if(p1_ship.isAlive())
                        moveShip(true);
                    if (TwoPlayer && p2_ship.isAlive())
                        moveShip(false);

                    // Blink players if invulnerable
                    if(p1_ship.isAlive())
                        p1_ship.checkInvulnerability();
                    if (TwoPlayer && p2_ship.isAlive())
                        p2_ship.checkInvulnerability();

                    //update shield status
                    if (p1_ship.shielded > 0)
                        p1_ship.shielded--;
                    if (TwoPlayer && p2_ship.isAlive() && p2_ship.shielded > 0)
                        p2_ship.shielded--;

                    if (p1_ship.isAlive() && p1_ship.shielded == 1)
                    {
                        updateShip(1, p1_ship, false);
                        p1_ship.shielded = 0;
                    }

                    if(TwoPlayer && p2_ship.isAlive() && p2_ship.shielded == 1)
                    {
                        updateShip(2, p2_ship, false);
                        p2_ship.shielded = 0;
                    }

                    //update speed boost status
                    if(p1_ship.isAlive())
                        p1_ship.update_Speed();
                    if (TwoPlayer && p2_ship.isAlive())
                        p2_ship.update_Speed();

                    //Generate asteroids and coins
                    generateProjectiles();

                    //Update asteroid and coin positions
                    moveProjectiles(asteroids, this.Asteroid_Grid);
                    moveProjectiles(coins.OfType<Projectile>().ToList(), this.Coin_Grid);
                    moveProjectiles(healths.OfType<Projectile>().ToList(), this.Health_Grid);
                    moveProjectiles(shields.OfType<Projectile>().ToList(), this.PowerUp_Grid);
                    moveProjectiles(speedUps.OfType<Projectile>().ToList(), this.PowerUp_Grid);
                    moveProjectiles(bombs.OfType<Projectile>().ToList(), this.PowerUp_Grid);

                    //Check for collisions
                    detectCollision();

                    //Update Scores
                    if (p1_ship.isAlive())
                    {
                        p1_ship.score++;
                        this.Score1.Text = p1_ship.score.ToString();
                    }
                    if(TwoPlayer && p2_ship.isAlive())
                    {
                        p2_ship.score++;
                        this.Score2.Text = p2_ship.score.ToString();
                    }

                    //Check if players are still alive
                    if (!p1_ship.isAlive())
                        Player1.Visibility = System.Windows.Visibility.Collapsed;
                    if (TwoPlayer && !p2_ship.isAlive())
                        Player2.Visibility = System.Windows.Visibility.Collapsed;
                    if (!p1_ship.isAlive() && (!TwoPlayer || (TwoPlayer && !p2_ship.isAlive())))
                        gameOver();
                });
            }
        }

        private Thickness getRandomEdge()
        {
            Thickness loc = new Thickness();
            loc.Left = Right_Margin + 200;
            loc.Top = Math.Max(Bottom_Margin - rand.Next(0, (int)Bottom_Margin), Top_Margin);
            return loc;
        }

        private Image initializeProjectileImage(Uri spriteUri, bool rotate = false)
        {
            Image newImage = new Image();

            newImage.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            newImage.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            newImage.Margin = getRandomEdge();

            TransformedBitmap temp = new TransformedBitmap();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = spriteUri;
            bi.EndInit();
            temp.BeginInit();
            temp.Source = bi;
            RotateTransform transform;
            if (rotate)
            {
                transform = new RotateTransform(rand.Next(0, 3) * 90);
            }
            else
            {
                transform = new RotateTransform(0);
            }
            temp.Transform = transform;
            temp.EndInit();
            newImage.Source = temp;


            return newImage;
        }
        private void generateProjectiles()
        {
            // Place any threshold/generation changes that need to occur
            // based on difficulty in this section
            int asteroid_threshold;
            switch (diff)
            {
                case Difficulty.easy:
                    asteroid_threshold = ASTEROID_THRESHOLD_EASY;
                    break;
                case Difficulty.med:
                    asteroid_threshold = ASTEROID_THRESHOLD_MEDIUM;
                    break;
                case Difficulty.hard:
                    asteroid_threshold = ASTEROID_THRESHOLD_HARD;
                    break;
                default:
                    asteroid_threshold = ASTEROID_THRESHOLD_EASY;
                    break;
            }

            if (rand.Next(0, 1000) > HEALTH_THRESHOLD)
            {
                Health newHealth = new Health();
                newHealth.image = initializeProjectileImage(new Uri(@"../../Assets/health.png", UriKind.Relative));

                //Size the image
                newHealth.image.Width = HEALTH_HEIGHT;
                newHealth.image.Height = HEALTH_HEIGHT;

                // Initialize health's speed
                newHealth.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newHealth.speed.Y = 0;

                //Initialize health's value
                newHealth.value = 10;

                //Add health to grid
                newHealth.bitmap = (TransformedBitmap)newHealth.image.Source;
                healths.Add(newHealth);
                this.Health_Grid.Children.Add(newHealth.image);
            }

            if (rand.Next(0, 1000) > asteroid_threshold)
            {
                Projectile newAsteroid = new Projectile();

                // Determines which image the new asteroid will be
                int asteroidType = rand.Next(0, 3);
                switch (asteroidType)
                {
                    case 0:
                        newAsteroid.image = initializeProjectileImage(new Uri(@"../../Assets/asteroid_large.png", UriKind.Relative), true);
                        break;
                    case 1:
                        newAsteroid.image = initializeProjectileImage(new Uri(@"../../Assets/asteroid_small.png", UriKind.Relative), true);
                        break;
                    case 2:
                        newAsteroid.image = initializeProjectileImage(new Uri(@"../../Assets/asteroid_medium.png", UriKind.Relative), true);
                        break;
                    default:
                        newAsteroid.image = initializeProjectileImage(new Uri(@"../../Assets/asteroid_large.png", UriKind.Relative), true);
                        break;

                }

                newAsteroid.bitmap = (TransformedBitmap)newAsteroid.image.Source;
                newAsteroid.image.Width = rand.Next(MIN_ASTEROID_WIDTH, MAX_ASTEROID_WIDTH);
                this.Asteroid_Grid.Children.Add(newAsteroid.image);
                newAsteroid.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newAsteroid.speed.Y = 0;
                asteroids.Add(newAsteroid);
            }

            if (rand.Next(0, 1000) > COIN_THRESHOLD)
            {
                Coin newCoin = new Coin();
                // newCoin.image = new Image();
                newCoin.image = initializeProjectileImage(new Uri(@"../../Assets/gold_coin.png", UriKind.Relative));

                //Size the image
                newCoin.image.Width = COIN_WIDTH;
                newCoin.image.Height = COIN_HEIGHT;

                // Initialize coin's speed
                newCoin.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newCoin.speed.Y = 0;

                //Initialize coin's value
                newCoin.value = 100;
                newCoin.bitmap = (TransformedBitmap)newCoin.image.Source;

                //Add coin to grid
                coins.Add(newCoin);
                this.Coin_Grid.Children.Add(newCoin.image);
            }

            //create shields
            if (rand.Next(0, 1000) > POWERUP_THRESHOLD)
            {
                Shield newShield = new Shield();
                // newShiled.image = new Image();
                newShield.image = initializeProjectileImage(new Uri(@"../../Assets/shield.png", UriKind.Relative));

                //Size the image
                newShield.image.Width = SHIELD_WIDTH;
                newShield.image.Height = SHIELD_HEIGHT;

                // Initialize shield's speed
                newShield.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newShield.speed.Y = 0;

                newShield.bitmap = (TransformedBitmap)newShield.image.Source;

                //Add shield to grid
                shields.Add(newShield);
                this.PowerUp_Grid.Children.Add(newShield.image);
            }

            //create speed boosts
            if (rand.Next(0, 1000) > POWERUP_THRESHOLD)
            {
                Speed_Boost newSpeedUp = new Speed_Boost();
                // newSpeed.image = new Image();
                newSpeedUp.image = initializeProjectileImage(new Uri(@"../../Assets/speed_boost.png", UriKind.Relative));

                //Size the image
                newSpeedUp.image.Width = SPEED_WIDTH;
                newSpeedUp.image.Height = SPEED_HEIGHT;

                // Initialize speed's speed
                newSpeedUp.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newSpeedUp.speed.Y = 0;

                newSpeedUp.bitmap = (TransformedBitmap)newSpeedUp.image.Source;

                //Add speed to grid
                speedUps.Add(newSpeedUp);
                this.PowerUp_Grid.Children.Add(newSpeedUp.image);
            }

            //create bombs
            if (rand.Next(0, 1000) > POWERUP_THRESHOLD)
            {
                Bomb newBomb = new Bomb();
                // newBomb.image = new Image();
                newBomb.image = initializeProjectileImage(new Uri(@"../../Assets/bomb.png", UriKind.Relative));

                //Size the image
                newBomb.image.Width = BOMB_WIDTH;
                newBomb.image.Height = BOMB_HEIGHT;

                // Initialize bomb's speed
                newBomb.speed.X = rand.Next(BASE_MIN_PROJECTILE_SPEED, BASE_MAX_PROJECTILE_SPEED) * difficulty_multiplier;
                newBomb.speed.Y = 0;

                newBomb.bitmap = (TransformedBitmap)newBomb.image.Source;

                //Add bomb to grid
                bombs.Add(newBomb);
                this.PowerUp_Grid.Children.Add(newBomb.image);
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
                if (checkCollision(p1_ship, asteroids[i]) && ! p1_ship.isInvulnerable() && p1_ship.shielded == 0)
                {
                    calculateDamage(p1_ship, asteroids[i]);
                    this.Shield1.Text = "SHIELDS: " + p1_ship.shield.ToString() + "%";
                    p1_ship.becomeInvulnerable();
                    
                    collision = true;
                }

                if (TwoPlayer && p2_ship.isAlive() && checkCollision(p2_ship, asteroids[i]) && ! p2_ship.isInvulnerable() && p2_ship.shielded == 0)
                {
                    calculateDamage(p2_ship, asteroids[i]);
                    this.Shield2.Text = "SHIELDS: " + p2_ship.shield.ToString() + "%";
                    p2_ship.becomeInvulnerable();

                    collision = true;
                }
                /*
                if (collision)
                {
                    this.Asteroid_Grid.Children.Remove(asteroids[i].image);
                    asteroids.RemoveAt(i);
                    i--;
                }
                */ 
            }

            //Check for collision with coins
            for (int i = 0; i < coins.Count; ++i)
            {
                collision = false;
                if (p1_ship.isAlive() && checkCollision(p1_ship, coins[i]))
                {
                    p1_ship.score += coins[i].value;
                    collision = true;
                }

                if (TwoPlayer && p2_ship.isAlive() && checkCollision(p2_ship, coins[i]))
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
                if (p1_ship.isAlive() && checkCollision(p1_ship, healths[i]))
                {
                    p1_ship.shield = Math.Min(100, p1_ship.shield + healths[i].value);
                    this.Shield1.Text = "SHIELDS: " + p1_ship.shield.ToString() + "%";
                    collision = true;
                }

                if (TwoPlayer && p2_ship.isAlive() && checkCollision(p2_ship, healths[i]))
                {
                    p2_ship.shield = Math.Min(100, p2_ship.shield + healths[i].value);
                    this.Shield2.Text = "SHIELDS: " + p2_ship.shield.ToString() + "%";
                    collision = true;
                }

                if (collision)
                {
                    this.Health_Grid.Children.Remove(healths[i].image);
                    healths.RemoveAt(i);
                    i--;
                }
            }

            //Check for collision with shields
            for (int i = 0; i < shields.Count; ++i)
            {
                collision = false;
                if (p1_ship.isAlive() && checkCollision(p1_ship, shields[i]))
                {
                    p1_ship.shielded = 500;
                    updateShip(1, p1_ship, true);
                    collision = true;
                }

                if (TwoPlayer && p2_ship.isAlive() && checkCollision(p2_ship, shields[i]))
                {
                    p2_ship.shielded = 500;
                    updateShip(2, p2_ship, true);
                    collision = true;
                }

                if (collision)
                {
                    this.PowerUp_Grid.Children.Remove(shields[i].image);
                    shields.RemoveAt(i);
                    i--;
                }
            }

            //Check for collision with speed boosts
            for (int i = 0; i < speedUps.Count; ++i)
            {
                collision = false;
                if (p1_ship.isAlive() && checkCollision(p1_ship, speedUps[i]))
                {
                    p1_ship.boosted = 500;
                    p1_ship.speed_multiplier = 2;
                    collision = true;
                }

                if (TwoPlayer && p2_ship.isAlive() && checkCollision(p2_ship, speedUps[i]))
                {
                    p2_ship.boosted = 500;
                    p2_ship.speed_multiplier = 2;
                    collision = true;
                }

                if (collision)
                {
                    this.PowerUp_Grid.Children.Remove(speedUps[i].image);
                    speedUps.RemoveAt(i);
                    i--;
                }
            }

             //Check for collision with bombs
            for (int i = 0; i < bombs.Count; ++i)
            {
                collision = false;
                if (p1_ship.isAlive() && checkCollision(p1_ship, bombs[i]))
                {
                    collision = true;
                }

                if (TwoPlayer && p2_ship.isAlive() && checkCollision(p2_ship, bombs[i]))
                {
                    collision = true;
                }

                if (collision)
                {
                    this.PowerUp_Grid.Children.Remove(bombs[i].image);
                    bombs.RemoveAt(i);
                    for(int j = 0; j < asteroids.Count; ++j){
                        this.Asteroid_Grid.Children.Remove(asteroids[j].image);
                        asteroids.RemoveAt(j);
                        j--;
                    }
                    i--;
                }
            }
        }

        private void updateShip(int playerNum, Spaceship ship, bool shielded)
        {
            ship.bitmap = new TransformedBitmap();
            BitmapImage bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            if(shielded)
                bmpImage.UriSource = new Uri(@"../../Assets/player" + playerNum.ToString() + @"_small_with_shield.png", UriKind.Relative);
            else
                bmpImage.UriSource = new Uri(@"../../Assets/player" + playerNum.ToString() + @"_small_with_fire.png", UriKind.Relative);
                
            bmpImage.EndInit();
            ship.bitmap.BeginInit();
            ship.bitmap.Source = bmpImage;
            ship.bitmap.Transform = new ScaleTransform(1, 1);
            ship.bitmap.EndInit();
            if (shielded)
            {
                ship.image.Width = 150;
                ship.image.Height = 150;
            }
            else
            {
                ship.image.Width = 100;
                ship.image.Height = 100;
            }
            if(playerNum == 1){
                ship.image = this.Player1;
                this.Player1.Source = ship.bitmap;
            }
            if(playerNum == 2){
                ship.image = this.Player2;
                this.Player2.Source = ship.bitmap;
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
            double speed = ship.speed.X * ship.speed_multiplier + asteroid.speed.X;
            double sizeRatio = (asteroid.image.ActualHeight * asteroid.image.ActualWidth) / (ship.image.ActualHeight * ship.image.ActualWidth);

            if (ship.shielded == 0)
            {
                ship.shield = Math.Max(0, ship.shield - Math.Max(MIN_COLLISION_DAMAGE, ((int)(speed * sizeRatio) / 4)));
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
                    App.menuDelay++;
                    App.lastInputType = inType;
                    pause_selectOpt();
                    break;
                case Key.Escape:
                    if (!game_over)
                    {
                        pause_updateFont(opt.resume);
                        App.menuDelay++;
                        App.lastInputType = inType;
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
            foreach (TextBlock nameChar in scoreboard.nameChars)
                nameChar.Visibility = System.Windows.Visibility.Visible;
            foreach (Border border in scoreboard.borders)
                border.Visibility = System.Windows.Visibility.Visible;
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
                    App.menuDelay++;
                    App.lastInputType = inType;
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
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Left:
                case Key.A:
                    if (hs_curLetter > 0)
                        hs_updateFont(hs_curLetter - 1);
                    App.menuDelay++;
                    App.lastInputType = inType;
                    break;
                case Key.Right:
                case Key.D:
                    if (hs_curLetter < 9)
                        hs_updateFont(hs_curLetter + 1);
                    App.menuDelay++;
                    App.lastInputType = inType;
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
                    foreach (TextBlock nameChar in scoreboard.nameChars)
                        nameChar.Visibility = System.Windows.Visibility.Collapsed;
                    foreach (Border border in scoreboard.borders)
                        border.Visibility = System.Windows.Visibility.Collapsed;

                    this.pause_background.Opacity = 0;

                    highScoreInput = InputType.none;
                    App.menuDelay++;
                    App.lastInputType = inType;
                    gameOver();
                    break;
            }
        }

        private void hs_updateFont(int hs_nextLetter)
        {
            scoreboard.nameChars[hs_curLetter].Text = hs_letters[hs_curLetter].ToString();
            scoreboard.nameChars[hs_curLetter].Foreground = Brushes.White;
            scoreboard.borders[hs_curLetter].BorderBrush = Brushes.White;
            
            scoreboard.nameChars[hs_nextLetter].Foreground = Brushes.Yellow;
            scoreboard.borders[hs_nextLetter].BorderBrush = Brushes.Yellow;

            hs_curLetter = hs_nextLetter;
        }

        private class Scoreboard
        {
            public string[] names;
            public long[] scores;
            private int highScoreIndex;
            private string path;
            public List<TextBlock> nameChars;
            public List<Border> borders;

            public Scoreboard()
            {
                names = new string[10];
                scores = new long[10];
                highScoreIndex = -1;
                path = String.Format("{0}..\\..\\Assets\\scores.txt", System.AppDomain.CurrentDomain.BaseDirectory);

                readInScoreboard();
                nameChars = new List<TextBlock>();
                borders = new List<Border>();
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
