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
        private static ToddJoystick joy;
        private static Point prevJoyLoc, newJoyLoc;

        public App()
        {
            timer.Interval = 10;
            timer.Enabled = true;
            timer.Elapsed += timerOnElapsed;

            prevJoyLoc.X = 0;
            prevJoyLoc.Y = 0;
        }

        
        public static void timerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (joy != null)
                    {
                        joy.State(ref newJoyLoc);

                        if (newJoyLoc.X < -5)
                            inputEvent.keyDown(false, System.Windows.Input.Key.A);
                        if (newJoyLoc.X > 5)
                            inputEvent.keyDown(false, System.Windows.Input.Key.D);
                        if (newJoyLoc.Y < -5)
                            inputEvent.keyDown(false, System.Windows.Input.Key.W);
                        if (newJoyLoc.Y > 5)
                            inputEvent.keyDown(false, System.Windows.Input.Key.S);

                        if(newJoyLoc.X == 0 && prevJoyLoc.X < 0)
                            inputEvent.keyUp(false, System.Windows.Input.Key.A);
                        if (newJoyLoc.X == 0 && prevJoyLoc.X > 0)
                            inputEvent.keyUp(false, System.Windows.Input.Key.D);
                        if (newJoyLoc.Y == 0 && prevJoyLoc.Y < 0)
                            inputEvent.keyUp(false, System.Windows.Input.Key.W);
                        if (newJoyLoc.Y == 0 && prevJoyLoc.Y > 0)
                            inputEvent.keyUp(false, System.Windows.Input.Key.S);
                    }
                });
            }
        }

        public static void checkForJoy()
        {
            if (ToddJoystick.NumJoysticks() > 0)
            {
                if(joy == null)
                    joy = new ToddJoystick();
            }
            else
                joy = null;
        }
    }
}
