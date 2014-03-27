using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace SpaceAceWPF
{
    public class JoyDownArgs : EventArgs
    {
       public Key Key;

        public JoyDownArgs(Key _Key)
        {
            Key = _Key;
        }

        public Key Message
        {
            get { return Key; }
            set { Key = value; }
        }
    }

    public class JoyUpArgs : EventArgs
    {
        public Key Key;

        public JoyUpArgs(Key _Key)
        {
            Key = _Key;
        }

        public Key Message
        {
            get { return Key; }
            set { Key = value; }
        }
    }
}
