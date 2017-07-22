using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseWinGUI
{
    public partial class ResizableDropDownForm : ReattachableBaseForm
    {
        //public bool ResizableBorderLeft { get; set; }
        //public bool ResizableBorderRight { get; set; }
        //public bool ResizableBorderTop { get; set; }
        //public bool ResizableBorderBottom { get; set; }

        private bool mPinTopLeft;
        private bool mPinTopRight;
        private bool mPinBottomLeft;
        private bool mPinBottomRight;

        public bool PinTopLeft
        {
            get
            {
                return mPinTopLeft;
            }
            set
            {
                mPinTopLeft = value;
                mPinTopRight = mPinBottomLeft = mPinBottomRight = !value;
            }
        }
        public bool PinTopRight
        {
            get
            {
                return mPinTopRight;
            }
            set
            {
                mPinTopRight = value;
                mPinTopLeft = mPinBottomLeft = mPinBottomRight = !value;
            }
        }
        public bool PinBottomRight
        {
            get
            {
                return mPinBottomRight;
            }
            set
            {
                mPinBottomRight = value;
                mPinTopLeft = mPinTopRight = mPinBottomLeft = !value;
            }
        }
        public bool PinBottomLeft
        {
            get
            {
                return mPinBottomLeft;
            }
            set
            {
                mPinBottomLeft = value;
                mPinTopLeft = mPinTopRight = mPinBottomRight = !value;
            }
        }

        public ResizableDropDownForm()
        {
            InitializeComponent();

            this.Text = string.Empty;
            this.ControlBox = false;
            this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.ShowInTaskbar = false;
            this.MinimumSize = new Size(100, 40);

            //ResizableBorderBottom = false;
            //ResizableBorderLeft = false;
            //ResizableBorderTop = false;
            //ResizableBorderRight = false;

            PinBottomLeft = true;   // default
        }

        public void ShowShipmentDialogDropDown(Form parentForm, Control dropDownFromControl)
        {
            this.StartPosition = FormStartPosition.Manual;

            // Position the form
            if (PinTopLeft)
            {
                Point clientPoint = parentForm.PointToScreen(new Point(dropDownFromControl.Left, dropDownFromControl.Top));
                this.Top = clientPoint.Y - this.Height;
                this.Left = clientPoint.X;
            }
            else if (PinBottomLeft)
            {
                Point clientPoint = parentForm.PointToScreen(new Point(dropDownFromControl.Left, dropDownFromControl.Bottom));
                this.Top = clientPoint.Y;
                this.Left = clientPoint.X;
            }
            else if (PinTopRight)
            {
                Point clientPoint = parentForm.PointToScreen(new Point(dropDownFromControl.Right, dropDownFromControl.Top));
                this.Top = clientPoint.Y - this.Height;
                this.Left = clientPoint.X - this.Width;
            }
            else if (PinBottomRight)
            {
                Point clientPoint = parentForm.PointToScreen(new Point(dropDownFromControl.Right, dropDownFromControl.Bottom));
                this.Top = clientPoint.Y;
                this.Left = clientPoint.X - this.Width;

            }

            this.Visible = false;   // this makes sure that we are not displaying an already displayed form (we should not get to this normally)
            this.Show(parentForm);

        }

        public void ShowDialogDropDown(UserControl ucParent, Rectangle dropDownControlRectangle)
        {
            this.StartPosition = FormStartPosition.Manual;

            // Position the form
            if (PinTopLeft)
            {
                Point clientPoint = ucParent.PointToScreen(new Point(dropDownControlRectangle.Left, dropDownControlRectangle.Top));
                this.Top = clientPoint.Y - this.Height;
                this.Left = clientPoint.X;
            }
            else if (PinBottomLeft)
            {
                Point clientPoint = ucParent.PointToScreen(new Point(dropDownControlRectangle.Left, dropDownControlRectangle.Bottom));
                this.Top = clientPoint.Y;
                this.Left = clientPoint.X;
            }
            else if (PinTopRight)
            {
                Point clientPoint = ucParent.PointToScreen(new Point(dropDownControlRectangle.Right, dropDownControlRectangle.Top));
                this.Top = clientPoint.Y - this.Height;
                this.Left = clientPoint.X - this.Width;
            }
            else if (PinBottomRight)
            {
                Point clientPoint = ucParent.PointToScreen(new Point(dropDownControlRectangle.Right, dropDownControlRectangle.Bottom));
                this.Top = clientPoint.Y;
                this.Left = clientPoint.X - this.Width;

            }

            this.Visible = false;   // this makes sure that we are not displaying an already displayed form (we should not get to this normally)
            this.Show(ucParent);

        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                if (PinTopLeft)
                {
                    switch (m.Result.ToInt32())
                    {
                        // convert unwanted border resizing to HTCLIENT
                        case 10: m.Result = (IntPtr)1; break;   // Fixed Left
                        case 15: m.Result = (IntPtr)1; break;   // Fixed Bottom
                        case 16: m.Result = (IntPtr)1; break;   // Fixed Bottom Left
                        case 13: m.Result = (IntPtr)12; break;  // Top Left to Top
                        case 17: m.Result = (IntPtr)11; break;  // Bottom Right to Right
                        default:
                            break;
                    }
                }
                else if (PinBottomLeft)
                {
                    switch (m.Result.ToInt32())
                    {
                        // convert unwanted border resizing to HTCLIENT
                        case 10: m.Result = (IntPtr)1; break;   // Fixed Left
                        case 12: m.Result = (IntPtr)1; break;   // Fixed Top
                        case 13: m.Result = (IntPtr)1; break;   // Fixed Top Left
                        case 14: m.Result = (IntPtr)11; break;  // Top Right to Right
                        case 16: m.Result = (IntPtr)15; break;  // Bottom Left to Bottom
                        default:
                            break;
                    }
                }
                else if (PinTopRight)
                {
                    switch (m.Result.ToInt32())
                    {
                        // convert unwanted border resizing to HTCLIENT
                        case 11: m.Result = (IntPtr)1; break;   // Fixed Right
                        case 15: m.Result = (IntPtr)1; break;   // Fixed Bottom
                        case 17: m.Result = (IntPtr)1; break;   // Fixed Bottom Right
                        case 14: m.Result = (IntPtr)12; break;  // Top Right to Top
                        case 16: m.Result = (IntPtr)10; break;  // Bottom Left to Left
                        default:
                            break;
                    }
                }
                else if (PinBottomRight)
                {
                    switch (m.Result.ToInt32())
                    {
                        // convert unwanted border resizing to HTCLIENT
                        case 11: m.Result = (IntPtr)1; break;   // Fixed Right
                        case 12: m.Result = (IntPtr)1; break;   // Fixed Top
                        case 14: m.Result = (IntPtr)1; break;   // Fixed Top Right
                        case 13: m.Result = (IntPtr)10; break;  // Top Left to Left
                        case 17: m.Result = (IntPtr)15; break;  // Bottom Right to Bottom
                        default:
                            break;
                    }
                }
            }
        }

        enum HitTestValues
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21
        }

    }
}
