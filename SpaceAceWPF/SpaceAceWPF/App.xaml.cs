using System;
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
                    if (menuDelay > 0 && ((menuDelay < 30 && lastInputType == InputType.joy) || (menuDelay < 8 && (lastInputType == InputType.wasd || lastInputType == InputType.arrows))))
                        menuDelay++;
                    else
                        menuDelay = 0;

                    //Joystick Input
                    if (joy != null)
                    {
                        joy.State(ref newJoyLoc);

                        if (newJoyLoc.X < -5)
                            RaiseJoyDown(Key.A);
                        if (newJoyLoc.X > 5)
                            RaiseJoyDown(Key.D);
                        if (newJoyLoc.Y < -5)
                            RaiseJoyDown(Key.W);
                        if (newJoyLoc.Y > 5)
                            RaiseJoyDown(Key.S);

                        if (newJoyLoc.X >= -5 && prevJoyLoc.X < -5)
                            RaiseJoyUp(Key.A);
                        if (newJoyLoc.X <= 5 && prevJoyLoc.X > 5)
                            RaiseJoyUp(Key.D);
                        if (newJoyLoc.Y >= -5 && prevJoyLoc.Y < -5)
                            RaiseJoyUp(Key.W);
                        if (newJoyLoc.Y <= 5 && prevJoyLoc.Y > 5)
                            RaiseJoyUp(Key.S);

                        prevJoyLoc = newJoyLoc;
                    }
                    else if (Current.MainWindow.IsMouseOver)
                    {
                        if (!appLostFocus)
                        {
                            mouseLoc = System.Windows.Forms.Control.MousePosition;
                            newMouseDelta = new System.Drawing.Point(mouseLoc.X - mouseOrigin.X, mouseLoc.Y - mouseOrigin.Y);

                            if (newMouseDelta.X < -5)
                                RaiseJoyDown(Key.A);
                            if (newMouseDelta.X > 5)
                                RaiseJoyDown(Key.D);
                            if (newMouseDelta.Y < -5)
                                RaiseJoyDown(Key.W);
                            if (newMouseDelta.Y > 5)
                                RaiseJoyDown(Key.S);

                            if(newMouseDelta.X >= -5 && prevMouseDelta.X < -5)
                                RaiseJoyUp(Key.A);
                            if (newMouseDelta.X <= 5 && prevMouseDelta.X > 5)
                                RaiseJoyUp(Key.D);
                            if (newMouseDelta.Y >= -5 && prevMouseDelta.Y < -5)
                                RaiseJoyUp(Key.W);
                            if (newMouseDelta.Y <= 5 && prevMouseDelta.Y > 5)
                                RaiseJoyUp(Key.S);

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
