using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropDownComboBoxMultiLineEditor
{
    #region TransGroupCategory
    /// <summary>
    /// Public Enumeration. Used to determine the group into which a translation falls.
    /// </summary>
    public enum TransGroupCategory
    {
        /// <summary>
        /// Translation falls into a string category
        /// </summary>
        String,

        /// <summary>
        /// Translation falls into a Context Menu category
        /// </summary>
        ContextMenu,

        /// <summary>
        /// Translation falls into a Menu Item category
        /// </summary>
        MenuItem,

        /// <summary>
        /// Translation falls into a Tool Tip category
        /// </summary>
        ToolTip,

        /// <summary>
        /// Translation falls into a Tree Node category
        /// </summary>
        TreeNode,

        /// <summary>
        /// Translation falls into a Button Text category
        /// </summary>
        ButtonText,

        /// <summary>
        /// Translation falls into a Tab Name category
        /// </summary>
        TabName,

        /// <summary>
        /// Translation falls into a Verb category
        /// </summary>
        Verb,

        /// <summary>
        /// Translation falls into the exception category
        /// </summary>
        Exception,

        /// <summary>
        /// Translation falls into the exception category
        /// </summary>
        EnumText,

        /// <summary>
        /// Translation is for a form's caption
        /// </summary>
        FormName,

        /// <summary>
        /// Translation is for a label
        /// </summary>
        Label,
        /// <summary>
        /// Translation is for the main GUI form's menu items
        /// </summary>
        MainMenuText,
        /// <summary>
        /// Translations is for the main GUI form's menu and toolbutton tooltips
        /// </summary>
        MainMenuTooltip
    }

    #endregion
}
