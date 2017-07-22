namespace DropDownComboBoxMultiLineEditor
{
    partial class Form1
    {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboDropDownShipment = new System.Windows.Forms.ComboBox();
            this.lblShipment = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textExtControl1 = new TextBoxExt.ucMultiLineEdit();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.ucMultiLineEdit1 = new TextBoxExt.ucMultiLineEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // comboDropDownShipment
            // 
            this.comboDropDownShipment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboDropDownShipment.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboDropDownShipment.Location = new System.Drawing.Point(170, 12);
            this.comboDropDownShipment.Name = "comboDropDownShipment";
            this.comboDropDownShipment.Size = new System.Drawing.Size(200, 63);
            this.comboDropDownShipment.TabIndex = 139;
            this.comboDropDownShipment.Visible = false;
            this.comboDropDownShipment.Click += new System.EventHandler(this.comboDropDownShipment_Click);
            // 
            // lblShipment
            // 
            this.lblShipment.Location = new System.Drawing.Point(13, 17);
            this.lblShipment.Name = "lblShipment";
            this.lblShipment.Size = new System.Drawing.Size(154, 17);
            this.lblShipment.TabIndex = 138;
            this.lblShipment.Text = "Shipment";
            this.lblShipment.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(13, 82);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(154, 17);
            this.label1.TabIndex = 141;
            this.label1.Text = "Description";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textExtControl1
            // 
            this.textExtControl1.Location = new System.Drawing.Point(170, 217);
            this.textExtControl1.MaxLength = 32767;
            this.textExtControl1.MinimumSize = new System.Drawing.Size(0, 23);
            this.textExtControl1.Name = "textExtControl1";
            this.textExtControl1.ReadOnly = false;
            this.textExtControl1.Size = new System.Drawing.Size(197, 23);
            this.textExtControl1.TabIndex = 142;
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(170, 79);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(197, 20);
            this.txtDescription.TabIndex = 143;
            this.txtDescription.Click += new System.EventHandler(this.txtDescription_Click);
            // 
            // ucMultiLineEdit1
            // 
            this.ucMultiLineEdit1.Location = new System.Drawing.Point(170, 272);
            this.ucMultiLineEdit1.MaxLength = 32767;
            this.ucMultiLineEdit1.MinimumSize = new System.Drawing.Size(0, 23);
            this.ucMultiLineEdit1.Name = "ucMultiLineEdit1";
            this.ucMultiLineEdit1.ReadOnly = false;
            this.ucMultiLineEdit1.Size = new System.Drawing.Size(197, 23);
            this.ucMultiLineEdit1.TabIndex = 144;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 217);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(154, 17);
            this.label2.TabIndex = 145;
            this.label2.Text = "Description";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(10, 272);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(154, 17);
            this.label3.TabIndex = 146;
            this.label3.Text = "Description";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(170, 327);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(197, 20);
            this.textBox1.TabIndex = 147;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(170, 301);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(197, 20);
            this.textBox2.TabIndex = 148;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(170, 246);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(197, 20);
            this.textBox3.TabIndex = 149;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 386);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ucMultiLineEdit1);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.textExtControl1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboDropDownShipment);
            this.Controls.Add(this.lblShipment);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboDropDownShipment;
        private System.Windows.Forms.Label lblShipment;
        private System.Windows.Forms.Label label1;
        private TextBoxExt.ucMultiLineEdit textExtControl1;
        private System.Windows.Forms.TextBox txtDescription;
        private TextBoxExt.ucMultiLineEdit ucMultiLineEdit1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
    }
}

