using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropDownComboBoxMultiLineEditor
{
    public interface IContextSensitiveHelper
    {
        string FormContextSenstiveHelpID { get; }
    }

    public partial class ReattachableBaseForm : Form, IContextSensitiveHelper
    {
        private bool IsGroupForm;

        public ReattachableBaseForm()
        {
            InitializeComponent();

            IsGroupForm = false;
        }

        public ReattachableBaseForm(bool isGroupForm)
        {
            InitializeComponent();

            IsGroupForm = isGroupForm;
        }

        /// <summary>
        /// Get unique ID based on the content of the form to assist context sensitive help mapping.
        /// </summary>
        public virtual string FormContextSenstiveHelpID { get { return null; } }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        private const Int32 WM_SYSCOMMAND = 0x112;   // A window receives this message when the user chooses a command from the Window menu
        private const Int32 WM_CLOSE = 0x10;   // A window receives this message to close the form
        private const Int32 MF_SEPARATOR = 0x800;    // Horizontal separator for the Window menu
        private const Int32 MF_STRING = 0x0;         // Specifies that the menu item is a text string
        private const Int32 IDM_ATTACHMM = 1000;       // ID of the new Attach to MineMarket menu item in Window menu
        private const Int32 IDM_ATTACHGF = 1001;       // ID of the new Attach to Group Form menu item in Window menu
        private const Int32 MF_BYPOSITION = 0x400;   // Puts the new Attach menu item at the specified position (0 in the InsertMenu means index zero top of the Window menu)
        private const String ATTACH_KEYBOARDACCELERATORMMMDI = "\tF7";   // Keyboard accelerator to attach the window to the tabbed MDI interface of the solution form
        private const String ATTACH_KEYBOARDACCELERATORGFMDI = "\tF9";   // Keyboard accelerator to attach the window to the tabbed MDI interface of the group form

        /// <summary>
        /// Add menu items when the window's handle is created.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // only create the custom system menu if this form is not Modal (or shown via ShowDialog())
            if (!this.Modal)
            {
                // Get a handle to a copy of this form's system (window) menu
                IntPtr hSysMenu = GetSystemMenu(this.Handle, false);

                // Add a separator
                InsertMenu(hSysMenu, 0, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);

                if (!IsGroupForm)
                {
                    // Add the Attach to MineMarket menu item
                    InsertMenu(hSysMenu, 0, MF_BYPOSITION, IDM_ATTACHMM, "TranslationManager.GlobalStrings.AttachToMineMarket" + ATTACH_KEYBOARDACCELERATORMMMDI);

                    // Add the Attach to Group Form menu item
                    InsertMenu(hSysMenu, 0, MF_BYPOSITION, IDM_ATTACHGF, "TranslationManager.GlobalStrings.AttachToGroupForm" + ATTACH_KEYBOARDACCELERATORGFMDI);
                }
                else
                {
                    // Add the Attach to MineMarket menu item
                    InsertMenu(hSysMenu, 0, MF_BYPOSITION, IDM_ATTACHMM, "TranslationManager.GlobalStrings.AttachToMineMarket");

                }
            }
        }


        /// <summary>
        /// This method intercepts windows handles to see if an action relates back to our Attach menu item click from the Window menu.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLOSE)
                this.Enabled = false;  // This will disable any existing text validation when the form is closed and the text controls are disposed

            base.WndProc(ref m);

            // Check if the item was selected from the system context menu
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == IDM_ATTACHMM))
            {
                // Reattach the form in the main tab strip
                this.WindowState = FormWindowState.Normal;
                //GUIModuleManager.AttachFormInTabStrip(this);
            }
            else if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == IDM_ATTACHGF))
            {
                // Reattach the form in the main tab strip
                this.WindowState = FormWindowState.Normal;
                //GUIModuleManager.AttachFormInGroupForm(this);
            }

        }

        protected void ShowHelp()
        {
            //IHelpManager manager = HelpManagerFactory.CreateHelpManager();
            //manager.ShowContextSensitiveHelp();
        }
    }
}
