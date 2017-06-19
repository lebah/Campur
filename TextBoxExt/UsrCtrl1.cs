using System;
using System.Windows.Forms;

namespace TextBoxExt
{
    public partial class TextExtControl : UserControl
    {
        protected TextBox txtField;
        protected Button btnShowPopUp;

        public TextExtControl()
        {
            InitializeComponent();
        }        
        
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
            this.txtField.Location = new System.Drawing.Point(0, 1);
            this.txtField.MinimumSize = new System.Drawing.Size(0, 20);
            this.txtField.Multiline = true;
            this.txtField.Name = "txtField";
            this.txtField.Size = new System.Drawing.Size(233, 21);
            this.txtField.TabIndex = 0;
            this.txtField.SizeChanged += new System.EventHandler(this.txtField_SizeChanged);
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
            // TextExtControl
            // 
            this.Controls.Add(this.btnShowPopUp);
            this.Controls.Add(this.txtField);
            this.MinimumSize = new System.Drawing.Size(0, 23);
            this.Name = "TextExtControl";
            this.Size = new System.Drawing.Size(263, 24);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void txtField_SizeChanged(object sender, EventArgs e)
        {
            this.Size = txtField.Size;
        }

        private void btnShowPopUp_Click(object sender, EventArgs e)
        {

        }
    }
}
