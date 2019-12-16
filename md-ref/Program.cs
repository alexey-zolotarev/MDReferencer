using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace md_ref {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            if (Properties.Settings.Default.UpgradeRequired) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }




            //DevExpress.XtraEditors.WindowsFormsSettings.DefaultSettingsCompatibilityMode = DevExpress.XtraEditors.SettingsCompatibilityMode.v18_2;

            DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName = Properties.Settings.Default.SkinName;

            Params parameters = new Params();

            parameters.DefaultFontString = Properties.Settings.Default.DefaultFont;

            var cvt = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font)); 
            Font f = cvt.ConvertFromString(parameters.DefaultFontString) as Font;
            WindowsFormsSettings.DefaultFont = f;
            WindowsFormsSettings.DefaultMenuFont = f;


            
            parameters.RepFolder = Properties.Settings.Default.RepPath;
            parameters.HotkeyMemberString = Properties.Settings.Default.ShortcutMember;
            parameters.HotkeyClassString = Properties.Settings.Default.ShortcutClass;
            parameters.HotkeyNamespacesString = Properties.Settings.Default.ShortcutNamespace;
            parameters.HotkeyDocString = Properties.Settings.Default.ShortcutDoc;
            parameters.AutoMinimize = Properties.Settings.Default.AutoMinimize;
            parameters.ServiceMode = Properties.Settings.Default.ServiceMode;




            string formSizeString = Properties.Settings.Default.FormSize;
            string formLocationString = Properties.Settings.Default.FormLocation;

            Size FormSize = new Size(1400, 800);
            bool resetStoredLocation = false;
            var cvtSize = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size));
            Size storedSize = FormSize;
            try {
                storedSize = (Size)cvtSize.ConvertFromString(formSizeString);
            }
            catch { }
            if (storedSize.Width > 100 && storedSize.Height > 100)
                FormSize = storedSize;
            else {
                resetStoredLocation = true;
            }

            Rectangle screenClientBounds = Screen.GetWorkingArea(new Point(0, 0));
            Point FormLocation = new Point(screenClientBounds.Left, screenClientBounds.Top);

            var cvtLocation = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point));
            Point storedLocation = FormLocation;
            if (!resetStoredLocation) {
                try {
                    storedLocation = (Point)cvtLocation.ConvertFromString(formLocationString);
                }
                catch { }
            }
            if (storedLocation.X > 0 && storedLocation.Y > 0 && storedLocation.X < 5000 && storedLocation.Y < 4000)
                FormLocation = storedLocation;
            else {

            }


            SettingsForm settingsForm = new SettingsForm(parameters);
            if (settingsForm.ShowDialog() != DialogResult.OK) {
                return;
            }
            

            try {
                f = cvt.ConvertFromString(parameters.DefaultFontString) as Font;
            }
            catch {
                f = new Font("Tahoma", 9);
            }
            WindowsFormsSettings.DefaultFont = f;
            WindowsFormsSettings.DefaultMenuFont = f;

            Form1 mainForm = new Form1(parameters);
            mainForm.Size = FormSize;
            mainForm.Location = FormLocation;
            Application.Run(mainForm);
        }



    }

    public enum WorkspaceKind { Classes, Members, Namespaces, Docs }

    public static class Settings {
        static Settings() {
            WorkspaceNames = new Dictionary<WorkspaceKind, string>();

            WorkspaceNames.Add(WorkspaceKind.Classes, "Classes");
            WorkspaceNames.Add(WorkspaceKind.Members, "Members");
            WorkspaceNames.Add(WorkspaceKind.Namespaces, "Namespaces");
            WorkspaceNames.Add(WorkspaceKind.Docs, "Docs");
        }

        public static Dictionary<WorkspaceKind, string> WorkspaceNames;
    }

    public class Params {
        public string RepFolder;

        public string HotkeyMemberString;
        public string HotkeyClassString;
        public string HotkeyNamespacesString;
        public string HotkeyDocString;
        public string DefaultFontString;


        public bool ServiceMode;
        public bool AutoMinimize;

    }
}
