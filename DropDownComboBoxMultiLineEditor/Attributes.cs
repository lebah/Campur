using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropDownComboBoxMultiLineEditor
{
    #region Translate Attribute
    /// <summary>
    /// Attribute that marks a private object in a class as translatable at runtime
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Serializable]
    public sealed class TranslateAttribute : Attribute
    {
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nativeText">The native text to be displayed if no translation is available</param>
        /// <param name="description">Description of where the string is to be used.</param>
        /// <param name="groupName">Used to group the attribute when displaying for translation ie "String" or "MenuItem" or "ButtonText" etc.</param>
        public TranslateAttribute(string nativeText, string description, TransGroupCategory groupName)
        {
            NativeText = nativeText;
            Description = description;
            GroupName = groupName;
        }

        #endregion

        #region Public properties
        /// <summary>
        /// Property. Describes the purpose of the string that has been marked for translation 
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Property. Native text to be displayed if not translation is available.
        /// </summary>
        public string NativeText { get; private set; }

        /// <summary>
        /// Group name
        /// </summary>
	    public TransGroupCategory GroupName { get; private set; }

        /// <summary>
        ///  A name given to the attribute used to group 
        ///  the string when displaying for translation.
        ///  I.E. if the thing being translated is a menuitem 
        ///  then groupname should be "MenuItem"
        /// </summary>
        public string GroupNameAsString
        {
            get
            {
                string returnString = "Unknown";
                switch (GroupName)
                {
                    case TransGroupCategory.ButtonText:
                        returnString = "Button Text";
                        break;
                    case TransGroupCategory.ContextMenu:
                        returnString = "Context Menu";
                        break;
                    case TransGroupCategory.EnumText:
                        returnString = "Enum Text";
                        break;
                    case TransGroupCategory.Exception:
                        returnString = "Exception";
                        break;
                    case TransGroupCategory.FormName:
                        returnString = "Form Name";
                        break;
                    case TransGroupCategory.Label:
                        returnString = "Label";
                        break;
                    case TransGroupCategory.MainMenuText:
                        returnString = "Main Menu Text";
                        break;
                    case TransGroupCategory.MainMenuTooltip:
                        returnString = "Main Menu Tooltip";
                        break;
                    case TransGroupCategory.MenuItem:
                        returnString = "Menu Item";
                        break;
                    case TransGroupCategory.String:
                        returnString = "String";
                        break;
                    case TransGroupCategory.TabName:
                        returnString = "Tab Name";
                        break;
                    case TransGroupCategory.ToolTip:
                        returnString = "Tool Tip";
                        break;
                    case TransGroupCategory.TreeNode:
                        returnString = "Tree Node";
                        break;
                    case TransGroupCategory.Verb:
                        returnString = "Verb";
                        break;
                }
                return returnString;
            }
        }

        #endregion
    }

    #endregion
}
