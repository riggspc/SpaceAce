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

        enum difficulty { easy, med, hard };
        difficulty diff = difficulty.easy;

        enum inputOpt { joy, wasd, arrows };
        inputOpt p1_in = inputOpt.wasd;
        inputOpt p2_in = inputOpt.arrows;

        private bool TwoPlayer = false;
        public SetupWindow(bool num_players)
        {
            InitializeComponent();
            App.timer.Elapsed += simulateMenuDelay;
            App.inputEvent.HandleKeyDown += setup_inputEvent;
            if (App.checkForJoy())
                p1_in = inputOpt.joy;

            TwoPlayer = num_players;
            if(TwoPlayer)
            {
                this.P2_Title.Visibility = Visibility.Visible;
                this.P2_Opt.Visibility = Visibility.Visible;
            }

            updateFont(opt.startGame);
        }

        private void updateFont(opt nextOpt)
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

            switch (nextOpt)
            {
                case opt.diff:
                    this.Diff_Opt.Foreground = Brushes.Yellow;
                    if(diff > difficulty.easy)
                        this.DiffArrL.Visibility = Visibility.Visible;
                    if (diff < difficulty.hard)
                        this.DiffArrR.Visibility = Visibility.Visible;
                    break;
                case opt.p1:
                    this.P1_Opt.Foreground = Brushes.Yellow;
                    if(p1_in > inputOpt.joy)
                        this.P1ArrL.Visibility = Visibility.Visible;
                    if(p1_in < inputOpt.arrows)
                        this.P1ArrR.Visibility = Visibility.Visible;
                    break;
                case opt.p2:
                    this.P2_Opt.Foreground = Brushes.Yellow;
                    if (p2_in > inputOpt.joy)
                        this.P2ArrL.Visibility = Visibility.Visible;
                    if (p2_in < inputOpt.arrows)
                        this.P2ArrR.Visibility = Visibility.Visible;
                    break;
                case opt.startGame:
                    this.StartGame.Foreground = Brushes.Yellow;
                    break;
                case opt.returnToStart:
                    this.ReturnToStart.Foreground = Brushes.Yellow;
                    break;
            }

            curOpt = nextOpt;
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
                    else if(curOpt == opt.startGame && !TwoPlayer)
                        updateFont(opt.p1);
                    else
                        updateFont(curOpt - 1);
                    menuDelay++;
                    lastPlayerToInput = player1;
                    break;
                case Key.Down:
                case Key.S:
                    if (curOpt == opt.returnToStart)
                        updateFont(opt.diff);
                    else if (curOpt == opt.p1 && !TwoPlayer)
                        updateFont(opt.startGame);
                    else
                        updateFont(curOpt + 1);
                    menuDelay++;
                    lastPlayerToInput = player1;
                    break;
                case Key.Space:
                case Key.Enter:
                    selectOpt();
                    break;
            }
        }

        private void selectOpt()
        {
            switch(curOpt)
            {
                case opt.startGame:
                    MainWindow main = new MainWindow(TwoPlayer);
                    App.Current.MainWindow = main;
                    main.Show();
                    this.Close();
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
