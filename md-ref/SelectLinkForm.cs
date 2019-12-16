using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;

namespace md_ref {
    public partial class SelectLinkForm : DevExpress.XtraEditors.XtraForm {



        BaseElement Elem = null;
        Form1 MyOwner = null;

        public SelectLinkForm(Form1 form, BaseElement elem, string previousWindowProcessName) {
            InitializeComponent();
            btnLink.Tag = LinkType.Link;
            btnSeeAlsoLink.Tag = LinkType.SeeAlsoLink;
            btnSeeAlsoAltLink.Tag = LinkType.SeeAlsoAltLink;

            btnShortDescription.Tag = LinkType.ShortDescrption;

            MyOwner = form;
            this.Elem = elem;

            string copyPasteModePrefix = (MyOwner.CopyOrPasteLinkMode == Form1.CopyOrPasteLink.CopyPaste) ? "Copy&&Paste\r\n" : "Copy\r\n";

            if(MyOwner.CopyOrPasteLinkMode == Form1.CopyOrPasteLink.Copy) {
                Text = "Copy links";
            }
            if(MyOwner.CopyOrPasteLinkMode == Form1.CopyOrPasteLink.CopyPaste) {
                Text = "Copy & paste links";
                if(!string.IsNullOrEmpty(previousWindowProcessName.Trim())) {
                    Text += " to " + previousWindowProcessName;
                }
            }

            BaseAPIElement apiElem = Elem as BaseAPIElement;
            if (apiElem != null) {
                if (apiElem.IsMember || apiElem.IsClass || apiElem.IsNamespace) {
                    memoLink.Text = apiElem.Link;
                    btnLink.Text = copyPasteModePrefix + "link"; //"link to " + apiElem.MDType;
                    
                    
                    btnShortDescription.Text = copyPasteModePrefix + "short description";

                    lgShortDescription.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    
                    memoShortDescription.Text = apiElem.ShortDescription;
                }
            }
            CustomDocEl cdElem = Elem as CustomDocEl;
            if(cdElem != null) {
                memoLink.Text = cdElem.Link;
                btnLink.Text = copyPasteModePrefix + "link";// "link to custom doc";

                lgShortDescription.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                
            }



            btnSeeAlsoLink.Text = copyPasteModePrefix + "see also link";
            memoSeeAlsoLink.Text = Elem.LinkSeeAlso;

            btnSeeAlsoAltLink.Text = copyPasteModePrefix + "see also ALT link";
            memoSeeAlsoAltLink.Text = Elem.LinkSeeAlsoAlt;

            //if (!MyOwner.IsCalledFromExternalApplication) {
            //    btnLink.Enabled = false;
            //    btnSeeAlsoLink.Enabled = false;
            //}

            //this.AcceptButton = btnLink;









            //code from FormShown:

            FormButtons = new List<SimpleButton>();
            FormButtons.Add(btnLink);
            FormButtons.Add(btnSeeAlsoLink);
            FormButtons.Add(btnSeeAlsoAltLink);
            FormButtons.Add(btnShortDescription);

            for (int i = 0; i < FormButtons.Count; i++) {
                FormButtons[i].PreviewKeyDown += SelectLinkForm_PreviewKeyDown;
                FormButtons[i].KeyDown += SelectLinkForm_KeyDown;
            }


            layoutControl1.BeginUpdate();
            try {
                //size constraints
                LayoutControlItem item1 = layoutControl1.GetItemByControl(btnLink);
                LayoutControlItem item2 = layoutControl1.GetItemByControl(btnSeeAlsoLink);
                LayoutControlItem item3 = layoutControl1.GetItemByControl(btnShortDescription);
                LayoutControlItem item4 = layoutControl1.GetItemByControl(btnSeeAlsoAltLink);


                if (item1 == null || item2 == null || item3 == null || item4 == null)
                    return;

                Size sz1line = TextRenderer.MeasureText(" Short Description", WindowsFormsSettings.DefaultFont);
                Size sz2lines = TextRenderer.MeasureText(" Short Description \r\nShort Description", WindowsFormsSettings.DefaultFont);

                Size longLine = TextRenderer.MeasureText(
                    "- linkId: \"DevExpress.XtraCharts.Designer.ZoomingOptions2DModel.ZoomToRectangleMouseAction\"", WindowsFormsSettings.DefaultFont);



                item1.SizeConstraintsType = SizeConstraintsType.Custom;
                item2.SizeConstraintsType = SizeConstraintsType.Custom;
                item3.SizeConstraintsType = SizeConstraintsType.Custom;
                item4.SizeConstraintsType = SizeConstraintsType.Custom;

                item1.MinSize = new Size(sz2lines.Width + 10, sz2lines.Height + 10);

                item2.MinSize = item1.MinSize;
                item3.MinSize = item1.MinSize;

                item1.MaxSize = item1.MinSize;
                item2.MaxSize = item2.MinSize;
                item3.MaxSize = item3.MinSize;

                item4.MinSize = item3.MinSize;
                item4.MaxSize = item3.MaxSize;


                LayoutControlItem item11 = layoutControl1.GetItemByControl(memoLink);
                LayoutControlItem item22 = layoutControl1.GetItemByControl(memoSeeAlsoLink);
                LayoutControlItem item33 = layoutControl1.GetItemByControl(memoShortDescription);
                LayoutControlItem item44 = layoutControl1.GetItemByControl(memoSeeAlsoAltLink);

                if (item11 == null || item22 == null || item33 == null || item44 == null)
                    return;

                item11.SizeConstraintsType = SizeConstraintsType.Custom;
                item22.SizeConstraintsType = SizeConstraintsType.Custom;
                item33.SizeConstraintsType = SizeConstraintsType.Custom;
                item44.SizeConstraintsType = SizeConstraintsType.Custom;


                item11.MinSize = new Size(longLine.Width + 10, sz2lines.Height + 10);

                item22.MinSize = item11.MinSize;
                item33.MinSize = item11.MinSize;

                item11.MaxSize = new Size(0, item11.MinSize.Height);
                item22.MaxSize = item11.MaxSize;
                item33.MaxSize = item11.MaxSize;

                item44.MinSize = new Size(item33.MinSize.Width, (int)(item33.MinSize.Height * 1.5));
                item44.MaxSize = new Size(0, item44.MinSize.Height); ;
            }
            finally {
                layoutControl1.EndUpdate();
            }



















        }

