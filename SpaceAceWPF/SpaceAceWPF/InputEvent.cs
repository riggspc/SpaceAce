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
        public delegate void StatusUpdateHandler(bool player1, Key key);
        public event StatusUpdateHandler HandleKeyDown, HandleKeyUp;

        public void keyDown(bool player1, Key key)
        {
            if (HandleKeyDown != null) 
                HandleKeyDown(player1, key);
        }

        public void keyUp(bool player1, Key key)
        {
            if(HandleKeyUp != null)
                HandleKeyUp(player1, key);
        }
    }
}
