using BaseWinGUI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TextBoxExt
{
    public class ucMultiLineEdit : UserControl
    {
        private TextBox txtField;
        private Button btnShowPopUp;
        private ResizableDropDownForm mForm;
        private TextBox txtFieldOnPopUpForm;
        private ToolTip ttMain;

        //public event BadFormInitialization(object sender, EventArgs e)

        #region Property
        [Browsable(true)]
        public override string Text
        {
            get
            {
                return txtField.Text;
            }
            set
            {
                txtField.Text = value;
            }
        }

        [Browsable(true)]
        public int MaxLength
        {
            get
            {
                return txtField.MaxLength;
            }
            set
            {
                txtField.MaxLength =
                    txtFieldOnPopUpForm.MaxLength = value;
            }
        }

        [Browsable(true)]
        public bool ReadOnly
        {
            get
            {
                return txtField.ReadOnly;
            }
            set
            {
                txtField.ReadOnly =
                    txtFieldOnPopUpForm.ReadOnly = value;
            }
        }
        #endregion

        void txtField_TextChanged(object sender, EventArgs e)
        {
            txtFieldOnPopUpForm.Text = txtField.Text;
        }

        private void SetupFormToPopUp()
        {
            txtFieldOnPopUpForm = new TextBox();
            txtFieldOnPopUpForm.Multiline = true;
            txtFieldOnPopUpForm.ScrollBars = ScrollBars.Both;
            mForm = new ResizableDropDownForm();
            mForm.Hide();
            mForm.Controls.Add(txtFieldOnPopUpForm);
            txtFieldOnPopUpForm.Dock = DockStyle.Fill;
            mForm.MinimumSize = new Size(100, 40); // Set min size
            mForm.Size = new Size(this.Size.Width, this.Size.Height + 120);
            mForm.Deactivate += new EventHandler(mForm_Deactivate);            
        }

        protected void mForm_Deactivate(object sender, EventArgs e)
        {
            this.ParentForm.BringToFront();
            this.txtField.Focus();
            txtField.Text = txtFieldOnPopUpForm.Text;//.ToOneLine();          
            mForm.Hide();            
        }

        //protected override void OnValidated(EventArgs e)
        //{

        //}

        protected void btnShowPopUp_Click(object sender, EventArgs e)
        {
            // show the popup grid
            Point pt = txtField.Location;
            Size sz = txtField.Size;
            Rectangle cellRectangle = new Rectangle(pt, sz);
            mForm.ShowDialogDropDown(this, cellRectangle);
        }

        public ucMultiLineEdit()
        {
            InitializeComponent();
            SetupFormToPopUp();
            ttMain = new ToolTip();

            txtField.MouseHover += new EventHandler(MyUserControl_MouseHover);
            txtField.MouseLeave += new EventHandler(MyUserControl_MouseLeave);
        }

        private void MyUserControl_MouseLeave(object sender, EventArgs e)
        {
            ttMain.Hide(this);
        }

        private void MyUserControl_MouseHover(object sender, EventArgs e)
        {
            ttMain.Show("Hallo" + Text, this, PointToClient(MousePosition));
        }

        #region Designer
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtField = new System.Windows.Forms.TextBox();
            this.btnShowPopUp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtField
            // 
            this.txtField.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtField.Location = new System.Drawing.Point(0, 0);
            this.txtField.MinimumSize = new System.Drawing.Size(100, 23);
            this.txtField.Multiline = true;
            this.txtField.Name = "txtField";
            this.txtField.Size = new System.Drawing.Size(233, 23);
            this.txtField.TabIndex = 0;
            this.txtField.TextChanged += txtField_TextChanged;
            // 
            // btnShowPopUp
            // 
            this.btnShowPopUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowPopUp.Location = new System.Drawing.Point(234, 0);
            this.btnShowPopUp.Name = "btnShowPopUp";
            this.btnShowPopUp.Size = new System.Drawing.Size(29, 23);
            this.btnShowPopUp.TabIndex = 1;
            this.btnShowPopUp.Text = "...";
            this.btnShowPopUp.UseVisualStyleBackColor = true;
            this.btnShowPopUp.Click += new System.EventHandler(this.btnShowPopUp_Click);
            // 
            // ucMultiLineEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnShowPopUp);
            this.Controls.Add(this.txtField);
            this.MinimumSize = new System.Drawing.Size(133, 23);
            this.Name = "ucMultiLineEdit";
            this.Size = new System.Drawing.Size(263, 24);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
        #endregion
    }

    //public enum TextExtErrors
    //{
    //    NoError = 0,
    //    NotSet = 1,
    //    Others = 9,
    //}

    //public class TextExtEventArgs : EventArgs
    //{
    //    private string mMessage;
    //    private TextExtErrors mErrorNumber;

    //    public string Message
    //    {
    //        get
    //        {
    //            return mMessage;
    //        }
    //        set
    //        {
    //            mMessage = value;
    //        }
    //    }

    //    public TextExtErrors MErrorNumber { get => mErrorNumber; set => mErrorNumber = value; }
    //}
}
