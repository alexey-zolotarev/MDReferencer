

// Добавил:
//Application.RemoveMessageFilter(this); //<<---------------------------------



using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace md_ref {



    public class Hotkey : IMessageFilter {


        //static bool PrefilterRegistered = false;

        #region Interop

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int UnregisterHotKey(IntPtr hWnd, int id);

        private const uint WM_HOTKEY = 0x312;

        private const uint MOD_ALT = 0x1;
        private const uint MOD_CONTROL = 0x2;
        private const uint MOD_SHIFT = 0x4;
        private const uint MOD_WIN = 0x8;

        private const uint ERROR_HOTKEY_ALREADY_REGISTERED = 1409;

        #endregion

        private static int currentID;
        private const int maximumID = 0xBFFF;

        private Keys _keyCode;
        private bool _shift;
        private bool _control;
        private bool _alt;
        private bool _windows;

        [XmlIgnore]
        private int id;
        [XmlIgnore]
        private bool registered;
        [XmlIgnore]
        private Control windowControl;

        public event HandledEventHandler Pressed;

        public Hotkey() : this(Keys.None, false, false, false, false) {
            // No work done here!
        }


        //public static Dictionary<int, Hotkey> AllHotKeys = new Dictionary<int, Hotkey>();


        void InitKeysModifier(ref string str, string searchModifierName, ref bool modifier) {

            int startIndex = str.IndexOf(searchModifierName, System.StringComparison.CurrentCultureIgnoreCase);
            if (startIndex >= 0) {
                modifier = true;
                str = str.Remove(startIndex, searchModifierName.Length);
            }
        }

        public Hotkey(string shortcut) {
            //string parsedShortCut = shortcut;
            //InitKeysModifier(ref parsedShortCut, "Ctrl", ref _control);
            //InitKeysModifier(ref parsedShortCut, "Control", ref _control);
            //InitKeysModifier(ref parsedShortCut, "alt", ref _alt);
            //InitKeysModifier(ref parsedShortCut, "shift", ref _shift);
            //InitKeysModifier(ref parsedShortCut, "win", ref _windows);
            //InitKeysModifier(ref parsedShortCut, "windows", ref _windows);
            //parsedShortCut = parsedShortCut.Replace("+", "");
            //parsedShortCut = parsedShortCut.Replace("-", "");
            //parsedShortCut = parsedShortCut.Replace(" ", "");
            //try {
            //    parsedShortCut = parsedShortCut.ToUpper();
            //    var cvt = new KeysConverter();
            //    this.KeyCode = (Keys)cvt.ConvertFrom(parsedShortCut);
            //    IsValidFromString = true;
            //}
            //catch {
            //    IsValidFromString = false;
            //}

            string parsedShortCut = shortcut;
            InitKeysModifier(ref parsedShortCut, "Ctrl", ref _control);
            InitKeysModifier(ref parsedShortCut, "Control", ref _control);
            InitKeysModifier(ref parsedShortCut, "alt", ref _alt);
            InitKeysModifier(ref parsedShortCut, "shift", ref _shift);
            InitKeysModifier(ref parsedShortCut, "win", ref _windows);
            InitKeysModifier(ref parsedShortCut, "windows", ref _windows);
            parsedShortCut = parsedShortCut.Replace("+", "");
            parsedShortCut = parsedShortCut.Replace("-", "");
            parsedShortCut = parsedShortCut.Replace(" ", "");
            try {
                Keys key = (Keys)Enum.Parse(typeof(Keys), parsedShortCut, true);
                this.KeyCode = key;
                IsValidFromString = true;
            }
            catch {
                IsValidFromString = false;
            }
            if (!IsValidFromString)
                return;
            //Для интерпретации строк Alt+1, +2, ...
            try {
                var cvt = new KeysConverter();
                Keys newKey = (Keys)cvt.ConvertFrom(parsedShortCut.ToUpper());
                string keyName = Enum.GetName(typeof(Keys), newKey);
                this.KeyCode = newKey;
            }
            catch {
            }
        }

        public bool IsValidFromString {
            get; set;
        }

        public Hotkey(Keys keyCode, bool shift, bool control, bool alt, bool windows) {
            IsValidFromString = true;

            // Assign properties
            this.KeyCode = keyCode;
            this.Shift = shift;
            this.Control = control;
            this.Alt = alt;
            this.Windows = windows;

            //if (!PrefilterRegistered) {
            // Register us as a message filter
            Application.AddMessageFilter(this);


            //    PrefilterRegistered = true;
            //}
        }

        ~Hotkey() {
            // Unregister the hotkey if necessary
            if (this.Registered) { this.Unregister(); }
        }

        public Hotkey Clone() {
            // Clone the whole object
            return new Hotkey(this._keyCode, this._shift, this._control, this._alt, this._windows);
        }

        public bool GetCanRegister(Control windowControl) {
            // Handle any exceptions: they mean "no, you can't register" :)
            try {
                // Attempt to register
                if (!this.Register(windowControl)) { return false; }

                // Unregister and say we managed it
                this.Unregister();
                return true;
            }
            catch (Win32Exception) { return false; }
            catch (NotSupportedException) { return false; }
        }

        public bool Register(Control windowControl) {
            // Check that we have not registered
            if (this.registered) {
                //throw new NotSupportedException("You cannot register a hotkey that is already registered");
                return false;
            }

            // We can't register an empty hotkey
            if (this.Empty) {
                //throw new NotSupportedException("You cannot register an empty hotkey");
                return false;
            }

            // Get an ID for the hotkey and increase current ID
            this.id = Hotkey.currentID;
            Hotkey.currentID = Hotkey.currentID + 1 % Hotkey.maximumID;

            // Translate modifier keys into unmanaged version
            uint modifiers = (this.Alt ? Hotkey.MOD_ALT : 0) | (this.Control ? Hotkey.MOD_CONTROL : 0) |
                            (this.Shift ? Hotkey.MOD_SHIFT : 0) | (this.Windows ? Hotkey.MOD_WIN : 0);

            // Register the hotkey
            if (Hotkey.RegisterHotKey(windowControl.Handle, this.id, modifiers, _keyCode) == 0) {
                // Is the error that the hotkey is registered?
                if (Marshal.GetLastWin32Error() == ERROR_HOTKEY_ALREADY_REGISTERED) { return false; }
                else {
                    //throw new Win32Exception();
                    return false;
                }
            }

            // Save the control reference and register state
            this.registered = true;
            this.windowControl = windowControl;

            // We successfully registered
            return true;
        }

        public void Unregister() {
            // Check that we have registered
            if (!this.registered) {
                //throw new NotSupportedException("You cannot unregister a hotkey that is not registered");
                return;
            }

            // It's possible that the control itself has died: in that case, no need to unregister!
            if (!this.windowControl.IsDisposed) {
                // Clean up after ourselves
                int res = Hotkey.UnregisterHotKey(this.windowControl.Handle, this.id);
                if (res == 0) {
                    //throw new Win32Exception();
                    return;
                }
            }

            // Clear the control reference and register state
            this.registered = false;
            this.windowControl = null;

            Application.RemoveMessageFilter(this); //<<---------------------------------
        }

        private void Reregister() {
            // Only do something if the key is already registered
            if (!this.registered) { return; }

            // Save control reference
            Control windowControl = this.windowControl;

            // Unregister and then reregister again
            this.Unregister();
            this.Register(windowControl);
        }

        bool IMessageFilter.PreFilterMessage(ref Message message) {
            // Only process WM_HOTKEY messages
            if (message.Msg != Hotkey.WM_HOTKEY) { return false; }

            // Check that the ID is our key and we are registerd
            if (this.registered && (message.WParam.ToInt32() == this.id)) {
                // Fire the event and pass on the event if our handlers didn't handle it
                return this.OnPressed();
            }
            else { return false; }
        }

        private bool OnPressed() {
            // Fire the event if we can
            HandledEventArgs handledEventArgs = new HandledEventArgs(false);
            if (this.Pressed != null) { this.Pressed(this, handledEventArgs); }

            // Return whether we handled the event or not
            return handledEventArgs.Handled;
        }

        public override string ToString() {
            // We can be empty
            if (this.Empty) { return "(none)"; }

            // Build key name
            string keyName = Enum.GetName(typeof(Keys), this._keyCode); ;


            switch (this._keyCode) {
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    // Strip the first character
                    keyName = keyName.Substring(1);
                    break;
                default:
                    // Leave everything alone
                    break;
            }

            // Build modifiers
            string modifiers = "";
            if (this._shift) { modifiers += "Shift+"; }
            if (this._control) { modifiers += "Ctrl+"; }
            if (this._alt) { modifiers += "Alt+"; }
            if (this._windows) { modifiers += "Win+"; }

            // Return result
            if (keyName != null)
                return modifiers + keyName;
            else
                return modifiers + _keyCode;
        }

        public bool Empty {
            get { return this._keyCode == Keys.None; }
        }

        public bool Registered {
            get { return this.registered; }
        }

        public Keys KeyCode {
            get { return this._keyCode; }
            set {
                // Save and reregister
                this._keyCode = value;
                this.Reregister();
            }
        }

        public bool Shift {
            get { return this._shift; }
            set {
                // Save and reregister
                this._shift = value;
                this.Reregister();
            }
        }

        public bool Control {
            get { return this._control; }
            set {
                // Save and reregister
                this._control = value;
                this.Reregister();
            }
        }

        public bool Alt {
            get { return this._alt; }
            set {
                // Save and reregister
                this._alt = value;
                this.Reregister();
            }
        }

        public bool Windows {
            get { return this._windows; }
            set {
                // Save and reregister
                this._windows = value;
                this.Reregister();
            }
        }
    }


}