        enum LinkType { Link, SeeAlsoLink, ShortDescrption, SeeAlsoAltLink };

        private void btnLink_Click(object sender, EventArgs e) {
            ButtonClicked(sender as SimpleButton);
        }

        private void ButtonClicked(SimpleButton btn) {
            switch ((LinkType)btn.Tag) {
                case LinkType.Link:
                    MyOwner.SelectedLink = memoLink.Text;
                    break;
                case LinkType.SeeAlsoLink:
                    MyOwner.SelectedLink = memoSeeAlsoLink.Text;
                    break;
                case LinkType.SeeAlsoAltLink:
                    MyOwner.SelectedLink = memoSeeAlsoAltLink.Text;
                    break;
                case LinkType.ShortDescrption:
                    MyOwner.SelectedLink = memoShortDescription.Text;
                    break;
                
                default:
                    throw new Exception("Unknown link type");
                    
            }
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            MyOwner.SelectedLink = "";
            Close();
        }

        private void SelectLinkForm_Load(object sender, EventArgs e) {

            

        }

        List<SimpleButton> FormButtons;
        bool isButtonHidden(SimpleButton btn) {
            bool vis = btn.Visible;
            if(vis) {
                LayoutControlItem item = layoutControl1.GetItemByControl(btn);
                if (item != null)
                    vis = item.Visible;
                if(vis) {
                    if (item.Parent != null)
                        vis = item.Parent.Visible;
                }
            }
            return !vis;
        }



        private void SelectLinkForm_Shown(object sender, EventArgs e) {
            
        }

        private void SelectLinkForm_KeyDown(object sender, KeyEventArgs e) {
            SimpleButton btn = sender as SimpleButton;
            if (e.KeyCode == Keys.Down) {
                GetNextButton(btn).Focus();
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Up) {
                GetPrevButton(btn).Focus();
                e.Handled = true;

            }

        }

        private SimpleButton GetPrevButton(SimpleButton btn) {
            int index = FormButtons.IndexOf(btn);
            if (index < 0)
                return btn;
            int newIndex = index - 1;
            if (newIndex < 0)
                newIndex = FormButtons.Count - 1;
            SimpleButton prevButton = FormButtons[newIndex];
            if (isButtonHidden(prevButton))
                prevButton = GetPrevButton(prevButton);

            return prevButton;
        }

        private SimpleButton GetNextButton(SimpleButton btn) {
            int index = FormButtons.IndexOf(btn);
            if (index < 0)
                return btn;
            int newIndex = index + 1;
            if (newIndex >= FormButtons.Count)
                newIndex = 0;
            SimpleButton nextButton = FormButtons[newIndex];
            if (isButtonHidden(nextButton))
                nextButton = GetNextButton(nextButton);

            return nextButton;
        }

        private void SelectLinkForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                e.IsInputKey = true;
        }

        
        
    }
}
