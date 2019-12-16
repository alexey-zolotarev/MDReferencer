using DevExpress.Data.Filtering;
using DevExpress.Utils.Behaviors.Common;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;

using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace md_ref {





    public partial class Form1 : DevExpress.XtraBars.Ribbon.RibbonForm {

        public static string ProgName = "MDRef";
        public static string ProgVersion = "";

        
        //public DevExpress.XtraSplashScreen.SplashScreenManager SplashScreenManager1;

        Logger MyLogger;


        AvailReps AvailReps;

        string SelectedFolder = "";

        Params Parameters;

        public Form1(Params parameters) {

            this.Parameters = parameters;

            this.SelectedFolder = Parameters.RepFolder;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string assemblyVersion = "v" + version.Major + "." + version.Minor ;

            ProgVersion = " (" + assemblyVersion + ")";
            

            MyLogger = new Logger(this);

            InitializeComponent();

        }

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, String lpWindowName);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_SETTEXT = 0x000C;
        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 SC_MINIMIZE = 0xF020;
        public const Int32 SC_RESTORE = 0xF120;








        private void FilterGrid(WorkspaceKind workspaceKind) {
            if (AvailReps.CurrentUIWorkspace != null) {
                AvailReps.ShowUI(Settings.WorkspaceNames[workspaceKind]);
                
            }
            
        }

        private void ActivateGrid() {
            if(AvailReps.CurrentUIWorkspace != null)
                AvailReps.CurrentUIWorkspace.CurrentGrid.Focus();
            
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        IntPtr PreviousForegroundWindow;
        IntPtr PreviousActiveWindow;

        bool ActivateWindow(bool activateViaHotkey) {

            IntPtr foregroundWindow = GetForegroundWindow();
            IntPtr activeWindow = GetActiveWindow();

            this.Visible = true;

            //PreviousForegroundWindow = (IntPtr)0;
            //PreviousActiveWindow = (IntPtr)0;

            

            if (foregroundWindow == this.Handle) {
                if (!activateViaHotkey) {
                    PreviousForegroundWindow = (IntPtr)0;
                    PreviousActiveWindow = (IntPtr)0;
                }
                return true;
            }
            //Save previously active window
            PreviousForegroundWindow = foregroundWindow;
            PreviousActiveWindow = activeWindow;

            //Restore window
            if (this.WindowState == FormWindowState.Minimized)
                SendMessage(this.Handle, WM_SYSCOMMAND, SC_RESTORE, 0);
            this.Activate();
            return true;
        }


        public static bool IsApplicationActive() {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        



        protected override void OnClosing(CancelEventArgs e) {
            MyLogger.Log("OnClosing" + "WindowsState=Minimized");
            this.WindowState = FormWindowState.Minimized;
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;

        protected override CreateParams CreateParams {
            get {
                CreateParams myCp = base.CreateParams;
                //myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        protected override void OnLoad(EventArgs e) {
            PersistenceBehavior.SetSerializationEnabled(this, false);
            PersistenceBehavior.SetSerializationEnabled(ribbonControl1, false);

            gridControlClasses.ForceInitialize();
            gridControlMembers.ForceInitialize();
            gridControlNamespaces.ForceInitialize();
            gridControlCustomDocs.ForceInitialize();

            ribbonControl1.ForceInitialize();

            

            base.OnLoad(e);
        }


        MyHotKeys hotKeys;
        

        protected override void OnHandleCreated(EventArgs e) {
            base.OnHandleCreated(e);

            bool res1, res2, res3, res4;
            hotKeys = new MyHotKeys(this);
            

            res1 = hotKeys.RegisterHotKey(WorkspaceKind.Members, Parameters.HotkeyMemberString);
            res2 = hotKeys.RegisterHotKey(WorkspaceKind.Classes, Parameters.HotkeyClassString);
            res3 = hotKeys.RegisterHotKey(WorkspaceKind.Namespaces, Parameters.HotkeyNamespacesString);
            res4 = hotKeys.RegisterHotKey(WorkspaceKind.Docs, Parameters.HotkeyDocString);




            if (!res1 || !res2 || !res3 || !res4) {
                Console.WriteLine("Cannot register all the shortcuts");
            }

            hotKeys.HotKeyPressed += HotKeys_HotKeyPressed;


        }

        

        protected override void SetVisibleCore(bool value) {
            
            base.SetVisibleCore(value);
        }

        private void HotKeys_HotKeyPressed(WorkspaceKind wspKind) {
            if (Locked)
                return;
            if (!ActivateWindow(activateViaHotkey:true))
                return;
            FilterGrid(wspKind);
            ActivateGrid();
        }



        public class Nameof<T> {
            public static string Property<TProp>(Expression<Func<T, TProp>> expression) {
                var body = expression.Body as MemberExpression;
                if (body == null)
                    throw new ArgumentException("'expression' should be a member expression");
                return body.Member.Name;
            }
        }


        void MakeColumnFirstLargeAndSorted(GridControl grid, string colName) {
            ColumnView view;
            GridColumn col1;
            view = (grid.MainView as ColumnView);
            col1 = view.Columns[colName];

            if (col1 == null)
                return;
            col1.VisibleIndex = 0;
            col1.SortIndex = 0;
            col1.SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
            col1.Width = 200;

        }

        void FocusColumn(GridControl grid, string colName) {
            ColumnView view;
            GridColumn col1;
            view = (grid.MainView as ColumnView);
            col1 = view.Columns[colName];

            if (col1 != null && col1.Visible)
                view.FocusedColumn = col1;
            else {
                if (view.VisibleColumns.Count > 0) {
                    GridColumn firstVisCol = view.VisibleColumns[0];
                    view.FocusedColumn = firstVisCol;
                }
            }
        }


        void HideColumns(GridControl grid, string[] colNames) {
            foreach (string colName in colNames)
                HideColumn(grid, colName);
        }
        

        void HideColumn(GridControl grid, string colName) {
            ColumnView view;
            GridColumn col1;
            view = (grid.MainView as ColumnView);
            col1 = view.Columns[colName];
            if (col1 != null)
                col1.Visible = false;
        }


        private void Form1_Load(object sender, EventArgs e) {
            
            if(Parameters.AutoMinimize)
                this.WindowState = FormWindowState.Minimized;

            //this.ShowInTaskbar = false;


            

            InitGridBeforeRestoreLayout(gridControlClasses);
            InitGridBeforeRestoreLayout(gridControlMembers);
            InitGridBeforeRestoreLayout(gridControlNamespaces);
            InitGridBeforeRestoreLayout(gridControlCustomDocs);

            //Для десериализации колонок:
            gridControlClasses.DataSource = new List<ClassEl>();
            gridControlCustomDocs.DataSource = new List<CustomDocEl>();
            gridControlMembers.DataSource = new List<MemberEl>();
            gridControlNamespaces.DataSource = new List<NamespaceEl>();



            propertyGridControl1.SelectedObject = gridControlCustomDocs.MainView;

            //ProcessWatcher.StartWatch();


            notifyIconActionExit.Click += NotifyIconActionExit_Click;
            notifyIconActionOpen.Click += NotifyIconActionOpen_Click;

            string parentFolder = AvailReps.GetParentFolder(SelectedFolder);

            AvailReps = new AvailReps(this, parentFolder,
                gridControlMembers,
                gridControlClasses,
                gridControlNamespaces,
                gridControlCustomDocs,
                barCheckItemMembers,
                barCheckItemClasses,
                barCheckItemNamespaces,
                barCheckItemDocs,
                riLWorkspaceSelector,
                beiLWorkspaceSelector);


            btnHideForm.Links[0].BeginGroup = true;
            btnRefresh.Links[0].BeginGroup = true;



            ShouldRestoreClipboardSetting = DefaultShouldRestoreClipboard;
            DelayAfterPaste = DefaultDelayAfterPaste;
            PasteMode = DefaultPasteTextMode;

            bciShouldRestoreClipboard.EditValue = DefaultShouldRestoreClipboard;
            bciShouldRestoreClipboard.EditValueChanged += BciShouldRestoreClipboard_EditValueChanged;

            beiDelayAfterPaste.EditValue = DefaultDelayAfterPaste;
            beiDelayAfterPaste.EditValueChanged += BeiDelayAfterPaste_EditValueChanged;

            riRadioGroupPasteMode.Items.Clear();
            riRadioGroupPasteMode.Items.AddEnum(typeof(PasteTextMode));
            beiPasteMode.EditValue = DefaultPasteTextMode;
            beiPasteMode.EditValueChanged += BeiPasteMode_EditValueChanged;
        }

        private void BeiPasteMode_EditValueChanged(object sender, EventArgs e) {
            PasteMode = (PasteTextMode)beiPasteMode.EditValue;
        }

        private void BeiDelayAfterPaste_EditValueChanged(object sender, EventArgs e) {
            DelayAfterPaste = Convert.ToInt32(beiDelayAfterPaste.EditValue);
        }

        private void BciShouldRestoreClipboard_EditValueChanged(object sender, EventArgs e) {
            ShouldRestoreClipboardSetting = Convert.ToBoolean(bciShouldRestoreClipboard.EditValue);
        }

        bool DefaultShouldRestoreClipboard = true;
        int DefaultDelayAfterPaste = 100;
        PasteTextMode DefaultPasteTextMode = PasteTextMode.SendKeys;

        enum PasteTextMode {
            SendKeys,
            SendInput,
            keybd_event
        }

        
        bool ShouldRestoreClipboardSetting {
            get; set;
        }

        
        int DelayAfterPaste {
            get; set;
        }

        
        PasteTextMode PasteMode {
            get;set;
        }

        private void NotifyIconActionOpen_Click(object sender, EventArgs e) {
            ShowFormFromNotifyIcon();
        }
    
        void ShowFormFromNotifyIcon() {
            Show();
            this.WindowState = FormWindowState.Normal;

            //Restore window
            if (this.WindowState == FormWindowState.Minimized)
                SendMessage(this.Handle, WM_SYSCOMMAND, SC_RESTORE, 0);
            this.Activate();

        }

        public bool NeedCloseApp = false;

        private void NotifyIconActionExit_Click(object sender, EventArgs e) {
            NeedCloseApp = true;
            this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e) {

            InitGridAfterLoad(gridControlClasses);
            InitGridAfterLoad(gridControlMembers);
            InitGridAfterLoad(gridControlNamespaces);
            InitGridAfterLoad(gridControlCustomDocs);

            InitRibbonAfterLoad();
            

            Application.DoEvents();
            bool res = AvailReps.SelectWorkspace(SelectedFolder);
            if(!res) {
                string s = "You have selected a non-repository folder";
                SetNotifyIconStatus(NotifyIconStatus.ready, s);
                if(AvailReps != null)
                    AvailReps.ClearFormCaption(s);
        }


            string col1 = Nameof<BaseElement>.Property(ee => ee.Search);

            if(!GridLayoutExistsAndItWasLoaded) {
                MakeColumnFirstLargeAndSorted(gridControlClasses, col1);
                MakeColumnFirstLargeAndSorted(gridControlMembers, col1);
                MakeColumnFirstLargeAndSorted(gridControlNamespaces, col1);
                MakeColumnFirstLargeAndSorted(gridControlCustomDocs, col1);


                string colFullClassName = Nameof<MemberEl>.Property(ee => ee.FullClassName);
                string colMDType = Nameof<MemberEl>.Property(ee => ee.MDType);
                string colFullName = Nameof<MemberEl>.Property(ee => ee.FullName);
                string colMDUid = Nameof<MemberEl>.Property(ee => ee.MDUid);
                string colMDName = Nameof<MemberEl>.Property(ee => ee.MDName);
                string colUidName = Nameof<MemberEl>.Property(ee => ee.UIDName);

                string[] GridMembersHideColumnNames = new string[] { colFullClassName, colMDType, colFullName, colMDUid, colMDName, colUidName };
                HideColumns(gridControlMembers, GridMembersHideColumnNames);
                
                string[] GridClassesHideColumnNames = new string[] { colMDType, colFullName, colMDUid, colMDName, colUidName };
                HideColumns(gridControlClasses, GridClassesHideColumnNames);

                string[] GridNamespacesHideColumnNames = new string[] { colMDType, colFullName, colMDUid, colMDName, colUidName };
                HideColumns(gridControlNamespaces, GridNamespacesHideColumnNames);

                string colName = Nameof<CustomDocEl>.Property(ee => ee.Name);
                string[] GridDocsHideColumnNames = new string[] { colName, colMDUid, colMDName, colUidName };
                HideColumns(gridControlCustomDocs, GridDocsHideColumnNames);

            }

            FocusColumn(gridControlClasses, col1);
            FocusColumn(gridControlMembers, col1);
            FocusColumn(gridControlNamespaces, col1);
            FocusColumn(gridControlCustomDocs, col1);

        }

        private void InitRibbonAfterLoad() {
            Font fDefault = new Font(WindowsFormsSettings.DefaultFont.FontFamily, 8.25f, WindowsFormsSettings.DefaultFont.Style);
            Size szDefault = TextRenderer.MeasureText(" Namespaces ", fDefault);
            Size sz = TextRenderer.MeasureText(" Namespaces ", WindowsFormsSettings.DefaultFont);
            float fontRatio = (float)(sz.Width) / (float)(szDefault.Width);

            barCheckItemNamespaces.LargeWidth = sz.Width + 20;
            barCheckItemMembers.LargeWidth = (int)(50 * fontRatio) + 20;
            barCheckItemClasses.LargeWidth = (int)(50 * fontRatio) + 20;
            barCheckItemDocs.LargeWidth = (int)(50 * fontRatio) + 20;

            bciAutoMinimize.Checked = Parameters.AutoMinimize;


            ribbonPageGroupServiceMode.Visible = Parameters.ServiceMode;

            barCheckItemMembers.Hint = hotKeys.HotKeyMember.ToString();
            barCheckItemClasses.Hint = hotKeys.HotKeyClass.ToString();
            barCheckItemNamespaces.Hint = hotKeys.HotKeyNamespace.ToString();
            barCheckItemDocs.Hint = hotKeys.HotKeyDoc.ToString();

        }

        


        string currentLayoutVersion = "v1.0";

        private void InitGridAfterLoad(GridControl grid) {
            GridView view = grid.MainView as GridView;

            view.OptionsBehavior.Editable = false;
            view.OptionsBehavior.AllowIncrementalSearch = true;

            view.OptionsSelection.MultiSelect = true;
            view.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;

            //WindowsFormsSettings.FilterCriteriaDisplayStyle = FilterCriteriaDisplayStyle.Visual;
            view.OptionsView.FilterCriteriaDisplayStyle = FilterCriteriaDisplayStyle.Visual;

            //view.OptionsFilter.MRUFilterListCount = 5;
            //view.OptionsFilter.MRUFilterListPopupCount = 5;

            view.OptionsView.ShowFooter = true;

            view.OptionsView.ShowViewCaption = true;
        }




        private void InitGridBeforeRestoreLayout(GridControl grid) {
            GridView view = grid.MainView as GridView;
            
            //Unsubscribe from events first
            view.KeyDown -= gridView1_KeyDown;
            view.KeyDown += gridView1_KeyDown;
            view.DoubleClick -= gridView1_DoubleClick;
            view.DoubleClick += gridView1_DoubleClick;
            
            view.OptionsLayout.LayoutVersion = currentLayoutVersion;
            view.BeforeLoadLayout += View_BeforeLoadLayout;

            view.CustomDrawEmptyForeground += View_CustomDrawEmptyForeground;

            view.PopupMenuShowing += View_PopupMenuShowing;
            
        }

        private void View_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) {
            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row) {
                int rowHandle = e.HitInfo.RowHandle;
                e.Menu.Items.Clear();
                DXMenuItem item = CreateOpenFileMenuItem(view, rowHandle);
                
                e.Menu.Items.Add(item);
            }
        }

        DXMenuItem CreateOpenFileMenuItem(GridView view, int rowHandle) {
            DXMenuItem menuItem = new DXMenuItem("&Open File", new EventHandler(OnOpenFileItemClick));
            
            menuItem.Tag = view.GetRow(rowHandle);
            BaseElement el = menuItem.Tag as BaseElement;
            menuItem.Enabled = (el != null && el.FilePath != null);

            return menuItem;
        }

        void OnOpenFileItemClick(object sender, EventArgs e) {
            DXMenuItem item = sender as DXMenuItem;
            BaseElement el = item.Tag as BaseElement;
            if (el != null) {
                Process.Start(el.FilePath);
            }
        }

        private void View_CustomDrawEmptyForeground(object sender, DevExpress.XtraGrid.Views.Base.CustomDrawEventArgs e) {

            string s;
            GridView view = sender as GridView;
            // Get the number of records in the underlying data source.
            IEnumerable<BaseElement> dataSource = view.GridControl.DataSource as IEnumerable<BaseElement>;
            //List<A> listOfA = new List<B>().ConvertAll(x => (A)x);
            //= view.GridControl.DataSource as List<BaseElement>;

            string uiName = "";
            if (AvailReps != null && AvailReps.CurrentUIWorkspace != null)
                uiName = AvailReps.CurrentUIWorkspace.CurrentUIName;


            if (dataSource == null || dataSource.Count() == 0) {
                if (AvailReps.IsLoadingData)
                    s = "Loading...";
                else
                    s = "No " + uiName + " in the current repository";
            }
            else //no records meet the filter criteria
                s = "Clear the filter to show records";
            Font font = new Font("Tahoma", 12, FontStyle.Bold);
            Rectangle r = new Rectangle(e.Bounds.Left + 5, e.Bounds.Top + 5, e.Bounds.Width - 5,
              e.Bounds.Height - 5);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(s, font, Brushes.DarkGray, r, sf);


        }

        bool GridLayoutExistsAndItWasLoaded = false;

        private void View_BeforeLoadLayout(object sender, DevExpress.Utils.LayoutAllowEventArgs e) {
            if (e.PreviousVersion != currentLayoutVersion)
                e.Allow = false;
            else {
                //позволять загружать лэйаут
                GridLayoutExistsAndItWasLoaded = true;
            }

        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {
            
           


            //if (hotKeyMember != null && hotKeyMember.Registered) {
            //    hotKeyMember.Unregister();
            //}
            hotKeys.UnregisterHotKey(WorkspaceKind.Members);
            hotKeys.UnregisterHotKey(WorkspaceKind.Classes);
            hotKeys.UnregisterHotKey(WorkspaceKind.Namespaces);
            hotKeys.UnregisterHotKey(WorkspaceKind.Docs);



            if (AvailReps != null && AvailReps.CurrentUIWorkspace != null)
                Properties.Settings.Default.RepPath = AvailReps.CurrentUIWorkspace.FullFolderName;

            Properties.Settings.Default.ShortcutMember = Parameters.HotkeyMemberString;
            Properties.Settings.Default.ShortcutClass = Parameters.HotkeyClassString;
            Properties.Settings.Default.ShortcutNamespace = Parameters.HotkeyNamespacesString;
            Properties.Settings.Default.ShortcutDoc = Parameters.HotkeyDocString;

            Properties.Settings.Default.SkinName = DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName;

            var cvtSize = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size));
            var cvtLocation = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point));
            string sSize = cvtSize.ConvertToString(FormSize);
            string sLocation = cvtLocation.ConvertToString(FormLocation);
            Properties.Settings.Default.FormLocation = sLocation;
            Properties.Settings.Default.FormSize = sSize;

            var cvt = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));
            string fontStr = cvt.ConvertToString(WindowsFormsSettings.DefaultFont);
            Properties.Settings.Default.DefaultFont = fontStr;

            Properties.Settings.Default.AutoMinimize = Parameters.AutoMinimize;
            Properties.Settings.Default.ServiceMode = Parameters.ServiceMode ;


            Properties.Settings.Default.Save();
        }



        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        bool WaitForForegroundWindow(IntPtr waitForHWnd) {
            
            int maxCount = 15;
            int ctr = 0;
            bool isTargetForegroundWindow; 
            bool exit= GetForegroundWindow() == waitForHWnd;
            isTargetForegroundWindow = exit;

            while (!exit) {
                SetForegroundWindow(waitForHWnd);
                Thread.Sleep(40);
                isTargetForegroundWindow = GetForegroundWindow() == waitForHWnd;
                bool maxCountReached = (ctr++) > maxCount;
                exit = isTargetForegroundWindow || maxCountReached;
            }
            return isTargetForegroundWindow;
        }

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr handle);

        [DllImport("user32.dll")]
        public static extern bool IsZoomed(IntPtr handle);

        bool ShowAndSetForegroundWindow(IntPtr hWnd) {
            if (IsIconic(hWnd))
                ShowWindow(hWnd, 9); //Restore from minimized state
            else {
                if (IsZoomed(hWnd))
                    ShowWindow(hWnd, 3); // Maximize the window
                else
                    ShowWindow(hWnd, 5); // Normal
            }
            bool res = SetForegroundWindow(hWnd);
            return res;
        }


        public enum CopyOrPasteLink { CopyPaste, Copy}

        public CopyOrPasteLink CopyOrPasteLinkMode;
        public string SelectedLink = "";

        public string ReadyToSendSelectedLink {
            get {
                if (SelectedLink == "") {
                    return "";
                }
                string res = "";
                char[] specialChars = new char[] { '+', '^', '%', '~', '(', ')', '[', ']', '{', '}' };
                foreach (char c in SelectedLink) {
                    if (specialChars.Contains(c))
                        res += "{" + c + "}";
                    else
                        res += c;
                }
                return res;
            }
        }




        private void gridView1_KeyDown(object sender, KeyEventArgs e) {
            GridView view = sender as GridView;
            if (e.KeyCode == Keys.Enter && view.IsDataRow(view.FocusedRowHandle))
                SelectRow(view, view.FocusedRowHandle);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowTitle(IntPtr hWnd) {
            if (hWnd == IntPtr.Zero)
                return null;
            var length = GetWindowTextLength(hWnd)+1;
            var title = new StringBuilder(length);
            GetWindowText(hWnd, title, length);
            return title.ToString();
        }



        public const int WM_PASTE = 0x0302;

        public bool Locked { get; set; }

        private void SelectRow(GridView view, int focusedRowHandle) {
            //MessageBox.Show("Row selected");
            BaseElement elem = view.GetRow(focusedRowHandle) as BaseElement;
            if (elem == null) {
                return;
                //throw new Exception("something's wrong");
            }

            SelectedLink = "";


            if (IsCalledFromExternalApplication)
                CopyOrPasteLinkMode = CopyOrPasteLink.CopyPaste;
            else
                CopyOrPasteLinkMode = CopyOrPasteLink.Copy;


            int previousWindowProcessId = -1;
            if(IsCalledFromExternalApplication) {

                GetWindowThreadProcessId(PreviousForegroundWindow, out previousWindowProcessId);

            }
            string processName = "";
            if(previousWindowProcessId != -1) {
                //get process name
                Process pr= Process.GetProcessById(previousWindowProcessId);
                processName = pr.MainWindowTitle;
            }


            processName = GetWindowTitle(PreviousForegroundWindow);


            SelectLinkForm f = new SelectLinkForm(this, elem, processName);

            Locked = true;
            DialogResult dRes = f.ShowDialog(this);
            Locked = false;

            if (dRes != DialogResult.Cancel) {

                //Send keys to form
                if (SelectedLink != "") {
                    //copy paste
                    //IDataObject oldval = Clipboard.GetDataObject();

                    Dictionary<String, Object> dic = new Dictionary<String, Object>();
                    IDataObject ido = Clipboard.GetDataObject();
                    string[] excludeFormats = new string[] { "EnhancedMetafile" };
                    string[] clipboardFormats = ido.GetFormats(false).Except(excludeFormats).ToArray();

                    bool canRestoreClipboardDataObject = ShouldRestoreClipboardSetting;
                    try {
                        foreach (String s in clipboardFormats)
                            dic.Add(s, ido.GetData(s));
                        
                    }
                    catch {
                        canRestoreClipboardDataObject = false;
                    }


                    string oldText = "";
                    bool canRestoreClipboardText = ShouldRestoreClipboardSetting;

                    try {
                        oldText = Clipboard.GetText();
                    }
                    catch {
                        canRestoreClipboardText = false;
                    }
                    Clipboard.SetText(SelectedLink, TextDataFormat.UnicodeText);


                    IntPtr CurrentPreviousForegroundWindow = PreviousForegroundWindow;

                    //Locked = true; //todo visible
                    if (IsCalledFromExternalApplication) {
                        MyLogger.Log("PRIOR-TO-SetForegroundWindow -- PreviousForegroundWindow=" + CurrentPreviousForegroundWindow);

                        //Previously, the code below was just a call to SetForegroundWindow
                        bool resSetForegroundWindow = ShowAndSetForegroundWindow(CurrentPreviousForegroundWindow);

                        if (!resSetForegroundWindow) {
                            MyLogger.Log("SetForegroundWindow returns False");

                        }
                        else {
                            MyLogger.Log("SetForegroundWindow TRUE");

                        }
                        MyLogger.Log("PreviousForegroundWindow=" + CurrentPreviousForegroundWindow);
                        MyLogger.Log("GetForegroundWindow=" + GetForegroundWindow());



                        bool res = WaitForForegroundWindow(CurrentPreviousForegroundWindow);
                        if(!res) {
                            MyLogger.Log("WaitForForegroundWindow=FALSE; foreground window not focused");
                        }
                            
                    }

                    Application.DoEvents(); //todo visible

                    

                    //Locked = false; //todo visible

                    if (CopyOrPasteLinkMode == CopyOrPasteLink.CopyPaste) {

                        bool isReallySetForegroundWindow = WaitForForegroundWindow(CurrentPreviousForegroundWindow);//again

                        
                        if (isReallySetForegroundWindow) {
                            MyLogger.Log("Pasting Text");

                            //SendMessage(CurrentPreviousForegroundWindow, WM_PASTE, 0, 0);

                            switch(PasteMode) {
                                case PasteTextMode.SendKeys:
                                    SendKeys.Send("^v");
                                    MyLogger.Log("Sent Ctrl+V via SendKeys");
                                    break;
                                case PasteTextMode.SendInput:
                                    Keyboard.SendCtrlV();
                                    MyLogger.Log("Sent Ctrl+V via SendInput");
                                    break;
                                case PasteTextMode.keybd_event:
                                    Keyboard2.SendCtrlV();
                                    MyLogger.Log("Sent Ctrl+V via keybd_event");
                                    break;

                            }

                            //if (DelayAfterPaste > 0) {
                                MyLogger.Log("Sleep for " + DelayAfterPaste + " ms");
                                Thread.Sleep(DelayAfterPaste);
                            //}

                        }
                        else {
                            MyLogger.Log("WaitForForegroundWindow= FALSE:  NOT PASTING TEXT");
                        }
                        bool clipboardDataObjectRestored = false;
                        try {

                            if (canRestoreClipboardDataObject) {
                                // restore
                                var dataObj = new DataObject();
                                foreach (String s in dic.Keys)
                                    dataObj.SetData(s, dic[s]);
                                Clipboard.SetDataObject(dataObj);
                                clipboardDataObjectRestored = true;
                            }

                        }
                        catch {

                        }

                        try {
                            if (!clipboardDataObjectRestored && canRestoreClipboardText)
                                if (!string.IsNullOrEmpty(oldText))
                                    Clipboard.SetText(oldText);
                        }
                        catch {

                        }

                    }


                    if (IsCalledFromExternalApplication && Parameters.AutoMinimize)
                        this.WindowState = FormWindowState.Minimized;

                    
                }
            }
            else {
                //if (IsCalledFromExternalApplication)
                //    this.WindowState = FormWindowState.Minimized;
            }


            //ClearPreviousForegroundWindow();
        }

        private void Form1_Deactivate(object sender, EventArgs e) {
            MyLogger.Log("Deactivate: Locked=" + Locked);

            if (IsApplicationActive()) {
                MyLogger.Log("Deactivate: IsApplicationActive=True");

            }
            else {
                //if (Locked) //todo visible
                //    return;
                //deactivate:
                if (IsCalledFromExternalApplication) {
                    MyLogger.Log("Deactivate: ClearPreviousForegroundWindow");
                    MyLogger.Log("Deactivate: WindowState = FormWindowState.Minimized");


                    ClearPreviousForegroundWindow();

                    if(Parameters.AutoMinimize)
                        this.WindowState = FormWindowState.Minimized;

                    //MessageBox.Show("You are in the Form.Deactivate event.");
                }
            }
        }


        public void ClearPreviousForegroundWindow() {
            PreviousForegroundWindow = (IntPtr)0;
        }

        public bool IsCalledFromExternalApplication {
            get {
                return PreviousForegroundWindow != (IntPtr)0;
            }
        }

        private void gridView1_DoubleClick(object sender, EventArgs e) {
            GridView view = sender as GridView;
            Point pt = view.GridControl.PointToClient(Cursor.Position);
            var hi = view.CalcHitInfo(pt);
            if (hi.InDataRow) {
                SelectRow(view, hi.RowHandle);
            }
        }



        private void barCheckItemMembers_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            BarItemCheckedChanged(e.Item as BarCheckItem);
        }

        private void barCheckItemClasses_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            BarItemCheckedChanged(e.Item as BarCheckItem);
        }

        private void barCheckItemNamespaces_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            BarItemCheckedChanged(e.Item as BarCheckItem);
        }

        private void barCheckItemDocs_CheckedChanged(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            BarItemCheckedChanged(e.Item as BarCheckItem);
        }

        void BarItemCheckedChanged(BarCheckItem item) {
            if (!item.Checked)
                return;
            string UIName = item.Tag as string;
            if (UIName != null && AvailReps.CurrentUIWorkspace.CurrentUIName != UIName) {
                AvailReps.ShowUI(UIName);
                AvailReps.UpdateFormCaption();
                //if(AvailReps.CurrentUIWorkspace != null)
                //    propertyGridControl1.SelectedObject = AvailReps.CurrentUIWorkspace.CurrentGridView;
            }
        }

        private void btnRefresh_ItemClick(object sender, ItemClickEventArgs e) {
            AvailReps.ReloadCurrentWorkspace();
        }

        public enum NotifyIconStatus { busy, ready}


        internal void SetNotifyIconStatus(NotifyIconStatus status, string s) {
            try {
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                if (status == NotifyIconStatus.busy)
                    notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
                if (status == NotifyIconStatus.ready)
                    notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("Form1.IconOptions.Icon")));
                if (s.Length > 63)
                    notifyIcon1.Text = s.Substring(0, 63);
                else
                    notifyIcon1.Text = s;
            }
            catch {

            }
        }

        private void Form1_Resize(object sender, EventArgs e) {
            MyLogger.Log("Form.Resize - WindowState=" + this.WindowState + " (if Minimized, the form is auto hidden)");
            if (this.WindowState == FormWindowState.Minimized) {
                Hide();
                notifyIcon1.Visible = true;
            }
            else {
                FormSize = this.Size;
                FormLocation = this.Location;
            }
        }

        Point FormLocation;
        Size FormSize;

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            //ShowFormFromNotifyIcon();
            //notifyIcon1_MouseClick(sender, e);
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e) {
            MyLogger.Log("NotifyIconClick WindowState=" + this.WindowState + " (if Minimized, the form is ShowFormFromNotifyIcon)");

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle) {
                if (this.WindowState == FormWindowState.Minimized)
                    ShowFormFromNotifyIcon();
                else
                    this.Close();
            }
        }

        internal void EnableOrDisableControls(bool enable) {
            //beiLWorkspaceSelector.Enabled = enable;
            //barCheckItemMembers.Enabled = enable;
            //barCheckItemNamespaces.Enabled = enable;
            //barCheckItemDocs.Enabled = enable;
            //barCheckItemClasses.Enabled = enable;
            //btnRefresh.Enabled = enable;
            ribbonControl1.Enabled = enable;


            gridControlMembers.Enabled = enable;
            gridControlClasses.Enabled = enable;
            gridControlNamespaces.Enabled = enable;
            gridControlCustomDocs.Enabled = enable;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            this.WindowState = FormWindowState.Minimized;
            e.Cancel = !NeedCloseApp;
            if(NeedCloseApp) {
                (gridControlClasses.MainView as ColumnView).FindFilterText = "";
                (gridControlCustomDocs.MainView as ColumnView).FindFilterText = "";
                (gridControlMembers.MainView as ColumnView).FindFilterText = "";
                (gridControlNamespaces.MainView as ColumnView).FindFilterText = "";
            }
        }



        private void btnExitApp_ItemClick(object sender, ItemClickEventArgs e) {
            NeedCloseApp = true;
            this.Close();
        }

        private void btnHideForm_ItemClick(object sender, ItemClickEventArgs e) {
            this.Close();
        }

        private void officeNavigationBar1_QueryPeekFormContent(object sender, DevExpress.XtraBars.Navigation.QueryPeekFormContentEventArgs e) {
            
        }

        private void officeNavigationBar1_ItemClick(object sender, DevExpress.XtraBars.Navigation.NavigationBarItemEventArgs e) {
            if (e.Item != navItemGridSettings)
                return;
            flyoutPanel1.ShowBeakForm(Cursor.Position, immediate: true);
            flyoutPanel1.Focus();
        }



        private void bciAutoMinimize_CheckedChanged(object sender, ItemClickEventArgs e) {
            BarCheckItem checkItem = e.Item as BarCheckItem;
            if (checkItem == null)
                return;
            Parameters.AutoMinimize = checkItem.Checked;
        }
    }




    public class MyHotKeys {
        public MyHotKeys(Form f) {
            form = f;


            HotKeyMember = new Hotkey();
            HotKeyClass = new Hotkey();
            HotKeyNamespace = new Hotkey();
            HotKeyDoc = new Hotkey();

            dic = new Dictionary<WorkspaceKind, Hotkey>();
            dic.Add(WorkspaceKind.Members, HotKeyMember);
            dic.Add(WorkspaceKind.Classes, HotKeyClass);
            dic.Add(WorkspaceKind.Namespaces, HotKeyNamespace);
            dic.Add(WorkspaceKind.Docs, HotKeyDoc);

            HotKeyMember.Pressed += HotKeyMember_Pressed;
            HotKeyClass.Pressed += HotKeyClass_Pressed;
            HotKeyNamespace.Pressed += HotKeyNamespace_Pressed;
            HotKeyDoc.Pressed += HotKeyDoc_Pressed;

        }

        Form form = null;

        void RaiseHotKeyPressed(WorkspaceKind kind) {
            if (HotKeyPressed != null)
                HotKeyPressed(kind);
        }

        private void HotKeyDoc_Pressed(object sender, HandledEventArgs e) {
            RaiseHotKeyPressed(WorkspaceKind.Docs);
        }

        private void HotKeyNamespace_Pressed(object sender, HandledEventArgs e) {
            RaiseHotKeyPressed(WorkspaceKind.Namespaces);
        }

        private void HotKeyClass_Pressed(object sender, HandledEventArgs e) {
            RaiseHotKeyPressed(WorkspaceKind.Classes);
        }

        private void HotKeyMember_Pressed(object sender, HandledEventArgs e) {
            RaiseHotKeyPressed(WorkspaceKind.Members);
        }

        public delegate void HotKeyPressedEventHandler(WorkspaceKind hotKey);
        public event HotKeyPressedEventHandler HotKeyPressed;




        public Hotkey HotKeyMember, HotKeyClass, HotKeyNamespace, HotKeyDoc;





        Dictionary<WorkspaceKind, Hotkey> dic;

        public bool RegisterHotKey(WorkspaceKind hkKind, string hotKeyString) {
            Hotkey temp = new Hotkey(hotKeyString);
            if (temp.IsValidFromString) {
                return RegisterHotKey(hkKind, temp.Control, temp.Alt, temp.Shift, temp.Windows, temp.KeyCode);
            }
            return false;
        }

        public bool RegisterHotKey(WorkspaceKind hkKind, bool Control, bool Alt, bool Shift, bool Win, System.Windows.Forms.Keys key) {
            Hotkey temp = new Hotkey(key, Shift, Control, Alt, Win);
            bool canRegister = temp.GetCanRegister(form);
            if (!canRegister)
                return false;

            //Unregister the current HotKey
            Hotkey hk = dic[hkKind];

            UnregisterHotKey(hkKind);
            //Register the current HotKeyMember
            hk.Windows = Win;
            hk.Alt = Alt;
            hk.Control = Control;
            hk.Shift = Shift;
            hk.KeyCode = key;
            hk.Register(form);
            return true;
        }


        public void UnregisterHotKey(WorkspaceKind hkKind) {
            Hotkey hk = dic[hkKind];
            if (hk != null && hk.Registered) {
                hk.Unregister();
            }
        }


    }


    public class AvailReps {


        public Form1 Form11;

        GridControl GridMembers, GridClasses, GridNamespaces, GridDocs;
        BarCheckItem ChItemMembers, ChItemClasses, ChItemNamespaces, ChItemDocs;

        public AvailReps(Form1 f, string parentFolder,
            GridControl gridMembers, GridControl gridClasses, GridControl gridNamespaces, GridControl gridDocs,
            BarCheckItem chItemMembers, BarCheckItem chItemClasses, BarCheckItem chItemNamespaces, BarCheckItem chItemDocs,
            RepositoryItemLookUpEdit riLWorkspaceSelector,
            BarEditItem barEditItemWorkspaceSelector
            ) {
            this.Form11 = f;
            this.GridClasses = gridClasses;
            this.GridDocs = gridDocs;
            this.GridMembers = gridMembers;
            this.GridNamespaces = gridNamespaces;

            this.ChItemClasses = chItemClasses;
            this.ChItemDocs = chItemDocs;
            this.ChItemMembers = chItemMembers;
            this.ChItemNamespaces = chItemNamespaces;


            AllWorkspaces = new Dictionary<string, RepUIWorkspace>();

            AllRepsFolder = parentFolder;

            RepSelectorLookup = riLWorkspaceSelector;
            RepSelectorBarEditItem = barEditItemWorkspaceSelector;
            RepSelectorBarEditItem.EditValue = null;


            RepSelectorLookup.DataSource = new List<ChildFolder>();


            DirectoryInfo di = new DirectoryInfo(AllRepsFolder);
            DirectoryInfo[] childDirInfos;
            try {
                childDirInfos = di.GetDirectories();
            }
            catch {
                return;
            }
            ChildFolders = new List<ChildFolder>();
            foreach (var cdi in childDirInfos) {
                try {
                    FileInfo[] fInfos = cdi.GetFiles("docascode.json");

                    if (fInfos.Length > 0)
                        ChildFolders.Add(new ChildFolder(cdi));
                }
                catch {

                }
            }


            PopulateLookupWithAvailReps();

            //RepSelectorImageComboRI.EditValueChanged += ImageComboBoxRepSelector_EditValueChanged;
            barEditItemWorkspaceSelector.EditValueChanged += BarEditItemWorkspaceSelector_EditValueChanged;



        }

        public bool IsLoadingData = false;

        

        IOverlaySplashScreenHandle OverlaySplashScreenHandle = null;


        public bool NeedReload { get; set; }

        private void BarEditItemWorkspaceSelector_EditValueChanged(object sender, EventArgs e) {


            OverlayWindowOptions op = new OverlayWindowOptions();
            OverlaySplashScreenHandle = SplashScreenManager.ShowOverlayForm(Form11, op);
            Application.DoEvents();


            try {

                BarEditItem item = sender as BarEditItem;
                ChildFolder chFolder = item.EditValue as ChildFolder;

                if (chFolder == null) {
                
                    return;
                }
                else {
                    Form11.labelWarningNoFolder.Visible = false;

                }


                IsLoadingData = true;
                Form11.EnableOrDisableControls(!IsLoadingData);

                
                

                string prevUIName = Settings.WorkspaceNames[WorkspaceKind.Members]; //Выбор Members
                bool NeedSelectDocsIfNoMembers = false;
                if (CurrentUIWorkspace != null)
                    prevUIName = CurrentUIWorkspace.CurrentUIName;
                else
                    NeedSelectDocsIfNoMembers = true;


                if (NeedReload) {
                    CurrentUIWorkspace = ReloadAndGetWorkspace(chFolder.FullName);
                    NeedReload = false;
                }
                else {
                    CurrentUIWorkspace = GetWorkspace(chFolder.FullName, true);
                }

                // По умлчанию выбирается предыдущий UI (members, classes или ...)

                if (NeedSelectDocsIfNoMembers && CurrentUIWorkspace.IsMembersGridEmpty()) {
                    string docsUIName = Settings.WorkspaceNames[WorkspaceKind.Docs];
                    ShowUI(docsUIName);
                }
                else
                    ShowUI(prevUIName);

                UpdateFormCaption();

                IsLoadingData = false;

                Form11.EnableOrDisableControls(!IsLoadingData);

                this.CurrentUIWorkspace.CurrentGrid.Focus();

            }
            finally {
                try {
                    //this.Form11.SplashScreenManager1.CloseWaitForm();

                    SplashScreenManager.CloseOverlayForm(OverlaySplashScreenHandle);
                    //SplashScreenManager.CloseForm(false);

                    ////CloseProgressPanel(handle);
                }
                catch {

                }
            }


        }

        public void ShowUI(string UIName) {
            if (CurrentUIWorkspace == null)
                return;
            CurrentUIWorkspace.ShowUICore(UIName);
            Form11.propertyGridControl1.SelectedObject = CurrentUIWorkspace.CurrentGridView;
        }

        public void UpdateFormCaption() {
            Form11.Text = CurrentUIWorkspace.ShortFolderName + " - " + CurrentUIWorkspace.CurrentUIName +  " - " + Form1.ProgName + Form1.ProgVersion;
        }


        public void ClearFormCaption(string s) {
            Form11.Text = s + " - " + Form1.ProgName + Form1.ProgVersion;
        }

        RepUIWorkspace coreCurrentUIWorkspace = null;
        public RepUIWorkspace CurrentUIWorkspace {
            get {
                return coreCurrentUIWorkspace;
            }
            private set {
                if (value != coreCurrentUIWorkspace) {
                    coreCurrentUIWorkspace = value;
                    foreach (var uiControls in coreCurrentUIWorkspace.UIControlsDictionary) {
                        uiControls.Value.Grid.DataSource = uiControls.Value.DataSource;
                    }
                }
            }
        }

        public string AllRepsFolder {
            get; private set;
        }
        

        Dictionary<string, RepUIWorkspace> AllWorkspaces;


        private RepUIWorkspace ReloadAndGetWorkspace(string fullChildFolderName) {
            if (AllWorkspaces.Keys.Contains(fullChildFolderName))
                AllWorkspaces.Remove(fullChildFolderName);
            return GetWorkspace(fullChildFolderName, true);
            
        }


        private RepUIWorkspace GetWorkspace(string fullChildFolderName, bool aboutToChangeWorkspace) {
            if (AllWorkspaces.Keys.Contains(fullChildFolderName)) //todo - caseinsensitive compare...
                return AllWorkspaces[fullChildFolderName];
            if (aboutToChangeWorkspace) {
                ClearFormCaption("Loading " + fullChildFolderName + "...");
                //clear the current grid's data source
                if (CurrentUIWorkspace != null)
                    foreach (var uiControls in CurrentUIWorkspace.UIControlsDictionary) {
                        uiControls.Value.Grid.DataSource = null;
                        uiControls.Value.Grid.Refresh();
                    }
            }
            RepUIWorkspace wsp = new RepUIWorkspace(fullChildFolderName);


            List<MemberEl> MemberDS = new List<MemberEl>();
            List<ClassEl> ClassDS = new List<ClassEl>();
            List<NamespaceEl> NamespaceDS = new List<NamespaceEl>();
            List<CustomDocEl> DocDS = new List<CustomDocEl>();

            RepUIControls ClassControlsUI = new RepUIControls(Settings.WorkspaceNames[WorkspaceKind.Classes], GridClasses, ClassDS, ChItemClasses);
            RepUIControls MemberControlsUI = new RepUIControls(Settings.WorkspaceNames[WorkspaceKind.Members], GridMembers, MemberDS, ChItemMembers);
            RepUIControls NamespaceControlsUI = new RepUIControls(Settings.WorkspaceNames[WorkspaceKind.Namespaces], GridNamespaces, NamespaceDS, ChItemNamespaces);
            RepUIControls DocsControlsUI = new RepUIControls(Settings.WorkspaceNames[WorkspaceKind.Docs], GridDocs, DocDS, ChItemDocs);

            wsp.AddUI(ClassControlsUI);
            wsp.AddUI(MemberControlsUI);
            wsp.AddUI(NamespaceControlsUI);
            wsp.AddUI(DocsControlsUI);


            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //Stopwatch stopwatch = Stopwatch.StartNew();

            string s = Form1.ProgName + " - Loading...";
            Form11.SetNotifyIconStatus(Form1.NotifyIconStatus.busy, s);
            Application.DoEvents();


            Rep rep1 = new Rep(wsp.FullFolderName, Form11);

            rep1.LoadApi(MemberDS, ClassDS, NamespaceDS, ReportProgress);
            
            rep1.LoadCustomDocs(DocDS, ReportProgress);

            s = Form1.ProgName  + " - " + rep1.ProjectName + " - Ready";
            Form11.SetNotifyIconStatus(Form1.NotifyIconStatus.ready, s);

            //stopwatch.Stop();
            //MyLogger.Log("Time elapsed " + stopwatch.ElapsedMilliseconds);

            Application.DoEvents();

            AllWorkspaces.Add(fullChildFolderName, wsp);
            return wsp;
        }


        public void ReportProgress(object sender, string text) {
            text = Form1.ProgName + " - " + text;
            Form11.SetNotifyIconStatus(Form1.NotifyIconStatus.busy, text);
            if (SplashScreenManager.Default != null) {
                try {
                    //    Form11.SplashScreenManager1.SetWaitFormCaption(text);
                    SplashScreenManager.Default.SetWaitFormDescription(text);
                }
                catch {
                    //
                    //}

                }
            }
        }



        RepositoryItemLookUpEdit RepSelectorLookup;
        BarEditItem RepSelectorBarEditItem;

        public List<ChildFolder> ChildFolders {
            get; private set;
        }

        public static string GetParentFolder(string childFolder) {
            DirectoryInfo di = new DirectoryInfo(childFolder);
            return di.Parent.FullName;
        }

        public void PopulateLookupWithAvailReps() {
            List<ChildFolder> dataSource = new List<ChildFolder>();
            RepSelectorLookup.DataSource = dataSource;

            dataSource.AddRange(ChildFolders);

            //foreach(ChildFolder child in ChildFolders) {
            //    RepSelectorImageComboRI.Items.Add(child.ShortName, child, -1);
            //    //RepSelectorImageComboRI.Items.Add(child);
            //}
        }



        public bool SelectWorkspace(string fullChildFolderName) {
            bool found = false;
            List<ChildFolder> dataSource = RepSelectorLookup.DataSource as List<ChildFolder>;
            foreach (ChildFolder chF in dataSource) {
                if (chF.FullName.Equals(fullChildFolderName, StringComparison.CurrentCultureIgnoreCase)) {
                    RepSelectorBarEditItem.EditValue = chF;
                    found = true;
                    break;
                }
            }
            return found;
        }

        internal void ReloadCurrentWorkspace() {
            NeedReload = true;
            BarEditItemWorkspaceSelector_EditValueChanged(RepSelectorBarEditItem, new EventArgs());
        }
    }


    public class ChildFolder {
        public ChildFolder(DirectoryInfo di) {
            FullName = di.FullName;
            ShortName = di.Name;
            if (FullName.EndsWith("\\"))
                FullName = FullName.Substring(0, FullName.Length - 1);
        }
        public string FullName;
        public string ShortName;
        public override string ToString() {
            return ShortName;
        }

        public override bool Equals(object obj) {
            ChildFolder cf = obj as ChildFolder;
            if (cf == null)
                return false;
            return this.FullName == cf.FullName;
        }


    }


    public class RepUIWorkspace {



        string RepName;

        public string FullFolderName {
            get; private set;
        }

        string coreShortFolderName = null;
        public string ShortFolderName {
            get {
                if (coreShortFolderName != null)
                    return coreShortFolderName;
                DirectoryInfo di = new DirectoryInfo(FullFolderName);
                coreShortFolderName = di.Name;
                return coreShortFolderName;
            }
        }

        public RepUIWorkspace(string fullFolderName) {

            FullFolderName = fullFolderName;
            RepName = ShortFolderName;
        }

        public Dictionary<string, RepUIControls> UIControlsDictionary = new Dictionary<string, RepUIControls>();

        public string CurrentUIName {
            get; private set;
        }
        public GridControl CurrentGrid {
            get; private set;
        }
        public GridView CurrentGridView {
            get; private set;
        }
        public BarCheckItem CurrentBarCheckItem {
            get; private set;
        }

        public void AddUI(RepUIControls controls) {
            UIControlsDictionary.Add(controls.UIName, controls);



        }



        public bool IsMembersGridEmpty() {
            string membersUIName = Settings.WorkspaceNames[WorkspaceKind.Members];

            foreach (var dicItem in UIControlsDictionary) {
                GridControl grid = dicItem.Value.Grid;
                if (dicItem.Key.Equals(membersUIName, StringComparison.CurrentCultureIgnoreCase)) {
                    return (grid.MainView as ColumnView).DataRowCount == 0;
                }
            }
            return false;
        }



        public void ShowUICore(string UIName) {
            
            foreach (var dicItem in UIControlsDictionary) {
                GridControl grid = dicItem.Value.Grid;

                if (dicItem.Key.Equals(UIName, StringComparison.CurrentCultureIgnoreCase)) {

                    grid.Visible = true;

                    grid.BringToFront();
                    grid.Dock = DockStyle.Fill;

                    grid.RefreshDataSource();

                    CurrentUIName = UIName;
                    CurrentGrid = grid;
                    CurrentGridView = grid.MainView as GridView;
                    CurrentBarCheckItem = dicItem.Value.BarCheckItem;

                    CurrentBarCheckItem.Checked = true;


                }

            }


            foreach (var dicItem in UIControlsDictionary) {
                GridControl grid = dicItem.Value.Grid;

                if (!dicItem.Key.Equals(UIName, StringComparison.CurrentCultureIgnoreCase)) {
                    grid.Visible = false;
                }
            }




        }

    }


    public class RepUIControls {
        public RepUIControls(string uIName, GridControl grid, object dataSource, BarCheckItem checkItem) {
            UIName = uIName;
            Grid = grid;
            DataSource = dataSource;
            BarCheckItem = checkItem;

            //Grid.DataSource = dataSource;
            BarCheckItem.Tag = uIName;
        }

        public string UIName;
        public GridControl Grid;
        public object DataSource;
        public BarCheckItem BarCheckItem;

    }


    //static class ProcessWatcher {
    //    public static void StartWatch() {
    //        _timer = new Timer();
    //        _timer.Interval = 100;
    //        _timer.Tick += _timer_Tick;
    //        _timer.Start();
    //    }

    //    private static void _timer_Tick(object sender, EventArgs e) {
    //        setLastActive();
    //    }


    //    [DllImport("user32.dll")]
    //    static extern IntPtr GetForegroundWindow();

    //    public static IntPtr LastHandle {
    //        get {
    //            return _previousToLastHandle;
    //        }
    //    }

    //    private static void setLastActive() {
    //        IntPtr currentHandle = GetForegroundWindow();
    //        if (currentHandle != _previousHandle) {
    //            _previousToLastHandle = _previousHandle;
    //            _previousHandle = currentHandle;
    //        }
    //    }

    //    private static Timer _timer;
    //    private static IntPtr _previousHandle = IntPtr.Zero;
    //    private static IntPtr _previousToLastHandle = IntPtr.Zero;
    //}


    public class Rep {

        const string jSonFileName = "docascode.json";

        public Rep(string _folder, Form1 owner) {
            if (_folder.EndsWith(@"\"))
                _folder = _folder.Substring(0, _folder.Length - 1);


            Folder = _folder;
            ApiFolders = new List<string>();

            ResetRepAttributes();

            //Найти и прочесть файл docascode.json
            IsValid = readRepAttributes();

            this.Owner = owner;

        }

        Form1 Owner;

        public void ResetRepAttributes() {
            ApiFolders.Clear();
            CustomDocFolder = "";


            ProjectName = "";
        }

        public bool IsValid {
            get; set;
        }

        public string Folder {
            get; set;
        }
        public string ProjectName {
            get; set;
        }


        public bool HasApiFolders {
            get {
                return IsValid && ApiFolders.Count > 0;
            }

        }

        public List<string> ApiFolders {
            get; set;
        }
        public string CustomDocFolder {
            get; set;
        }


        public bool HasCustomDocFolder {
            get {
                return IsValid && (CustomDocFolder != "");
            }
        }

        public static bool checkFolderExists(string path) {
            return Directory.Exists(path);
        }

        public bool NeedCloseApp {
            get {
                if (Owner != null)
                    return Owner.NeedCloseApp;
                return false;
            }
        }

        public bool readRepAttributes() {
            bool res = false;

            string fullJSonFileName = Folder + "\\" + jSonFileName;

            if (!File.Exists(fullJSonFileName))
                return false;

            string json = File.ReadAllText(fullJSonFileName);
            Dictionary<string, object> json_Dictionary = (new System.Web.Script.Serialization.JavaScriptSerializer()).Deserialize<Dictionary<string, object>>(json);



            ResetRepAttributes();


            const string apiDocFolderJSonParentCollection = "documentProcessors";
            const string apiDocFolderJSonSuffixValue = "Reference";
            const string projectNameKey = "projectName";

            //Ключ projectName
            var prName = json_Dictionary[projectNameKey];
            if (prName != null) {
                ProjectName = prName as string;
                res = true;
            }

            
            if (json_Dictionary.ContainsKey(apiDocFolderJSonParentCollection)) {
                var apiDocFolderParentCollection = json_Dictionary[apiDocFolderJSonParentCollection] as Dictionary<string, object>;
                //Проверить, есть ли ключ "documentProcessors"
                //если есть, прочесть каталог с api
                if (apiDocFolderParentCollection != null) {
                    string shortFolderName = "";// = apiDocFolderParentCollection[apiDocFolderJSonKey];
                    foreach (var item in apiDocFolderParentCollection) {
                        //ManagedReference - это value, а apidoc - это key
                        if (item.Value.ToString().EndsWith(apiDocFolderJSonSuffixValue, StringComparison.CurrentCultureIgnoreCase)) {
                            shortFolderName = item.Key;
                            if (shortFolderName != "") {
                                string fullFolderName = Folder + "\\" + shortFolderName;
                                if (checkFolderExists(fullFolderName)) {
                                    ApiFolders.Add(fullFolderName);
                                }
                            }
                        }

                    }


                }
            }

            //Custom Doc Folder
            string fullCustomDocFolder = Folder + "\\" + "articles";
            if (checkFolderExists(fullCustomDocFolder)) {

                CustomDocFolder = fullCustomDocFolder;
            }

            return res;
        }

        public delegate void ReportProgress(object sender, string text);

        //public string getProgressText(int index, int count) {
        //    string s = string.Format("{0} of {1}", index, count);

        //    return s;
        //}

        public void LoadApi(List<MemberEl> MemberDS, List<ClassEl> ClassDS, List<NamespaceEl> NamespaceDS, ReportProgress progressCallback) {
            MemberDS.Clear();
            ClassDS.Clear();
            NamespaceDS.Clear();

            if (!IsValid)
                return;
            if (!HasApiFolders)
                return;



            foreach (string apiFolder in ApiFolders) {
                DirectoryInfo apiDirInfo = new DirectoryInfo(apiFolder);
                if (apiDirInfo == null)
                    return;
                FileInfo[] mdFilesInfos;
                try {
                    mdFilesInfos = apiDirInfo.GetFiles("*.md", SearchOption.AllDirectories);
                }
                catch {
                    continue;
                }

                
                
                string progressTextPrefix = "Loading " + this.ProjectName + " api from ";
                //string progressTextSuffix = " of " + mdFilesInfos.Length;
                    

                

                string prevDirectory = "";

                List<string> memberNamesInCurrentDir = new List<string>();
                //List<MemberEl> membersInCurrentDir = new List<MemberEl>();
                List<string> overloadNamesInCurrentDir = new List<string>();

                string progressText = progressTextPrefix + apiDirInfo.Name + "...";
                progressCallback(this, progressText);

                foreach (var fileInfo in mdFilesInfos) {

                    if (NeedCloseApp)
                        break;
                

                    //BaseAPIElement apiElem = MDParser.ParseMDApiHeader(fileInfo);
                    //if (apiElem.IsMember) {

                    //    MemberDS.Add(apiElem as MemberEl);

                    //}
                    BaseAPIElement apiElem = MDParser.ParseMDApiHeader(fileInfo);
                    if (apiElem.IsMember) {

                        string currentDir = fileInfo.DirectoryName;
                        if (prevDirectory != currentDir) {
                            memberNamesInCurrentDir.Clear();
                            //membersInCurrentDir.Clear();
                            overloadNamesInCurrentDir.Clear();

                            prevDirectory = currentDir;
                        }

                        MemberEl memberEl = apiElem as MemberEl;

                        if (memberNamesInCurrentDir.Contains(memberEl.FullUntemplatedName)) {
                            //add overload
                            //..

                            if (!overloadNamesInCurrentDir.Contains(memberEl.FullUntemplatedName)) {
                                MemberElOverload memberOverload = new MemberElOverload(memberEl);
                                MemberDS.Add(memberOverload);
                                overloadNamesInCurrentDir.Add(memberEl.FullUntemplatedName);
                            }

                        }
                        memberNamesInCurrentDir.Add(memberEl.FullUntemplatedName);
                        //membersInCurrentDir.Add(memberEl);

                        MemberDS.Add(memberEl);

                        Application.DoEvents();

                    }
                    else {
                        if (apiElem.IsClass)
                            ClassDS.Add(apiElem as ClassEl);
                        else {
                            if (apiElem.IsNamespace)
                                NamespaceDS.Add(apiElem as NamespaceEl);
                        }
                    }
                    //if (++breakID > 4000) //<-----------------------------------------------------------------------
                    //    break;//<-----------------------------------------------------------------------
                }
            }
        }



        public void LoadCustomDocs(List<CustomDocEl> CustomDocDS, ReportProgress progressCallback) {
            CustomDocDS.Clear();


            if (!IsValid)
                return;
            if (!HasCustomDocFolder)
                return;


            DirectoryInfo cdDirInfo = new DirectoryInfo(CustomDocFolder);
            if (cdDirInfo == null)
                return;
            FileInfo[] mdFilesInfos = cdDirInfo.GetFiles("*.md", SearchOption.AllDirectories);
            


            

            

            string progressTextPrefix = "Loading docs from ";

            string progressText = progressTextPrefix + cdDirInfo.Name + "...";
            progressCallback(this, progressText);

            foreach (var fileInfo in mdFilesInfos) {

                if (NeedCloseApp)
                    break;


                CustomDocEl cdElem = MDParser.ParseMDCustomDocHeader(fileInfo, CustomDocFolder);
                CustomDocDS.Add(cdElem);

                //if (++breakID > 4000) //<-----------------------------------------------------------------------
                //    break;//<-----------------------------------------------------------------------
            }

        }


    }


    public class MDParser {

        


        static public List<string> ReadMDHeader(FileInfo fileInfo) {
            List<string> lines = new List<string>();

            using (System.IO.StreamReader streamReader = new System.IO.StreamReader(fileInfo.FullName)) {

                int dashCount = 0;
                int lineCount = 0;
                while (streamReader.Peek() >= 0 && lineCount++ < 100) {
                    string l = streamReader.ReadLine();
                    if (l.Trim().StartsWith("--"))
                        dashCount++;
                    else
                        lines.Add(l);
                    if (dashCount > 1)
                        break;
                }

            }
            return lines;
        }

        static public Dictionary<string, string> GetMDHeaderKeysAndValues(List<string> list) {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string prevKey = "";

            foreach (string s in list) {
                if (s == "")
                    continue;
                if (s.StartsWith(" ")) {
                    if(prevKey != "") {
                        string prevKeyValue = "";
                        bool res = dic.TryGetValue(prevKey, out prevKeyValue);
                        string newKeyValue = prevKeyValue;
                        if (prevKeyValue != "")
                            newKeyValue = prevKeyValue + "\r\n";
                        newKeyValue += s;
                        dic[prevKey] = newKeyValue;
                    }
                }
                else {
                    
                    int colonIndex = s.IndexOf(":");
                    if (colonIndex > 0) {
                        string s1 = s.Substring(0, colonIndex).Trim().ToLower();
                        string s2 = s.Substring(colonIndex + 1).Trim();
                        prevKey = s1;
                        if(s2 == ">-") {
                            s2 = "";
                        }
                        if (!dic.Keys.Contains(s1))
                            dic.Add(s1, s2);
                    }
                }
            }
            return dic;
        }


        static public BaseAPIElement ParseMDApiHeader(FileInfo fileInfo) {
            BaseAPIElement elem = null;

            List<string> lines = ReadMDHeader(fileInfo);
            Dictionary<string, string> dic = GetMDHeaderKeysAndValues(lines);

            string bareUid = null;
            string bareName = null;
            string bareType = null;
            string bareSummary = "";

            bool res1 = dic.TryGetValue("uid", out bareUid);
            bool res2 = dic.TryGetValue("name", out bareName);
            bool res3 = dic.TryGetValue("type", out bareType);

            bool res4 = dic.TryGetValue("summary", out bareSummary);
            if (bareSummary == null)
                bareSummary = "";
            bareSummary = bareSummary.Trim();

            if(bareSummary.Length > 0) {
                if (bareSummary.StartsWith("'") || bareSummary.StartsWith("\"") || bareSummary.StartsWith("`"))
                    bareSummary = bareSummary.Substring(1, bareSummary.Length - 1);
                if (bareSummary.EndsWith("'") || bareSummary.EndsWith("\"") || bareSummary.EndsWith("`"))
                    bareSummary = bareSummary.Substring(0, bareSummary.Length - 1);
                bareSummary = bareSummary.Trim();
            }
            
            if (!(res1 && res2 && res3)) {
                //throw new Exception("header does not contains required attributes");
                return ApiElementFactory.CreateApiElement(bareUid, bareName, bareType, bareSummary, fileInfo.FullName);
            }
            else {
                elem = ApiElementFactory.CreateApiElement(bareUid, bareName, bareType, bareSummary, fileInfo.FullName);
            }
            return elem;
        }

        static public bool RemovePrefix(string initialString, string prefix, out string finalString) {
            finalString = initialString;
            if (!initialString.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
                return false;

            finalString = initialString.Substring(prefix.Length).Trim();
            return true;
        }


        /*
          ---
          uid: "7891"
          title: Controls and Libraries
          owner: Alexey Zolotarev
          seealso: []
         */
        static public CustomDocEl ParseMDCustomDocHeader(FileInfo fileInfo, string parentFolder) {
            CustomDocEl elem = null;


            List<string> lines = ReadMDHeader(fileInfo);
            Dictionary<string, string> dic = GetMDHeaderKeysAndValues(lines);

            string bareUid = null;
            string bareTitle = null;
            //string bareOwner = dic["owner"];

            bool res1 = dic.TryGetValue("uid", out bareUid);
            bool res2 = dic.TryGetValue("title", out bareTitle);


            if (!(res2 && res2)) {
                //throw new Exception("Custom doc header does not contains required attributes");
                return new CustomDocEl(bareUid, bareTitle, fileInfo.FullName, fileInfo.FullName);
            }
            else {
                string shortFolder = fileInfo.FullName;
                if (fileInfo.FullName.StartsWith(parentFolder)) {
                    shortFolder = fileInfo.FullName.Substring(parentFolder.Length + 1);

                    elem = new CustomDocEl(bareUid, bareTitle, shortFolder, fileInfo.FullName);
                }
            }
            return elem;
        }
    }


    public enum ApiElementType { Member, Class, Namespace, Unknown };

    public enum MemberType { Property, Method, Event, Constructor, Class, Enum, Field, Namespace, Delegate, Interface, Struct, AttachedProperty, AttachedEvent, Operator, EnumMember, Unknown, Module, Variable, TypeAlias, Function, Package, InstanceMethod, Protocol, ClassMethod };




    public class ApiElementFactory {
        public static BaseAPIElement CreateApiElement(string uid, string name, string typeAsString, string summary, string filePath) {
            MemberType coreMemberType = BaseAPIElement.GetMemberType(typeAsString);

            if (coreMemberType == MemberType.Unknown) {
                //MemberType.Unknown интерпретируется как MemberType.Member

                //throw new Exception("Unknown Member Type");
            }

            List<string> splitClauses = SplitUid(uid);

            if (IsMember(coreMemberType)) {
                return new MemberEl(uid, name, typeAsString, coreMemberType, splitClauses, summary, filePath);
            }
            if (IsClass(coreMemberType))
                return new ClassEl(uid, name, typeAsString, coreMemberType, splitClauses, summary, filePath);
            if (IsNamespace(coreMemberType))
                return new NamespaceEl(uid, name, typeAsString, coreMemberType, splitClauses, summary, filePath);

            return new BaseAPIElement(uid, name, typeAsString, coreMemberType, splitClauses, summary, filePath);
        }

        public static string RemoveArgumentsFromShortName(string s) {
            string res = s;
            int firstRoundBracketIndex = s.IndexOf("(");
            if(firstRoundBracketIndex > 0) {
                res = s.Substring(0, firstRoundBracketIndex);
            }
            return res;
        }


        public static List<string> SplitUid(string uid) {

            // Members
            //Конструкторы имеют формат:
            //uid: DevExpress.DataAccess.UI.FilterEditorControl.#ctor(System.Func{DevExpress.XtraEditors.FilterControl})
            //name: FilterEditorControl(Func<FilterControl>)
            //
            int firstRoundBracketIndex = uid.IndexOf("(");
            int lastDotIndex;

            if (firstRoundBracketIndex < 0)
                lastDotIndex = uid.LastIndexOf(".");
            else {
                //Ищем точку ДО символа '('
                bool dotFound = false;
                int index = firstRoundBracketIndex;
                while (!dotFound && index > 0) {
                    index--;
                    if (uid[index] == '.')
                        dotFound = true;
                }
                if (dotFound)
                    lastDotIndex = index;
                else {
                    //throw new Exception("'.' char not found in Full Name");
                    return new List<string>() { uid };
                }
            }
            //todo
            //Check how "DevExpress.XtraEditors" is split.


            string lastClause = uid.Substring(lastDotIndex + 1);
            string remainingClause = "";
            if (lastDotIndex >= 0)
                remainingClause = uid.Substring(0, lastDotIndex);
            else {

            }
            List<string> clauses = SplitString(remainingClause);
            clauses.Add(lastClause);

            //asp
            //uid: js-ASPxClientBinaryImage.Cast.static(obj)
            //name: Cast(obj)
            if (clauses.Last() == "static(obj)")
                clauses.RemoveAt(clauses.Count - 1);


            return clauses;
        }

        static List<string> SplitString(string s) {
            List<string> list = new List<string>();
            if (s != "") {
                string[] splitItems = s.Split('.');
                list.AddRange(splitItems);
            }
            return list;
        }



        static List<MemberType> MemberKind = new List<MemberType>() { MemberType.Unknown, MemberType.Property, MemberType.Method, MemberType.Event, MemberType.Field, MemberType.Constructor, MemberType.AttachedProperty, MemberType.AttachedEvent, MemberType.Operator, MemberType.EnumMember, MemberType.Variable, MemberType.Function, MemberType.InstanceMethod, MemberType.ClassMethod };

        static List<MemberType> ClassKind = new List<MemberType>() {          MemberType.Class, MemberType.Interface, MemberType.Enum, MemberType.Struct, MemberType.Delegate, MemberType.TypeAlias, MemberType.Protocol };

        static List<MemberType> ClassNamespaceKind = new List<MemberType>() { MemberType.Class, MemberType.Interface, MemberType.Enum, MemberType.Struct, MemberType.Delegate, MemberType.TypeAlias, MemberType.Protocol, MemberType.Namespace, MemberType.Module, MemberType.Package };



        public static bool IsMember(MemberType Type) {
            return MemberKind.Contains(Type);
        }

        public static bool IsConstructor(MemberType Type) {
            return Type == MemberType.Constructor;
        }

        public static bool IsClass(MemberType Type) {
            return ClassKind.Contains(Type);
        }

        public static bool IsNamespace(MemberType Type) {
            return (Type == MemberType.Namespace) || (Type == MemberType.Module) || (Type == MemberType.Package);
        }

        public static bool IsClassOrNamespace(MemberType Type) {
            return ClassNamespaceKind.Contains(Type);
        }


    }


    public class BaseElement {

        public BaseElement() {

        }

        public BaseElement(string filePath) {
            this.FilePath = filePath;
        }

        public string MDUid { get; set; }
        public string MDName { get; set; }

        //Без скобочек, начальных апострофов....
        public string Name { get; set; }

        public string UIDName { get; set; }

        internal virtual string Link {
            get {
                return "[](xref:)";
            }
        }

        internal virtual string LinkSeeAlso {
            get {
                string s = string.Format("- linkId: \"{0}\"", MDUid);
                return s;
            }
        }

        internal virtual string LinkSeeAlsoAlt {
            get {
                string s = "- linkType: HRef" + "\r\n";
                s += "  linkId: xref:" + MDUid + "\r\n";
                s += "  altText: " + Name;

                return s;
            }
        }


        //[!summary-include(DevExpress.XtraGrid.GridControl.ExportToHtml*)]
        internal virtual string ShortDescription {
            get {
                string s = string.Format("[!summary-include ({0})]", MDUid);
                return s;
            }
        }

        public virtual string Search {
            get {
                return MDName;
            }
        }

        public string FilePath {
            get;set;
        }
    }


    //uid: "2412"
    //title: 'How to: Customize Look And Feel of All Controls within Application'
    public class CustomDocEl : BaseElement {

        const string Quotes = "\"";
        const string Apostr = "'";

        public CustomDocEl(string uid, string name, string mdFilefolder, string filePath) :base(filePath) {
            MdFileFolder = mdFilefolder;
            MDName = name;

            if (isStartEndWithQuotes(uid))
                uid = removeStartEndChars(uid);
            else {
                if (isStartEndWithApostr(uid))
                    uid = removeStartEndChars(uid);
            }

            if (isStartEndWithApostr(name))
                name = removeStartEndChars(name);
            else {
                if (isStartEndWithQuotes(name))
                    name = removeStartEndChars(name);
            }

            MDUid = uid;
            Name = name;
        }

        public string MdFileFolder {
            get; set;
        }

        string removeStartEndChars(string s) {
            if (s != null)
                return s.Substring(1, s.Length - 2);
            else
                return s;
        }

        bool isStartEndWithQuotes(string s) {
            if (s != null)

                return s.StartsWith(Quotes) && s.EndsWith(Quotes);
            else
                return false;
        }

        bool isStartEndWithApostr(string s) {
            if (s != null)

                return s.StartsWith(Apostr) && s.EndsWith(Apostr);
            else
                return false;
        }

        //Без начальных кавычек и апострофов (для custom docов)
        //Свойство Name определено в базовом классе
        //public string Name { get; set; }


        //[Workspace Manager](xref:17674)
        internal override string Link {
            get {
                return string.Format("[{0}](xref:{1})", Name, MDUid);
            }
        }

        // todo
        //- linkType: HRef
        //  linkId: xref:8869
        //  altText: Grid Find Panel

        //- linkId: "114963"
        internal override string LinkSeeAlso {
            get {
                string s = string.Format("- linkId: \"{0}\"", MDUid);
                return s;
            }
        }

        public override string Search {
            get {
                return Name;
            }
        }

    }

    public class BaseAPIElement : BaseElement {

        public BaseAPIElement() {

        }

        public BaseAPIElement(string filePath) : base(filePath) {

        }

        public BaseAPIElement(string uid, string name, string typeAsString, MemberType memberType, List<string> splitClauses, string summary, string filePath) :base(filePath) {
            Summary = summary;
            MDUid = uid;
            MDTypeAsString = typeAsString;
            MDName = name;

            MDType = memberType;

            //Init Name, FullName
            string lastClause = splitClauses.Last();
            UIDName = lastClause;
            Name = getBareName(lastClause);
            if (Name.IndexOf('`') >= 0)
                //Name = ApiElementFactory.RemoveArgumentsFromShortName(MDName);
                Name = getBareName(MDName);


            int splitItemCount = splitClauses.Count;
            string fullName = splitClauses.First();
            for (int i = 1; i < splitItemCount - 1; i++) {
                fullName += "." + splitClauses[i];
            }
            fullName += "." + Name;
            FullName = fullName;
        }

        public static MemberType GetMemberType(string typeAsString) {
            Array enumMembers = Enum.GetValues(typeof(MemberType));
            MemberType MDType = MemberType.Unknown;

            foreach (var m in enumMembers) {
                if (String.Equals(m.ToString(), typeAsString, StringComparison.CurrentCultureIgnoreCase)) {
                    MDType = (MemberType)m;
                    break;
                }
            }
            return MDType;
        }





        internal string MDTypeAsString { get; set; }

        public MemberType MDType { get; set; }


        string getBareName(string rawName) {
            //Возвращаем имя без круглых скобок, если они есть
            int roundBracketIndex = rawName.IndexOf('(');
            if (roundBracketIndex < 0)
                return rawName;
            else {
                string res = rawName.Substring(0, roundBracketIndex);
                return res;
            }
        }


        //Без скобочек
        
        public string FullName { get; set; }

        string coreFullUntemplatedName = null;
        internal string FullUntemplatedName {
            get {
                if(coreFullUntemplatedName == null) {
                    string fName = FullName;
                    if(fName != null) {
                        int angleBracketIndex = fName.IndexOf('<');
                        if (angleBracketIndex < 0)
                            coreFullUntemplatedName = fName;
                        else {
                            coreFullUntemplatedName = fName.Substring(0, angleBracketIndex);
                        }
                    }
                }
                return coreFullUntemplatedName;
            }
        }

        internal virtual ApiElementType Type {
            get {
                return ApiElementType.Unknown;
            }
        }

        internal virtual bool IsMember {
            get {
                return false;
            }
        }
        internal virtual bool IsClass {
            get {
                return false;
            }
        }
        internal virtual bool IsNamespace {
            get {
                return false;
            }
        }


        public string Summary { get; set; }


    }


    public class MemberEl : BaseAPIElement {

        public static int Counter = 0;

        public MemberEl() {

        }

        public MemberEl(string filePath) : base(filePath) {

        }

        public MemberEl(string uid, string name, string typeAsString, MemberType memberType, List<string> splitClauses, string summary, string filePath) : base(uid, name, typeAsString, memberType, splitClauses, summary, filePath) {
            //ID = Counter++;

            //uid: js-ASPxClientBinaryImage.Cast.static(obj)
            //name: Cast(obj)
            //type: Method

            if (uid.Contains("js-ASPx")) {

            }

            if (splitClauses.Count - 2 >= 0) {
                ClassName = splitClauses[splitClauses.Count - 2];
                FullClassName = FullName.Substring(0, FullName.Length - Name.Length - 1);

                if (FullClassName.Length - ClassName.Length - 1 > 0)
                    Namespace = FullClassName.Substring(0, FullClassName.Length - ClassName.Length - 1);
            }



        }

        //public int ID {
        //    get; set;
        //}

        //public bool HasOverloads {
        //    get;set;
        //}

        internal override ApiElementType Type {
            get {
                return ApiElementType.Member;
            }
        }
        internal override bool IsMember {
            get {
                return true;
            }
        }
        internal bool IsConstructor {
            get {
                return MDType == MemberType.Constructor;
            }
        }

        public string ClassName {
            get; set;
        }

        public string FullClassName {
            get; set;
        }

        public string Namespace {
            get; set;
        }


        //[PrintingSystemBase.ExportToXls](xref:DevExpress.XtraPrinting.PrintingSystemBase.ExportToXls*)
        //[TreeList.ActiveFilterCriteria](xref:DevExpress.XtraTreeList.TreeList.ActiveFilterCriteria)
        //[SvgImageBox.FindItem(Predicate\<SvgImageItem>)](xref:DevExpress.XtraEditors.SvgImageBox.FindItem(System.Predicate{DevExpress.XtraEditors.SvgImageItem}))
        internal override string Link {
            get {
                string s = string.Format("[{0}.{1}](xref:{2})", ClassName, Name, MDUid);
                if(s.IndexOf('<')>= 0)
                    s = s.Replace("<", "\\<");

                return s;
            }
        }

        public override string Search {
            get {
                return string.Format("{0}.{1}", Name, ClassName);
            }
        }


        internal override string LinkSeeAlsoAlt {
            get {
                string s = "- linkType: HRef" + "\r\n";
                s += "  linkId: xref:" + MDUid + "\r\n";
                s += "  altText: " + ClassName + "." + Name;

                return s;
            }
        }

    }


    public class MemberElOverload : MemberEl {

        

        public MemberElOverload(MemberEl el) {
            nameCore = el.Name;

            this.ClassName = el.ClassName;
            this.FullClassName = el.FullClassName;
            this.FullName = el.FullName + "*";

            this.MDType = el.MDType;
            this.MDTypeAsString = el.MDTypeAsString;

            this.MDUid = this.FullName;
            this.Name = el.Name + "*";
            this.MDName = this.Name;
            this.MDName = this.Name; //this.FullName;

            this.Namespace = el.Namespace;

            this.UIDName = this.Name;

            this.Summary = "(overload) " + el.Summary;


        }

        string nameCore;

        public override string Search {
            get {
                return string.Format("{0}.{1}*", nameCore, ClassName);
            }
        }

        internal override string Link {
            get {
                string s = string.Format("[{0}.{1}](xref:{2})", ClassName, nameCore, MDUid);
                return s;
            }
        }

        internal override string LinkSeeAlso {
            get {
                string s = string.Format("- linkId: \"{0}\"", MDUid);
                return s;
            }
        }


    }


    public class ClassEl : BaseAPIElement {

        public ClassEl(string uid, string name, string typeAsString, MemberType memberType, List<string> splitClauses, string summary, string filePath) : base(uid, name, typeAsString, memberType, splitClauses, summary, filePath) {
            Namespace = FullName.Substring(0, FullName.Length - Name.Length - 1);
        }
        internal override ApiElementType Type {
            get {
                return ApiElementType.Class;
            }
        }
        internal override bool IsClass {
            get {
                return true;
            }
        }

        public string Namespace {
            get; set;
        }


        internal override string Link {
            get {
                string s = string.Format("[](xref:{0})", MDUid);
                return s;
            }
        }

    }


    public class NamespaceEl : BaseAPIElement {

        public NamespaceEl(string uid, string name, string typeAsString, MemberType memberType, List<string> splitClauses, string summary, string filePath) : base(uid, name, typeAsString, memberType, splitClauses, summary, filePath) {

        }
        internal override ApiElementType Type {
            get {
                return ApiElementType.Namespace;
            }
        }
        internal override bool IsNamespace {
            get {
                return true;
            }
        }

        internal override string Link {
            get {
                string s = string.Format("[](xref:{0})", MDUid);
                return s;
            }
        }
    }



    public class MDHeader {
        string coreStringType = "";
        MemberType coreMemberType = MemberType.Unknown;
        public string UID { get; set; }
        public string Name { get; set; }
        public string StringType {
            get {
                return coreStringType;
            }
            set {
                coreStringType = value;
                SetType();
            }
        }

        Array enumMembers = Enum.GetValues(typeof(MemberType));

        void SetType() {
            foreach (var m in enumMembers) {
                if (String.Equals(m.ToString(), coreStringType, StringComparison.CurrentCultureIgnoreCase)) {
                    coreMemberType = (MemberType)m;
                    return;
                }
            }

            //throw new Exception("Unknown Member Type");
            coreMemberType = MemberType.Unknown;
        }

        public MemberType Type {
            get {
                return coreMemberType;
            }
        }



        

        public bool IsMember {
            get {
                return ApiElementFactory.IsMember(Type);
            }
        }

        public bool IsClass {
            get {
                return ApiElementFactory.IsClass(Type);
            }
        }

        public bool IsNamespace {
            get {
                return ApiElementFactory.IsNamespace(Type);
            }
        }

        public bool IsClassOrNamespace {
            get {
                return ApiElementFactory.IsClassOrNamespace(Type);
            }
        }





        public bool OK {
            get; set;
        }


        
    }


    public class Logger {
        Form1 Owner;
        public Logger(Form1 owner) {
            Owner = owner;
        }

        public void Log(string s) {
            Debug.WriteLine(s);
        }

    }

}
