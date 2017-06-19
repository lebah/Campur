using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropDownComboBoxMultiLineEditor
{
    public partial class Form1 : Form
    {
        private ResizableDropDownForm dropDownForm;
        private TextBox txtShipmentOnPopUpForm;
        private TextBox txtDescriptionOnPopUpForm;
        private ResizableDropDownForm dropDownFormDescription;


        private void InitializeTextExtControl()
        {
            
        }


        void dropDownFormDescription_Deactivate(object sender, EventArgs e)
        {
            this.BringToFront();
            txtDescription.Focus();
            txtDescription.Text = txtDescriptionOnPopUpForm.Text;//.ToOneLine();
            dropDownFormDescription.Hide();
        }

        private void txtDescription_Click(object sender, EventArgs e)
        {
            dropDownFormDescription.ShowShipmentDialogDropDown(this, txtDescription);
        }

        void dropDownForm_Deactivate(object sender, EventArgs e)
        {
            this.BringToFront();
            comboDropDownShipment.Focus();
            comboDropDownShipment.Text = txtShipmentOnPopUpForm.Text;//.ToOneLine();
            dropDownForm.Hide();
        }

        private void comboDropDownShipment_Click(object sender, EventArgs e)
        {
            dropDownForm.ShowShipmentDialogDropDown(this, comboDropDownShipment);
        }

        public Form1()
        {
            InitializeComponent();
            InitializeDropDownForm();
            InitializeTextBoxForm();
            InitializeTextExtControl();
        }

        private void InitializeTextBoxForm()
        {
            txtDescriptionOnPopUpForm = new TextBox();
            txtDescriptionOnPopUpForm.Multiline = true;
            txtDescriptionOnPopUpForm.ScrollBars = ScrollBars.Both;
            dropDownFormDescription = new ResizableDropDownForm();
            dropDownFormDescription.Hide();
            dropDownFormDescription.Controls.Add(txtDescriptionOnPopUpForm);
            txtDescriptionOnPopUpForm.Dock = DockStyle.Fill;
            dropDownFormDescription.MinimumSize = new Size(100, 40); // Set min size
            dropDownFormDescription.Size = new Size(txtDescription.Size.Width, txtDescription.Size.Height + 60); // Set min size
            dropDownFormDescription.Deactivate += new EventHandler(dropDownFormDescription_Deactivate);
            dropDownFormDescription.PinBottomRight = true; // Set where to pin
            txtDescription.ScrollBars = ScrollBars.Both;
        }

        private void InitializeDropDownForm()
        {
            txtShipmentOnPopUpForm = new TextBox();
            txtShipmentOnPopUpForm.Multiline = true;
            dropDownForm = new ResizableDropDownForm();
            dropDownForm.Hide();
            dropDownForm.Controls.Add(txtShipmentOnPopUpForm);
            txtShipmentOnPopUpForm.Dock = DockStyle.Fill;
            dropDownForm.MinimumSize = new Size(200, 60); // Set min size
            dropDownForm.Deactivate += new EventHandler(dropDownForm_Deactivate);
            dropDownForm.PinBottomRight = true; // Set where to pin
        }
    }

    public static class StringExtendion
    {
        public static string ToOneLine(this string str)
        {
            return ConvertLineToWhiteSpaces(str);
        }

        public static string ConvertLineToWhiteSpaces(this string value)
        {
            return Regex.Replace(value, System.Environment.NewLine, " ");
        }
    }
}