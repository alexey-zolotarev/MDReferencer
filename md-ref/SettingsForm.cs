using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.IO;

namespace md_ref {
    public partial class SettingsForm : DevExpress.XtraEditors.XtraForm {
        public SettingsForm(Params parameters) {
            //string defaultFolder, string HotkeyMemberString, string HotkeyClassString, string HotkeyNamespacesString, string HotkeyDocString, string defaultFontString

            this.Parameters = parameters;

            InitializeComponent();

            

            shEBMembers.Init(Parameters.HotkeyMemberString);
            shEBClasses.Init(Parameters.HotkeyClassString);
            shEBNamespaces.Init(Parameters.HotkeyNamespacesString);
            shEBDocs.Init(Parameters.HotkeyDocString);


            teDefaultFont.Text = Parameters.DefaultFontString;
            btnEditChooseFolder.Text = Parameters.RepFolder;
        }

        Params Parameters;

        private void buttonEdit1_EditValueChanged(object sender, EventArgs e) {
            try {
                ButtonEdit btnEdit = sender as ButtonEdit;
                DirectoryInfo di = new DirectoryInfo(btnEdit.Text);
                Parameters.RepFolder = di.FullName;
                btnOK.Enabled = di.Exists;
                if (Parameters.RepFolder.EndsWith("\\"))
                    Parameters.RepFolder = Parameters.RepFolder.Substring(0, Parameters.RepFolder.Length - 1);
            }
            catch { }
        }
        
        

        private void SpecifySettingsForm_Load(object sender, EventArgs e) {
            this.Text = Form1.ProgName + " - Settings";
        }

        private void SpecifySettingsForm_FormClosing(object sender, FormClosingEventArgs e) {
            Parameters.HotkeyMemberString = shEBMembers.GetTextEditText();
            Parameters.HotkeyClassString = shEBClasses.GetTextEditText();
            Parameters.HotkeyNamespacesString = shEBNamespaces.GetTextEditText();
            Parameters.HotkeyDocString = shEBDocs.GetTextEditText();

            Parameters.DefaultFontString = teDefaultFont.Text;

        }

        private void SpecifySettingsForm_Shown(object sender, EventArgs e) {
            this.ValidateChildren();
        }
    }
}