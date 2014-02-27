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
        public App()
        {
            timer.Interval = 10;
            timer.Enabled = true;
            timer.Elapsed += timerOnElapsed;
        }

        public static void timerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (App.Current != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (joy != null)
                        joy.State(ref inputEvent);
                });
            }
        }

        public static void checkForJoy()
        {
            if (ToddJoystick.NumJoysticks() > 0 && joy == null)
                joy = new ToddJoystick();
            else
                joy = null;
        }
    }
}
