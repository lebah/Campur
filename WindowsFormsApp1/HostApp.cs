using FlashTrackBarControlLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class HostApp : System.Windows.Forms.Form
    {
        /// <summary>
        ///    Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components;
        protected internal FlashTrackBar flashTrackBar1;

        public HostApp()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

        }

        /// <summary>
        ///    Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///    Required method for Designer support - do not modify
        ///    the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.flashTrackBar1 = new FlashTrackBar();
            this.Text = "Control Example";
            this.ClientSize = new System.Drawing.Size(600, 450);
            flashTrackBar1.BackColor = System.Drawing.Color.Black;
            flashTrackBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            flashTrackBar1.TabIndex = 0;
            flashTrackBar1.ForeColor = System.Drawing.Color.White;
            flashTrackBar1.Text = "Drag the Mouse and say Wow!";
            flashTrackBar1.Value = 73;
            flashTrackBar1.Size = new System.Drawing.Size(600, 450);
            this.Controls.Add(this.flashTrackBar1);
        }
    }
}
