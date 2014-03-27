using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Timers;

namespace SpaceAceWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    
    public partial class App : Application
    {
        public static Timer timer = new Timer(10);
        public static InputEvent inputEvent = new InputEvent();
        private static ToddJoystick joy = null;
        private static Point prevJoyLoc, newJoyLoc;

        public App()
        {
            timer.Interval = 10;
            timer.Enabled = true;
            timer.Elapsed += timerOnElapsed;
            prevJoyLoc.X = 0;
            prevJoyLoc.Y = 0;
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
                            inputEvent.joyDown(System.Windows.Input.Key.A);
                        if (newJoyLoc.X > 5)
                            inputEvent.joyDown(System.Windows.Input.Key.D);
                        if (newJoyLoc.Y < -5)
                            inputEvent.joyDown(System.Windows.Input.Key.W);
                        if (newJoyLoc.Y > 5)
                            inputEvent.joyDown(System.Windows.Input.Key.S);

                        if(newJoyLoc.X >= -5 && prevJoyLoc.X < -5)
                            inputEvent.joyUp(System.Windows.Input.Key.A);
                        if (newJoyLoc.X <= 5 && prevJoyLoc.X > 5)
                            inputEvent.joyUp(System.Windows.Input.Key.D);
                        if (newJoyLoc.Y >= -5 && prevJoyLoc.Y < -5)
                            inputEvent.joyUp(System.Windows.Input.Key.W);
                        if (newJoyLoc.Y <= 5 && prevJoyLoc.Y > 5)
                            inputEvent.joyUp(System.Windows.Input.Key.S);

                        prevJoyLoc = newJoyLoc;
                    }
                });
            }
        }

        public static bool checkForJoy()
        {
            if (ToddJoystick.NumJoysticks() > 0)
            {
                if (joy == null)
                    joy = new ToddJoystick();
                return true;
            }
            else
            {
                joy = null;
                return false;
            }
        }
    }

    public enum Difficulty { easy, med, hard };
    public enum InputType { joy, wasd, arrows, none };

}
