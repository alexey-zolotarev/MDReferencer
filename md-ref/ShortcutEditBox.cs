using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils.VisualEffects;
using DevExpress.Utils.Controls;

namespace md_ref {
    public partial class ShortcutEditBox : UserControl, IXtraResizableControl {
        public ShortcutEditBox() {
            InitializeComponent();
        }

        public void Init(string textEditText) {
            

            validatedHotKey = textEditText;

            textEdit1.Text = textEditText;
        }

        public string GetTextEditText() {
            return validatedHotKey;
        }

        private void textEdit1_EditValueChanged(object sender, EventArgs e) {
            TextEdit te = sender as TextEdit;
            te.DoValidate();
        }

        string validatedHotKey = "";

        public event EventHandler Changed;

        public Size MinSize {
            get {
                return new Size((int)(textEdit1.Width*2.5), textEdit1.Height + 10);
            }
        }

        public Size MaxSize {
            get {
                return MinSize;
            }
        }

        public bool IsCaptionVisible {
            get {
                return false;
            }
        }

        private void textEdit1_Validating(object sender, CancelEventArgs e) {
            bool valid = true;

            TextEdit te = sender as TextEdit;

            validatedHotKey = te.Text;

            Hotkey hk = new Hotkey(te.Text);
            valid = hk.IsValidFromString;
            //labelControl1.Text = (hk.IsValidFromString ? "valid" : "invalid");

            if (hk.IsValidFromString) {
                bool canReg = hk.GetCanRegister(this);
                //labelControl1.Text = canReg ? "can register" : "cannot register";
                valid = canReg;
            }

            
            //FindForm().Text = hk.ToString();
            if (valid) {
                validatedHotKey = hk.ToString();
                vhShortcut.Properties.State = ValidationHintState.Valid;
                vhShortcut.Properties.ValidState.Text = "OK (" + validatedHotKey + ")";
            }
            else {
                
                vhShortcut.Properties.State = ValidationHintState.Invalid;
            }
        }
    }
}
