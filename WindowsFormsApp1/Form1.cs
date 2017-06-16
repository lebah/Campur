using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();            ;
            InitPropertyGrid();
        }

        private void InitPropertyGrid()
        {
            // The initial constructor code goes here.

            PropertyGrid propertyGrid1 = new PropertyGrid();
            propertyGrid1.CommandsVisibleIfAvailable = true;
            propertyGrid1.Location = new Point(10, 20);
            propertyGrid1.Size = new System.Drawing.Size(400, 300);
            propertyGrid1.TabIndex = 1;
            propertyGrid1.Text = "Property Grid";
            propertyGrid1.Dock = DockStyle.Right;
            propertyGrid1.PropertyTabs.AddTabType(typeof(TypeCategoryTab));

            this.tabPage2.Controls.Add(propertyGrid1);

            propertyGrid1.SelectedObject = textBox2;
        }

        private void DisplayTypeInfo()
        {
            TypeInfo t = typeof(Calendar).GetTypeInfo();
            IEnumerable<PropertyInfo> pList = t.DeclaredProperties;
            IEnumerable<MethodInfo> mList = t.DeclaredMethods;

            StringBuilder sb = new StringBuilder();

            sb.Append("Properties:");
            foreach (PropertyInfo p in pList)
            {

                sb.Append(Environment.NewLine + p.DeclaringType.Name + ": " + p.Name + " & Type :" + p.PropertyType.Name);
            }
            sb.Append(Environment.NewLine + "Methods:");
            foreach (MethodInfo m in mList)
            {
                sb.Append(Environment.NewLine + m.DeclaringType.Name + ": " + m.Name);
            }

            textBox1.Text = sb.ToString();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DisplayTypeInfo();
        }
    }

    // This component adds a TypeCategoryTab to the property browser 
    // that is available for any components in the current design mode document.
    [PropertyTabAttribute(typeof(TypeCategoryTab), PropertyTabScope.Document)]
    public class TypeCategoryTabComponent : System.ComponentModel.Component
    {
        public TypeCategoryTabComponent()
        {
        }
    }

    // A TypeCategoryTab property tab lists properties by the 
    // category of the type of each property.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class TypeCategoryTab : PropertyTab
    {
        [Browsable(true)]
        // This string contains a Base-64 encoded and serialized example property tab image.
        private string img = "AAEAAAD/////AQAAAAAAAAAMAgAAAFRTeXN0ZW0uRHJhd2luZywgVmVyc2lvbj0xLjAuMzMwMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWIwM2Y1ZjdmMTFkNTBhM2EFAQAAABVTeXN0ZW0uRHJhd2luZy5CaXRtYXABAAAABERhdGEHAgIAAAAJAwAAAA8DAAAA9gAAAAJCTfYAAAAAAAAANgAAACgAAAAIAAAACAAAAAEAGAAAAAAAAAAAAMQOAADEDgAAAAAAAAAAAAD///////////////////////////////////9ZgABZgADzPz/zPz/zPz9AgP//////////gAD/gAD/AAD/AAD/AACKyub///////+AAACAAAAAAP8AAP8AAP9AgP////////9ZgABZgABz13hz13hz13hAgP//////////gAD/gACA/wCA/wCA/wAA//////////+AAACAAAAAAP8AAP8AAP9AgP////////////////////////////////////8L";

        public TypeCategoryTab()
        {
        }

        // Returns the properties of the specified component extended with 
        // a CategoryAttribute reflecting the name of the type of the property.
        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            PropertyDescriptorCollection props;
            if (attributes == null)
                props = TypeDescriptor.GetProperties(component);
            else
                props = TypeDescriptor.GetProperties(component, attributes);

            PropertyDescriptor[] propArray = new PropertyDescriptor[props.Count];
            for (int i = 0; i < props.Count; i++)
            {
                // Create a new PropertyDescriptor from the old one, with 
                // a CategoryAttribute matching the name of the type.
                propArray[i] = TypeDescriptor.CreateProperty(props[i].ComponentType, props[i], new CategoryAttribute(props[i].PropertyType.Name));
            }
            return new PropertyDescriptorCollection(propArray);
        }

        public override PropertyDescriptorCollection GetProperties(object component)
        {
            return this.GetProperties(component, null);
        }

        // Provides the name for the property tab.
        public override string TabName
        {
            get
            {
                return "Properties by Type 2";
            }
        }

        // Provides an image for the property tab.
        public override Bitmap Bitmap
        {
            get
            {
                Bitmap bmp = new Bitmap(DeserializeFromBase64Text(img));
                return bmp;
            }
        }

        // This method can be used to retrieve an Image from a block of Base64-encoded text.
        private Image DeserializeFromBase64Text(string text)
        {
            Image img = null;
            byte[] memBytes = Convert.FromBase64String(text);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(memBytes);
            img = (Image)formatter.Deserialize(stream);
            stream.Close();
            return img;
        }
    }
}
