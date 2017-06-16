using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
}
