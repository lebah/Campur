using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropDownComboBoxMultiLineEditor
{
    #region Translation object class
    /// <summary>
    /// Represents an individual translation. 
    /// </summary>
    [DataTable("Translation", true)]
    [TypeConverter(typeof(GenericBTObjectConverter))]
    [ObjectCategory("Localization", "Translation")]
    public class Translation : BTNamedBusinessObject
    {
        #region Private declarations
        private TranslationItems mTranslationItems = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Private, compulsory constructor used by persistence mechanism during an object load.
        /// </summary>
        /// <param name="key">The previously assigned key for the new object.</param>
        private Translation(BTKey key) : base(key)
        {
            mTranslationItems = new TranslationItems(this);
        }

        /// <summary>
        /// Public constructor used to create a new instance of a translation.
        /// </summary>
        /// <param name="name">The name of the translation</param>
        /// <param name="key">A new id.</param>
        public Translation(string name, BTKey key)
            : base(key)
        {
            mTranslationItems = new TranslationItems(this);
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">name of the translation</param>
        public Translation(string name) : base()
        {
            mTranslationItems = new TranslationItems(this);
            Name = name;
        }

        public Translation() : base()
        {
            mTranslationItems = new TranslationItems(this);
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Collection. Collection of translation items
        /// </summary>
        [CollectionTable("TransTransItems", "TranslationID", "TranslationItemID", typeof(TranslationItem))]
        [TypeConverter(typeof(GenericBTCollectionConverter))]
        [Mincom.MineMarket.DAL.Category("Hierarchy"), Mincom.MineMarket.DAL.Description("The translation items for this translation")]
        [Editor(typeof(BaseCollectionHashtableEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public TranslationItems TranslationItems
        {
            get
            {
                return mTranslationItems;
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Method used to create a deep copy of this object.
        /// </summary>
        /// <returns>New translation item</returns>
        public Translation Copy(string newTranslationName)
        {
            Translation newTranslation = new Translation(newTranslationName);
            foreach (TranslationItem transItem in this.mTranslationItems)
                newTranslation.TranslationItems.Add(transItem.Copy());
            return newTranslation;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="transGeneralType"></param>
        ///// <returns></returns>
        //public IEnumerable GetEnumerator(TransGeneralType transGeneralType)
        //{
        //    return new CategoryTransEnumerator(transGeneralType, mTranslationItems.GetList());
        //}
        #endregion

        #region Implementation of IKeyedObject
        /// <summary>
        /// The unique identity of this Translation instance
        /// </summary>
        [DataField("TranslationID", false, typeof(BTKey))]
        public override BTKey ID
        {
            get
            {
                return base.mID;
            }
        }
        #endregion

        //		#region Implementation of INamedObject
        //		/// <summary>
        //		/// Property. The name of the Area instance
        //		/// </summary>
        //		[DataField("Name", true, typeof(string))]
        //		[Mincom.MineMarket.DAL.Category("Identity"), Mincom.MineMarket.DAL.Description("The name of this business entity")]
        //		public string Name
        //		{
        //			get
        //			{
        //				if (mName == null || mName == string.Empty)
        //					mName = NameGenerator.GetName(this);
        //				return mName;
        //			}
        //			set
        //			{	
        //				mOldValue = mName;
        //				mName = value;
        //				base.PropertyChanged("Name", value);
        //			}
        //		}
        //		[DataField("NameGenerated", false, typeof(bool))]
        //		[Browsable(false)]
        //		public bool NameGenerated
        //		{
        //			get
        //			{
        //				return mNameGenerated;
        //			}
        //			set
        //			{
        //				mOldValue = mNameGenerated;
        //				mNameGenerated = value;
        //				PropertyChanged("NameGenerated", value);
        //			}
        //		}
        //		#endregion

        #region Static GenericObjectFactor Method
        /// <summary>
        /// Static method that returns a generic object factory
        /// </summary>
        /// <returns>An object factory ready to generate instances of this class</returns>
        public static GenericObjectFactory GetFactory()
        {
            //rather than create a new factory for each call, use the utility object
            //to implement simple caching. This is done because the factory object contains a remote reference
            //that is expensive to create often
            return GenericObjectFactory.GetFactory(typeof(Translation));
        }
        #endregion

    }
    #endregion
}
