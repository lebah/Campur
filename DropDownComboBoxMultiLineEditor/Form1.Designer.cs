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
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
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
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(170, 81);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(200, 40);
            this.txtDescription.TabIndex = 140;
            this.txtDescription.Click += new System.EventHandler(this.txtDescription_Click);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(631, 386);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDescription);
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
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label1;
    }
}

