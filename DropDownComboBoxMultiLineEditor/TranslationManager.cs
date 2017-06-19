
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropDownComboBoxMultiLineEditor
{
    #region TranslationManager
    /// <summary>
    /// Summary description for TranslationManager.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class TranslationManager
    {
        #region Constants ...

        private const string NativeTranslationName = "NATIVEBULKTRAK";

        #endregion

        #region Translations

        [Translate("Translation '{0}' already exist in the system, do you want to replace it with the new translation?", "The text displayed when a translation that already exist in the system is being imported", TransGroupCategory.TabName)]
        private static string mReplaceTranslation = "";

        #endregion

        #region Private Static declarations

        private static object mLockObject = new object();
        private static Translation mUsersTranslation = null;
        private static ArrayList mAllTranslations = null;
        private static bool mAllTransLoaded = false;
        private static ArrayList mRemovedObjects = new ArrayList(0);
        //private static TranslationItem mTransItem = null;
        private static int mTranslationCount;
        private static Hashtable mObjectPropertyDescriptors = new Hashtable();
        /// <summary>
        /// Public boolean used to test the translation mechanism.
        /// Should be set to false normally
        /// </summary>
        private static bool mTranslateAllToHashForTesting = false;
        private static Hashtable objPropFriendlyNames = new Hashtable();
        private static Hashtable objFriendlyNames = new Hashtable();
        private static Hashtable objCategory = new Hashtable();
        private static Hashtable mAllEnums = new Hashtable();
        private static Dictionary<string, NativeTranslationItem> mNativeTranslations;
        private static HashSet<string> mHaveLoadedStaticTranslationsForAssembly = new HashSet<string>();

        #endregion

        #region Public Static declarations

        /// <summary>
        /// Method used to retrieve the display name of the object.
        /// </summary>
        /// <param name="type">The type to retrieve the name of.</param>
        /// <returns>Returns the translated name of the object.</returns>
        public static string GetDisplayName(Type type)
        {
            InitialiseTranslationManager();
            if (type == null)
                return string.Empty;
            string name = type.Name;

            //Check to see if the name exists in the cache.
            if (objFriendlyNames.ContainsKey(type))
            {
                name = (string)objFriendlyNames[type];
            }
            else
            {
                //Name didn't exist in the cache so load the cache.
                LoadObjectNames(type);
                //Retrieve the name from the cache.
                if (objFriendlyNames.Contains(type))
                {
                    name = (string)objFriendlyNames[type];
                }
            }
            return name;
        }

        private static void LoadObjectNames(Type type)
        {
            object[] objectAtts = type.GetCustomAttributes(typeof(ObjectCategoryAttribute), true);
            if (objectAtts.Length > 0)
            {
                ObjectCategoryAttribute objectAtt = (ObjectCategoryAttribute)objectAtts[0];
                objFriendlyNames.Add(type, LoadTranslationString("ObjectCategory." + type.FullName + ".Name", objectAtt.NativeName));
                objCategory.Add(type, LoadTranslationString("ObjectCategory." + type.FullName + ".Category", objectAtt.NativeCategory));
            }
        }


        /// <summary>
        /// Method used to retrieve the category of the object.
        /// </summary>
        /// <param name="type">The type to retrieve the category for.</param>
        /// <returns>Returns the translated category of the object.</returns>
        public static string GetCategory(Type type)
        {
            string category = "";

            //Check to see if the category exists in the cache.
            if (objCategory.ContainsKey(type))
            {
                category = (string)objCategory[type];
            }
            else
            {
                //category didn't exist in the cache so load the cache
                LoadObjectNames(type);
                //Get the category from the cache.
                if (objCategory.Contains(type))
                {
                    category = (string)objCategory[type];
                }
            }
            return category;
        }

        /// <summary>
        /// Used to retrieve a list of properties friendly names for this object.
        /// </summary>
        /// <returns>Returns a list of properties friendly names for this object keyed from its property name.</returns>
        public static PropertyDescriptorCollection GetPropertiesFriendlyNames(Type type, Attribute[] attributes)
        {
            PropertyDescriptorCollection ret = new PropertyDescriptorCollection(null);

            //Get the properties of this object
            PropertyDescriptorCollection propDescCol = TypeDescriptor.GetProperties(type, attributes);
            foreach (PropertyDescriptor propDesc in propDescCol)
            {
                if (propDesc.PropertyType.IsSubclassOf(typeof(BaseCollectionHashTable)) && propDesc.Attributes.Contains(new ExpandableCollectionAttribute()))
                {
                    //manufacture a new propertydescriptor (minus the UITypeEditor attribute)
                    ret.Add(new TranslatedPropertyDescriptor(propDesc, type, true));
                }
                else
                {
                    //Need to pass in the owner object type for use with translation.
                    ret.Add(new TranslatedPropertyDescriptor(propDesc, type));
                }
            }
            return ret;
        }
        /// <summary>
        /// Used to retrieve a list of properties friendly names for this object.
        /// </summary>
        /// <returns>Returns a list of properties friendly names for this object keyed from its property name.</returns>
        public static PropertyDescriptorCollection GetPropertiesFriendlyNames(Type type)
        {
            return GetPropertiesFriendlyNames(type, null);
        }

        /// <summary>
        /// String used to prefix/suffix all translated strings for testing
        /// </summary>
        public static string TranslateAllToHashForTestingString = "#";


        /// <summary>
        /// The translation ID for the current user. If there is no current user 
        /// (i.e. on the server) then it will be a null BTKey
        /// </summary>
        public static BTKey TranslationID = BTKey.NullKey;
        #endregion

        #region Public static properties
        /// <summary>
        /// Gets or Sets whether or not the strings all need to have a 
        /// special character placed in the front and end of the word to 
        /// highlight words that have been translated.
        /// Only used to test translation.
        /// </summary>
        public static bool TranslateAllToHashForTesting
        {
            get
            {
                return mTranslateAllToHashForTesting;
            }
            set
            {
                mTranslateAllToHashForTesting = value;
            }
        }
        /// <summary>
        /// Returns a count of the number of translations.
        /// </summary>
        public static int TranslationCount
        {
            get
            {
                return mAllTranslations.Count;
            }
        }
        #endregion

        #region Public Static methods
        /// <summary>
        /// Returns a list of native translation items which have a part or all of their native text values match a search criteria
        /// </summary>
        /// <param name="searchString">The text to search for</param>
        /// <param name="matchCase">Whether the search is case-sensitive</param>
        /// <param name="matchWholeWord">Whether to match against the whole word or the search string is contained in the native text</param>
        /// <returns></returns>
        public static List<NativeTranslationItem> SearchForTranslationString(string searchString, bool matchCase, bool matchWholeWord)
        {
            List<NativeTranslationItem> retlist = new List<NativeTranslationItem>();
            StringComparison sc = matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

            foreach (NativeTranslationItem item in mNativeTranslations.Values)
            {
                if (matchWholeWord)
                {
                    if (string.Compare(item.NativeText, searchString, sc) == 0)
                        retlist.Add(item);
                }
                else if (item.NativeText.IndexOf(searchString, sc) >= 0)
                    retlist.Add(item);
            }

            return retlist;
        }

        /// <summary>
        /// Returns a list of translation items which have a part or all of their translated values match a search criteria
        /// </summary>
        /// <param name="translations">List of the translations to search against</param>
        /// <param name="searchString">The text to search for</param>
        /// <param name="matchCase">Whether the search is case-sensitive</param>
        /// <param name="matchWholeWord">Whether to match against the whole word or the search string is contained in the translation</param>
        /// <returns></returns>
        public static List<TranslationItem> SearchForTranslationString(Translation[] translations, string searchString, bool matchCase, bool matchWholeWord)
        {
            List<TranslationItem> retlist = new List<TranslationItem>();

            if (!string.IsNullOrEmpty(searchString))
            {
                foreach (Translation translation in translations)
                {
                    StringComparison sc = matchCase ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;

                    foreach (TranslationItem item in translation.TranslationItems)
                    {
                        if (string.IsNullOrEmpty(item.TranslatedString)) continue;

                        if (matchWholeWord)
                        {
                            if (string.Compare(item.TranslatedString, searchString, sc) == 0)
                                retlist.Add(item);
                        }
                        else if (item.TranslatedString.IndexOf(searchString, sc) >= 0)
                            retlist.Add(item);
                    }
                }
            }

            return retlist;
        }

        public static NativeTranslationItem GetNativeTranslation(string key)
        {
            if (mNativeTranslations.ContainsKey(key))
                return mNativeTranslations[key];
            return null;
        }

        /// <summary>
        /// Public Static. Method used to initialise the translation 
        /// manager
        /// </summary>
        public static void InitialiseTranslationManager()
        {
            lock (mLockObject)
            {
                if (mNativeTranslations == null)
                    mNativeTranslations = new Dictionary<string, NativeTranslationItem>();
                if (mAllTranslations == null)
                    mAllTranslations = new ArrayList(0);
                if (mUsersTranslation == null)
                    PreloadTranslation();
            }
        }

        /// <summary>
        /// Static Method.  Used to create a new translation from an existing translation
        /// </summary>
        /// <param name="existingTranslation">A reference to the translation to be copied. Uses the native translation if null is passed</param>
        /// <param name="transName">The name of the new translation</param>
        /// <returns></returns>
        public static ArrayList CreateFromExistingTranslation(Translation existingTranslation, string transName)
        {
            if (HasTranslationWithName(transName))
                throw new TranslationAlreadyExists(transName);

            if (existingTranslation == null)
            {
                if (mNativeTranslations.Count == 0)
                    LoadNativeTranslations();

                Translation newTranslation = new Translation(transName);

                foreach (NativeTranslationItem item in mNativeTranslations.Values)
                    newTranslation.TranslationItems.Add(item.GetNewTranslationItem());

                mAllTranslations.Add(newTranslation);
            }
            else
            {
                Translation newTranslation = existingTranslation.Copy(transName);
                mAllTranslations.Add(newTranslation);
            }

            return mAllTranslations;
        }

        /// <summary>
        /// Static Method.  Used to check to see if there is a translation with the given name
        /// </summary>
        /// <param name="transName">Name to check against</param>
        /// <returns>True if there is a translation with that name, false otherwise</returns>
        public static bool HasTranslationWithName(string transName)
        {
            foreach (Translation trans in mAllTranslations)
            {
                if (string.Compare(trans.Name, transName, true) == 0)
                    return true;
            }

            return (string.Compare(NativeTranslationName, transName, true) == 0);
        }

        /// <summary>
        /// Static Method. Used to load the translations for a given form or control.
        /// </summary>
        /// <param name="transObject">Any object which needs to be translated</param>
        public static void LoadTranslation(Object transObject)
        {
            string baseTranslationItemKey = transObject.GetType().FullName;

            if (mUsersTranslation == null)
                PreloadTranslation();

            //get reference to private members
            FieldInfo[] fields = transObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (FieldInfo field in fields)
            {
                //If there is a Translate string attribute then set the text value to that.
                if (field.IsDefined(typeof(TranslateAttribute), true))
                {
                    TranslateAttribute transAtt = (TranslateAttribute)((object[])field.GetCustomAttributes(typeof(TranslateAttribute), true)).GetValue(0);

                    string fieldName = field.Name;
                    string translatedValue;
                    TranslationItem transItem = (TranslationItem)mUsersTranslation.TranslationItems[baseTranslationItemKey + "." + fieldName];
                    if (transItem == null)
                        translatedValue = transAtt.NativeText;
                    else
                        translatedValue = transItem.TranslatedString;

                    //Testing mechanism for translation
                    if (TranslateAllToHashForTesting)
                        translatedValue = TranslateAllToHashForTestingString + translatedValue + TranslateAllToHashForTestingString;

                    field.SetValue(transObject, translatedValue);
                }
            }

        }

        /// <summary>
        /// Static Method. Used to get the string translation for a key.
        /// </summary>
        /// <param name="translationItemKey">Unique key used to locate the translationItem</param>
        /// <param name="defaultValue">Value to return if there is no translation</param>
        /// <returns></returns>
        public static string LoadTranslationString(string translationItemKey, string defaultValue)
        {
            string returnString = defaultValue;

            if (mUsersTranslation == null)
                PreloadTranslation();

            TranslationItem transItem = mUsersTranslation.TranslationItems[translationItemKey] as TranslationItem;
            if (transItem != null)
                returnString = transItem.TranslatedString;

            if (TranslateAllToHashForTesting)
                returnString = TranslateAllToHashForTestingString + returnString + TranslateAllToHashForTestingString;

            return returnString;
        }

        /// <summary>
        /// Static Method. Used to load all of the translation 
        /// both from the DB and the native translations.
        /// </summary>
        public static void LoadAllTranslations()
        {
            if (!mAllTransLoaded)
            {
                //Load the native object translations
                LoadNativeTranslations();

                //Load all of the translations from the database.
                ArrayList allTranslations = Translation.GetFactory().GetByCriteria();
                foreach (Translation trans in allTranslations)
                {
                    //Add the translation to the translations arraylist
                    if (!mAllTranslations.Contains(trans))
                        mAllTranslations.Add(trans);
                }

                mAllTransLoaded = true;
                mTranslationCount = mAllTranslations.Count;
            }
        }

        public static ArrayList GetAllTranslations()
        {
            if (!mAllTransLoaded)
                LoadAllTranslations();
            ArrayList ret = new ArrayList(mAllTranslations);
            return ret;
        }

        /// <summary>
        /// Loads the translation items into an arraylist for the Native BulkTrak translation object.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ArrayList LoadNativeTranslations(TransGeneralType type)
        {
            ArrayList translations = new ArrayList(0);

            if (mNativeTranslations.Count == 0)
                LoadNativeTranslations();

            foreach (NativeTranslationItem item in mNativeTranslations.Values)
            {
                if (item.ObjectType == type)
                    translations.Add(item);
            }

            return translations;
        }

        /// <summary>
        /// Static Method. Used to add another translation.
        /// </summary>
        /// <param name="translationName">The name of the new translation</param>
        /// <returns></returns>
        public static ArrayList AddNewTranslation(string translationName)
        {
            Translation newTranslation = new Translation(translationName);
            mAllTranslations.Add(newTranslation);
            return mAllTranslations;

        }
        /// <summary>
        /// Method used to save all translations
        /// </summary>
        public static void RemoveTranslation(Translation trans)
        {
            if (trans != null)
            {
                if (trans.IsReferenced())
                    throw new UnableToDeleteTranslationException();

                foreach (TranslationItem transItem in trans.TranslationItems)
                    transItem.Delete();

                trans.TranslationItems.Clear();
                trans.Delete();
                mAllTranslations.Remove(trans);
            }
        }

        /// <summary>
        /// Method used to get a string representation of an object type for
        /// general grouping on a translation item
        /// </summary>
        /// <param name="objType"></param>
        /// 		/// <returns>Enum indicating the general type of the translation</returns>
        public static TransGeneralType GetGeneralObjectType(Type objType)
        {
            TransGeneralType type = TransGeneralType.Other;

            if (objType.IsSubclassOf(typeof(PersistentObject)) || objType.IsSubclassOf(typeof(BTBusinessObject)))
                type = TransGeneralType.BusinessObject;
            else if (objType.IsSubclassOf(typeof(Form)) || objType.IsSubclassOf(typeof(UserControl)))
                type = TransGeneralType.Form;
            //            else if (objType.GetInterface("IKeyedObject", true) != null)
            //                type = TransGeneralType.IKeyedObject;
            else if (objType.IsSubclassOf(typeof(Exception)))
                type = TransGeneralType.Exception;
            else if (objType == typeof(TranslationManager.GlobalStrings))
                type = TransGeneralType.GlobalString;
            else if (objType.IsSubclassOf(typeof(Northwoods.Go.GoObject)))
                type = TransGeneralType.DiagramObject;
            else if (objType == typeof(GUI.GUIModuleManager))
                type = TransGeneralType.GUIModuleManager;
            else if (objType == typeof(Enum) || objType.IsSubclassOf(typeof(Enum)))
                type = TransGeneralType.Enum;
            else
                type = TransGeneralType.Other;

            return type;
        }

        /// <summary>
        /// A method used to load all of the static string translations 
        /// for a given assembly.
        /// </summary>
        /// <param name="assembly">The assembly to load the static 
        /// string translations for.</param>
        public static void LoadStaticStringTranslations(Assembly assembly)
        {
            lock (mLockObject)
            {
                if (mUsersTranslation == null)
                    PreloadTranslation();

                if (!mHaveLoadedStaticTranslationsForAssembly.Add(assembly.FullName))
                    return;

                Type[] types = assembly.GetTypesEx();

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] != null)
                    {
                        FieldInfo[] fields = types[i].GetFields(BindingFlags.NonPublic | BindingFlags.Static);

                        foreach (FieldInfo field in fields)
                        {
                            object[] objAtt = (object[])field.GetCustomAttributes(typeof(TranslateAttribute), true);
                            if (objAtt.Length > 0)
                            {
                                TranslateAttribute transAtt = (TranslateAttribute)objAtt.GetValue(0);
                                if (transAtt != null)
                                {
                                    TranslationItem transItem = (TranslationItem)mUsersTranslation.TranslationItems[types[i].FullName + "." + field.Name];
                                    if (transItem != null)
                                    {
                                        string transString = transItem.TranslatedString;
                                        if (TranslateAllToHashForTesting)
                                            transString = TranslateAllToHashForTestingString + transString + TranslateAllToHashForTestingString;

                                        field.SetValue(null, transString);
                                    }
                                    else
                                    {
                                        string transString = transAtt.NativeText;
                                        if (TranslateAllToHashForTesting)
                                            transString = TranslateAllToHashForTestingString + transString + TranslateAllToHashForTestingString;

                                        field.SetValue(null, transString);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static string GetTranslatedString(Type objectType, string propertyName)
        {
            string returnString = propertyName;
            bool foundProperty = false;
            PropertyDescriptorCollection propDescCol = mObjectPropertyDescriptors[objectType] as PropertyDescriptorCollection;
            if (propDescCol == null)
            {
                propDescCol = GetPropertiesFriendlyNames(objectType, new Attribute[0]);
                mObjectPropertyDescriptors.Add(objectType, propDescCol);
            }

            if (propDescCol != null)
            {
                foreach (TranslatedPropertyDescriptor tpd in propDescCol)
                {
                    if (tpd.Name.ToUpper() == propertyName.ToUpper())
                    {
                        returnString = tpd.DisplayName;
                        foundProperty = true;
                        break;
                    }
                }
            }

            Debug.Assert(foundProperty, "Didn't find a property of the name: " + propertyName + " on the object type: " + objectType.Name + " for translation");

            return returnString;
        }

        public static string GetTranslatedDescription(Type objectType, string propertyName)
        {
            string returnString = string.Empty;
            bool foundProperty = false;
            PropertyDescriptorCollection propDescCol = mObjectPropertyDescriptors[objectType] as PropertyDescriptorCollection;
            if (propDescCol == null)
            {
                propDescCol = GetPropertiesFriendlyNames(objectType, new Attribute[0]);
                mObjectPropertyDescriptors.Add(objectType, propDescCol);
            }

            if (propDescCol != null)
            {
                foreach (TranslatedPropertyDescriptor tpd in propDescCol)
                {
                    if (tpd.Name.ToUpper() == propertyName.ToUpper())
                    {
                        returnString = tpd.Description;
                        foundProperty = true;
                        break;
                    }
                }
            }

            Debug.Assert(foundProperty, "Didn't find a property of the name: " + propertyName + " on the object type: " + objectType.Name + " for translation");

            return returnString;
        }

        public static string GetFormattedTranslation(string translated, params object[] param)
        {
            return string.Format(translated, param);
        }

        public static void ExportTranslation(Translation trans)
        {
            string filename;
            System.Windows.Forms.SaveFileDialog savedlg = new SaveFileDialog();
            savedlg.DefaultExt = "Zip";
            savedlg.AddExtension = true;
            savedlg.Filter = "Zip Files (*.zip)|*.zip| Xml Files (*.xml)|*.xml";

            if (savedlg.ShowDialog() == DialogResult.OK)
            {
                filename = savedlg.FileName;

                if (Path.GetExtension(filename).ToUpper() == ".ZIP")
                {
                    MemoryStream ms = new MemoryStream();
                    XmlTextWriter writer = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
                    writer.Formatting = System.Xml.Formatting.Indented;
                    XMLSerializer serializer = new XMLSerializer();
                    serializer.Serialize(writer, new ArrayList(new object[] { trans }), "Translation", false, false, false);

                    writer.Flush();

                    FileStream fs = null;
                    if (File.Exists(filename))
                        fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                    else
                        fs = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite);

                    GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, true);
                    zipStream.Write(ms.ToArray(), 0, (int)ms.Length);
                    zipStream.Close();
                    //fs.Position = 0;

                    fs.Flush();
                    fs.Close();
                    writer.Close();
                }
                else if (Path.GetExtension(filename).ToUpper() == ".XML")
                {
                    XmlTextWriter writer = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
                    writer.Formatting = System.Xml.Formatting.Indented;
                    XMLSerializer serializer = new XMLSerializer();
                    serializer.Serialize(writer, trans, "Translation");
                }
                else
                    throw new ApplicationException("Please select either an XML or ZIP file type for export.");
            }
        }

        public static void ImportTranslation()
        {
            System.Windows.Forms.OpenFileDialog opendlg = new OpenFileDialog();
            string filename;

            opendlg.DefaultExt = "Zip";
            opendlg.AddExtension = true;
            opendlg.Filter = "Zip Files (*.zip)|*.zip| Xml Files (*.xml)|*.xml";

            if (opendlg.ShowDialog() == DialogResult.OK)
            {
                filename = opendlg.FileName;

                if (Path.GetExtension(filename).ToUpper() == ".ZIP")
                {
                    FileStream sourceFile = new FileStream(filename, FileMode.Open, FileAccess.Read);

                    GZipStream zipStream = new GZipStream(sourceFile, CompressionMode.Decompress, true);

                    byte[] bufferWrite = new byte[4];
                    sourceFile.Position = (int)sourceFile.Length - 4;
                    sourceFile.Read(bufferWrite, 0, 4);
                    sourceFile.Position = 0;
                    int bufferLength = BitConverter.ToInt32(bufferWrite, 0);

                    byte[] buffer = new byte[bufferLength + 100];
                    int readOffset = 0;
                    int totalBytes = 0;

                    // Loop through the compressed stream and put it into the buffer
                    while (true)
                    {
                        int bytesRead = zipStream.Read(buffer, readOffset, 100);

                        // If we reached the end of the data
                        if (bytesRead == 0)
                            break;

                        readOffset += bytesRead;
                        totalBytes += bytesRead;
                    }

                    // Close the streams
                    sourceFile.Close();
                    zipStream.Close();

                    MemoryStream ms = new MemoryStream(buffer);
                    XmlTextReader reader = new XmlTextReader(ms);
                    XMLSerializer serializer = new XMLSerializer();
                    Translation trans = (Translation)serializer.Deserialize(reader, "Translation", typeof(Translation))[0];
                    if (trans != null)
                    {
                        if (HasTranslationWithName(trans.Name))
                        {
                            DialogResult result = MessageBox.Show(String.Format(mReplaceTranslation, trans.Name), TranslationManager.GlobalStrings.Warning, MessageBoxButtons.YesNoCancel);
                            if (result == DialogResult.Yes)
                                ReplaceExistingTranslation(trans.Name, trans);
                            else if (result == DialogResult.No)
                            {
                                mAllTranslations.Add(trans.Copy(HasTranslationWithName(trans.Name) ? trans.Name + (TranslationCount + 1) : trans.Name));
                                trans.Delete();
                            }
                        }
                        else
                        {
                            mAllTranslations.Add(trans.Copy(trans.Name));
                            trans.Delete();
                        }
                    }
                }
                else if (Path.GetExtension(filename).ToUpper() == ".XML")
                {
                    XmlTextReader reader = new XmlTextReader(filename);
                    XMLSerializer serializer = new XMLSerializer();
                    Translation trans = (Translation)serializer.Deserialize(reader, "Translation", typeof(Translation))[0];
                    if (trans != null)
                    {
                        if (HasTranslationWithName(trans.Name))
                        {
                            DialogResult result = MessageBox.Show(String.Format(mReplaceTranslation, trans.Name), TranslationManager.GlobalStrings.Warning, MessageBoxButtons.YesNoCancel);
                            if (result == DialogResult.Yes)
                                ReplaceExistingTranslation(trans.Name, trans.Copy(trans.Name));
                            else if (result == DialogResult.No)
                            {
                                mAllTranslations.Add(trans.Copy(HasTranslationWithName(trans.Name) ? trans.Name + (TranslationCount + 1) : trans.Name));
                                trans.Delete();
                            }
                        }
                        else
                        {
                            mAllTranslations.Add(trans.Copy(trans.Name));
                            trans.Delete();
                        }
                    }
                }
                else
                    throw new ApplicationException("Please select a valid translation to import (XML or Zip)");
            }
        }

        /// <summary>
        /// Replace an existing translation with an new one with the same name
        /// </summary>
        /// <param name="translationname"></param>
        /// <param name="newtranslation"></param>
        /// <returns></returns>
        public static void ReplaceExistingTranslation(string translationname, Translation newtranslation)
        {
            Translation removetrans = null;
            foreach (Translation trans in mAllTranslations)
            {
                if (string.Compare(trans.Name, translationname, true) == 0)
                {
                    removetrans = trans;
                    break;
                }
            }

            if (removetrans != null && newtranslation != null)
            {
                //SC3940: Remove all translation items from existing translation in order to preserve the ID if the translation is in use
                removetrans.TranslationItems.Clear();
                foreach (TranslationItem transItem in removetrans.TranslationItems.GetList())
                {
                    removetrans.TranslationItems.Remove(transItem);
                    transItem.Delete();
                }

                foreach (TranslationItem transItem in newtranslation.TranslationItems)
                    removetrans.TranslationItems.Add(transItem.Copy());

                newtranslation.Delete();//all info has been copied into the existing translation.
            }
        }


        /// <summary>
        /// Gets an individual enum wrapper for a particular enum value.
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="transEnum"></param>
        /// <returns></returns>
        public static EnumWrapper GetEnumTranslatedString(Type enumType, Enum transEnum)
        {
            if (!mAllEnums.ContainsKey(enumType))
                LoadEnumWrappers(enumType);

            return ((Hashtable)mAllEnums[enumType])[transEnum] as EnumWrapper;
        }

        /// <summary>
        /// Gets an arraylist of translated enum wrappers for a particular 
        /// enum type for the specified enum values.
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="enums"></param>
        /// <returns></returns>
        public static ArrayList GetEnumTranslatedString(Type enumType, ArrayList enums)
        {
            ArrayList ret = new ArrayList();
            foreach (Enum e in enums)
                ret.Add(GetEnumTranslatedString(enumType, e));
            return ret;
        }

        /// <summary>
        /// Gets an arraylist of translated enum wrappers for a particular enum type.
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static ArrayList GetEnumTranslatedString(Type enumType)
        {
            return GetEnumTranslatedString(enumType, new ArrayList(Enum.GetValues(enumType)));
        }

        /// <summary>
        /// Gets an arraylist of all translated enum wrappers for a particular enum type, excluding a specified collection of enum values.
        /// </summary>
        /// <param name="enumValuesToExclude">The collection of enum values to exclude.</param>
        /// <param name="enumType">The enum type to get the collection of enum wrappers for.</param>
        /// <returns></returns>
        public static ArrayList GetEnumTranslatedString(IEnumerable enumValuesToExclude, Type enumType)
        {
            ArrayList enumValues = new ArrayList(Enum.GetValues(enumType));
            if (enumValuesToExclude != null)
            {
                foreach (Enum enumValue in enumValuesToExclude)
                    enumValues.Remove(enumValue);
            }
            return GetEnumTranslatedString(enumType, enumValues);
        }

        /// <summary>
        /// Strips a string of any ampersands.  This is so we can use menu item strings for things like tooltips, which don't support the ampersand indicating a keyboard shortcut.
        /// </summary>
        /// <param name="s">The string to modify.</param>
        /// <returns>The modified string which does not contain any ampersands.</returns>
        public static string StripAmpersand(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;
            else
                return s.Replace("&", string.Empty);
        }

        #endregion

        #region private static methods

        /// <summary>
        /// Static Method. Method used to load the native translations
        /// for all objects in BulkTrak.Business dll
        /// NOTE: if the native translations are going to be loaded for
        /// the BulkTrakGUI dll then the method LoadGUIDefaultTranslations
        /// on the class GUITranslationManager in the BulkTrakGUI needs to be called
        /// </summary>
        private static void LoadNativeTranslations()
        {
            mNativeTranslations = LoadNativeTranslations(TypeResolver.GetRegisteredAssemblies().Cast<Assembly>());
        }

        public static Dictionary<string, NativeTranslationItem> LoadNativeTranslations(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            Dictionary<string, Type> loadedTypes = new Dictionary<string, Type>();
            Dictionary<string, NativeTranslationItem> nativeTranslations = new Dictionary<string, NativeTranslationItem>();

            foreach (Assembly assembly in assemblies)
            {
                //Get the assembly with the object type of PersistentObject in it
                //Assembly assembly = Assembly.GetAssembly(typeof(BTBusinessObject));
                Type[] types = assembly.GetTypesEx();

                TransGeneralType generalObjectType;

                //Loop through all of the object types found in the assembly
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    //if (type.IsEnum)
                    //    Console.WriteLine("");

                    if (!loadedTypes.ContainsKey(type.FullName))
                    {
                        loadedTypes.Add(type.FullName, type);
                        //Get the general object type string for grouping
                        generalObjectType = TranslationManager.GetGeneralObjectType(type);

                        string baseTranslationKey = type.FullName;

                        #region Getting the objects attributes
                        object[] objectAtts = type.GetCustomAttributes(true);

                        if (objectAtts.Length > 0)
                        {
                            foreach (object objectAtt in objectAtts)
                                AddTranslationItem(objectAtt, type, null, nativeTranslations);
                        }
                        #endregion

                        #region Getting private Members
                        FieldInfo[] fields = null;
                        //Getting all of the private members for the types[i] object type
                        if (type.IsEnum)
                            fields = type.GetFields();
                        else
                            fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

                        foreach (FieldInfo field in fields)
                        {
                            //See if field has a translationAttribute associated with it.
                            object[] objAtt = (object[])field.GetCustomAttributes(typeof(TranslateAttribute), true);
                            if (objAtt.Length > 0)
                            {
                                //if (type.IsEnum)
                                //    Console.WriteLine("");
                                TranslateAttribute transAtt = (TranslateAttribute)objAtt.GetValue(0);
                                if (transAtt != null)
                                {
                                    //Only create the trans item if it doesn't alreay exist.
                                    if (!nativeTranslations.ContainsKey(baseTranslationKey + "." + field.Name))
                                    {
                                        NativeTranslationItem item = new NativeTranslationItem(baseTranslationKey + "." + field.Name);
                                        item.OwnerType = type;
                                        item.NativeText = transAtt.NativeText;
                                        item.Description = transAtt.Description;
                                        item.Category = transAtt.GroupNameAsString;
                                        item.ObjectType = generalObjectType;
                                        nativeTranslations.Add(item.Key, item);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Getting properties to retrieve friendly name, category, Mincom.MineMarket.DAL.Description attributes
                        PropertyInfo[] propInfos = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                        if (propInfos.Length > 0)
                        {
                            //Loop through all of the property infos
                            foreach (PropertyInfo propInfo in propInfos)
                            {
                                string targetPropName = propInfo.Name;

                                //Get all of the custom attributes for this property.
                                object[] propCustAtts = propInfo.GetCustomAttributes(true);
                                //Loop through all of the Attributes for this property.
                                foreach (object propAttObject in propCustAtts)
                                    AddTranslationItem(propAttObject, type, propInfo.Name, nativeTranslations);
                            }
                        }
                        #endregion
                    }
                }
            }

            return nativeTranslations;
        }

        private static void AddTranslationItem(object objectAtt, System.Type type, string targetName, Dictionary<string, NativeTranslationItem> nativeTranslations)
        {
            if (objectAtt is Mincom.MineMarket.DAL.ObjectCategoryAttribute)
            {
                #region Object Category
                Mincom.MineMarket.DAL.ObjectCategoryAttribute att = (Mincom.MineMarket.DAL.ObjectCategoryAttribute)objectAtt;
                Debug.Assert(type != null, "The type of this translation item should not be null");

                if (!nativeTranslations.ContainsKey("ObjectCategory." + type.FullName + ".Name"))
                {
                    NativeTranslationItem transName = new NativeTranslationItem("ObjectCategory." + type.FullName + ".Name");
                    transName.Category = "Object Category Name";
                    transName.NativeText = att.NativeName;
                    transName.TargetName = "Object Name";
                    transName.OwnerType = type;
                    nativeTranslations.Add(transName.Key, transName);
                }

                if (!nativeTranslations.ContainsKey("ObjectCategory." + type.FullName + ".Category"))
                {
                    NativeTranslationItem transName = new NativeTranslationItem("ObjectCategory." + type.FullName + ".Category");
                    transName.Category = "Object Category";
                    transName.NativeText = att.NativeCategory;
                    transName.OwnerType = type;
                    nativeTranslations.Add(transName.Key, transName);
                }
                #endregion
            }
            else if (objectAtt is Mincom.MineMarket.DAL.FriendlyNameAttribute)
            {
                #region Friendly Name
                Mincom.MineMarket.DAL.FriendlyNameAttribute att = (Mincom.MineMarket.DAL.FriendlyNameAttribute)objectAtt;

                if (!nativeTranslations.ContainsKey(type.FullName + ".FriendlyName." + att.FriendlyNameUnTranslated))
                {
                    NativeTranslationItem transName = new NativeTranslationItem(type.FullName + ".FriendlyName." + att.FriendlyNameUnTranslated);
                    transName.Category = "Friendly Name";
                    transName.NativeText = att.FriendlyNameUnTranslated;
                    transName.TargetName = targetName;
                    transName.OwnerType = type;
                    transName.ObjectType = TranslationManager.GetGeneralObjectType(type);
                    nativeTranslations.Add(transName.Key, transName);

                }
                #endregion
            }
            else if (objectAtt is Mincom.MineMarket.DAL.CategoryAttribute)
            {
                #region Category
                Mincom.MineMarket.DAL.CategoryAttribute att = (Mincom.MineMarket.DAL.CategoryAttribute)objectAtt;
                if (!nativeTranslations.ContainsKey(type.FullName + ".Category." + att.CategoryUnTranslated))
                {
                    NativeTranslationItem transName = new NativeTranslationItem(type.FullName + ".Category." + att.CategoryUnTranslated);
                    transName.Category = "Category";
                    transName.NativeText = att.CategoryUnTranslated;
                    transName.TargetName = targetName;
                    transName.OwnerType = type;
                    transName.ObjectType = TranslationManager.GetGeneralObjectType(type);
                    nativeTranslations.Add(transName.Key, transName);
                }
                #endregion
            }
            else if (objectAtt is Mincom.MineMarket.DAL.DescriptionAttribute)
            {
                #region Description
                Mincom.MineMarket.DAL.DescriptionAttribute att = (Mincom.MineMarket.DAL.DescriptionAttribute)objectAtt;

                if (!nativeTranslations.ContainsKey(type.FullName + ".Description." + att.DescriptionUnTranslated))
                {
                    NativeTranslationItem transName = new NativeTranslationItem(type.FullName + ".Description." + att.DescriptionUnTranslated);
                    transName.Category = "Description";
                    transName.NativeText = att.DescriptionUnTranslated;
                    transName.TargetName = targetName;
                    transName.OwnerType = type;
                    transName.ObjectType = TranslationManager.GetGeneralObjectType(type);
                    nativeTranslations.Add(transName.Key, transName);
                }
                #endregion
            }
        }

        /// <summary>
        /// Method used to load the translation for a user
        /// </summary>
        private static void PreloadTranslation()
        {
            if (BulkTrakClient.CurrentUser == null || BulkTrakClient.CurrentUser.Translation == null)
                mUsersTranslation = new Translation(NativeTranslationName, HardKeys.NativeTranslation);
            else
                //Get the translation from the database
                mUsersTranslation = BulkTrakClient.CurrentUser.Translation;

        }

        /// <summary>
        /// Gets a list of enum wrappers for a particular enum.
        /// </summary>
        /// <param name="enumType">The enum type to get the friendly names for.</param>
        /// <returns></returns>
        private static Hashtable LoadEnumWrappers(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                Debug.Assert(false, "This type is not an enum!");
                return new Hashtable();
            }

            if (mAllEnums.ContainsKey(enumType))
                return (Hashtable)mAllEnums[enumType];
            else
            {
                Hashtable enumWrappers = new Hashtable();
                FieldInfo[] fields = enumType.GetFields();
                Enum e = null;
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == enumType)
                    {
                        object[] atts = field.GetCustomAttributes(typeof(TranslateAttribute), true);
                        e = (Enum)Enum.Parse(enumType, field.Name);
                        string name = field.Name;
                        if (atts.Length > 0)
                            name = LoadTranslationString(enumType.FullName + "." + field.Name, ((TranslateAttribute)atts[0]).NativeText);

                        enumWrappers.Add(e, new EnumWrapper(e, name));
                    }
                }
                mAllEnums.Add(enumType, enumWrappers);

                TranslateDoTNetEnums(enumWrappers, enumType);

                return enumWrappers;
            }
        }

        private static void TranslateDoTNetEnums(Hashtable enumWrappers, Type enumType)
        {

            Hashtable translatedEnum = null;

            switch (enumType.Name)
            {
                case "DayOfWeek":
                    translatedEnum = LoadEnumWrappers(typeof(TranslatedDayOfWeek));

                    TranslateDotNetEnum(translatedEnum.Values, enumWrappers.Values);

                    break;

                default:
                    break;
            }



        }

        private static void TranslateDotNetEnum(ICollection translatedEnums, ICollection dotNetEnums)
        {
            EnumWrapperNameComparer cmp = new EnumWrapperNameComparer();

            foreach (EnumWrapper translatedwrapper in translatedEnums)
            {
                foreach (EnumWrapper dotNetWrapper in dotNetEnums)
                {
                    if (cmp.CompareByValue(translatedwrapper, dotNetWrapper) == 0)
                    {
                        dotNetWrapper.Name = translatedwrapper.Name;
                    }
                }
            }
        }


        #endregion

        #region GlobalStrings class

        public class GlobalStrings
        {
            #region Action

            [Translate("&New", "The global string for 'New'", TransGroupCategory.String)]
            private static string mNew = string.Empty;
            [Translate("&Open", "The global string for 'Open'", TransGroupCategory.String)]
            private static string mOpen = string.Empty;
            [Translate("&Edit", "The global string for 'Edit'", TransGroupCategory.String)]
            private static string mEdit = string.Empty;
            [Translate("&View", "The global string for 'View'", TransGroupCategory.String)]
            private static string mView = string.Empty;
            [Translate("Rena&me", "The global string for 'Rename'", TransGroupCategory.String)]
            private static string mRename = string.Empty;
            [Translate("&Delete", "The global string for 'Delete'", TransGroupCategory.String)]
            private static string mDelete = string.Empty;
            [Translate("Delete All", "The global string for 'Delete All'", TransGroupCategory.String)]
            private static string mDeleteAll = string.Empty;
            [Translate("&Add", "The global string for 'Add'.", TransGroupCategory.String)]
            private static string mAdd = string.Empty;
            [Translate("&Remove", "The global string for 'Remove'", TransGroupCategory.String)]
            private static string mRemove = string.Empty;
            [Translate("Remove All", "The global string for 'Remove All'", TransGroupCategory.String)]
            private static string mRemoveAll = string.Empty;
            [Translate("&Unassign", "The global string for 'Unassign'", TransGroupCategory.String)]
            private static string mUnassign = string.Empty;
            [Translate("&Cancel", "The global string for 'Cancel'", TransGroupCategory.String)]
            private static string mCancel = string.Empty;
            [Translate("&OK", "The global string for 'OK'", TransGroupCategory.String)]
            private static string mOk = string.Empty;
            [Translate("&Close", "The global string for 'Close'", TransGroupCategory.String)]
            private static string mClose = string.Empty;
            [Translate("&Detach", "The global string for 'Detach'", TransGroupCategory.String)]
            private static string mDetach = string.Empty;
            [Translate("&Attach to MineMarket", "The global string for 'Attach to MineMarket'", TransGroupCategory.String)]
            private static string mAttachToMineMarket = string.Empty;
            [Translate("&Attach to Group Form", "The global string for 'Attach to Group Form'", TransGroupCategory.String)]
            private static string mAttachToGroupForm = string.Empty;
            [Translate("&Apply", "The global string for 'Apply'", TransGroupCategory.String)]
            private static string mApply = string.Empty;
            [Translate("&Clear", "The global string for 'Clear'", TransGroupCategory.String)]
            private static string mClear = string.Empty;
            [Translate("&Select", "The global string for 'Select'", TransGroupCategory.String)]
            private static string mSelect = string.Empty;
            [Translate("&Select All", "The global string for 'Select All'", TransGroupCategory.String)]
            private static string mSelectAll = string.Empty;
            [Translate("&Deselect", "The global string for 'Deselect'", TransGroupCategory.String)]
            private static string mDeselect = string.Empty;
            [Translate("&Deselect All", "The global string for 'Deselect All'", TransGroupCategory.String)]
            private static string mDeselectAll = string.Empty;
            [Translate("Refresh", "The global string for 'Refresh'", TransGroupCategory.String)]
            private static string mRefresh = string.Empty;
            [Translate("Refresh All", "The global string for 'Refresh All'", TransGroupCategory.String)]
            private static string mRefreshAll = string.Empty;
            [Translate("Copy", "The global string for 'Copy'", TransGroupCategory.String)]
            private static string mCopy = string.Empty;
            [Translate("Paste", "The global string for 'Paste'", TransGroupCategory.String)]
            private static string mPaste = string.Empty;
            [Translate("Remove Column", "The string for 'Remove Column'.", TransGroupCategory.String)]
            private static string mRemoveColumn = string.Empty;

            // chart context menu strings
            [Translate("Show &Wizard", "The global string for 'Show Wizard'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemWizard = string.Empty;
            [Translate("&Reset Appearance", "The global string for 'Reset Appearance'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemSetDefaults = string.Empty;
            [Translate("&Save Appearance", "The global string for 'Save Appearance'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemSaveAppearance = string.Empty;
            [Translate("&Load Appearance", "The global string for 'Save Appearance'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemLoadAppearance = string.Empty;
            [Translate("Send To &Clipboard", "The global string for 'Send To Clipboard'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemToClipboard = string.Empty;
            [Translate("Save To &File", "The global string for 'Save To File'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemToFile = string.Empty;
            [Translate("Send To &Printer", "The global string for 'Send To Printer'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mChartItemToPrinter = string.Empty;
            [Translate("&Print", "The global string for 'Print'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mPrint = string.Empty;

            [Translate("Print &All", "The global string for 'Print All'.", TransGroupCategory.ContextMenu)]
            private static string mPrintAll = string.Empty;
            [Translate("Print Pre&view", "The global string for 'Print Preview'. Used by chart context menus.", TransGroupCategory.ContextMenu)]
            private static string mPrintPreview = string.Empty;

            [Translate("Move &First", "The string for 'Move First'.", TransGroupCategory.String)]
            private static string mMoveFirst = string.Empty;
            [Translate("Move &Last", "The string for 'Move Last'.", TransGroupCategory.String)]
            private static string mMoveLast = string.Empty;
            [Translate("Move &Up", "The string for 'Move Up'.", TransGroupCategory.String)]
            private static string mMoveUp = string.Empty;
            [Translate("Move Do&wn", "The string for 'Move Down'", TransGroupCategory.String)]
            private static string mMoveDown = string.Empty;

            [Translate("Include Actuals", "The string for 'Display Actuals' on the planning screen.", TransGroupCategory.String)]
            private static string mDisplayActuals = string.Empty;
            [Translate("Display Planned Only", "The string for 'Display Planned Only' on the planning screen.", TransGroupCategory.String)]
            private static string mDisplayPlanned = string.Empty;

            [Translate("Copy To Clipboard", "The text for the context menu item which copies all of the grid data onto the clipboard.", TransGroupCategory.String)]
            private static string mCopyToClipboard = string.Empty;

            [Translate("Duplicate", "The string for 'Duplicate'", TransGroupCategory.String)]
            private static string mDuplicate = string.Empty;
            [Translate("Activate", "The string for 'Activate'", TransGroupCategory.String)]
            private static string mActivate = string.Empty;
            [Translate("Deactivate", "The string for 'Deactivate'", TransGroupCategory.String)]
            private static string mDeactivate = string.Empty;
            [Translate("Accept", "The string for 'Accept'.", TransGroupCategory.String)]
            private static string mAccept = string.Empty;

            [Translate("Search Text", "The string for 'Search Text'.", TransGroupCategory.String)]
            private static string mSearchText = string.Empty;
            [Translate("&Search", "The string for 'Search'.", TransGroupCategory.String)]
            private static string mSearch = string.Empty;
            [Translate("Search Criteria", "The string for 'Search Criteria'.", TransGroupCategory.String)]
            private static string mSearchCriteria = string.Empty;
            [Translate("Search for Transactions", "The string for 'Search for Transactions'.", TransGroupCategory.String)]
            private static string mSearchForTransactions = string.Empty;
            [Translate("Search By", "The string for 'Search By'.", TransGroupCategory.String)]
            private static string mSearchBy = string.Empty;
            [Translate("Search For", "The string for 'Search For'.", TransGroupCategory.String)]
            private static string mSearchFor = string.Empty;
            [Translate("Sort By", "The string for 'Sort By'.", TransGroupCategory.String)]
            private static string mSortBy = string.Empty;

            [Translate("Sa&ve", "The string for 'Save'.", TransGroupCategory.String)]
            private static string mSave = string.Empty;

            [Translate("Assign All", "The string for 'Assign All'.", TransGroupCategory.String)]
            private static string mAssignAll = string.Empty;
            [Translate("Unassign All", "The string for 'Unassign All'.", TransGroupCategory.String)]
            private static string mUnassignAll = string.Empty;


            [Translate("Expand All Groups", "The caption of the 'Expand All Groups' context menu item.", TransGroupCategory.ContextMenu)]
            private static string mExpandAllGroups = string.Empty;
            [Translate("Collapse All Groups", "The caption of the 'Collapse All Groups' context menu item.", TransGroupCategory.ContextMenu)]
            private static string mCollapseAllGroups = string.Empty;

            [Translate("Add Survey", "The global string for 'Add Survey'.", TransGroupCategory.String)]
            private static string mAddSurvey = string.Empty;
            [Translate("Collapse All", "The text for the context menu item for collapsing all nodes in a tree view", TransGroupCategory.ContextMenu)]
            private static string mCollapseAll = string.Empty;
            [Translate("Expand All", "The text for the context menu item for expanding all nodes in a tree view", TransGroupCategory.ContextMenu)]
            private static string mExpandAll = string.Empty;
            [Translate("Collapse", "The text for the context menu item for collapsing sub-nodes under selected nod in a tree view", TransGroupCategory.ContextMenu)]
            private static string mCollapse = string.Empty;
            [Translate("Expand", "The text for the context menu item for expanding sub-nodes under selected nod in a tree view", TransGroupCategory.ContextMenu)]
            private static string mExpand = string.Empty;
            [Translate("Show &Document", "The global string for 'Show Document'.", TransGroupCategory.String)]
            private static string mShowDocument = string.Empty;
            [Translate("&Print Document", "The global string for 'Print Document'.", TransGroupCategory.String)]
            private static string mPrintDocument = string.Empty;
            [Translate("Group By", "The global string for 'Group By'.", TransGroupCategory.String)]
            private static string mGroupBy = string.Empty;
            [Translate("Contributor Value Format", "The global string for 'Contributor Value Format'.", TransGroupCategory.String)]
            private static string mContributorValueFormat = string.Empty;
            [Translate("Adjust", "The global string for 'Adjust'.", TransGroupCategory.String)]
            private static string mAdjust = string.Empty;
            [Translate("Undo", "The global string for 'Undo'.", TransGroupCategory.String)]
            private static string mUndo = string.Empty;
            [Translate("Commit", "The global string for 'Commit'.", TransGroupCategory.String)]
            private static string mCommit = string.Empty;

            [Translate("Open Source Stockpile", "The global string for 'Stockpile'.", TransGroupCategory.String)]
            private static string mOpenSourceStockpile = string.Empty;
            [Translate("Open Destination Stockpile", "The global string for 'Stockpile'.", TransGroupCategory.String)]
            private static string mOpenDestStockpile = string.Empty;

            [Translate("Copy Transactions", "The global string for 'Copy Transactions'.", TransGroupCategory.String)]
            private static string mCopyTransactions = string.Empty;

            #endregion

            #region UOM

            [Translate("Millimetres", "The global string for 'Millimetres'", TransGroupCategory.String)]
            private static string mMillimetres = string.Empty;
            [Translate("Inches", "The global string for 'Inches'", TransGroupCategory.String)]
            private static string mInches = string.Empty;
            [Translate("Metres", "The global string for 'Metres'", TransGroupCategory.String)]
            private static string mMetres = string.Empty;
            [Translate("Percent", "The global string for 'Percentage'.", TransGroupCategory.String)]
            private static string mPercentage = string.Empty;
            [Translate("ppm", "The global string for 'Parts Per Million'.", TransGroupCategory.String)]
            private static string mPartsPerMillion = string.Empty;
            [Translate("Metric Tonnes", "The global string for 'Metric Tonnes'.", TransGroupCategory.String)]
            private static string mTonnes = string.Empty;
            [Translate("Mass", "The global string for 'Mass'.", TransGroupCategory.String)]
            private static string mMass = string.Empty;
            [Translate("Unit of Measure", "The global string for 'UOM'.", TransGroupCategory.String)]
            private static string mMassUOM = string.Empty;
            [Translate("Kilograms", "The global string for 'Kilograms'.", TransGroupCategory.String)]
            private static string mKilograms = string.Empty;
            [Translate("BCM", "The global string for 'BCM'.", TransGroupCategory.String)]
            private static string mBCM = string.Empty;
            [Translate("Pounds", "The global string for 'Pounds'.", TransGroupCategory.String)]
            private static string mPound = string.Empty;
            [Translate("Troy Ounce", "The global string for 'Troy Ounce'.", TransGroupCategory.String)]
            private static string mTroyOunce = string.Empty;
            [Translate("Grams Per Tonne", "The global string for 'Grams Per Tonne'.", TransGroupCategory.String)]
            private static string mGramsPerTonne = string.Empty;
            [Translate("Each", "The global string for 'Each'.", TransGroupCategory.String)]
            private static string mEach = string.Empty;
            [Translate("Kilometre", "The global string for 'Kilometre'.", TransGroupCategory.String)]
            private static string mKilometre = string.Empty;
            [Translate("Nautical Mile", "The global string for 'Nautical Mile'.", TransGroupCategory.String)]
            private static string mNauticalMile = string.Empty;
            [Translate("Mile", "The global string for 'Mile'.", TransGroupCategory.String)]
            private static string mMile = string.Empty;

            [Translate("Trip", "The global string for 'Trip'.", TransGroupCategory.String)]
            private static string mTrip = string.Empty;
            [Translate("BCM Per Day", "The global string for 'BCM Per Day'.", TransGroupCategory.String)]
            private static string mBCMPerDay = string.Empty;
            [Translate("BCM Per Hour", "The global string for 'BCM Per Hour'.", TransGroupCategory.String)]
            private static string mBCMPerHour = string.Empty;
            [Translate("BCM Per Trip", "The global string for 'BCM Per Trip'.", TransGroupCategory.String)]
            private static string mBCMPerTrip = string.Empty;

            [Translate("Megajoules", "The global string for 'Megajoules'.", TransGroupCategory.String)]
            private static string mMegajoules = string.Empty;
            [Translate("Kilocalories", "The global string for 'Kilocalories'.", TransGroupCategory.String)]
            private static string mKilocalories = string.Empty;
            [Translate("BTU", "The global string for 'BTU'.", TransGroupCategory.String)]
            private static string mBTU = string.Empty;

            [Translate("Grams", "The global string for 'Grams'.", TransGroupCategory.String)]
            private static string mGrams = string.Empty;
            [Translate("Milligrams", "The global string for 'Milligrams'.", TransGroupCategory.String)]
            private static string mMilligrams = string.Empty;
            [Translate("Avoirdupois Ounces", "The global string for 'Avoirdupois Ounces'.", TransGroupCategory.String)]
            private static string mAvoirdupoisOunces = string.Empty;
            [Translate("Long Tons", "The global string for 'Long Tons'.", TransGroupCategory.String)]
            private static string mLongTons = string.Empty;
            [Translate("Short Tons", "The global string for 'Short Tons'.", TransGroupCategory.String)]
            private static string mShortTons = string.Empty;

            [Translate("kcal/kg", "The global string for 'kcal/kg'.", TransGroupCategory.String)]
            private static string mKCalPerKg = string.Empty;
            [Translate("MJ/kg", "The global string for 'MJ/kg'.", TransGroupCategory.String)]
            private static string mMJPerKg = string.Empty;
            [Translate("BTU/lb", "The global string for 'BTU/lb'.", TransGroupCategory.String)]
            private static string mBTUPerPound = string.Empty;

            #endregion

            #region Messages

            [Translate("Drag a column header here to group by that column", "The global string for 'Drag a column header here to group by that column'.", TransGroupCategory.String)]
            private static string mDragAColumnHeaderHereToGroupByThatColumn = string.Empty;

            [Translate("Unable to delete item as it is referenced by other objects", "The global string for the unable to delete message", TransGroupCategory.String)]
            private static string mUnableToDelete = string.Empty;
            [Translate("Current security settings are insufficient to perform this action.", "The global string for the security access denied message", TransGroupCategory.String)]
            private static string mSecurityDeniedOperation = string.Empty;
            [Translate("Text entered for {0} exceeds maximum length of {1} characters.", "The global string for the text value too long message.", TransGroupCategory.String)]
            private static string mTextTooLong = string.Empty;
            [Translate("The value entered is not a valid date time.", "The message displayed if a user enters an invalid date into a datetime field in a grid.", TransGroupCategory.String)]
            private static string mInvalidDate = string.Empty;
            [Translate("The value entered is not a valid number.", "The message displayed if a user enters invalid data into a number field.", TransGroupCategory.String)]
            private static string mInvalidNumber = string.Empty;
            [Translate("There is no vessel class defined for this vessel. Lot samples can not be created.", "Message displayed to user when they attempt to do lot sampling without a vessel class defined.", TransGroupCategory.String)]
            private static string mNoVesselClassDefined = string.Empty;
            [Translate("Location can not be updated because transactions exist against the process flow.", "The message displayed when a user attempts to change a location of a process flow which has transactions.", TransGroupCategory.String)]
            private static string mTransactionsExistAgainstProcessFlow = string.Empty;
            [Translate("An ore source can not be used as the destination location of a process flow.", "The message displayed when a user attempts to use an ore source as a destination on a process flow.", TransGroupCategory.String)]
            private static string mInvalidProcessFlowDestination = string.Empty;
            [Translate("Invalid Date Range", "The message displayed when a user enters an invalid date range", TransGroupCategory.String)]
            private static string mInvalidDateRange = string.Empty;
            [Translate("Invalid mapping object configured", "The global string for 'Invalid mapping object configured'", TransGroupCategory.String)]
            private static string mInvalidMapObject = string.Empty;
            [Translate("Reclaim Data Is Invalid", "The global string for 'Reclaim Data Is Invalid'.", TransGroupCategory.String)]
            private static string mReclaimDataIsInvalid = string.Empty;
            [Translate("Reclaim Failed", "The global string for 'Reclaim Failed'.", TransGroupCategory.String)]
            private static string mReclaimFailed = string.Empty;
            [Translate("The reclaim does not have all required values entered, or contains invalid data.  Check all fields contain valid values.", "The global string for 'Reclaim Missing Info'.", TransGroupCategory.String)]
            private static string mReclaimMissingInfo = string.Empty;
            [Translate("The reclaim failed with the following error:", "The global string for 'ReclaimFailureDescription'.", TransGroupCategory.String)]
            private static string mReclaimFailureDescription = string.Empty;
            [Translate("No material was reclaimed", "The global string for 'No Material Was Reclaimed'.", TransGroupCategory.String)]
            private static string mNoMaterialReclaimed = string.Empty;

            [Translate("Not enough privileges to lock/unlock the journal", "The error when trying to lock/unlock a journal without privileges", TransGroupCategory.String)]
            private static string mCannotLockUnlockDueRights = string.Empty;
            [Translate("Operation Completed Succesfully", "Text for the 'Operation Completed Succesfully' message on an invoice batch operation", TransGroupCategory.String)]
            private static string mSuccesfulBatchOperation = string.Empty;
            [Translate("The following errors occurred while locking the invoice:", "Text preceding a list of errors in a message box, after invoice locking operation", TransGroupCategory.String)]
            private static string mFollowingErrorsOccurredLocking = string.Empty;
            [Translate("The following errors occurred while unlocking the invoice:", "Text preceding a list of errors in a message box, after invoice unlocking operation", TransGroupCategory.String)]
            private static string mFollowingErrorsOccurredUnLocking = string.Empty;

            [Translate("The Planning Period property cannot be changed after the plan data is generated by the Plan Rollup.", "Validation error message when Planning Period is changed", TransGroupCategory.Exception)]
            private static string mPlanningPeriodCannotBeChangedText = string.Empty;
            [Translate("Planning Period change error", "Planning Period change error message box title.", TransGroupCategory.Exception)]
            private static string mPlanningPeriodChangeErrorTitle = string.Empty;

            [Translate("The Plan UOM property cannot be changed after the plan data is generated by the Plan Rollup.", "Validation error message when Plan UOM is changed.", TransGroupCategory.Exception)]
            private static string mPlanUOMCannotBeChangedText = string.Empty;
            [Translate("Plan UOM change error", "Plan UOM change error message box title.", TransGroupCategory.Exception)]
            private static string mPlanUOMChangeErrorTitle = string.Empty;


            #endregion

            #region Date/Time

            [Translate("Start Date", "The global string for 'Start Date'.", TransGroupCategory.String)]
            private static string mStartDate = string.Empty;
            [Translate("End Date", "The global string for 'End Date'.", TransGroupCategory.String)]
            private static string mEndDate = string.Empty;
            [Translate("Date", "The global string for 'Date'.", TransGroupCategory.String)]
            private static string mDate = string.Empty;
            [Translate("Time", "The global string for 'Time'.", TransGroupCategory.String)]
            private static string mTime = string.Empty;
            [Translate("Date Period", "The string for 'Date Period'.", TransGroupCategory.String)]
            private static string mDatePeriod = string.Empty;
            [Translate("Time of Arrival", "The string for 'Time of Arrival' used in the shipment and train explorers.", TransGroupCategory.String)]
            private static string mTimeOfArrival = string.Empty;
            [Translate("Time of Departure", "The string for 'Time of Departure' used in the shipment and train explorers.", TransGroupCategory.String)]
            private static string mTimeOfDeparture = string.Empty;
            [Translate("Arrival Date", "The caption for the arrival date row in the despatch dates grid.", TransGroupCategory.String)]
            private static string mDespatchArrivalDates = string.Empty;
            [Translate("Departure Date", "The caption for the departure date row in the despatch dates grid.", TransGroupCategory.String)]
            private static string mDespatchDepatureDates = string.Empty;
            [Translate("Start Loading", "The caption for the start loading date row in the despatch dates grid.", TransGroupCategory.String)]
            private static string mDespatchStartLoadingDates = string.Empty;
            [Translate("End Loading", "The caption for the end loading date row in the despatch dates grid.", TransGroupCategory.String)]
            private static string mDespatchEndLoadingDates = string.Empty;
            [Translate("Start Unloading", "The caption for the start unloading date row in the despatch dates grid.", TransGroupCategory.String)]
            private static string mDespatchStartUnloadingDates = string.Empty;
            [Translate("End Unloading", "The caption for the end unloading date row in the despatch dates grid.", TransGroupCategory.String)]
            private static string mDespatchEndUnloadingDates = string.Empty;
            [Translate("Start Date", "The string for 'Start Date'.", TransGroupCategory.String)]
            private static string mFromDate = string.Empty;
            [Translate("End Date", "The string for 'End Date'.", TransGroupCategory.String)]
            private static string mToDate = string.Empty;
            [Translate("Contracted Date", "The original planned delivery date on the despatchorder displayed inside of planning.", TransGroupCategory.String)]
            private static string mContractedDeliveryDate = string.Empty;
            [Translate("Day", "The global string for 'Day'.", TransGroupCategory.String)]
            private static string mDay = string.Empty;
            [Translate("Days", "The global string for 'Days'.", TransGroupCategory.String)]
            private static string mDays = string.Empty;
            [Translate("Weeks", "The global string for 'Weeks'.", TransGroupCategory.String)]
            private static string mWeeks = string.Empty;
            [Translate("Hour", "The global string for 'Hour'.", TransGroupCategory.String)]
            private static string mHour = string.Empty;
            [Translate("Minute", "The global string for 'Minute'.", TransGroupCategory.String)]
            private static string mMinute = string.Empty;
            [Translate("Start Loading", "The global string for 'Start Loading'.", TransGroupCategory.String)]
            private static string mStartLoading = string.Empty;
            [Translate("End Loading", "The global string for 'End Loading'.", TransGroupCategory.String)]
            private static string mEndLoading = string.Empty;

            //Date options
            [Translate("Any", "The global string for 'Any'", TransGroupCategory.String)]
            private static string mAny = string.Empty;
            [Translate("End of Shift", "The global string for 'EndOfShift'", TransGroupCategory.String)]
            private static string mEndOfShift = string.Empty;
            [Translate("Start of Shift", "The global string for 'StartOfShift'", TransGroupCategory.String)]
            private static string mStartOfShift = string.Empty;
            [Translate("End of Previous Shift", "The global string for 'EndOfPreviousShift'", TransGroupCategory.String)]
            private static string mEndOfPreviousShift = string.Empty;
            [Translate("Start of Previous Shift", "The global string for 'StartOfPreviousShift'", TransGroupCategory.String)]
            private static string mStartOfPreviousShift = string.Empty;

            #endregion

            #region Days of week

            [Translate("Sunday", "The global string for 'Sunday'.", TransGroupCategory.String)]
            private static string mSunday = string.Empty;

            [Translate("Monday", "The global string for 'Monday'.", TransGroupCategory.String)]
            private static string mMonday = string.Empty;

            [Translate("Tuesday", "The global string for 'Tuesday'.", TransGroupCategory.String)]
            private static string mTuesday = string.Empty;

            [Translate("Wednesday", "The global string for 'Wednesday'.", TransGroupCategory.String)]
            private static string mWednesday = string.Empty;

            [Translate("Thursday", "The global string for 'Thurdays'.", TransGroupCategory.String)]
            private static string mThursday = string.Empty;

            [Translate("Friday", "The global string for 'Friday'.", TransGroupCategory.String)]
            private static string mFriday = string.Empty;

            [Translate("Saturday", "The global string for 'Saturday'.", TransGroupCategory.String)]
            private static string mSaturday = string.Empty;

            #endregion

            #region TW Recon Changes

            [Translate("Opening", "The global string for 'Opening Balance' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconOpeningBalance = string.Empty;

            [Translate("Closing", "The global string for 'Closing Balance' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconClosingBalance = string.Empty;

            [Translate("Closing (0 stockpiles)", "The global string for 'Closing Balance' for TW Reconciliations when therer are no available stockpiles for a location.", TransGroupCategory.String)]
            private static string mTWReconClosingBalanceNoStockpiles = string.Empty;

            [Translate("Net Change", "The global string for 'Net Change' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconNetChange = string.Empty;

            [Translate("Inputs", "The global string for 'Inputs' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconInputs = string.Empty;

            [Translate("Outputs", "The global string for 'Outputs' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconOutputs = string.Empty;

            [Translate("Adjustments", "The global string for 'Adjustments' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconAdjustments = string.Empty;

            [Translate("Discrepancy", "The global string for 'Discrepancy' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mTWReconDiscrepancy = string.Empty;

            [Translate("Duration", "The global string for 'Duration' for TW Reconciliation.", TransGroupCategory.String)]
            private static string mDuration = string.Empty;

            [Translate("Months", "The global string for 'Months'.", TransGroupCategory.String)]
            private static string mMonths = string.Empty;

            [Translate("Years", "The global string for 'Years'.", TransGroupCategory.String)]
            private static string mYears = string.Empty;

            [Translate("Weekly", "The global string for 'Weekly'.", TransGroupCategory.String)]
            private static string mWeekly = string.Empty;

            [Translate("Monthly", "The global string for message 'Monthly'.", TransGroupCategory.String)]
            private static string mMonthly = string.Empty;

            [Translate("Quarterly", "The global string for message 'Quarterly'.", TransGroupCategory.String)]
            private static string mQuarterly = string.Empty;

            #endregion

            [Translate("Appearance", "The global string for 'Appearance'", TransGroupCategory.String)]
            private static string mAppearance = string.Empty;
            [Translate("Description", "The global string for 'Description'", TransGroupCategory.String)]
            private static string mDescription = string.Empty;
            [Translate("State query took {0:0} milliseconds", "The global string for the status message for state query timing", TransGroupCategory.String)]
            private static string mQueryTimingStatusMessage = string.Empty;
            [Translate("Status", "The status", TransGroupCategory.String)]
            private static string mStatus = string.Empty;
            [Translate("Loading", "The default text for Loading operations", TransGroupCategory.String)]
            private static string mDefaultLoadText = string.Empty;
            [Translate("Unloading", "The default text for Unloading operations", TransGroupCategory.String)]
            private static string mDefaultUnLoadText = string.Empty;
            [Translate("Plan/Loading", "The text for planning/loading operations", TransGroupCategory.String)]
            private static string mPlanLoadingText = string.Empty;
            [Translate("Moisture", "The global string for 'Moisture'.", TransGroupCategory.String)]
            private static string mMoisture = string.Empty;
            [Translate("Traceability", "The global string for 'Traceability'.", TransGroupCategory.String)]
            private static string mTraceability = string.Empty;
            [Translate("&Test", "The global string for 'Test'.", TransGroupCategory.String)]
            private static string mTest = string.Empty;
            [Translate("Name", "The global string for 'Name'", TransGroupCategory.String)]
            private static string mName = string.Empty;
            [Translate("Value", "The global string for 'Value'", TransGroupCategory.String)]
            private static string mValue = string.Empty;

            // Should this be from the config?
            [Translate("As Received", "The global string for 'As Received'.", TransGroupCategory.String)]
            private static string mAsReceived = string.Empty;
            [Translate("Dry Basis", "The global string for 'Dry Weight'.", TransGroupCategory.String)]
            private static string mDryBasis = string.Empty;
            [Translate("Analyte Content", "The global string for 'Analyte Content'", TransGroupCategory.String)]
            private static string mAnalyteContent = string.Empty;
            [Translate("Content", "The global string for 'Content'", TransGroupCategory.String)]
            private static string mContent = string.Empty;

            [Translate("TBN", "The global string for a shipment with an unknown vessel.", TransGroupCategory.String)]
            private static string mTBN = string.Empty;
            [Translate("Planned", "The string for 'Planned'", TransGroupCategory.String)]
            private static string mPlanned = string.Empty;
            [Translate("Actual", "The string for 'Actual'", TransGroupCategory.String)]
            private static string mActual = string.Empty;
            [Translate("Capacity", "The string for 'Capacity'", TransGroupCategory.String)]
            private static string mCapacity = string.Empty;
            [Translate("Quantity", "The string for 'Quantity'", TransGroupCategory.String)]
            private static string mQuantity = string.Empty;
            [Translate("Chart", "The string for 'Chart'", TransGroupCategory.String)]
            private static string mChart = string.Empty;

            [Translate("View Transactions", "The string for 'View Transactions'", TransGroupCategory.String)]
            private static string mViewTransactions = string.Empty;
            [Translate("View Stockpile States", "The string for 'View Stockpile States'", TransGroupCategory.String)]
            private static string mViewStockpileStates = string.Empty;

            [Translate("Source Stockpiles", "The string for 'Source Stockpiles'", TransGroupCategory.String)]
            private static string mSourceStockpiles = string.Empty;
            [Translate("Destination Stockpiles", "The string for 'Destination Stockpiles'", TransGroupCategory.String)]
            private static string mDestinationStockpiles = string.Empty;
            [Translate("Source", "The string for 'Source'", TransGroupCategory.String)]
            private static string mSource = string.Empty;
            [Translate("Destination", "The string for 'Destination'", TransGroupCategory.String)]
            private static string mDestination = string.Empty;

            [Translate("Sequence", "The string for 'Sequence'", TransGroupCategory.String)]
            private static string mSequence = string.Empty;
            [Translate("Seq", "The global string for 'Seq'.", TransGroupCategory.String)]
            private static string mSeq = string.Empty;

            [Translate("Activated", "The string for 'Activated'", TransGroupCategory.String)]
            private static string mActivated = string.Empty;
            [Translate("Deactivated", "The string for 'Deactivated'", TransGroupCategory.String)]
            private static string mDeactivated = string.Empty;

            [Translate("State at departure", "The string for 'State at departure' which is used in the shipment and train explorers.", TransGroupCategory.String)]
            private static string mStateAtDeparture = string.Empty;

            [Translate("Billboard", "The string for 'Billboard'.", TransGroupCategory.String)]
            private static string mBillboard = string.Empty;
            [Translate("Time Line", "The string for 'Time Line'.", TransGroupCategory.String)]
            private static string mTimeLine = string.Empty;
            [Translate("Reverse View", "The string for 'Reverse View'.", TransGroupCategory.String)]
            private static string mReverseView = string.Empty;

            [Translate("Balance", "The string for 'Balance'.", TransGroupCategory.String)]
            private static string mBalance = string.Empty;
            [Translate("Current Tonnage", "The string for 'Current Tonnage'.", TransGroupCategory.String)]
            private static string mCurrentTonnage = string.Empty;

            [Translate("Total", "The string for 'Total'.", TransGroupCategory.String)]
            private static string mTotal = string.Empty;
            [Translate("Grand Total", "The string for 'Grand Total'.", TransGroupCategory.String)]
            private static string mGrandTotal = string.Empty;
            [Translate("Location", "The string for 'Location'.", TransGroupCategory.String)]
            private static string mLocation = string.Empty;
            [Translate("Locations", "The string for 'Locations'.", TransGroupCategory.String)]
            private static string mLocations = string.Empty;
            [Translate("View By &Day", "The text displayed for 'view by day'", TransGroupCategory.String)]
            private static string mViewByDay = string.Empty;
            [Translate("View By &Week", "The text displayed for 'view by week'", TransGroupCategory.String)]
            private static string mViewByWeek = string.Empty;
            [Translate("View By &Month", "The text displayed for 'view by month'", TransGroupCategory.String)]
            private static string mViewByMonth = string.Empty;
            [Translate("View By &Year", "The text displayed for 'view by year'", TransGroupCategory.String)]
            private static string mViewByYear = string.Empty;

            [Translate("Summary", "The text displayed for the word 'Summary'.", TransGroupCategory.String)]
            private static string mSummary = string.Empty;
            [Translate("Activity", "The text displayed for the word 'Activity'.", TransGroupCategory.String)]
            private static string mActivity = string.Empty;
            [Translate("Details", "The text displayed for the word 'Details'.", TransGroupCategory.String)]
            private static string mDetails = string.Empty;
            [Translate("Structure", "The text displayed for the word 'Structure'.", TransGroupCategory.String)]
            private static string mStructure = string.Empty;
            [Translate("Assigned", "The string for 'Assigned'.", TransGroupCategory.String)]
            private static string mAssigned = string.Empty;
            [Translate("(None)", "The global string for 'None'", TransGroupCategory.String)]
            private static string mNone = string.Empty;

            [Translate("Customers", "The global string for 'Customers'.", TransGroupCategory.String)]
            private static string mCustomers = string.Empty;
            [Translate("Shipping Agents", "The global string for 'Shipping Agents'.", TransGroupCategory.String)]
            private static string mShippingAgents = string.Empty;
            [Translate("Contractors", "The global string for 'Contractors'.", TransGroupCategory.String)]
            private static string mContractors = string.Empty;
            [Translate("Pilots", "The global string for 'Pilots Organisation Roles'.", TransGroupCategory.String)]
            private static string mPilots = string.Empty;
            [Translate("Commercial", "The global string for 'Commercial Organisation Roles'.", TransGroupCategory.String)]
            private static string mCommercial = string.Empty;
            [Translate("Superintending", "The global string for 'Superintending Organisation Role'.", TransGroupCategory.String)]
            private static string mSuperintending = string.Empty;

            [Translate("Ownership", "The global string for 'Ownership'.", TransGroupCategory.String)]
            private static string mOwnership = string.Empty;
            [Translate("Domains", "The global string for 'Domains'.", TransGroupCategory.String)]
            private static string mDomains = string.Empty;
            [Translate("Domain", "The global string for 'Domain'.", TransGroupCategory.String)]
            private static string mDomain = string.Empty;
            [Translate("User Group Domain Membership", "The global string for 'User Group Domain Membership'.", TransGroupCategory.String)]
            private static string mDomainMembership = string.Empty;

            [Translate("Static Groups", "The global string for 'Static Groups'.", TransGroupCategory.String)]
            private static string mStaticGroups = string.Empty;

            [Translate("Bill of lading", "The global string for 'Bill of lading'.", TransGroupCategory.String)]
            private static string mBillOfLading = string.Empty;
            [Translate("Required", "The global string for 'Required'.", TransGroupCategory.String)]
            private static string mRequired = string.Empty;
            [Translate("Delivered", "The global string for 'Delivered'.", TransGroupCategory.String)]
            private static string mDelivered = string.Empty;
            [Translate("Despatch", "The global string for 'Despatch'.", TransGroupCategory.String)]
            private static string mDespatch = string.Empty;
            [Translate("Quality", "The global string for 'Quality'.", TransGroupCategory.String)]
            private static string mQuality = string.Empty;
            [Translate("Selected", "The global string for 'Selected'.", TransGroupCategory.String)]
            private static string mSelected = string.Empty;
            [Translate("Comment", "The global string for 'Comment'.", TransGroupCategory.String)]
            private static string mComment = string.Empty;

            [Translate("Approval", "The global string for 'Approval'.", TransGroupCategory.String)]
            private static string mApproval = string.Empty;
            [Translate("Contracted Demand", "The mass required on the despatchorder displayed inside of planning.", TransGroupCategory.String)]
            private static string mContractedDemand = string.Empty;
            [Translate("Contracted Product", "The product required on the despatchorder displayed inside of planning.", TransGroupCategory.String)]
            private static string mContractedProduct = string.Empty;
            [Translate("Preferred Blend", "The preferred blend of products required to fulfil the given despatchorder in planning.", TransGroupCategory.String)]
            private static string mPreferredBlend = string.Empty;
            [Translate("Preferred Blend Total Quantity", "The preferred blend total quantity of products required to fulfil the given despatchorder in planning.", TransGroupCategory.String)]
            private static string mPreferredBlendTotalQuantity = string.Empty;
            [Translate("Blend", "The global string for 'Blend'.", TransGroupCategory.String)]
            private static string mBlend = string.Empty;
            [Translate("Delay", "The global string for 'Delay'.", TransGroupCategory.String)]
            private static string mDelay = string.Empty;
            [Translate("Delay (Hours)", "The global string for 'Delay (Hours)'.", TransGroupCategory.String)]
            private static string mDelayHours = string.Empty;

            [Translate("Loading Rate", "The global string for 'Loading Rate'", TransGroupCategory.String)]
            private static string mLoadingRate = string.Empty;

            [Translate("Currency", "The global string for 'Currency'", TransGroupCategory.String)]
            private static string mCurrency = string.Empty;
            [Translate("Invoice", "The global string for 'Invoice'", TransGroupCategory.String)]
            private static string mInvoice = string.Empty;
            [Translate("Price Adjustments", "The global string for 'Pricing Adjustments'", TransGroupCategory.String)]
            private static string mPricingAdjustments = string.Empty;

            [Translate("Weight", "The global string for 'weight'.", TransGroupCategory.String)]
            private static string mWeight = string.Empty;
            [Translate("Weight Basis", "The global string for 'weight basis'.", TransGroupCategory.String)]
            private static string mWeightBasis = string.Empty;
            [Translate("Lot", "The global string for 'Lot'.", TransGroupCategory.String)]
            private static string mLot = string.Empty;
            [Translate("Sub-Items", "The global string for 'Sub-Items'.", TransGroupCategory.String)]
            private static string mSubItems = string.Empty;
            [Translate("Package", "The global string for 'Package'.", TransGroupCategory.String)]
            private static string mPackage = string.Empty;
            [Translate("Packages", "The global string for 'packages'.", TransGroupCategory.String)]
            private static string mPackages = string.Empty;
            [Translate("To", "The global string for 'to'.", TransGroupCategory.String)]
            private static string mTo = string.Empty;

            [Translate("Invoice Report Template", "The global string for 'Invoice Report Template'", TransGroupCategory.String)]
            private static string mInvoiceReportTemplates = string.Empty;
            [Translate("Proforma Report Template", "The global string for 'Proforma Report Template'", TransGroupCategory.String)]
            private static string mProformaReportTemplates = string.Empty;

            [Translate("Length", "The global string for 'Length'", TransGroupCategory.String)]
            private static string mLength = string.Empty;
            [Translate("Width", "The global string for 'Width'", TransGroupCategory.String)]
            private static string mWidth = string.Empty;
            [Translate("Height", "The global string for 'Height'", TransGroupCategory.String)]
            private static string mHeight = string.Empty;

            [Translate("Containers", "The global string for 'containers'.", TransGroupCategory.String)]
            private static string mContainers = string.Empty;
            [Translate("Type", "The global string for 'type'.", TransGroupCategory.String)]
            private static string mType = string.Empty;
            [Translate("Transactions", "The global string for 'Transactions'.", TransGroupCategory.String)]
            private static string mTransactions = string.Empty;
            [Translate("Container Number", "The global string for 'Container Number'.", TransGroupCategory.String)]
            private static string mContainerNumber = string.Empty;
            [Translate("Of", "The global string for 'Of'.", TransGroupCategory.String)]
            private static string mOf = string.Empty;
            [Translate("Yes", "The global string for 'Yes'.", TransGroupCategory.String)]
            private static string mYes = string.Empty;
            [Translate("No", "The global string for 'No'.", TransGroupCategory.String)]
            private static string mNo = string.Empty;
            [Translate("Warning", "The global string for 'Warning'.", TransGroupCategory.String)]
            private static string mWarning = string.Empty;
            [Translate("Documents", "The global string for 'Documents'.", TransGroupCategory.String)]
            private static string mDocuments = string.Empty;
            [Translate("Weighted Average Grade", "The global string for 'Weighted Average Grade'.", TransGroupCategory.String)]
            private static string mWeightedAverageGrade = string.Empty;
            [Translate("Material", "The text for the Material node in the solution explorer treeview", TransGroupCategory.TreeNode)]
            private static string mMaterial = string.Empty;
            [Translate("Package Types", "The global string for 'Package Types'.", TransGroupCategory.String)]
            private static string mPackageTypes = string.Empty;

            [Translate("Reclaim", "The global string for 'Reclaim'.", TransGroupCategory.String)]
            private static string mReclaim = string.Empty;
            [Translate("Reclaimer Parameters", "The global string for 'Reclaimer'.", TransGroupCategory.String)]
            private static string mReclaimer = string.Empty;
            [Translate("Reclaim Details", "The global string for 'Reclaim Details'.", TransGroupCategory.String)]
            private static string mReclaimDetails = string.Empty;
            [Translate("Reclaimer", "The global string for 'Reclaimer Details'.", TransGroupCategory.String)]
            private static string mReclaimerDetails = string.Empty;
            [Translate("Stack", "The global string for 'Stack'.", TransGroupCategory.String)]
            private static string mStack = string.Empty;
            [Translate("Stacker Parameters", "The global string for 'Stacker'.", TransGroupCategory.String)]
            private static string mStacker = string.Empty;
            [Translate("Stack Details", "The global string for 'Stack Details'.", TransGroupCategory.String)]
            private static string mStackDetails = string.Empty;
            [Translate("Stacker", "The global string for 'Stacker Details'.", TransGroupCategory.String)]
            private static string mStackerDetails = string.Empty;

            [Translate("Side View", "The global string for 'Side View'.", TransGroupCategory.String)]
            private static string mSideView = string.Empty;
            [Translate("Top View", "The global string for 'Top View'.", TransGroupCategory.String)]
            private static string mTopView = string.Empty;
            [Translate("End View", "The global string for 'End View'.", TransGroupCategory.String)]
            private static string mEndView = string.Empty;

            [Translate("Properties", "The global string for 'Properties'.", TransGroupCategory.String)]
            private static string mProperties = string.Empty;
            [Translate("Group", "The global string for 'Group'.", TransGroupCategory.String)]
            private static string mGroup = string.Empty;
            [Translate("Rill Angle", "The global string for 'Rill Angle'.", TransGroupCategory.String)]
            private static string mRillAngle = string.Empty;
            [Translate("Reclaim Results", "The global string for 'Reclaim Results'.", TransGroupCategory.String)]
            private static string mReclaimResults = string.Empty;
            [Translate("Contributors", "The global string for 'Contributors'.", TransGroupCategory.String)]
            private static string mContributors = string.Empty;
            [Translate("Stockpile View", "The global string for 'Stockpile View'.", TransGroupCategory.String)]
            private static string mStockpileView = string.Empty;
            [Translate("Initial State", "The global string for 'Initial State'.", TransGroupCategory.String)]
            private static string mInitialState = string.Empty;
            [Translate("Unknown Reclaim", "The global string for 'Unknown Reclaim'.", TransGroupCategory.String)]
            private static string mUnknownReclaim = string.Empty;
            [Translate("Reclaim Plan", "The global string for 'Reclaim Plan'.", TransGroupCategory.String)]
            private static string mReclaimPlan = string.Empty;
            [Translate("Reclaimed Material", "The global string for 'Reclaimed Material'.", TransGroupCategory.String)]
            private static string mReclaimedMaterial = string.Empty;

            [Translate("3D View", "The global string for '3D View'.", TransGroupCategory.String)]
            private static string mThreeDView = string.Empty;
            [Translate("Timetable", "The global string for 'Timetable'.", TransGroupCategory.String)]
            private static string mTimeTable = string.Empty;
            [Translate("Density", "The global string for 'Density'.", TransGroupCategory.String)]
            private static string mDensity = string.Empty;

            [Translate("Colour By", "The global string for 'Colour By'.", TransGroupCategory.String)]
            private static string mColourBy = string.Empty;
            [Translate("Colour", "The global string for 'Colour'.", TransGroupCategory.String)]
            private static string mColour = string.Empty;

            [Translate("Age", "The global string for 'Age'.", TransGroupCategory.String)]
            private static string mAge = string.Empty;
            [Translate("Average Age", "The global string for 'Average Age'.", TransGroupCategory.String)]
            private static string mAverageAge = string.Empty;

            [Translate("Batch", "The global string for 'Batch'.", TransGroupCategory.String)]
            private static string mBatch = string.Empty;
            [Translate("Batch ID", "The global string for 'Batch ID'.", TransGroupCategory.String)]
            private static string mBatchID = string.Empty;

            [Translate("Mixture ID", "The global string for 'Mixture ID'.", TransGroupCategory.String)]
            private static string mMixtureID = string.Empty;
            [Translate("Mixed Product", "The global string for 'Mixed Product'.", TransGroupCategory.String)]
            private static string mMixedProduct = string.Empty;

            [Translate("Reconciliation", "The global string for 'Reconciliation'.", TransGroupCategory.String)]
            private static string mReconciliation = string.Empty;
            [Translate("Last Reconciliation", "The global string for 'Last Recon'.", TransGroupCategory.String)]
            private static string mLastRecon = string.Empty;

            [Translate("Factorise", "The global string for 'Factorise'.", TransGroupCategory.String)]
            private static string mFactorise = string.Empty;

            [Translate("Ready", "The global string for 'Ready'.", TransGroupCategory.String)]
            private static string mReady = string.Empty;

            [Translate("Searching", "The global string for 'Searching'.", TransGroupCategory.String)]
            private static string mSearching = string.Empty;

            [Translate("Planned V Actual", "The global string for 'Planned V Actual'.", TransGroupCategory.String)]
            private static string mPlannedVActual = string.Empty;

            [Translate("<Name Pending>", "The global string for objects that having a naming script naming action pending.", TransGroupCategory.String)]
            private static string mNamePending = string.Empty;

            [Translate("No Contributors", "The global string for 'No Contributors'.", TransGroupCategory.String)]
            private static string mNoContributors = string.Empty;

            [Translate("No Contributor Group Selected", "The global string for 'No Contributors'.", TransGroupCategory.String)]
            private static string mNoContributorGroupSelected = string.Empty;

            [Translate("All Contributors", "The global string for 'All Contributors'.", TransGroupCategory.String)]
            private static string mAllContributors = string.Empty;

            [Translate("All Contributor Groups", "The global string for 'All Contributor Groups'.", TransGroupCategory.String)]
            private static string mAllContributorGroups = string.Empty;

            [Translate("All", "The global string for 'All'.", TransGroupCategory.String)]
            private static string mAll = string.Empty;

            [Translate("Action Required", "The text to notify user that a user action is required to complete a process", TransGroupCategory.Label)]
            private static string mActionRequired = string.Empty;

            [Translate("Action", "The global string for 'Action'", TransGroupCategory.String)]
            private static string mAction = string.Empty;
            [Translate("Criteria", "The string for 'Criteria'.", TransGroupCategory.String)]
            private static string mCriteria = string.Empty;

            [Translate("Seller", "The string for 'Seller'.", TransGroupCategory.String)]
            private static string mSeller = string.Empty;

            [Translate("Buyer", "The string for 'Buyer'.", TransGroupCategory.String)]
            private static string mBuyer = string.Empty;

            [Translate("Bulk Package Group", "The string for 'Bulk Package Group'.", TransGroupCategory.String)]
            private static string mBulkPackageGroup = string.Empty;

            [Translate("Discrete Package Group", "The string for 'Discrete Package Group'.", TransGroupCategory.String)]
            private static string mDiscretePackageGroup = string.Empty;

            [Translate("Transaction date loaded is outside arrival and/or departure times.", "Exception message if transaction date loaded is outside arrival and/or depature times.", TransGroupCategory.String)]
            private static string mTransactionLoadDate = string.Empty;

            [Translate("© Copyright {0} ABB. All rights reserved.", "The string for the application copyright.", TransGroupCategory.String)]
            private static string mNewApplicationCopyright = string.Empty;

            [Translate("Confirm Delete", "The string for 'Confirm Delete'.", TransGroupCategory.String)]
            private static string mConfirmDelete = string.Empty;

            [Translate("Zoom", "The string for 'Zoom'.", TransGroupCategory.String)]
            private static string mZoom = string.Empty;

            [Translate("Zoom In", "The string for 'Zoom In'.", TransGroupCategory.String)]
            private static string mZoomIn = string.Empty;

            [Translate("Zoom Out", "The string for 'Zoom Out'.", TransGroupCategory.String)]
            private static string mZoomOut = string.Empty;

            [Translate("Reset", "The string for 'Reset'.", TransGroupCategory.String)]
            private static string mReset = string.Empty;

            [Translate("Contract Template", "The string for 'Contract Template'.", TransGroupCategory.String)]
            private static string mContractTemplate = string.Empty;

            [Translate("Toll Analyte", "The string for 'Toll Analyte'", TransGroupCategory.String)]
            private static string mTollAnalyte = string.Empty;

            [Translate("Show Contributors", "The string for 'Show Contributors'", TransGroupCategory.String)]
            private static string mShowContributors = string.Empty;

            [Translate("Palette", "The string for 'Palette'", TransGroupCategory.String)]
            private static string mPalette = string.Empty;

            [Translate("Web", "The string for 'Web'", TransGroupCategory.String)]
            private static string mWeb = string.Empty;

            [Translate("System", "The string for 'System'", TransGroupCategory.String)]
            private static string mSystem = string.Empty;

            [Translate("Creation Completed", "The string for 'Creation Completed'", TransGroupCategory.String)]
            private static string mCreationCompleted = string.Empty;

            [Translate("Default Invoice", "Name of the default invoice report template.", TransGroupCategory.String)]
            private static string mDefaultInvoice = string.Empty;

            [Translate("Default Proforma", "Name of the default proforma report template.", TransGroupCategory.String)]
            private static string mDefaultProforma = string.Empty;

            [Translate("Record:|of", "The text displayed in the record navigator control at the bottom of Janus grids.", TransGroupCategory.String)]
            private static string mRecordNavigatorText = string.Empty;

            [Translate("Version", "The string for 'Version'", TransGroupCategory.String)]
            private static string mVersion = string.Empty;

            [Translate("Open Analyte Status History", "The string for 'Open Analyte Status History' context menu option", TransGroupCategory.String)]
            private static string mOpenAnalyteStatusHistory = string.Empty;

            [Translate("Display", "The string for 'Display'", TransGroupCategory.String)]
            private static string mDisplay = string.Empty;
            [Translate("Create", "The string for 'Create'", TransGroupCategory.String)]
            private static string mCreateText = string.Empty;

            [Translate("Today", "The string for 'Today'", TransGroupCategory.String)]
            private static string mToday = string.Empty;

            [Translate("MineMarket", "The string for 'MineMarket'", TransGroupCategory.String)]
            private static string mMineMarket = string.Empty;

            [Translate("Default", "The string for 'Default'", TransGroupCategory.String)]
            private static string mDefault = string.Empty;

            [Translate("True", "The string for 'True'", TransGroupCategory.String)]
            private static string mTrue = string.Empty;

            [Translate("False", "The string for 'False'", TransGroupCategory.String)]
            private static string mFalse = string.Empty;

            [Translate("Auto Resize Columns", "The global string for 'Auto Resize Columns'", TransGroupCategory.String)]
            private static string mAutoResizeColumns = string.Empty;

            [Translate("The gross mass must be greater than or equal to the net mass.", "Message displays during Gross and Net Mass validation", TransGroupCategory.String)]
            private static string mNetGrossMassInvalid = string.Empty;

            #region Clipboard warnings
            [Translate("Warning. The clipboard data is larger than the selected range.\nExcess data will be inserted into cells outside of the range and existing data overridden.\nContinue with clipboard paste?", "", TransGroupCategory.String)]
            private static string mClipboardDataLargerThanRangeMessage = string.Empty;
            [Translate("Warning. The clipboard data is too large to fit in the grid. Data will be truncated.", "", TransGroupCategory.String)]
            private static string mClipboardDataLargerThanGridMessage = string.Empty;
            [Translate("Unable to paste data from clipboard. The data format is not compatible with the grid.", "", TransGroupCategory.String)]
            private static string mIncompatibleClipboardDataMessage = string.Empty;
            [Translate("Unable to paste data from clipboard. The clipboard contains non-numeric values.", "", TransGroupCategory.String)]
            private string mClipboardContainsNonNumericDataMessage = string.Empty;
            [Translate("Unable to access data in the Windows clipboard.", "", TransGroupCategory.String)]
            private static string mWindowsClipboardInaccessibleMessage = string.Empty;

            [Translate("Process Flows", "Generic text for Process Flows used in grids, forms, etc.", TransGroupCategory.String)]
            private static string mProcessFlows = string.Empty;
            [Translate("Products", "Generic text for Products used in grids, forms, etc.", TransGroupCategory.String)]
            private static string mProducts = string.Empty;
            [Translate("Product", "Generic text for Product used in grids, forms, etc.", TransGroupCategory.String)]
            private static string mProduct = string.Empty;
            #endregion

            private static string mCopyrightYears = null;

            private GlobalStrings()
            {
            }

            static GlobalStrings()
            {
                AssemblyCopyrightAttribute attribute = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>();
                if (attribute != null && !string.IsNullOrEmpty(attribute.Copyright))
                {
                    Match match = Regex.Match(attribute.Copyright, @"(\d{4})(\-\d{4})?");
                    mCopyrightYears = match.Success ? match.Value : null;
                }
            }

            public static string System
            {
                get { return mSystem; }
            }
            public static string Web
            {
                get { return mWeb; }
            }
            public static string Palette
            {
                get { return mPalette; }
            }

            public static string BulkPackageGroup
            {
                get { return mBulkPackageGroup; }
            }

            public static string DiscretePackageGroup
            {
                get { return mDiscretePackageGroup; }
            }

            public static string OpenSourceStockpile
            {
                get { return mOpenSourceStockpile; }
            }

            public static string OpenDestStockpile
            {
                get { return mOpenDestStockpile; }
            }

            public static string New
            {
                get { return mNew; }
            }

            public static string Open
            {
                get { return mOpen; }
            }

            public static string Edit
            {
                get { return mEdit; }
            }

            public static string View
            {
                get { return mView; }
            }

            public static string Rename
            {
                get { return mRename; }
            }

            public static string Delete
            {
                get { return mDelete; }
            }

            public static string DeleteAll
            {
                get { return mDeleteAll; }
            }

            public static string Add
            {
                get { return mAdd; }
            }

            public static string Warning
            {
                get { return mWarning; }
            }

            public static string Remove
            {
                get { return mRemove; }
            }

            public static string RemoveAll
            {
                get { return mRemoveAll; }
            }

            public static string Unassign
            {
                get { return mUnassign; }
            }

            public static string Appearance
            {
                get { return mAppearance; }
            }

            public static string Description
            {
                get { return mDescription; }
            }

            public static string Ok
            {
                get { return mOk; }
            }

            public static string Clear
            {
                get { return mClear; }
            }

            public static string Select
            {
                get { return mSelect; }
            }

            public static string SelectAll
            {
                get { return mSelectAll; }
            }

            public static string Deselect
            {
                get { return mDeselect; }
            }

            public static string DeselectAll
            {
                get { return mDeselectAll; }
            }

            public static string Cancel
            {
                get { return mCancel; }
            }

            public static string Close
            {
                get { return mClose; }
            }

            public static string Detach
            {
                get { return mDetach; }
            }

            public static string AttachToMineMarket
            {
                get { return mAttachToMineMarket; }
            }

            public static string AttachToGroupForm
            {
                get { return mAttachToGroupForm; }
            }

            public static string Apply
            {
                get { return mApply; }
            }

            public static string QueryTimingStatusMessage
            {
                get { return mQueryTimingStatusMessage; }
            }

            public static string Status
            {
                get { return mStatus; }
            }

            public static string ChartItemWizard
            {
                get { return mChartItemWizard; }
            }

            public static string ChartItemSetDefaults
            {
                get { return mChartItemSetDefaults; }
            }

            public static string ChartItemSaveAppearance
            {
                get { return mChartItemSaveAppearance; }
            }

            public static string ChartItemLoadAppearance
            {
                get { return mChartItemLoadAppearance; }
            }

            public static string ChartItemToClipboard
            {
                get { return mChartItemToClipboard; }
            }

            public static string ChartItemToFile
            {
                get { return mChartItemToFile; }
            }

            public static string ChartItemToPrinter
            {
                get { return mChartItemToPrinter; }
            }

            public static string Print
            {
                get { return mPrint; }
            }

            public static string PrintAll
            {
                get { return mPrintAll; }
            }

            public static string PrintPreview
            {
                get { return mPrintPreview; }
            }

            public static string Moisture
            {
                get { return mMoisture; }
            }

            public static string AsReceived
            {
                get { return mAsReceived; }
            }

            public static string DryBasis
            {
                get { return mDryBasis; }
            }

            public static string Tonnes
            {
                get { return mTonnes; }
            }

            public static string Mass
            {
                get { return mMass; }
            }

            public static string MassUOM
            {
                get { return mMassUOM; }
            }

            public static string Kilograms
            {
                get { return mKilograms; }
            }

            public static string BCM
            {
                get { return mBCM; }
            }

            public static string Traceability
            {
                get { return mTraceability; }
            }

            public static string Percentage
            {
                get { return mPercentage; }
            }

            public static string PartsPerMillion
            {
                get { return mPartsPerMillion; }
            }

            public static string Test
            {
                get { return mTest; }
            }

            public static string Loading
            {
                get { return mDefaultLoadText; }
            }

            public static string PlanLoading
            {
                get { return mPlanLoadingText; }
            }

            public static string UnLoading
            {
                get { return mDefaultUnLoadText; }
            }

            public static string Name
            {
                get { return mName; }
            }

            public static string AnalyteContent
            {
                get { return mAnalyteContent; }
            }

            public static string Value
            {
                get { return mValue; }
            }

            public static string Content
            {
                get { return mContent; }
            }

            public static string UnableToDelete
            {
                get { return mUnableToDelete; }
            }

            public static string SecurityDenied
            {
                get { return mSecurityDeniedOperation; }
            }

            public static string TextTooLong
            {
                get { return mTextTooLong; }
            }

            public static string TBN
            {
                get { return mTBN; }
            }

            public static string MoveFirst
            {
                get { return mMoveFirst; }
            }

            public static string MoveLast
            {
                get { return mMoveLast; }
            }

            public static string MoveUp
            {
                get { return mMoveUp; }
            }

            public static string MoveDown
            {
                get { return mMoveDown; }
            }

            public static string Planned
            {
                get { return mPlanned; }
            }

            public static string CopyToClipboard
            {
                get { return mCopyToClipboard; }
            }

            public static string DisplayActuals
            {
                get { return mDisplayActuals; }
            }

            public static string DisplayPlanned
            {
                get { return mDisplayPlanned; }
            }

            public static string Refresh
            {
                get { return mRefresh; }
            }

            public static string RefreshAll
            {
                get { return mRefreshAll; }
            }
            //
            //			private static string mDisplayActuals = string.Empty;
            //			[Translate("Include Actuals", "The string for 'Display Actuals' on the planning screen.", TransGroupCategory.String)]
            //			private static string mD = "";
            //			[Translate("Display Planned Only", "The string for 'Display Planned Only' on the planning screen.", TransGroupCategory.String)]

            public static string Millimetres
            {
                get { return mMillimetres; }
            }

            public static string Inches
            {
                get { return mInches; }
            }

            public static string Metres
            {
                get { return mMetres; }
            }

            public static string Copy
            {
                get { return mCopy; }
            }

            public static string Paste
            {
                get { return mPaste; }
            }

            public static string Actual
            {
                get { return mActual; }
            }

            public static string InvalidDate
            {
                get { return mInvalidDate; }
            }

            public static string InvalidNumber
            {
                get { return mInvalidNumber; }
            }

            public static string Capacity
            {
                get { return mCapacity; }
            }

            public static string Quantity
            {
                get { return mQuantity; }
            }

            public static string Chart
            {
                get { return mChart; }
            }

            public static string ViewTransactions
            {
                get { return mViewTransactions; }
            }

            public static string ViewStockpileStates
            {
                get { return mViewStockpileStates; }
            }

            public static string NoVesselClassDefined
            {
                get { return mNoVesselClassDefined; }
            }

            public static string SourceStockpiles
            {
                get { return mSourceStockpiles; }
            }

            public static string DestinationStockpiles
            {
                get { return mDestinationStockpiles; }
            }

            public static string Sequence
            {
                get { return mSequence; }
            }

            public static string Seq
            {
                get { return mSeq; }
            }

            public static string Duplicate
            {
                get { return mDuplicate; }
            }

            public static string Activated
            {
                get { return mActivated; }
            }

            public static string Deactivated
            {
                get { return mDeactivated; }
            }

            public static string Activate
            {
                get { return mActivate; }
            }

            public static string Deactivate
            {
                get { return mDeactivate; }
            }

            public static string Destination
            {
                get { return mDestination; }
            }

            public static string Source
            {
                get { return mSource; }
            }

            public static string DatePeriod
            {
                get { return mDatePeriod; }
            }

            public static string Accept
            {
                get { return mAccept; }
            }

            public static string TransactionsExistAgainstProcessFlow
            {
                get { return mTransactionsExistAgainstProcessFlow; }
            }

            public static string InvalidProcessFlowDestination
            {
                get { return mInvalidProcessFlowDestination; }
            }

            public static string StateAtDeparture
            {
                get { return mStateAtDeparture; }
            }

            public static string TimeOfArrival
            {
                get { return mTimeOfArrival; }
            }

            public static string TimeOfDeparture
            {
                get { return mTimeOfDeparture; }
            }

            public static string SearchText
            {
                get { return mSearchText; }
            }

            public static string Search
            {
                get { return mSearch; }
            }

            public static string SearchBy
            {
                get { return mSearchBy; }
            }

            public static string SortBy
            {
                get { return mSortBy; }
            }

            public static string SearchFor
            {
                get { return mSearchFor; }
            }

            public static string SearchCriteria
            {
                get { return mSearchCriteria; }
            }

            public static string SearchForTransactions
            {
                get { return mSearchForTransactions; }
            }

            public static string Save
            {
                get { return mSave; }
            }

            public static string Billboard
            {
                get { return mBillboard; }
            }

            public static string TimeLine
            {
                get { return mTimeLine; }
            }

            public static string ReverseView
            {
                get { return mReverseView; }
            }

            public static string Balance
            {
                get { return mBalance; }
            }

            public static string CurrentTonnage
            {
                get { return mCurrentTonnage; }
            }

            public static string RemoveColumn
            {
                get { return mRemoveColumn; }
            }

            public static string Total
            {
                get { return mTotal; }
            }

            public static string GrandTotal
            {
                get { return mGrandTotal; }
            }

            public static string Location
            {
                get { return mLocation; }
            }

            public static string Locations
            {
                get { return mLocations; }
            }

            public static string ViewByDay
            {
                get { return mViewByDay; }
            }

            public static string ViewByWeek
            {
                get { return mViewByWeek; }
            }

            public static string ViewByMonth
            {
                get { return mViewByMonth; }
            }

            public static string ViewByYear
            {
                get { return mViewByYear; }
            }

            public static string DespatchArrivalDates
            {
                get { return mDespatchArrivalDates; }
            }

            public static string DespatchDepartureDates
            {
                get { return mDespatchDepatureDates; }
            }

            public static string DespatchStartLoadingDates
            {
                get { return mDespatchStartLoadingDates; }
            }

            public static string DespatchEndLoadingDates
            {
                get { return mDespatchEndLoadingDates; }
            }

            public static string DespatchStartUnloadingDates
            {
                get { return mDespatchStartUnloadingDates; }
            }

            public static string DespatchEndUnloadingDates
            {
                get { return mDespatchEndUnloadingDates; }
            }

            public static string FromDate
            {
                get { return mFromDate; }
            }

            public static string ToDate
            {
                get { return mToDate; }
            }

            public static string Summary
            {
                get { return mSummary; }
            }

            public static string Activity
            {
                get { return mActivity; }
            }

            public static string Details
            {
                get { return mDetails; }
            }

            public static string Structure
            {
                get { return mStructure; }
            }

            public static string AssignAll
            {
                get { return mAssignAll; }
            }

            public static string UnassignAll
            {
                get { return mUnassignAll; }
            }

            public static string Assigned
            {
                get { return mAssigned; }
            }

            public static string None
            {
                get { return mNone; }
            }

            public static string Customers
            {
                get { return mCustomers; }
            }

            public static string ShippingAgents
            {
                get { return mShippingAgents; }
            }

            public static string Contractors
            {
                get { return mContractors; }
            }

            public static string Pilots
            {
                get { return mPilots; }
            }

            public static string Commercial
            {
                get { return mCommercial; }
            }

            public static string Superintending
            {
                get { return mSuperintending; }
            }

            public static string Ownership
            {
                get { return mOwnership; }
            }
            public static string Domains
            {
                get { return mDomains; }
            }
            public static string Domain
            {
                get { return mDomain; }
            }
            public static string DomainMembership
            {
                get { return mDomainMembership; }
            }

            public static string BillOfLading
            {
                get { return mBillOfLading; }
            }

            public static string StaticGroups
            {
                get { return mStaticGroups; }
            }

            public static string Required
            {
                get { return mRequired; }
            }

            public static string Delivered
            {
                get { return mDelivered; }
            }

            public static string Despatch
            {
                get { return mDespatch; }
            }

            public static string Quality
            {
                get { return mQuality; }
            }

            public static string Selected
            {
                get { return mSelected; }
            }

            public static string Date
            {
                get { return mDate; }
            }

            public static string Time
            {
                get { return mTime; }
            }

            public static string InvalidDateRange
            {
                get { return mInvalidDateRange; }
            }

            public static string Comment
            {
                get { return mComment; }
            }

            public static string Approval
            {
                get { return mApproval; }
            }

            public static string Kilometre
            {
                get { return mKilometre; }
            }
            public static string NauticalMile
            {
                get { return mNauticalMile; }
            }
            public static string Mile
            {
                get { return mMile; }
            }
            public static string ContractedDeliveryDate
            {
                get { return mContractedDeliveryDate; }
            }
            public static string ContractedDemand
            {
                get { return mContractedDemand; }
            }
            public static string ContractedProduct
            {
                get { return mContractedProduct; }
            }
            public static string PreferredBlend
            {
                get { return mPreferredBlend; }
            }
            public static string PreferredBlendTotalQuantity
            {
                get { return mPreferredBlendTotalQuantity; }
            }

            public static string Day
            {
                get { return mDay; }
            }

            public static string Days
            {
                get { return mDays; }
            }

            public static string Weeks
            {
                get { return mWeeks; }
            }

            public static string Hour
            {
                get { return mHour; }
            }
            public static string Minute
            {
                get { return mMinute; }
            }

            public static string Weekly
            {
                get { return mWeekly; }
            }

            public static string Monthly
            {
                get { return mMonthly; }
            }

            public static string Quarterly
            {
                get { return mQuarterly; }
            }

            public static string StartLoading
            {
                get { return mStartLoading; }
            }
            public static string EndLoading
            {
                get { return mEndLoading; }
            }

            public static string Blend
            {
                get { return mBlend; }
            }

            public static string Any
            {
                get { return mAny; }
            }

            public static string EndOfShift
            {
                get { return mEndOfShift; }
            }

            public static string StartOfShift
            {
                get { return mStartOfShift; }
            }

            public static string EndOfPreviousShift
            {
                get { return mEndOfPreviousShift; }
            }

            public static string StartOfPreviousShift
            {
                get { return mStartOfPreviousShift; }
            }

            public static string Delay
            {
                get { return mDelay; }
            }

            public static string DelayHours
            {
                get { return mDelayHours; }
            }

            public static string LoadingRate
            {
                get { return mLoadingRate; }
            }

            public static string InvalidMapObject
            {
                get { return mInvalidMapObject; }
            }

            public static string Currency
            {
                get { return mCurrency; }
            }

            public static string Invoice
            {
                get { return mInvoice; }
            }
            public static string PricingAdjustments
            {
                get { return mPricingAdjustments; }
            }

            public static string Weight
            {
                get { return mWeight; }
            }

            public static string WeightBasis
            {
                get { return mWeightBasis; }
            }

            public static string Lot
            {
                get { return mLot; }
            }
            public static string SubItems
            {
                get { return mSubItems; }
            }
            public static string Package
            {
                get { return mPackage; }
            }
            public static string Packages
            {
                get { return mPackages; }
            }

            public static string To
            {
                get { return mTo; }
            }

            public static string InvoiceReportTemplate
            {
                get { return mInvoiceReportTemplates; }
            }

            public static string ProformaReportTemplate
            {
                get { return mProformaReportTemplates; }
            }

            public static string Length
            {
                get { return mLength; }
            }

            public static string Width
            {
                get { return mWidth; }
            }

            public static string Height
            {
                get { return mHeight; }
            }

            public static string Containers
            {
                get { return mContainers; }
            }

            public static string Type
            {
                get { return mType; }
            }

            public static string Transactions
            {
                get { return mTransactions; }
            }

            public static string CopyTransactions
            {
                get { return mCopyTransactions; }
            }

            public static string ContainerNumber
            {
                get { return mContainerNumber; }
            }

            public static string Of
            {
                get { return mOf; }
            }

            public static string Yes
            {
                get { return mYes; }
            }

            public static string No
            {
                get { return mNo; }
            }

            public static string ShowDocument
            {
                get { return mShowDocument; }
            }

            public static string PrintDocument
            {
                get { return mPrintDocument; }
            }

            public static string Documents
            {
                get { return mDocuments; }
            }

            public static string WeightedAverageGrade
            {
                get { return mWeightedAverageGrade; }
            }

            public static string ExpandAllGroups
            {
                get { return mExpandAllGroups; }
            }
            public static string CollapseAllGroups
            {
                get { return mCollapseAllGroups; }
            }

            public static string AddSurvey
            {
                get { return mAddSurvey; }
            }
            public static string CollapseAll
            {
                get { return mCollapseAll; }
            }
            public static string ExpandAll
            {
                get { return mExpandAll; }
            }
            public static string Collapse
            {
                get { return mCollapse; }
            }
            public static string Expand
            {
                get { return mExpand; }
            }
            public static string Material
            {
                get { return mMaterial; }
            }

            public static string PackageTypes
            {
                get { return mPackageTypes; }
            }

            public static string Reclaim
            {
                get { return mReclaim; }
            }

            public static string Reclaimer
            {
                get { return mReclaimer; }
            }

            public static string ReclaimDetails
            {
                get { return mReclaimDetails; }
            }

            public static string ReclaimerDetails
            {
                get { return mReclaimerDetails; }
            }

            public static string Stack
            {
                get { return mStack; }
            }

            public static string Stacker
            {
                get { return mStacker; }
            }

            public static string StackDetails
            {
                get { return mStackDetails; }
            }

            public static string StackerDetails
            {
                get { return mStackerDetails; }
            }

            public static string SideView
            {
                get { return mSideView; }
            }

            public static string TopView
            {
                get { return mTopView; }
            }

            public static string EndView
            {
                get { return mEndView; }
            }

            public static string ThreeDView
            {
                get { return mThreeDView; }
            }

            public static string Properties
            {
                get { return mProperties; }
            }

            public static string Group
            {
                get { return mGroup; }
            }

            public static string RillAngle
            {
                get { return mRillAngle; }
            }

            public static string ReclaimResults
            {
                get { return mReclaimResults; }
            }

            public static string Contributors
            {
                get { return mContributors; }
            }

            public static string StockpileView
            {
                get { return mStockpileView; }
            }

            public static string InitialState
            {
                get { return mInitialState; }
            }

            public static string UnknownReclaim
            {
                get { return mUnknownReclaim; }
            }

            public static string ReclaimDataIsInvalid
            {
                get { return mReclaimDataIsInvalid; }
            }

            public static string ReclaimMissingInfo
            {
                get { return mReclaimMissingInfo; }
            }

            public static string ReclaimFailed
            {
                get { return mReclaimFailed; }
            }

            public static string ReclaimFailureDescription
            {
                get { return mReclaimFailureDescription; }
            }

            public static string NoMaterialReclaimed
            {
                get { return mNoMaterialReclaimed; }
            }

            public static string CannotLockUnlockDueRights
            {
                get { return mCannotLockUnlockDueRights; }
            }

            public static string SuccesfulBatchOperation
            {
                get { return mSuccesfulBatchOperation; }
            }

            public static string FollowingErrorsOccurredLocking
            {
                get { return mFollowingErrorsOccurredLocking; }
            }

            public static string FollowingErrorsOccurredUnLocking
            {
                get { return mFollowingErrorsOccurredUnLocking; }
            }

            public static string PlanningPeriodCannotBeChangedText
            {
                get { return mPlanningPeriodCannotBeChangedText; }
            }

            public static string PlanningPeriodChangeErrorTitle
            {
                get { return mPlanningPeriodChangeErrorTitle; }
            }

            public static string PlanUOMCannotBeChangedText
            {
                get { return mPlanUOMCannotBeChangedText; }
            }

            public static string PlanUOMChangeErrorTitle
            {
                get { return mPlanUOMChangeErrorTitle; }
            }

            public static string ReclaimPlan
            {
                get { return mReclaimPlan; }
            }

            public static string ReclaimedMaterial
            {
                get { return mReclaimedMaterial; }
            }

            public static string ColourBy
            {
                get { return mColourBy; }
            }

            public static string TimeTable
            {
                get { return mTimeTable; }
            }

            public static string Colour
            {
                get { return mColour; }
            }

            public static string Age
            {
                get { return mAge; }
            }

            public static string AverageAge
            {
                get { return mAverageAge; }
            }

            public static string Batch
            {
                get { return mBatch; }
            }

            public static string BatchID
            {
                get { return mBatchID; }
            }

            public static string Density
            {
                get { return mDensity; }
            }

            public static string MixtureID
            {
                get { return mMixtureID; }
            }

            public static string MixedProduct
            {
                get { return mMixedProduct; }
            }

            public static string Reconciliation
            {
                get { return mReconciliation; }
            }

            public static string LastRecon
            {
                get { return mLastRecon; }
            }

            public static string Factorise
            {
                get { return mFactorise; }
            }

            public static string Adjust
            {
                get { return mAdjust; }
            }

            public static string Undo
            {
                get { return mUndo; }
            }

            public static string Commit
            {
                get { return mCommit; }
            }

            public static string TWReconOpeningBalance
            {
                get { return mTWReconOpeningBalance; }
            }

            public static string TWReconClosingBalance
            {
                get { return mTWReconClosingBalance; }
            }

            public static string TWReconClosingBalanceNoStockpiles
            {
                get { return mTWReconClosingBalanceNoStockpiles; }
            }

            public static string TWReconInputs
            {
                get { return mTWReconInputs; }
            }

            public static string TWReconOutputs
            {
                get { return mTWReconOutputs; }
            }

            public static string TWReconNetChange
            {
                get { return mTWReconNetChange; }
            }

            public static string TWReconAdjustments
            {
                get { return mTWReconAdjustments; }
            }

            public static string TWReconDiscrepancy
            {
                get { return mTWReconDiscrepancy; }
            }

            public static string Duration
            {
                get { return mDuration; }
            }

            public static string Months
            {
                get { return mMonths; }
            }

            public static string Years
            {
                get { return mYears; }
            }

            public static string Ready
            {
                get { return mReady; }
            }

            public static string Searching
            {
                get { return mSearching; }
            }

            public static string PlannedVActual
            {
                get { return mPlannedVActual; }
            }

            public static string GroupBy
            {
                get { return mGroupBy; }
            }

            public static string ContributorValueFormat
            {
                get { return mContributorValueFormat; }
            }

            public static string StartDate
            {
                get { return mStartDate; }
            }

            public static string EndDate
            {
                get { return mEndDate; }
            }

            public static string NamePending
            {
                get { return mNamePending; }
            }

            public static string NoContributors
            {
                get { return mNoContributors; }
            }

            public static string AllContributors
            {
                get { return mAllContributors; }
            }

            public static string NoContributorGroupSelected
            {
                get { return mNoContributorGroupSelected; }
            }


            public static string AllContributorGroups
            {
                get { return mAllContributorGroups; }
            }


            public static string All
            {
                get
                {
                    return mAll;
                }
            }
            public static string DragAColumnHeaderHereToGroupByThatColumn
            {
                get { return mDragAColumnHeaderHereToGroupByThatColumn; }
            }

            public static string InvalidTransactionDateRange
            {
                get { return mTransactionLoadDate; }
            }

            #region Days of week

            public static string Monday
            {
                get { return mMonday; }
            }

            public static string Tuesday
            {
                get { return mTuesday; }
            }

            public static string Wednesday
            {
                get { return mWednesday; }
            }

            public static string Thursday
            {
                get { return mThursday; }
            }

            public static string Friday
            {
                get { return mFriday; }
            }

            public static string Saturday
            {
                get { return mSaturday; }
            }

            public static string Sunday
            {
                get { return mSunday; }
            }

            #endregion

            public static string ActionRequired
            {
                get { return mActionRequired; }
            }

            public static string Action
            {
                get { return mAction; }
            }

            public static string Criteria
            {
                get { return mCriteria; }
            }

            public static string Seller
            {
                get { return mSeller; }
            }

            public static string Buyer
            {
                get { return mBuyer; }
            }

            public static string TroyOunce
            {
                get { return mTroyOunce; }
            }

            public static string Pound
            {
                get { return mPound; }
            }

            public static string GramsPerTonne
            {
                get { return mGramsPerTonne; }
            }

            public static string Each
            {
                get { return mEach; }
            }

            public static string Trip
            {
                get { return mTrip; }
            }

            public static string BCMPerDay
            {
                get { return mBCMPerDay; }
            }

            public static string BCMPerHour
            {
                get { return mBCMPerHour; }
            }

            public static string BCMPerTrip
            {
                get { return mBCMPerTrip; }
            }

            public static string Megajoules
            {
                get { return mMegajoules; }
            }

            public static string Kilocalories
            {
                get { return mKilocalories; }
            }

            public static string BTU
            {
                get { return mBTU; }
            }

            public static string Grams
            {
                get { return mGrams; }
            }

            public static string Milligrams
            {
                get { return mMilligrams; }
            }

            public static string AvoirdupoisOunces
            {
                get { return mAvoirdupoisOunces; }
            }

            public static string LongTons
            {
                get { return mLongTons; }
            }

            public static string ShortTons
            {
                get { return mShortTons; }
            }

            public static string KCalPerKg
            {
                get { return mKCalPerKg; }
            }

            public static string MJPerKg
            {
                get { return mMJPerKg; }
            }

            public static string BTUPerPound
            {
                get { return mBTUPerPound; }
            }

            /// <summary>
            /// The string for the application copyright (default = '© Copyright {0} ABB. All rights reserved.').
            /// </summary>
            public static string ApplicationCopyright
            {
                get { return String.Format(mNewApplicationCopyright, mCopyrightYears); }
            }

            public static string ConfirmDelete
            {
                get { return mConfirmDelete; }
            }

            public static string Zoom
            {
                get { return mZoom; }
            }

            public static string ZoomIn
            {
                get { return mZoomIn; }
            }

            public static string ZoomOut
            {
                get { return mZoomOut; }
            }

            public static string Reset
            {
                get { return mReset; }
            }

            public static string ContractTemplate
            {
                get { return mContractTemplate; }
            }

            public static string TollAnalyte
            {
                get { return mTollAnalyte; }
            }

            public static string ShowContributors
            {
                get
                {
                    return mShowContributors;
                }
            }

            public static string CreationCompleted
            {
                get
                {
                    return mCreationCompleted;
                }

            }

            public static string DefaultInvoice
            {
                get
                {
                    return mDefaultInvoice;
                }
            }

            public static string DefaultProforma
            {
                get
                {
                    return mDefaultProforma;
                }
            }

            public static string Version
            {
                get
                {
                    return mVersion;
                }
            }

            public static string OpenAnalyteStatusHistory
            {

                get
                {
                    return mOpenAnalyteStatusHistory;
                }

            }

            public static string RecordNavigatorText
            {
                get
                {
                    return mRecordNavigatorText;
                }
            }

            public static string Display
            {
                get
                {
                    return mDisplay;
                }
            }

            public static string Today
            {
                get
                {
                    return mToday;
                }
            }

            public static string Create
            {
                get
                {
                    return mCreateText;
                }
            }

            public static string MineMarket
            {
                get
                {
                    return mMineMarket;
                }
            }

            public static string Default
            {
                get
                {
                    return mDefault;
                }
            }

            public static string True
            {
                get
                {
                    return mTrue;
                }
            }

            public static string False
            {
                get
                {
                    return mFalse;
                }
            }

            public static string AutoResizeColumns
            {
                get
                {
                    return mAutoResizeColumns;
                }
            }

            public static string NetGrossMassInvalid
            {
                get
                {
                    return mNetGrossMassInvalid;
                }
            }

            public static string ClipboardDataLargerThanRangeMessage
            {
                get
                {
                    return mClipboardDataLargerThanRangeMessage;
                }
            }

            public static string ClipboardDataLargerThanGridMessage
            {
                get
                {
                    return mClipboardDataLargerThanGridMessage;
                }
            }

            public static string IncompatibleClipboardDataMessage
            {
                get
                {
                    return mIncompatibleClipboardDataMessage;
                }
            }

            public static string ClipboardContainsNonNumericDataMessage
            {
                get
                {
                    return ClipboardContainsNonNumericDataMessage;
                }
            }

            public static string WindowsClipboardInaccessibleMessage
            {
                get
                {
                    return mWindowsClipboardInaccessibleMessage;
                }
            }

            public static string ProcessFlows
            {
                get
                {
                    return mProcessFlows;
                }
            }

            public static string Products
            {
                get
                {
                    return mProducts;
                }
            }
            public static string Product
            {
                get
                {
                    return mProduct;
                }
            }
        }
        #endregion

    }

    #endregion
}
