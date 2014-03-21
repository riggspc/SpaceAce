using System;
using System.Windows;
using SlimDX.DirectInput;
using System.Windows.Input;

namespace SpaceAceWPF
{
    class ToddJoystick : IDisposable
    {
        ///         
        /// Joystick Handle
        /// 
        private Joystick _joystick;

        public static int NumJoysticks()
        {
            DirectInput direct = new DirectInput();
            int count = direct.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly).Count;
            direct.Dispose();
            return count;
        }

        public void State(ref Point joyLoc)
        {
            if (_joystick.Poll().IsFailure)
            {
                joyLoc.X = 0;
                joyLoc.Y = 0;
            }
            else
            {
                var curJoyLoc = _joystick.GetCurrentState();
                joyLoc.X = curJoyLoc.X;
                joyLoc.Y = curJoyLoc.Y;
            }
        }

        public ToddJoystick()
        {
            DirectInput dinput = new DirectInput();

            // search for devices
            foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly)){
                try{
                    _joystick = new Joystick(dinput, device.InstanceGuid);
                    break;
                }
                catch(DirectInputException){}
            }

            if(_joystick == null)
                throw new Exception("No joystick found");
                
            foreach(DeviceObjectInstance deviceObject in _joystick.GetObjects())
            {
                if((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                    _joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100,100);
            }

            _joystick.Acquire();
        }   

        public void Release(){
            if(_joystick != null)
            {
                _joystick.Unacquire();
                _joystick.Dispose();
            }
            _joystick = null;
        }


        public void Dispose()
        {
            Release();
        }
    }
}
