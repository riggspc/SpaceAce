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
        public static Timer aTimer = new Timer(10);
        public App()
        {
            aTimer.Interval = 10;
            aTimer.Enabled = true;
        }
            
    }
}
