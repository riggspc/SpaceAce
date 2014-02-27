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
        public delegate void StatusUpdateHandler(bool keyboard, Key key);
        public event StatusUpdateHandler HandleKeyDown, HandleKeyUp;

        public void keyDown(bool keyboard, Key key)
        {
            if (HandleKeyDown != null) 
                HandleKeyDown(keyboard, key);
        }

        public void keyUp(bool keyboard, Key key)
        {
            if(HandleKeyUp != null)
                HandleKeyUp(keyboard, key);
        }
    }
}
