using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ImagePathHistory
{
    /*
             * A
             * 1. Open dialog box
             * 2. select the png file
             * 3. if ok the file pathn save to xml "imagepPathHistory.xml" 
             *      a. if the xml file not exist create new one "imagepPathHistory.xml" 
             * 5. if not ok 
             *      (a. the image is not exist
             *      b. the path is not valid
             *      ) 
             *      we load default image (CONST = project directory)
             * 4. load the image to iamgebox and add to the combobox
             *
             * B
             * 1. Every time we add path we need to validate all of the existing path & the file is exist
             * 
             * 
             * */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {       
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            // Create File Dialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.Title = "Open Image";
            dlg.DefaultExt = "*.png";
            dlg.Filter = "PNG (*.png)|*.png";
            dlg.Multiselect = false;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in the plugin file textbox
            if (result == true)
            {
                string filename = dlg.FileName;
            }
        }        
    }
}
