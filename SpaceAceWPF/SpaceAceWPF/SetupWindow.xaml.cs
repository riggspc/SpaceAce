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
using System.Timers;

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        enum opt { diff, p1, p2, startGame, returnToStart };
        opt curOpt = opt.startGame;

        Difficulty diff = Difficulty.easy;
        private string[] diffString = { "     EASY     ", "    MEDIUM    ", "     HARD     " };

        enum inputOpt { joy, wasd, arrows };
        inputOpt p1_in = inputOpt.wasd;
        inputOpt p2_in = inputOpt.arrows;
        private string[] inputOptString = { "   JOYSTICK   ", "     WASD     ", "ARROW KEYS" };

        private bool validConfig = true;
        private bool TwoPlayer = false;
        public SetupWindow(bool num_players)
        {
            InitializeComponent();
            App.timer.Elapsed += simulateMenuDelay;
            App.inputEvent.HandleKeyDown += setup_inputEvent;
            if (App.checkForJoy())
            {
                p1_in = inputOpt.joy;
                this.P1_Opt.Text = inputOptString[(int) p1_in];
            }

            TwoPlayer = num_players;
            if(TwoPlayer)
            {
                this.P2_Title.Visibility = Visibility.Visible;
                this.P2_Opt.Visibility = Visibility.Visible;
            }
        }

        private void updateFont(opt nextOpt)
        {
            if (curOpt != nextOpt)
            {
                switch (curOpt)
                {
                    case opt.diff:
                        this.Diff_Opt.Foreground = Brushes.White;
                        this.DiffArrL.Visibility = Visibility.Collapsed;
                        this.DiffArrR.Visibility = Visibility.Collapsed;
                        break;
                    case opt.p1:
                        this.P1_Opt.Foreground = Brushes.White;
                        this.P1ArrL.Visibility = Visibility.Collapsed;
                        this.P1ArrR.Visibility = Visibility.Collapsed;
                        break;
                    case opt.p2:
                        this.P2_Opt.Foreground = Brushes.White;
                        this.P2ArrL.Visibility = Visibility.Collapsed;
                        this.P2ArrR.Visibility = Visibility.Collapsed;
                        break;
                    case opt.startGame:
                        this.StartGame.Foreground = Brushes.White;
                        break;
                    case opt.returnToStart:
                        this.ReturnToStart.Foreground = Brushes.White;
                        break;
                }

                curOpt = nextOpt;
            }
            else
            {
                switch (curOpt)
                {
                    case opt.diff:
                        this.Diff_Opt.Text = diffString[(int) diff];
                        break;
                    case opt.p1:
                        this.P1_Opt.Text = inputOptString[(int) p1_in];
                        break;
                    case opt.p2:
                        this.P2_Opt.Text = inputOptString[(int)p2_in];
                        break;
                }
            }

            switch (nextOpt)
            {
                case opt.diff:
                    this.Diff_Opt.Foreground = Brushes.Yellow;
                    if(diff > Difficulty.easy)
                        this.DiffArrL.Visibility = Visibility.Visible;
                    else
                        this.DiffArrL.Visibility = Visibility.Collapsed;
                    if (diff < Difficulty.hard)
                        this.DiffArrR.Visibility = Visibility.Visible;
                    else
                        this.DiffArrR.Visibility = Visibility.Collapsed;
                    break;
                case opt.p1:
                    this.P1_Opt.Foreground = Brushes.Yellow;
                    if(p1_in > inputOpt.joy)
                        this.P1ArrL.Visibility = Visibility.Visible;
                    else
                        this.P1ArrL.Visibility = Visibility.Collapsed;
                    if(p1_in < inputOpt.arrows)
                        this.P1ArrR.Visibility = Visibility.Visible;
                    else
                        this.P1ArrR.Visibility = Visibility.Collapsed;
                    break;
                case opt.p2:
                    this.P2_Opt.Foreground = Brushes.Yellow;
                    if (p2_in > inputOpt.joy)
                        this.P2ArrL.Visibility = Visibility.Visible;
                    else
                        this.P2ArrL.Visibility = Visibility.Collapsed;
                    if (p2_in < inputOpt.arrows)
                        this.P2ArrR.Visibility = Visibility.Visible;
                    else
                        this.P2ArrR.Visibility = Visibility.Collapsed;
                    break;
                case opt.startGame:
                    this.StartGame.Foreground = Brushes.Yellow;
                    break;
                case opt.returnToStart:
                    this.ReturnToStart.Foreground = Brushes.Yellow;
                    break;
            }
        }

        private void setup_keyDown(object sender, KeyEventArgs e)
        {
            setup_inputEvent(false, e.Key);
        }

        private int menuDelay = 0;
        public void simulateMenuDelay(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (menuDelay > 0 && ((menuDelay < 10 && !lastPlayerToInput) || (menuDelay < 30 && lastPlayerToInput)))
                        menuDelay++;
                    else
                        menuDelay = 0;
                });
            }
        }

        private bool lastPlayerToInput = true;
        private void setup_inputEvent(bool player1, Key key)
        {
            if (menuDelay != 0)
                return;

            switch (key)
            {
                case Key.Up:
                case Key.W:
                    if (curOpt == opt.diff)
                        updateFont(opt.returnToStart);
                    else if((curOpt == opt.startGame) && !TwoPlayer)
                        updateFont(opt.p1);
                    else if((curOpt == opt.returnToStart) && !validConfig)
                    {
                        if (TwoPlayer)
                            updateFont(opt.p2);
                        else
                            updateFont(opt.p1);
                    }
                    else
                        updateFont(curOpt - 1);
                    menuDelay++;
                    lastPlayerToInput = player1;
                    break;
                case Key.Down:
                case Key.S:
                    if (curOpt == opt.returnToStart)
                        updateFont(opt.diff);
                    else if ((((curOpt == opt.p1) && !TwoPlayer) || ((curOpt == opt.p2) && TwoPlayer)) && !validConfig)
                        updateFont(opt.returnToStart);
                    else if ((curOpt == opt.p1) && !TwoPlayer)
                        updateFont(opt.startGame);
                    else
                        updateFont(curOpt + 1);
                    menuDelay++;
                    lastPlayerToInput = player1;
                    break;
                case Key.Left:
                case Key.A:
                    switch(curOpt)
                    {
                        case opt.diff:
                            if (diff > Difficulty.easy)
                                diff--;
                            checkConfig();
                            updateFont(curOpt);
                            break;
                        case opt.p1:
                            if (p1_in > inputOpt.joy)
                                p1_in--;
                            checkConfig();
                            updateFont(curOpt);
                            break;
                        case opt.p2:
                            if (p2_in > inputOpt.joy)
                                p2_in--;
                            checkConfig();
                            updateFont(curOpt);
                            break;
                    }
                    break;
                case Key.Right:
                case Key.D:
                    switch(curOpt)
                    {
                        case opt.diff:
                            if (diff < Difficulty.hard)
                                diff++;
                            checkConfig();
                            updateFont(curOpt);
                            break;
                        case opt.p1:
                            if (p1_in < inputOpt.arrows)
                                p1_in++;
                            checkConfig();
                            updateFont(curOpt);
                            break;
                        case opt.p2:
                            if (p2_in < inputOpt.arrows)
                                p2_in++;
                            checkConfig();
                            updateFont(curOpt);
                            break;
                    }
                    break;
                case Key.Space:
                case Key.Enter:
                    selectOpt();
                    break;
            }
        }

        private void checkConfig()
        {
            if(TwoPlayer && (p1_in == p2_in))
            {
                validConfig = false;
                this.StartGame.Text = "PLAYERS CAN'T HAVE THE SAME INPUT DEVICES";
                this.StartGame.Foreground = Brushes.Gray;
            }
            else if((p1_in == inputOpt.joy || (TwoPlayer && (p2_in == inputOpt.joy))) && !App.checkForJoy())
            {
                validConfig = false;
                this.StartGame.Text = "NO JOYSTICK DETECTED";
                this.StartGame.Foreground = Brushes.Gray;
            }
            else
            {
                validConfig = true;
                this.StartGame.Text = "START GAME";
                this.StartGame.Foreground = Brushes.White;
            }
        }

        private void selectOpt()
        {
            switch(curOpt)
            {
                case opt.startGame:
                    if (validConfig)
                    {
                        MainWindow main = new MainWindow(TwoPlayer, diff);
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
            }
        }
    }

    
}
