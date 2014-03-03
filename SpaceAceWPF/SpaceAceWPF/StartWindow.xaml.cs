﻿using System;
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
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        enum opt { play1, play2, viewHigh, exitGame};
        opt curOpt = opt.play1;

        public StartWindow()
        {
            InitializeComponent();
            App.checkForJoy();
            App.timer.Elapsed += simulateMenuDelay;
            App.inputEvent.HandleJoyDown += start_joyDown;

            updateFont(opt.play1);
        }

        private void updateFont(opt nextOpt)
        {
            switch (curOpt)
            {
                case opt.play1:
                    this.play1.Foreground = Brushes.White;
                    break;
                case opt.play2:
                    this.play2.Foreground = Brushes.White;
                    break;
                case opt.viewHigh:
                    this.viewHigh.Foreground = Brushes.White;
                    break;
                case opt.exitGame:
                    this.exitGame.Foreground = Brushes.White;
                    break;
            }

            switch(nextOpt)
            {
                case opt.play1:
                    this.play1.Foreground = Brushes.Yellow;
                    break;
                case opt.play2:
                    this.play2.Foreground = Brushes.Yellow;
                    break;
                case opt.viewHigh:
                    this.viewHigh.Foreground = Brushes.Yellow;
                    break;
                case opt.exitGame:
                    this.exitGame.Foreground = Brushes.Yellow;
                    break;
            }

            curOpt = nextOpt;
        }

        private void start_keyDown(object sender, KeyEventArgs e)
        {
            start_inputEvent(true, e.Key);
        }

        private void start_joyDown(Key key)
        {
            start_inputEvent(false, key);
        }

        private int menuDelay = 0;
        public void simulateMenuDelay(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (menuDelay > 0 && ((menuDelay < 30 && !lastPlayerToInput) || (menuDelay < 10 && lastPlayerToInput)))
                        menuDelay++;
                    else
                        menuDelay = 0;
                });
            }
        }

        private bool lastPlayerToInput = true;
        private void start_inputEvent(bool keyboard, Key key)
        {
            if (menuDelay != 0)
                return;

            switch (key)
            {
                case Key.Up:
                case Key.W:
                    if (curOpt == opt.play1)
                        updateFont(opt.exitGame);
                    else
                        updateFont(curOpt - 1);
                    menuDelay++;
                    lastPlayerToInput = keyboard;
                    break;
                case Key.Down:
                case Key.S:
                    if (curOpt == opt.exitGame)
                        updateFont(opt.play1);
                    else
                        updateFont(curOpt + 1);
                    menuDelay++;
                    lastPlayerToInput = keyboard;
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
                case opt.play1:
                    SetupWindow setup1 = new SetupWindow(false);
                    App.Current.MainWindow = setup1;
                    setup1.Show();
                    this.Close();
                    break;
                case opt.play2:
                    SetupWindow setup2 = new SetupWindow(true);
                    App.Current.MainWindow = setup2;
                    setup2.Show();
                    this.Close();
                    break;
                case opt.viewHigh:
                    ScoreboardWindow scoreboard = new ScoreboardWindow();
                    App.Current.MainWindow = scoreboard;
                    scoreboard.Show();
                    this.Close();
                    break;
                case opt.exitGame:
                    Application.Current.Shutdown();
                    break;
            }
        }
    }
}
