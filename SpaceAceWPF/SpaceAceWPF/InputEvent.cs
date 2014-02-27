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
        public event StatusUpdateHandler Input;

        public void keyDown(bool keyboard, Key key)
        {
            if (Input == null) 
                return;
            Input(keyboard, key);
        }
    }
}
