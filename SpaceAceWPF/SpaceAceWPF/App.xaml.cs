﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Timers;

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    
    public partial class App : Application
    {
        public static Timer timer = new Timer(10);
        public static event EventHandler<JoyUpArgs> joyUp;
        public static event EventHandler<JoyDownArgs> joyDown;
        private static ToddJoystick joy = null;
        private static Point prevJoyLoc, newJoyLoc;
        private static System.Drawing.Point mouseLoc, mouseOrigin, newMouseDelta, prevMouseDelta;
        private static bool appLostFocus = false;

        public App()
        {
            timer.Interval = 10;
            timer.Enabled = true;
            timer.Elapsed += timerOnElapsed;
            prevJoyLoc.X = 0;
            prevJoyLoc.Y = 0;
            mouseOrigin.X = 10;
            mouseOrigin.Y = 10;
            prevMouseDelta.X = 0;
            prevMouseDelta.Y = 0;
            checkForJoy();
        }

        public static int menuDelay = 0;
        public static InputType lastInputType = InputType.wasd;
        public static void timerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    //Simulate menu delay
                    if (menuDelay > 0 && ((menuDelay < 30 && lastInputType == InputType.joy) || (menuDelay < 5 && (lastInputType == InputType.wasd || lastInputType == InputType.arrows))))
                        menuDelay++;
                    else
                        menuDelay = 0;

                    //Joystick Input
                    if (joy != null)
                    {
                        joy.State(ref newJoyLoc);

                        if (newJoyLoc.X < -5)
                            RaiseJoyDown(Key.Left);
                        if (newJoyLoc.X > 5)
                            RaiseJoyDown(Key.Right);
                        if (newJoyLoc.Y < -5)
                            RaiseJoyDown(Key.Up);
                        if (newJoyLoc.Y > 5)
                            RaiseJoyDown(Key.Down);

                        if (newJoyLoc.X >= -5 && prevJoyLoc.X < -5)
                            RaiseJoyUp(Key.Left);
                        if (newJoyLoc.X <= 5 && prevJoyLoc.X > 5)
                            RaiseJoyUp(Key.Right);
                        if (newJoyLoc.Y >= -5 && prevJoyLoc.Y < -5)
                            RaiseJoyUp(Key.Up);
                        if (newJoyLoc.Y <= 5 && prevJoyLoc.Y > 5)
                            RaiseJoyUp(Key.Down);

                        prevJoyLoc = newJoyLoc;
                    }
                    else if (Current.MainWindow.IsMouseOver)
                    {
                        if (!appLostFocus)
                        {
                            mouseLoc = System.Windows.Forms.Control.MousePosition;
                            newMouseDelta = new System.Drawing.Point(mouseLoc.X - mouseOrigin.X, mouseLoc.Y - mouseOrigin.Y);

                            if (newMouseDelta.X < -2)
                                RaiseJoyDown(Key.Left);
                            if (newMouseDelta.X > 2)
                                RaiseJoyDown(Key.Right);
                            if (newMouseDelta.Y < -2)
                                RaiseJoyDown(Key.Up);
                            if (newMouseDelta.Y > 2)
                                RaiseJoyDown(Key.Down);

                            if(newMouseDelta.X >= -2 && prevMouseDelta.X < -2)
                                RaiseJoyUp(Key.Left);
                            if (newMouseDelta.X <= 2 && prevMouseDelta.X > 2)
                                RaiseJoyUp(Key.Right);
                            if (newMouseDelta.Y >= -2 && prevMouseDelta.Y < -2)
                                RaiseJoyUp(Key.Up);
                            if (newMouseDelta.Y <= 2 && prevMouseDelta.Y > 2)
                                RaiseJoyUp(Key.Down);

                            prevMouseDelta = newMouseDelta;
                        }
                        else
                            appLostFocus = false;
                        
                        System.Windows.Forms.Cursor.Position = mouseOrigin;
                    }
                    else
                        appLostFocus = true;

                });
            }
        }

        private static void RaiseJoyDown(Key key)
        {
            EventHandler<JoyDownArgs> handler = joyDown;
            if (handler != null)
            {
                handler(null, new JoyDownArgs(key));
            }
        }

        private static void RaiseJoyUp(Key key)
        {
            EventHandler<JoyUpArgs> handler = joyUp;
            if (handler != null)
            {
                handler(null, new JoyUpArgs(key));
            }
        }

        public static void checkForJoy()
        {
            if (ToddJoystick.NumJoysticks() > 0)
            {
                if (joy == null)
                    joy = new ToddJoystick();
            }
            else
                joy = null;
        }
    }

    public enum Difficulty { easy, med, hard };
    public enum InputType { joy, wasd, arrows, none };

}
