using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SpaceAceWPF
{
    public class InputEvent
    {
        public delegate void StatusUpdateHandler(Key key);
        public event StatusUpdateHandler HandleJoyUp, HandleJoyDown;

        public void joyDown(Key key)
        {
            if (HandleJoyUp != null) 
                HandleJoyUp(key);
        }

        public void joyUp(Key key)
        {
            if(HandleJoyUp != null)
                HandleJoyUp(key);
        }
    }
}
