using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace md_ref {
    class Keyboard2{

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public const int KEYEVENTF_KEYDOWN = 0x0000; // New definition
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag
        public const int VK_LCONTROL = 0xA2; //Left Control key code
        public const int KEY_A = 0x41; //A key code
        public const int KEY_C = 0x43; //C key code
        public const int KEY_V = 0x56; //V key code


        public static void SendCtrlV() {
            // Hold Control down and press V
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(KEY_V, 0, KEYEVENTF_KEYDOWN, 0);
            keybd_event(KEY_V, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);

            // Hold Control down and press C
            //keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYDOWN, 0);
            //keybd_event(KEY_C, 0, KEYEVENTF_KEYDOWN, 0);
            //keybd_event(KEY_C, 0, KEYEVENTF_KEYUP, 0);
            //keybd_event(VK_LCONTROL, 0, KEYEVENTF_KEYUP, 0);
        }


    }
}
