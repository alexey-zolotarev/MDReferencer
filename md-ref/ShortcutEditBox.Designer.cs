namespace md_ref {
    partial class ShortcutEditBox {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutEditBox));
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.adornerUIManager1 = new DevExpress.Utils.VisualEffects.AdornerUIManager(this.components);
            this.vhShortcut = new DevExpress.Utils.VisualEffects.ValidationHint();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.adornerUIManager1)).BeginInit();
            this.SuspendLayout();
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(0, 5);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Properties.EditValueChangedFiringMode = DevExpress.XtraEditors.Controls.EditValueChangedFiringMode.Buffered;
            this.textEdit1.Size = new System.Drawing.Size(165, 20);
            this.textEdit1.TabIndex = 0;
            this.textEdit1.EditValueChanged += new System.EventHandler(this.textEdit1_EditValueChanged);
            this.textEdit1.Validating += new System.ComponentModel.CancelEventHandler(this.textEdit1_Validating);
            // 
            // adornerUIManager1
            // 
            this.adornerUIManager1.Elements.Add(this.vhShortcut);
            this.adornerUIManager1.Owner = this;
            // 
            // vhShortcut
            // 
            this.vhShortcut.Properties.InvalidState.HintLocation = DevExpress.Utils.VisualEffects.ValidationHintLocation.Right;
            this.vhShortcut.Properties.InvalidState.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("vhShortcut.Properties.InvalidState.ImageOptions.SvgImage")));
            this.vhShortcut.Properties.InvalidState.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.vhShortcut.Properties.InvalidState.ShowHint = DevExpress.Utils.DefaultBoolean.True;
            this.vhShortcut.Properties.InvalidState.Text = "Invalid shortcut";
            this.vhShortcut.Properties.ValidState.HintLocation = DevExpress.Utils.VisualEffects.ValidationHintLocation.Right;
            this.vhShortcut.Properties.ValidState.ImageOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("vhShortcut.Properties.ValidState.ImageOptions.SvgImage")));
            this.vhShortcut.Properties.ValidState.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            this.vhShortcut.Properties.ValidState.ShowHint = DevExpress.Utils.DefaultBoolean.True;
            this.vhShortcut.Properties.ValidState.Text = "OK";
            this.vhShortcut.TargetElement = this.textEdit1;
            // 
            // ShortcutEditBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textEdit1);
            this.Name = "ShortcutEditBox";
            this.Size = new System.Drawing.Size(353, 30);
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.adornerUIManager1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.Utils.VisualEffects.AdornerUIManager adornerUIManager1;
        private DevExpress.Utils.VisualEffects.ValidationHint vhShortcut;
    }
}
